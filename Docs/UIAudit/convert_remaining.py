"""Prepare NGUI replacements without discarding existing missing-script records.

Unity generates native candidates. This tool preserves source identities, controller
data and callback references, including legacy binary prefab data Unity cannot save.
Run from the project root; dependencies are isolated in Library/UGUIMigration/Python.
"""
from pathlib import Path
import sys
import re
import json
import hashlib
import argparse
import gzip

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Library/UGUIMigration/Python'))
import yaml
import UnityPy

OUT = ROOT / 'Docs/UIAudit/NGUIComplete'
WORK = ROOT / 'Library/UGUIMigration/Complete'
HEADER = '%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'

def read_json(path):
    data = path.read_bytes() if path.exists() else gzip.decompress(Path(str(path)+'.gz').read_bytes())
    return json.loads(data)


class Flow(dict):
    pass


class Dumper(yaml.SafeDumper):
    pass


Dumper.add_representer(Flow, lambda d, v: d.represent_mapping('tag:yaml.org,2002:map', v, flow_style=True))


def block_dict(block):
    return next(iter(yaml.safe_load(re.sub(r'^---[^\n]*\n', '', block)).values()))


def emit(class_id, file_id, name, values):
    return f'--- !u!{class_id} &{file_id}\n' + yaml.dump({name: values}, Dumper=Dumper, sort_keys=False, allow_unicode=True, width=1000000)


def source_blocks(path, data=None):
    file = ROOT / path
    if data is None: data = file.read_bytes()
    if data.startswith(b'%YAML'):
        return {int(m[2]): (int(m[1]), m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)[^\n]*\n.*?(?=^--- !u!|\Z)', data.decode('utf-8-sig').replace('\r\n','\n'), re.M | re.S)}
    env = UnityPy.load(data)
    asset = next(iter(env.files.values()))

    def convert(value):
        if isinstance(value, dict):
            if set(value) == {'m_FileID', 'm_PathID'}:
                result = Flow(fileID=value['m_PathID'])
                if value['m_FileID']:
                    external = asset.externals[value['m_FileID'] - 1]
                    # Unity 5 source-prefab GUIDs store the low nibble first in
                    # each byte. Verify against the asset's .meta GUID, not hex().
                    guid = ''.join(f'{byte & 15:x}{byte >> 4:x}' for byte in external.guid)
                    result.update(guid=guid, type=external.type)
                return result
            return {k: convert(v) for k, v in value.items()}
        if isinstance(value, (list, tuple)):
            return [convert(v) for v in value]
        if isinstance(value, bytes):
            return value.hex()
        return value

    blocks = {}
    for obj in env.objects:
        # Legacy prefab metadata is replaced by Unity's modern prefab serialization.
        if obj.type.name == 'Prefab':
            continue
        values = convert(obj.parse_as_dict())
        values.pop('m_PrefabParentObject', None)
        values.pop('m_PrefabInternal', None)
        values.update(m_CorrespondingSourceObject=Flow(fileID=0), m_PrefabInstance=Flow(fileID=0), m_PrefabAsset=Flow(fileID=0))
        if obj.type.name == 'GameObject':
            values['m_Component'] = [{'component': value[1]} if isinstance(value, list) else value for value in values['m_Component']]
        blocks[obj.path_id] = (obj.type.value, emit(obj.type.value, obj.path_id, obj.type.name, values))
    return blocks


def dump_sources():
    inventory = json.loads((OUT / 'inventory.json').read_text(encoding='utf-8'))['assets']
    for asset in inventory:
        if not asset['components']:
            continue
        path = asset['path']
        dest = WORK / 'SourceYaml' / path
        if dest.exists():
            continue
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(HEADER + ''.join(b for _, b in source_blocks(path).values()), encoding='utf-8')
    print('Read source records, including binary-prefab controller and missing-script data.')


def blocks_from_text(text):
    return {int(m[2]): (int(m[1]), m[0]) for m in re.finditer(r'^--- !u!(\d+) &(-?\d+)[^\n]*\n.*?(?=^--- !u!|\Z)', text, re.M | re.S)}


def map_text(text, mapping):
    text = re.sub(r'\{fileID: (-?\d+)\}', lambda m: '{fileID: ' + str(mapping.get(int(m[1]), int(m[1]))) + '}', text)
    return re.sub(r'^(--- !u!\d+ &)(-?\d+)', lambda m: m[1] + str(mapping.get(int(m[2]), int(m[2]))), text, flags=re.M)


def prepare():
    plans = read_json(OUT / 'candidate-plan.json')['assets']
    changes = {}
    external_maps = {}
    for asset in plans:
        identity = {i['replacement']: i['original'] for i in asset['identities']}
        removed = {c['original']: c['replacement'] for c in asset['components']}
        source_map = {identity[old]: identity.get(new, new) for old, new in removed.items()}
        guid = re.search(r'^guid: (\w+)', (ROOT / (asset['source'] + '.meta')).read_text(), re.M)[1]
        external_maps[guid] = source_map
        final = {**identity, **{old: identity.get(new, new) for old, new in removed.items()}, **{i: 0 for i in asset.get('synthetic') or []}}
        source = blocks_from_text((WORK / 'SourceYaml' / asset['source']).read_text(encoding='utf-8'))
        metadata = {fid for fid, (cid, block) in source.items() if cid == 1001 and 'm_RootGameObject:' in block}
        for fid in metadata:
            external_maps[guid][fid] = block_dict(source[fid][1])['m_RootGameObject']['fileID']
        candidate = blocks_from_text((ROOT / asset['candidate']).read_text(encoding='utf-8-sig'))
        output = {}
        for fid, (cid, block) in candidate.items():
            if fid in removed or fid in (asset.get('synthetic') or []):
                continue
            if cid == 1:
                block = re.sub(r'^  - component: \{fileID: (-?\d+)\}\n', lambda m: '' if int(m[1]) in removed else m[0], block, flags=re.M)
            output[final.get(fid, fid)] = (cid, map_text(block, final))

        # Keep all original custom controllers and missing-script payloads exactly.
        # Scene settings absent from the temporary prefab are retained as well.
        for fid, (cid, block) in source.items():
            if fid in source_map or fid in metadata:
                continue
            if cid == 114 or fid not in output:
                block = re.sub(r'^  m_Prefab(?:Internal|ParentObject):[^\n]*\n', '', block, flags=re.M)
                output[fid] = (cid, map_text(block, source_map))

        # Restore the original component-list entries removed only from the review
        # candidate to let Unity save a prefab containing pre-existing missing scripts.
        for fid, (cid, block) in list(output.items()):
            if cid != 1 or fid not in source:
                continue
            old_go = block_dict(source[fid][1])
            # Unity names a saved candidate root after its GUID-based file name.
            # Restore source names because game code also uses Transform.Find/name.
            old_name = next(line for line in source[fid][1].splitlines() if line.startswith('  m_Name:'))
            block = re.sub(r'^  m_Name:.*$', lambda m: old_name, block, flags=re.M)
            current_ids = set(int(x) for x in re.findall(r'component: \{fileID: (-?\d+)\}', block))
            missing = []
            for item in old_go['m_Component']:
                component = next(iter(item.values()))['fileID']
                if component not in current_ids and component not in source_map and component in output:
                    missing.append(f'  - component: {{fileID: {component}}}\n')
            if missing:
                block = block.replace('  m_Component:\n', '  m_Component:\n' + ''.join(missing))
            output[fid] = (cid, block)

        # Raw callback records preserve references to missing controllers too. Unity's
        # deserialized EventDelegate represents such references as null.
        callbacks = {}
        for comp in asset['components']:
            old_id = identity[comp['original']]
            new_id = source_map[old_id]
            if not new_id:
                continue
            original = block_dict(source[old_id][1])
            typ = comp['originalType']
            fields = []
            if typ == 'UIEventTrigger': fields = ['onPress','onRelease','onClick','onHoverOver','onHoverOut','onDragStart','onDrag','onDragEnd','onDoubleClick']
            elif typ == 'UIButton':
                fields = ['onClick'] if original.get('m_Enabled', 1) else []
                feedback = {k: original.get(k) for k in ('hover','pressed','disabledColor','duration','hoverSprite','pressedSprite','disabledSprite','pixelSnap')}
                feedback['target'] = original.get('tweenTarget') or original['m_GameObject']
                if not feedback['target'].get('fileID'): feedback['target'] = original['m_GameObject']
                feedback['enabled'] = bool(original.get('m_Enabled', 1))
                feedback['clickOtherSprite'] = original.get('ClickOtherSprite', Flow(fileID=0))
                feedback['clickOtherSprite'] = original.get('ClickOtherSprite', Flow(fileID=0))
                callbacks.setdefault(new_id, {}).setdefault('buttonFeedback', []).append(feedback)
            elif typ.startswith('Tween'): fields = ['onFinished']
            elif typ in ('UIToggle','UISlider','UIProgressBar'): fields = ['onChange']
            elif typ == 'UIInput': fields = ['onSubmit','onChange']
            for field in fields:
                callbacks.setdefault(new_id, {}).setdefault(field, []).extend(original.get(field, []))
            if typ == 'UIToggledComponents':
                for field in ('activate', 'deactivate'):
                    callbacks.setdefault(new_id, {})[field] = original.get(field, [])
        for fid, fields in callbacks.items():
            values = block_dict(output[fid][1])
            values.update(fields)
            block = emit(114, fid, 'MonoBehaviour', values)
            # PyYAML emits PPtrs in block form here; normalize all pointer dictionaries.
            def pointers(value):
                if isinstance(value, dict):
                    if 'fileID' in value and set(value) <= {'fileID','guid','type'}:
                        return Flow(**{k: (source_map.get(v, v) if k == 'fileID' and 'guid' not in value else v) for k, v in value.items()})
                    return {k: pointers(v) for k, v in value.items()}
                if isinstance(value, list): return [pointers(v) for v in value]
                return value
            output[fid] = (114, emit(114, fid, 'MonoBehaviour', pointers(values)))
        text = HEADER + ''.join(block for _, block in output.values())
        source_ids = set(source)
        old_missing = set(int(x) for x in re.findall(r'\{fileID: (-?\d+)\}', ''.join(b for _, b in source.values()))) - source_ids - {0}
        refs = set(int(x) for x in re.findall(r'\{fileID: (-?\d+)\}', text)) - {0}
        assert not refs - set(output) - old_missing, (asset['source'], refs - set(output) - old_missing)
        assert all(i in output for i in source_ids - source_map.keys() - metadata), asset['source']
        changes[asset['source']] = text

    def map_external(match):
        fid, guid, tail = match.groups()
        return '{fileID: ' + str(external_maps.get(guid, {}).get(int(fid), int(fid))) + ', guid: ' + guid + tail + '}'

    # Update references from assets that were already UGUI or that contain no UI.
    animation_scripts = {}
    native_dir = ROOT / 'Assets/MainGame/Script/UI/UGUI/Integrated'
    for old in (ROOT / 'Assets/NGUI').rglob('*.cs.meta'):
        typ = old.name[:-8]
        native = 'GameUIElement' if typ in ('UIWidget','UISprite','UITexture','UILabel') else 'GameUIPanel' if typ == 'UIPanel' else 'GameUI' + typ if typ.startswith('Tween') else None
        if native and (native_dir / (native + '.cs.meta')).exists():
            old_guid = re.search(r'^guid: (\w+)', old.read_text(), re.M)[1]
            new_guid = re.search(r'^guid: (\w+)', (native_dir / (native + '.cs.meta')).read_text(), re.M)[1]
            animation_scripts[old_guid] = new_guid
    animation_scripts = {}
    native_dir = ROOT / 'Assets/MainGame/Script/UI/UGUI/Integrated'
    for old in (ROOT / 'Assets/NGUI').rglob('*.cs.meta'):
        typ = old.name[:-8]
        native = 'GameUIElement' if typ in ('UIWidget','UISprite','UITexture','UILabel') else 'GameUIPanel' if typ == 'UIPanel' else 'GameUI' + typ if typ.startswith('Tween') else None
        if native and (native_dir / (native + '.cs.meta')).exists():
            old_guid = re.search(r'^guid: (\w+)', old.read_text(), re.M)[1]
            new_guid = re.search(r'^guid: (\w+)', (native_dir / (native + '.cs.meta')).read_text(), re.M)[1]
            animation_scripts[old_guid] = new_guid
    for path in (ROOT / 'Assets').rglob('*'):
        name = path.relative_to(ROOT).as_posix()
        if path.suffix not in ('.prefab','.unity','.asset','.anim','.controller') or name in changes or name.startswith(('Assets/NGUI/', 'Assets/Editor/Remaining')):
            continue
        data = path.read_bytes()
        if not data.startswith(b'%YAML'):
            continue
        text = data.decode('utf-8-sig')
        updated = re.sub(r'\{fileID: (-?\d+), guid: (\w+)([^}]*)\}', map_external, text)
        if path.suffix == '.anim':
            updated = re.sub(r'guid: (\w+)', lambda m: 'guid: ' + animation_scripts.get(m[1], m[1]), updated)
        if path.suffix == '.anim':
            updated = re.sub(r'guid: (\w+)', lambda m: 'guid: ' + animation_scripts.get(m[1], m[1]), updated)
        if text != updated:
            changes[name] = updated

    manifest = []
    for path, text in changes.items():
        text = re.sub(r'\{fileID: (-?\d+), guid: (\w+)([^}]*)\}', map_external, text)
        stage = WORK / 'Prepared' / path
        stage.parent.mkdir(parents=True, exist_ok=True)
        stage.write_text(text, encoding='utf-8')
        manifest.append({'path': path, 'before': hashlib.sha256((ROOT / path).read_bytes()).hexdigest(), 'after': hashlib.sha256(stage.read_bytes()).hexdigest()})
    (WORK / 'prepared-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    (WORK / 'external-maps.json').write_text(json.dumps(external_maps), encoding='utf-8')
    print('Prepared', len(manifest), 'assets; original controller IDs, missing scripts and callbacks preserved.')


def prepare_code():
    mapping = dict.fromkeys(['UILabel','UISprite','UITexture','UIWidget'], 'GameUIElement')
    mapping.update(UIPanel='GameUIPanel', UITweener='GameUITween', TweenAlpha='GameUITweenAlpha', TweenPosition='GameUITweenPosition', TweenScale='GameUITweenScale', TweenRotation='GameUITweenRotation',
                   UIGrid='GameUIGrid', UIScrollView='GameUIScroll', UISpriteAnimation='GameUISpriteAnimation', UIFont='TMP_FontAsset', NGUITools='GameUIRoot')
    candidates = json.loads((ROOT / 'Docs/UIAudit/NGUIRemaining/code-candidates.json').read_text(encoding='utf-8'))
    manifest = json.loads((WORK / 'prepared-manifest.json').read_text(encoding='utf-8'))
    for item in candidates:
        path = item['path']
        if item['editor'] or path.startswith(('Assets/Scripts_New/', 'Assets/TK2DROOT/')):
            continue
        text = (ROOT / path).read_text(encoding='utf-8-sig')
        if path.endswith('/Util.cs'):
            for signature in ['public static void SetUILabelColor(UILabel label, int value)', 'public static void SetSpritePixelPerfect(UISprite spr, string sprName, bool bPixelPerfect = true)']:
                start = text.index(signature)
                brace = text.index('{', start)
                depth = 1; end = brace + 1
                while depth:
                    depth += (text[end] == '{') - (text[end] == '}'); end += 1
                text = text[:start] + text[end:]
        text = re.sub(r'\b(' + '|'.join(mapping) + r')\b', lambda m: mapping[m[1]], text)
        text = text.replace('EventDelegate.Callback', 'System.Action')
        if 'using BaseBall.BallPlay.UGUI;' not in text:
            prefix = re.match(r'(?:\s*#(?:define|undef)[^\n]*\n)*', text).end()
            text = text[:prefix] + 'using BaseBall.BallPlay.UGUI;\n' + text[prefix:]
        if 'TMP_FontAsset' in text and 'using TMPro;' not in text:
            text = 'using TMPro;\n' + text
        stage = WORK / 'Prepared' / path
        stage.parent.mkdir(parents=True, exist_ok=True)
        stage.write_text(text, encoding='utf-8')
        manifest.append({'path': path, 'before': hashlib.sha256((ROOT / path).read_bytes()).hexdigest(), 'after': hashlib.sha256(stage.read_bytes()).hexdigest()})
    (WORK / 'prepared-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print('Prepared runtime controller type changes.')


def apply():
    manifest = json.loads((WORK / 'prepared-manifest.json').read_text(encoding='utf-8'))
    for item in manifest:
        path = ROOT / item['path']; stage = WORK / 'Prepared' / item['path']
        assert hashlib.sha256(path.read_bytes()).hexdigest() == item['before'], item['path']
        assert hashlib.sha256(stage.read_bytes()).hexdigest() == item['after'], item['path']
    for item in manifest:
        path = ROOT / item['path']; backup = WORK / 'BeforeApply' / item['path']
        if not backup.exists():
            backup.parent.mkdir(parents=True, exist_ok=True); backup.write_bytes(path.read_bytes())
        path.write_bytes((WORK / 'Prepared' / item['path']).read_bytes())
    (OUT / 'apply-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    print('Applied', len(manifest), 'files; original bytes retained under Library/UGUIMigration/Complete.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--dump-sources', action='store_true')
    parser.add_argument('--prepare', action='store_true')
    parser.add_argument('--prepare-code', action='store_true')
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    if args.dump_sources:
        dump_sources()
    if args.prepare:
        prepare()
    if args.prepare_code:
        prepare_code()
    if args.apply:
        apply()
