"""Step 2 only: preserve existing object IDs and subtree layout while adding UGUI roots.

Run from the repository root after the step-1 snapshot. Refuses changed inputs.
No asset deletions, animation edits, or per-widget Canvas consolidation.
"""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = Path(__file__).resolve().parent
baseline = json.loads((AUDIT / 'layout-baseline.json').read_text(encoding='utf-8'))
assets = [a for a in baseline['assets'] if a.get('rootsSettings')]


def value(body, key, replacement):
    result, count = re.subn(r'^  ' + re.escape(key) + r':[^\n]*$', '  ' + key + ': ' + replacement, body, flags=re.M)
    assert count == 1, key
    return result


def children(body, ids):
    result, count = re.subn(r'^  m_Children:(?: \[\])?\n(?:  - \{fileID: \d+\}\n)*',
                           '  m_Children:' + ('\n' + ''.join('  - {fileID: ' + x + '}\n' for x in ids) if ids else ' []\n'), body, flags=re.M)
    assert count == 1
    return result


def header(cls, ident, kind, go=None):
    result = f'--- !u!{cls} &{ident}\n{kind}:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n'
    if go: result += f'  m_GameObject: {{fileID: {go}}}\n'
    return result


def gameobject(ident, name, components, layer):
    return header(1, ident, 'GameObject') + '  serializedVersion: 6\n  m_Component:\n' + ''.join(f'  - component: {{fileID: {c}}}\n' for c in components) + f'  m_Layer: {layer}\n  m_Name: {name}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n'


def rect(ident, go, parent, descendants, canvas=False):
    result = header(224, ident, 'RectTransform', go) + '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n'
    result += '  m_LocalScale: {x: 0.0027777778, y: 0.0027777778, z: 0.0027777778}\n' if canvas else '  m_LocalScale: {x: 1, y: 1, z: 1}\n'
    result += '  m_ConstrainProportionsScale: 0\n  m_Children: []\n'
    result = children(result, descendants)
    result += f'  m_Father: {{fileID: {parent}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}\n'
    result += '  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n' if canvas else '  m_AnchorMin: {x: 0, y: 0}\n  m_AnchorMax: {x: 1, y: 1}\n'
    return result + '  m_AnchoredPosition: {x: 0, y: 0}\n' + ('  m_SizeDelta: {x: 1280, y: 720}\n' if canvas else '  m_SizeDelta: {x: 0, y: 0}\n') + '  m_Pivot: {x: 0.5, y: 0.5}\n'


def behaviour(ident, go, guid):
    return header(114, ident, 'MonoBehaviour', go) + f'  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {{fileID: 11500000, guid: {guid}, type: 3}}\n  m_Name:\n  m_EditorClassIdentifier:\n'


changes = {}
mapping = []
for asset in assets:
    path = ROOT / asset['source']
    assert hashlib.sha256(path.read_bytes()).hexdigest() == asset['sha256'], f'Input changed: {path}'
    source = path.read_text(encoding='utf-8')
    sections = source.split('--- !u!')
    blocks = {re.match(r'\d+ &(\d+)', b)[1]: '--- !u!' + b for b in sections[1:]}
    ordered = list(blocks)
    base = 890020000000000000
    new = [str(base + n) for n in range(1, 12)]
    assert not set(new) & blocks.keys()
    go, tr, canvas, scaler, raycaster, hud, hudtr, effects, effectstr, popups, popuptr = new
    root = asset['roots'][0]
    camera = asset['cameras'][0]
    camera_node = asset['nodes'][camera['node']]
    camera_tr = camera_node['transform']
    old_children = re.findall(r'^  - \{fileID: (\d+)\}$', blocks[camera_tr], re.M)
    main = old_children[0]
    popup_children = old_children[1:]
    blocks[root['transform']] = children(value(blocks[root['transform']], 'm_LocalScale', '{x: 1, y: 1, z: 1}'), [camera_tr, tr])
    blocks[camera_tr] = children(value(blocks[camera_tr], 'm_LocalPosition', '{x: 0, y: 0, z: -1}'), [])
    blocks[camera['component']] = value(value(blocks[camera['component']], 'near clip plane', '-9'), 'far clip plane', '11')
    blocks[main] = value(blocks[main], 'm_Father', '{fileID: ' + hudtr + '}')
    for child in popup_children: blocks[child] = value(blocks[child], 'm_Father', '{fileID: ' + popuptr + '}')
    transforms = {re.search(r'  m_GameObject: \{fileID: (\d+)\}', b)[1]: key for key,b in blocks.items() if b.startswith(('--- !u!4 ', '--- !u!224 '))}
    canvas_nodes = {re.search(r'  m_GameObject: \{fileID: (\d+)\}', b)[1] for b in blocks.values() if b.startswith('--- !u!223 ')}
    canvas_transforms = {transforms[node] for node in canvas_nodes}
    for ident, block in list(blocks.items()):
        if not block.startswith('--- !u!223 '): continue
        current = transforms[re.search(r'  m_GameObject: \{fileID: (\d+)\}', block)[1]]
        inherited = False
        while current in blocks:
            current = re.search(r'  m_Father: \{fileID: (\d+)\}', blocks[current])[1]
            if current in canvas_transforms: inherited = True; break
        if not inherited: blocks[ident] = value(block, 'm_OverrideSorting', '1')
    root_component = asset['rootsSettings'][0]['component']
    blocks[root_component] = blocks[root_component].rstrip() + f'\n  rootCanvas: {{fileID: {canvas}}}\n  hudLayer: {{fileID: {hudtr}}}\n  effectsLayer: {{fileID: {effectstr}}}\n  popupLayer: {{fileID: {popuptr}}}\n\n'
    blocks[go] = gameobject(go, 'Canvas', [tr, canvas, scaler, raycaster], root['layer'])
    blocks[tr] = rect(tr, go, root['transform'], [hudtr, effectstr, popuptr], True)
    blocks[canvas] = header(223, canvas, 'Canvas', go) + f'''  m_Enabled: 1
  serializedVersion: 3
  m_RenderMode: 1
  m_Camera: {{fileID: {camera['component']}}}
  m_PlaneDistance: 1
  m_PixelPerfect: 0
  m_ReceivesEvents: 1
  m_OverrideSorting: 0
  m_OverridePixelPerfect: 0
  m_SortingBucketNormalizedSize: 0
  m_VertexColorAlwaysGammaSpace: 0
  m_AdditionalShaderChannelsFlag: 25
  m_UpdateRectTransformForStandalone: 0
  m_SortingLayerID: 0
  m_SortingOrder: 0
  m_TargetDisplay: 0
'''
    match_mode = '1' if asset['rootsSettings'][0]['fields']['fitWidth'] == '1' else '0'
    blocks[scaler] = behaviour(scaler, go, '0cd44c1031e13a943bb63640046fad76') + f'''  m_UiScaleMode: 1
  m_ReferencePixelsPerUnit: 100
  m_ScaleFactor: 1
  m_ReferenceResolution: {{x: 1280, y: 720}}
  m_ScreenMatchMode: {match_mode}
  m_MatchWidthOrHeight: 1
  m_PhysicalUnit: 3
  m_FallbackScreenDPI: 96
  m_DefaultSpriteDPI: 96
  m_DynamicPixelsPerUnit: 1
  m_PresetInfoIsWorld: 0
'''
    blocks[raycaster] = behaviour(raycaster, go, 'dc42784cf147c0c48a680349fa168899') + '''  m_IgnoreReversedGraphics: 1
  m_BlockingObjects: 0
  m_BlockingMask:
    serializedVersion: 2
    m_Bits: 4294967295
'''
    for ident, transform, name, descendants in [(hud,hudtr,'HUD',[main]), (effects,effectstr,'Effects',[]), (popups,popuptr,'Popups',popup_children)]:
        blocks[ident] = gameobject(ident, name, [transform], root['layer'])
        blocks[transform] = rect(transform, ident, tr, descendants)
    changes[path] = sections[0] + ''.join(blocks[k] for k in ordered + new)
    mapping.append({'source':asset['source'], 'guid':asset['guid'], 'rootTransform':root['transform'], 'camera':camera['component'], 'canvas':canvas, 'hud':hudtr, 'effects':effectstr, 'popups':popuptr, 'retainedMainPanelTransform':main, 'retainedPopupTransforms':popup_children})

# Scene placement remains intact; only the old root-scale overrides are replaced.
for asset in baseline['assets']:
    if not asset['source'].endswith('.unity'): continue
    path = ROOT / asset['source']; source = path.read_text(encoding='utf-8'); result = source
    for entry in mapping:
        pattern = r'(    - target: \{fileID: ' + entry['rootTransform'] + ', guid: ' + entry['guid'] + r', type: 3\}\n      propertyPath: m_LocalScale\.[xyz]\n      value: )[^\n]+'
        result = re.sub(pattern, r'\g<1>1', result)
    if result != source:
        assert hashlib.sha256(path.read_bytes()).hexdigest() == asset['sha256'], f'Input changed: {path}'
        changes[path] = result

for path, source in changes.items(): path.write_text(source, encoding='utf-8', newline='\n')
(AUDIT / 'Step2').mkdir(exist_ok=True)
(AUDIT / 'Step2' / 'root-mapping.json').write_text(json.dumps(mapping,indent=2)+'\n',encoding='utf-8')
print('Applied root structure:', len(assets), 'prefabs;', len(changes) - len(assets), 'scenes.')
