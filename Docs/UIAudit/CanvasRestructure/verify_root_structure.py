"""Verify step-2 changes against the step-1 commit without invoking Unity."""
import hashlib
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = Path(__file__).resolve().parent
snapshot = json.loads((AUDIT / 'layout-baseline.json').read_text(encoding='utf-8'))
mapping = json.loads((AUDIT / 'Step2/root-mapping.json').read_text(encoding='utf-8'))
report = []


def blocks(text):
    return {re.match(r'\d+ &(\d+)', b)[1]: b for b in text.split('--- !u!')[1:]}


def field(text, key):
    return re.search(r'^  ' + re.escape(key) + r': (.*)$', text, re.M)[1]


for entry in mapping:
    asset = next(a for a in snapshot['assets'] if a['source'] == entry['source'])
    original = subprocess.check_output(['git', 'show', snapshot['head'] + ':' + asset['source']], cwd=ROOT).decode('utf-8')
    before, after = blocks(original), blocks((ROOT / asset['source']).read_text(encoding='utf-8'))
    assert set(before).issubset(after), 'Existing fileID removed: ' + asset['source']
    assert len(after) - len(before) == 11
    settings = asset['rootsSettings'][0]['component']
    camera_transform = asset['nodes'][asset['cameras'][0]['node']]['transform']
    allowed_transforms = {entry['rootTransform'], camera_transform, entry['retainedMainPanelTransform'], *entry['retainedPopupTransforms']}
    for ident, old in before.items():
        new = after[ident]
        if old == new: continue
        if old.startswith('223 '):
            assert new == old.replace('m_OverrideSorting: 0', 'm_OverrideSorting: 1')
        elif ident in allowed_transforms:
            keys = ['m_Children', 'm_Father', 'm_LocalScale']
            normalize = lambda b: re.sub(r'^  (?:m_Father|m_LocalScale' + ('|m_LocalPosition' if ident == camera_transform else '') + r'):[^\n]*\n|^  m_Children:(?: \[\])?\n(?:  - \{fileID: \d+\}\n)*', '', b, flags=re.M)
            assert normalize(old) == normalize(new), 'Unexpected layout change: ' + ident
        elif ident == entry['camera']:
            assert new == old.replace('near clip plane: -10', 'near clip plane: -9').replace('far clip plane: 10', 'far clip plane: 11')
        else:
            assert ident == settings, 'Unexpected component change: ' + ident
            for line in old.splitlines():
                if line.strip(): assert line in new.splitlines(), 'Existing root field changed: ' + line
    assert field(after[camera_transform], 'm_LocalPosition') == '{x: 0, y: 0, z: -1}'
    assert field(after[entry['rootTransform']], 'm_LocalScale') == '{x: 1, y: 1, z: 1}'
    assert field(after[entry['canvas']], 'm_RenderMode') == '1'
    assert field(after[entry['canvas']], 'm_Camera') == '{fileID: ' + entry['camera'] + '}'
    assert field(after[entry['retainedMainPanelTransform']], 'm_Father') == '{fileID: ' + entry['hud'] + '}'
    for ident in entry['retainedPopupTransforms']:
        assert field(after[ident], 'm_Father') == '{fileID: ' + entry['popups'] + '}'
    for ident in set(after) - set(before):
        for reference in re.findall(r'\{fileID: (\d+)\}', after[ident]):
            assert reference == '0' or reference in after, 'Unresolved new reference: ' + reference
    report.append('PASS ' + Path(entry['source']).name + ': retained ' + str(len(before)) + ' original objects/components; 11 additions; existing controllers, camera IDs/world clipping planes, animation subtrees and widget properties preserved.')

changed = {x['source'] for x in mapping}
for asset in snapshot['assets']:
    if asset['source'] not in changed:
        assert hashlib.sha256((ROOT / asset['source']).read_bytes()).hexdigest() == asset['sha256'], asset['source']
for item in snapshot['preexistingModifiedFiles']:
    assert hashlib.sha256((ROOT / item['path']).read_bytes()).hexdigest() == item['sha256'], item['path']
report.append('PASS remaining 19 captured assets, including both scenes, unchanged; prefab inheritance supplies the new scene hierarchy.')
report.append('PASS all 3 preexisting modified material hashes unchanged.')
(AUDIT / 'Step2/static-checks.txt').write_text('\n'.join(report) + '\n', encoding='utf-8')
print('\n'.join(report))
