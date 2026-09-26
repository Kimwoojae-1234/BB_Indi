"""Read-only NGUI reference inventory. Does not load, save or change Unity assets."""
from pathlib import Path
from collections import Counter, defaultdict
import json
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Docs/UIAudit/NGUIRemaining'
OUT.mkdir(exist_ok=True)

def relative(path):
    return path.relative_to(ROOT).as_posix()

def read(path):
    return path.read_text(encoding='utf-8-sig', errors='replace')

def save(name, value):
    (OUT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

files = list((ROOT / 'Assets').rglob('*'))
guids = {}
for path in files:
    if path.suffix == '.meta':
        match = re.search(r'^guid: ([0-9a-f]{32})', read(path), re.M)
        if match:
            guids[match[1]] = relative(path)[:-5]
ngui = {guid: path for guid, path in guids.items() if path.startswith('Assets/NGUI/')}
build = re.findall(r'- enabled: 1\s+path: (.+)', read(ROOT / 'ProjectSettings/EditorBuildSettings.asset'))
graph = {}
owners = defaultdict(set)
assets = []
references = []
binary = []
for path in files:
    if path.suffix not in ('.prefab', '.unity', '.asset', '.anim', '.controller', '.overrideController'):
        continue
    name = relative(path)
    if name.startswith('Assets/NGUI/'):
        continue
    data = path.read_bytes()
    if not data.startswith(b'%YAML'):
        binary.append(name)
        continue
    source = data.decode('utf-8-sig', errors='replace')
    refs = set(re.findall(r'guid: ([0-9a-f]{32})', source))
    graph[name] = {guids[g] for g in refs if g in guids}
    scripts = re.findall(r'm_Script: \{[^\n]*guid: ([0-9a-f]{32})', source)
    for script in scripts:
        owners[guids.get(script, script)].add(name)
    counts = Counter(Path(ngui[g]).stem for g in scripts if g in ngui)
    if counts:
        assets.append({'path': name, 'ngui_components': sum(counts.values()), 'types': dict(counts), 'build_scene': name in build})
    ng_refs = sorted(ngui[g] for g in refs if g in ngui)
    if ng_refs:
        references.append({'path': name, 'ngui_dependencies': ng_refs})

reachable = set()
pending = build[:]
while pending:
    name = pending.pop()
    if name in reachable:
        continue
    reachable.add(name)
    pending.extend(graph.get(name, set()) - reachable)
for asset in assets:
    asset['serialized_build_reachable'] = asset['path'] in reachable

# Lexical candidates only: no comments or string literals. Conditional compilation
# and collisions with project-defined types require code review.
lex = re.compile(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*.*?\*/', re.S)
types = {Path(p).stem for p in ngui.values() if p.endswith('.cs') and '/Editor/' not in p}
pattern = re.compile(r'\b(' + '|'.join(re.escape(t) for t in sorted(types)) + r')\b')
code = []
for path in files:
    name = relative(path)
    if path.suffix != '.cs' or name.startswith('Assets/NGUI/'):
        continue
    source = lex.sub(lambda m: '\n' * m[0].count('\n'), read(path))
    aliases = re.findall(r'using\s+(\w+)\s*=', source)
    hits = []
    for number, line in enumerate(source.splitlines(), 1):
        found = sorted(set(pattern.findall(line)) - set(aliases))
        if found:
            hits.append({'line': number, 'types': found, 'code': line.strip()})
    if hits:
        code.append({'path': name, 'editor': '/Editor/' in name or '/Edirtor/' in name,
                     'serialized_owners': sorted(owners.get(name, set())), 'hits': hits})

summary = {
    'commit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip(),
    'scope': 'All Assets outside Assets/NGUI; YAML direct components plus serialized references. Binary assets and dynamic loads are not resolved.',
    'ngui_component_assets': len(assets),
    'ngui_components': sum(a['ngui_components'] for a in assets),
    'ngui_referencing_assets': len(references),
    'ngui_code_candidate_files': len(code),
    'binary_serialized_assets_not_inspected': len(binary),
    'build_scenes': build,
    'component_asset_groups': dict(Counter('/'.join(a['path'].split('/')[:3]) for a in assets)),
}
save('summary.json', summary)
save('component-assets.json', assets)
save('serialized-dependencies.json', references)
save('code-candidates.json', code)
save('binary-uninspected.json', binary)
print(json.dumps(summary, ensure_ascii=False, indent=2))
for asset in assets:
    print(f"{asset['ngui_components']:4} build={asset['build_scene']} reachable={asset['serialized_build_reachable']} {asset['path']}")
