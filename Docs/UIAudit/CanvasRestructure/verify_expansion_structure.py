"""Read-only verification of step-5 serialized identity, controller and layout changes."""
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = Path(__file__).resolve().parent
mapping = json.loads((AUDIT / 'Step5/layout-mapping.json').read_text(encoding='utf-8'))


def blocks(text):
    return {re.match(r'\d+ &(\d+)', b)[1]: b for b in text.split('--- !u!')[1:]}


def field(text, name):
    found = re.search(r'^  ' + re.escape(name) + r': (.*)$', text, re.M)
    return found[1] if found else None


def without(text, names):
    for name in names:
        text = re.sub(r'^  ' + re.escape(name) + r':[^\n]*\n', '', text, flags=re.M)
    return text.rstrip()


report = ['Step 5 serialized checks against ' + mapping['baseline']]
for asset in mapping['assets']:
    original = subprocess.check_output(['git', 'show', mapping['baseline'] + ':' + asset['source']], cwd=ROOT).decode('utf-8').replace('\r\n', '\n')
    before = blocks(original)
    after = blocks((ROOT / asset['source']).read_text(encoding='utf-8'))
    removed = set(asset['removedCanvases'])
    assert set(before) - set(after) == removed
    assert all(before[i].startswith('223 &') for i in removed)
    assert len(set(after) - set(before)) == len(asset['batches']) * 4 + 1
    members = {e['component']: e for batch in asset['batches'] for e in batch['members']}
    wrappers = {e['wrapper'] for e in members.values()}
    wrapper_gos = {re.search(r'fileID: (\d+)', field(before[t], 'm_GameObject'))[1] for t in wrappers}
    transformed = set(asset['convertedTransforms'])
    layout = set(asset['layoutComponents'])
    for ident, old in before.items():
        if ident in removed:
            continue
        new = after[ident]
        if old == new or '\n'.join(line.rstrip() for line in old.splitlines()) == '\n'.join(line.rstrip() for line in new.splitlines()):
            continue
        if ident in layout:
            old_fields = ['displayCanvas'] if ident in members else []
            assert without(old, old_fields) == without(new, old_fields + ['layoutRect', 'canvasBatch', 'presentationRoot']), 'Widget properties changed: ' + ident
        elif old.startswith(('4 &', '224 &')):
            assert field(old, 'm_GameObject') == field(new, 'm_GameObject')
            if ident not in wrappers:
                for name in ['m_LocalPosition', 'm_LocalRotation', 'm_LocalScale', 'm_Father']:
                    assert field(old, name) == field(new, name), 'Owner layout or animation parent changed: ' + ident + '/' + name
            if ident in transformed:
                assert new.startswith('224 &') and field(new, 'm_AnchorMin') == field(new, 'm_AnchorMax')
            # Original child order is preserved after filtering generated presentation rectangles.
            old_children = [i for i in re.findall(r'^  - \{fileID: (\d+)\}$', old, re.M) if i not in wrappers]
            new_children = [i for i in re.findall(r'^  - \{fileID: (\d+)\}$', new, re.M) if i in before and i not in wrappers]
            assert old_children == new_children, 'Controller sibling indices changed: ' + ident
        elif ident == re.search(r'fileID: (\d+)', field(before[asset['rootTransform']], 'm_GameObject'))[1]:
            expected = old.replace('  m_Layer:', '  - component: {fileID: ' + asset['dynamicBinding'] + '}\n  m_Layer:', 1)
            assert without(expected, []) == without(new, []), 'Root binding changed unrelated data'
        elif ident in wrapper_gos:
            filtered = re.sub(r'^  - component: \{fileID: (\d+)\}\n', lambda m: '' if m[1] in removed else m[0], old, flags=re.M)
            assert without(filtered, ['m_Name']) == without(new, ['m_Name'])
        else:
            raise AssertionError('Unexpected component mutation: ' + ident)
    for batch in asset['batches']:
        for e in batch['members']:
            assert field(after[e['component']], 'displayCanvas') == '{fileID: ' + batch['canvas'] + '}'
            assert field(after[e['wrapper']], 'm_Father') == '{fileID: ' + batch['transform'] + '}'
            for name in ['m_LocalPosition', 'm_LocalRotation', 'm_LocalScale']:
                assert field(after[e['wrapper']], name) == field(after[e['owner']], name)
    for ident, block in after.items():
        for local in re.findall(r'\{fileID: (\d+)\}', block):
            assert local == '0' or local in after, 'Missing local reference: ' + ident + ' -> ' + local
    report.append('PASS ' + Path(asset['source']).name + ': ' + str(len(transformed)) + ' Transform -> RectTransform; Canvas ' + str(asset['canvasBefore']) + ' -> ' + str(asset['canvasAfter']) + '; controller IDs, original sibling order, source TRS, widgets, tween/animation data preserved.')

scope = json.loads((AUDIT / 'Step5/scope.json').read_text(encoding='utf-8'))
changed = set(scope['assets'])
snapshot = json.loads((AUDIT / 'layout-baseline.json').read_text(encoding='utf-8'))
for asset in snapshot['assets']:
    path = asset['source']
    if path in changed: continue
    expected = subprocess.check_output(['git', 'show', scope['baseline'] + ':' + path], cwd=ROOT).decode('utf-8').replace('\r\n', '\n')
    assert (ROOT / path).read_text(encoding='utf-8') == expected, 'Out-of-scope asset changed: ' + path
import hashlib
for entry in scope['preexistingModifiedFiles']:
    assert hashlib.sha256((ROOT / entry['path']).read_bytes()).hexdigest() == entry['sha256'], 'Preexisting edit changed: ' + entry['path']
report.append('PASS 8 other audited assets including 3 main UI prefabs, tk2d Control/BattingView, MainCamera and 2 scenes unchanged from step 4.')
report.append('PASS 2 preexisting material edits preserved byte-for-byte.')
report.append('PASS no unresolved local references or changes to controller/pointer/clip/tween/Animator components.')
report.append('OVERALL PASS')
(AUDIT / 'Step5/static-checks.txt').write_text('\n'.join(report) + '\n', encoding='utf-8')
print('\n'.join(report))
