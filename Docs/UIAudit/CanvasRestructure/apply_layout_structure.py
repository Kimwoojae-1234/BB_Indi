"""Step 3: stable-ID RectTransform layout and contiguous sibling canvas batches.

Only applies to the committed step-2 prefabs. Controllers/animation paths stay put;
generated presentation rectangles move into a shared canvas next to their owners.
"""
import hashlib
import functools
import itertools
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = Path(__file__).resolve().parent
BASE = 'ae577638'
BATCH_GUID = 'b6020fa6a3134ca99f4d90f334b5fd10'


def field(block, name):
    match = re.search(r'^  ' + re.escape(name) + r': (.*)$', block, re.M)
    return match[1] if match else None


def ref(block, name):
    text = field(block, name)
    return re.search(r'fileID: (-?\d+)', text)[1] if text else '0'


def value(block, name, text):
    result, count = re.subn(r'^  ' + re.escape(name) + r':[^\n]*$', '  ' + name + ': ' + text, block, flags=re.M)
    assert count == 1, name
    return result


def set_children(block, children):
    text = '  m_Children:' + ('\n' + ''.join('  - {fileID: ' + c + '}\n' for c in children) if children else ' []\n')
    result, count = re.subn(r'^  m_Children:(?: \[\])?\n(?:  - \{fileID: \d+\}\n)*', text, block, flags=re.M)
    assert count == 1
    return result


@functools.lru_cache()
def script_guid(name):
    meta = ROOT / ('Assets/MainGame/Script/UI/UGUI/Integrated/' + name + '.cs.meta')
    return re.search(r'^guid: (\w+)', meta.read_text(), re.M)[1]


def header(kind, ident, name, go=None):
    text = f'{kind} &{ident}\n{name}:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n'
    return text + (f'  m_GameObject: {{fileID: {go}}}\n' if go else '')


def xy(text):
    match = re.match(r'\{x: ([^,]+), y: ([^,}]+)', text)
    return '{x: ' + match[1] + ', y: ' + match[2] + '}'


def pivot(widget):
    n = int(field(widget, 'mPivot'))
    return '{x: ' + str((n % 3) * .5) + ', y: ' + str(1 - n // 3 * .5) + '}'


def apply(path):
    before = path.read_text(encoding='utf-8')
    original = subprocess.check_output(['git', 'show', BASE + ':' + path.relative_to(ROOT).as_posix()], cwd=ROOT).decode('utf-8')
    assert before == original.replace('\r\n', '\n'), 'Input differs from step 2: ' + str(path)
    prefix, *parts = before.split('--- !u!')
    blocks = {re.match(r'\d+ &(\d+)', b)[1]: b for b in parts}
    order = list(blocks)
    widgets = {i: b for i, b in blocks.items() if 'guid: ' + script_guid('GameUIElement') in b}
    panels = {ref(b, 'm_GameObject'): i for i, b in blocks.items() if 'guid: ' + script_guid('GameUIPanel') in b}
    transforms = {ref(b, 'm_GameObject'): i for i, b in blocks.items() if b.startswith(('4 &', '224 &')) and ref(b, 'm_GameObject') != '0'}
    nodes = {tr: go for go, tr in transforms.items()}
    parents = {i: ref(blocks[i], 'm_Father') for i in nodes}
    children = {i: re.findall(r'^  - \{fileID: (\d+)\}$', blocks[i], re.M) for i in nodes}
    root_canvas = next(i for i, b in blocks.items() if b.startswith('223 &') and field(b, 'm_RenderMode') == '1')
    root_tr = transforms[ref(blocks[root_canvas], 'm_GameObject')]

    def panel_for(tr):
        while tr in nodes:
            if nodes[tr] in panels:
                return panels[nodes[tr]]
            tr = parents[tr]
        return '0'

    def index_path(tr):
        result = []
        while tr in parents and parents[tr] in children:
            result.append(children[parents[tr]].index(tr))
            tr = parents[tr]
        return tuple(reversed(result))

    source_rects = {}
    converted = set()
    for i, widget in widgets.items():
        tr = transforms[ref(widget, 'm_GameObject')]
        source_rects[tr] = widget
        while tr != root_tr and tr in parents:
            if blocks[tr].startswith('4 &'):
                converted.add(tr)
            tr = parents[tr]
    for tr in converted:
        block = blocks[tr].replace('4 &' + tr + '\nTransform:', '224 &' + tr + '\nRectTransform:', 1)
        block = re.sub(r'^  serializedVersion: \d+\n', '', block, flags=re.M)
        widget = source_rects.get(tr)
        own_pivot = pivot(widget) if widget else '{x: 0.5, y: 0.5}'
        size = '{x: ' + field(widget, 'mWidth') + ', y: ' + field(widget, 'mHeight') + '}' if widget else '{x: 0, y: 0}'
        block = block.rstrip() + '\n  m_AnchorMin: {x: 0.5, y: 0.5}\n  m_AnchorMax: {x: 0.5, y: 0.5}\n  m_AnchoredPosition: ' + xy(field(block, 'm_LocalPosition')) + '\n  m_SizeDelta: ' + size + '\n  m_Pivot: ' + own_pivot + '\n'
        blocks[tr] = block
    # Fixed anchors at the parent's pivot preserve the previous Transform origin.
    # Freeze existing rectangle children too: their previous Transform parent had no size.
    adjusted_anchors = []
    for tr in nodes:
        if not blocks[tr].startswith('224 &') or not (tr in converted or parents[tr] in converted):
            continue
        parent = blocks.get(parents[tr], '')
        anchor = field(parent, 'm_Pivot') or '{x: 0.5, y: 0.5}'
        for key in ['m_AnchorMin', 'm_AnchorMax']:
            blocks[tr] = value(blocks[tr], key, anchor)
        if tr in converted:
        if tr in converted:
            blocks[tr] = value(blocks[tr], 'm_AnchoredPosition', xy(field(blocks[tr], 'm_LocalPosition')))
        adjusted_anchors.append(tr)
    for i, widget in widgets.items():
        tr = transforms[ref(widget, 'm_GameObject')]
        blocks[i] = blocks[i].rstrip() + '\n  layoutRect: {fileID: ' + tr + '}\n'

    entries = []
    for i, w in widgets.items():
        canvas = ref(w, 'displayCanvas')
        if canvas == '0':
            continue
        owner = transforms[ref(w, 'm_GameObject')]
        wrapper = transforms[ref(blocks[canvas], 'm_GameObject')]
        assert parents[wrapper] == owner
        entries.append(dict(component=i, owner=owner, wrapper=wrapper, canvas=canvas,
                            panel=panel_for(owner), depth=int(field(w, 'mDepth')),
                            parent=parents[owner], layer=field(blocks[nodes[owner]], 'm_Layer')))
    entries.sort(key=lambda e: (e['panel'], e['depth'], index_path(e['owner'])))
    grouped = itertools.groupby(entries, lambda e: (e['panel'], e['depth'], e['parent'], e['layer']))
    batches = []
    removed = []
    next_id = 890030000000000000
    for key, members in grouped:
        members = list(members)
        if len(members) < 2:
            continue
        panel, depth, parent, layer = key
        go, tr, canvas, component = [str(next_id + n) for n in range(1, 5)]
        next_id += 4
        assert not {go, tr, canvas, component} & blocks.keys()
        blocks[go] = header(1, go, 'GameObject') + '  serializedVersion: 6\n  m_Component:\n' + ''.join('  - component: {fileID: ' + c + '}\n' for c in [tr, canvas, component]) + f'  m_Layer: {layer}\n  m_Name: UI Batch {depth} ({len(batches) + 1})\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n'
        anchor = field(blocks[parent], 'm_Pivot') or '{x: 0.5, y: 0.5}'
        blocks[tr] = header(224, tr, 'RectTransform', go) + '  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children: []\n  m_Father: {fileID: ' + parent + '}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n  m_AnchorMin: ' + anchor + '\n  m_AnchorMax: ' + anchor + '\n  m_AnchoredPosition: {x: 0, y: 0}\n  m_SizeDelta: {x: 0, y: 0}\n  m_Pivot: {x: 0.5, y: 0.5}\n'
        blocks[tr] = set_children(blocks[tr], [e['wrapper'] for e in members])
        blocks[canvas] = value(blocks[members[0]['canvas']].replace('223 &' + members[0]['canvas'], '223 &' + canvas, 1), 'm_GameObject', '{fileID: ' + go + '}')
        blocks[component] = header(114, component, 'MonoBehaviour', go) + '  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: ' + BATCH_GUID + ', type: 3}\n  m_Name:\n  m_EditorClassIdentifier:\n  displayCanvas: {fileID: ' + canvas + '}\n  sourceParent: {fileID: ' + parent + '}\n  panel: {fileID: ' + panel + '}\n  depth: ' + str(depth) + '\n  members:\n' + ''.join('  - {fileID: ' + e['component'] + '}\n' for e in members)
        children[parent].append(tr)
        for e in members:
            owner, wrapper = e['owner'], e['wrapper']
            children[owner].remove(wrapper)
            wrapper_go = nodes[wrapper]
            blocks[wrapper_go] = re.sub(r'^  - component: \{fileID: ' + e['canvas'] + r'\}\n', '', blocks[wrapper_go], flags=re.M)
            blocks[wrapper_go] = value(blocks[wrapper_go], 'm_Name', 'Presentation')
            for name in ['m_LocalPosition', 'm_LocalRotation', 'm_LocalScale']:
                blocks[wrapper] = value(blocks[wrapper], name, field(blocks[owner], name))
            blocks[wrapper] = value(blocks[wrapper], 'm_Father', '{fileID: ' + tr + '}')
            blocks[wrapper] = value(blocks[wrapper], 'm_AnchoredPosition', xy(field(blocks[owner], 'm_LocalPosition')))
            for name in ['m_AnchorMin', 'm_AnchorMax']:
                blocks[wrapper] = value(blocks[wrapper], name, '{x: 0.5, y: 0.5}')
            blocks[e['component']] = value(blocks[e['component']], 'displayCanvas', '{fileID: ' + canvas + '}').rstrip() + '\n  canvasBatch: {fileID: ' + component + '}\n  presentationRoot: {fileID: ' + wrapper + '}\n'
            del blocks[e['canvas']]
            removed.append(e['canvas'])
        batches.append(dict(component=component, canvas=canvas, transform=tr, parent=parent, members=members))
        order.extend([go, tr, canvas, component])
    for tr, values in children.items():
        blocks[tr] = set_children(blocks[tr], values)
    after = prefix + ''.join('--- !u!' + blocks[i] for i in order if i in blocks)
    path.write_text(after, encoding='utf-8', newline='\n')
    # Diff alignment around removed components can expose old Unity empty-field spaces.
    # Strip only those newly added whitespace lines rather than reformatting the asset.
    check = subprocess.run(['git', 'diff', '--check', '--', path.relative_to(ROOT).as_posix()], cwd=ROOT, capture_output=True, text=True)
    lines = after.splitlines(keepends=True)
    for line_number in re.findall(r':(\d+): trailing whitespace\.', check.stdout):
        index = int(line_number) - 1
        lines[index] = lines[index].rstrip() + '\n'
    if check.stdout:
        path.write_text(''.join(lines), encoding='utf-8', newline='\n')
    return dict(source=path.relative_to(ROOT).as_posix(), beforeSha256=hashlib.sha256(before.encode()).hexdigest(),
                convertedTransforms=sorted(converted), adjustedAnchors=adjusted_anchors,
                layoutComponents=list(widgets), removedCanvases=removed, batches=batches,
                canvasBefore=sum(b.startswith('223 &') for b in parts), canvasAfter=sum(b.startswith('223 &') for b in blocks.values()))


if __name__ == '__main__':
    paths = json.loads((AUDIT / 'Step2/root-mapping.json').read_text(encoding='utf-8'))
    results = [apply(ROOT / item['source']) for item in paths]
    (AUDIT / 'Step3').mkdir(exist_ok=True)
    (AUDIT / 'Step3/layout-mapping.json').write_text(json.dumps(dict(baseline=BASE, assets=results), indent=2) + '\n', encoding='utf-8')
    for item in results:
        print(Path(item['source']).name, 'RectTransforms:', len(item['convertedTransforms']), 'Canvas:', item['canvasBefore'], '->', item['canvasAfter'])
