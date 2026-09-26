"""Remove runtime material dependencies on the plugin, preserving vertex-color blending."""
from pathlib import Path
import re, uuid, json
root = Path(__file__).resolve().parents[2]
folder = root / 'Assets/MainGame/Script/UI/UGUI/Integrated'
shader = (folder/'GameUIAdditive.shader').read_text(encoding='utf8')
shader = shader.replace('Game/UI/Additive', 'Game/UI/Transparent Vertex Color')
shader = shader.replace('_MainTex ("Texture", 2D) = "white"', '_MainTex ("Texture", 2D) = "black"')
shader = re.sub(r'^.*_TintColor \(.*\n', '', shader, flags=re.M)
shader = shader.replace('ZTest [unity_GUIZTestMode]', 'ZTest LEqual\n        Offset -1, -1')
shader = shader.replace('Blend SrcAlpha One', 'Blend SrcAlpha OneMinusSrcAlpha')
shader = shader.replace('_Color, _TintColor, _TextureSampleAdd', '_Color, _TextureSampleAdd').replace(' * _TintColor * 2', '')
path = folder/'GameUITransparent.shader'; meta = Path(str(path)+'.meta')
guid = re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1] if meta.exists() else uuid.uuid4().hex
path.write_text(shader,encoding='utf8',newline='\n')
meta.write_text('fileFormatVersion: 2\nguid: '+guid+'\n',encoding='utf8',newline='\n')
report=root/'Docs/UIAudit/NGUIComplete'
paths=[line.split(' -> ')[0] for line in (report/'remaining-plugin-dependencies.txt').read_text(encoding='utf8').splitlines()]
for name in paths:
    file=root/name
    assert file.suffix=='.mat' and file.resolve().is_relative_to(root/'Assets')
    backup=root/'Library/UGUIMigration/Complete/BeforeMaterials'/name
    if not backup.exists(): backup.parent.mkdir(parents=True,exist_ok=True); backup.write_bytes(file.read_bytes())
    text=re.sub(r'm_Shader: \{[^}]+\}', 'm_Shader: {fileID: 4800000, guid: '+guid+', type: 3}',file.read_text(encoding='utf8'))
    file.write_text(text,encoding='utf8',newline='\n')
(report/'material-conversions.json').write_text(json.dumps(paths,indent=2),encoding='utf8')
print('Replaced plugin shader references in',len(paths),'materials, preserving texture and blending settings.')
