"""Final source/dependency audit, including GUIDs whose plugin assets were removed."""
from pathlib import Path
import re,json,subprocess
root=Path(__file__).resolve().parents[2];out=root/'Docs/UIAudit/NGUIComplete'
retired=root/'Library/UGUIMigration/Complete/Retired/Assets/NGUI'
guids={}
for meta in retired.rglob('*.meta'):
    match=re.search(r'^guid: (\w+)',meta.read_text(encoding='utf-8-sig',errors='replace'),re.M)
    if match:guids[match[1]]=meta.relative_to(retired).as_posix()[:-5]
manifest=out/'removed-plugin-guids.json'
if guids: manifest.write_text(json.dumps(guids,indent=2),encoding='utf8')
else: guids=json.loads(manifest.read_text(encoding='utf8'))
dependencies=[]
for path in (root/'Assets').rglob('*'):
    if path.suffix not in ('.prefab','.unity','.asset','.mat','.anim','.controller','.overrideController','.shader','.asmdef','.shadervariants','.meta'):continue
    data=path.read_bytes()
    if not (data.startswith(b'%YAML') or path.suffix in ('.meta','.asmdef','.shader')):continue
    for guid in set(re.findall(rb'guid: ([0-9a-fA-F]+)',data)):
        if guid.decode() in guids:dependencies.append({'asset':path.relative_to(root).as_posix(),'pluginAsset':guids[guid.decode()]})
report={'pluginFolderPresent':(root/'Assets/NGUI').exists(),'archivedPluginFiles':len(list(retired.rglob('*.*'))),'archivedPluginGuids':len(guids),'remainingSerializedDependencies':dependencies}
(out/'plugin-removal-checks.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report,ensure_ascii=False))
assert not report['pluginFolderPresent'] and not dependencies
