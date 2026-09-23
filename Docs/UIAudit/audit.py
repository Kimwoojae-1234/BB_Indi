from pathlib import Path
from collections import Counter, defaultdict
import re, json, csv

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Docs/UIAudit'
OUT.mkdir(parents=True, exist_ok=True)
def read(p):
    return p.read_text(encoding='utf-8-sig', errors='replace')
def rel(p):
    return p.relative_to(ROOT).as_posix()
def save(name, value):
    (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n')

files = list((ROOT/'Assets').rglob('*'))
guids = {}
for p in files:
    if p.suffix == '.meta':
        m=re.search(r'^guid: ([0-9a-f]{32})',read(p),re.M)
        if m: guids[m[1]]=rel(p)[:-5]
for pkg in (ROOT/'Library/PackageCache').glob('com.unity.ugui*'):
    for p in pkg.rglob('*.cs.meta'):
        m=re.search(r'^guid: ([0-9a-f]{32})',read(p),re.M)
        if m: guids[m[1]]=rel(p)[:-5]
def kind(path):
    if path.startswith('Assets/NGUI/'): return 'NGUI'
    if path.startswith('Assets/TK2DROOT/'): return 'tk2d'
    if '/Runtime/TMP/' in path: return 'TMP'
    if 'com.unity.ugui' in path: return 'UGUI'
    return 'Custom'
vendor=('Assets/NGUI/','Assets/TK2DROOT/','Assets/ThirdParty/','Assets/TextMesh Pro/Examples',
        'Assets/Spine/','Assets/MainGame/AssetStore/')
build=re.findall(r'- enabled: 1\s+path: (.+)',read(ROOT/'ProjectSettings/EditorBuildSettings.asset'))
serialized=[p for p in files if p.suffix in ('.prefab','.unity','.anim','.controller','.overrideController') and p.is_file()]
binary=[rel(p) for p in serialized if not p.read_bytes().startswith(b'%YAML') and not rel(p).startswith(vendor)]
sources={rel(p):read(p) for p in serialized if p.read_bytes().startswith(b'%YAML')}
graph={f:{guids[g] for g in re.findall(r'guid: ([0-9a-f]{32})',s) if g in guids} for f,s in sources.items()}
reachable=set(); queue=build[:]
while queue:
    f=queue.pop()
    if f in reachable: continue
    reachable.add(f); queue.extend(graph.get(f,set())-reachable)

assets=[]; text_inventory=[]; fonts=Counter(); scriptowners=defaultdict(list); clips=[]
for f,s in sources.items():
    if f.startswith(vendor): continue
    if f.endswith('.anim'):
        ng=sorted({guids[g] for g in re.findall(r'script: \{[^\n]*guid: ([0-9a-f]{32})',s) if g in guids and kind(guids[g])=='NGUI'})
        if ng: clips.append({'path':f,'ngui_scripts':ng,'build_serialized_reachable':f in reachable})
        continue
    if not f.endswith(('.prefab','.unity')): continue
    counts=Counter(); types=Counter(); missing=set(); scaler=[]; ngroot=[]; texts=0; buttons=0; gos={}
    for match in re.finditer(r'^--- !u!(\d+) &(-?\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S):
        cid,fid,b=match.groups()
        if cid=='1':
            name=re.search(r'^  m_Name: (.*)$',b,re.M)
            gos[fid]=name[1] if name else ''
    for match in re.finditer(r'^--- !u!(\d+) &(-?\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)',s,re.M|re.S):
        cid,fid,b=match.groups()
        if cid in ('223','225'): counts['UGUI']+=1; types[{'223':'Canvas','225':'CanvasGroup'}[cid]]+=1
        if cid!='114': continue
        gm=re.search(r'm_Script: \{[^\n]*guid: ([0-9a-f]{32})',b)
        if not gm: continue
        sp=guids.get(gm[1],''); typ=Path(sp).stem
        if not sp: missing.add(gm[1]); continue
        counts[kind(sp)]+=1; types[typ]+=1; scriptowners[sp].append(f)
        if typ=='CanvasScaler': scaler.append({k:v.strip() for k,v in re.findall(r'^  (m_UiScaleMode|m_ReferenceResolution|m_ScreenMatchMode|m_MatchWidthOrHeight): (.*)$',b,re.M)})
        if typ=='UIRoot': ngroot.append({k:v.strip() for k,v in re.findall(r'^  (scalingStyle|manualWidth|manualHeight|minimumHeight|maximumHeight|fitWidth|fitHeight): (.*)$',b,re.M)})
        if typ in ('TextMeshProUGUI','Text','UILabel'):
            texts+=1
            field='m_text' if typ=='TextMeshProUGUI' else 'm_Text' if typ=='Text' else 'mText'
            tm=re.search(r'^  '+field+r': (.*(?:\n(?!  \w+:|---)[^\n]*)*)',b,re.M)
            go=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',b)
            font=re.search(r'm_fontAsset: \{[^\n]*guid: ([0-9a-f]{32})',b)
            if font: fonts[guids.get(font[1],font[1])]+=1
            text_inventory.append({'path':f,'line':s.count('\n',0,match.start())+1,'object':gos.get(go[1],'') if go else '', 'type':typ,'serialized_text':tm[1].strip() if tm else '', 'font':guids.get(font[1],'') if font else '', 'build_serialized_reachable':f in reachable})
        if typ=='Button': buttons+=1
    if any(counts[k] for k in ('NGUI','UGUI','TMP','tk2d')) or f in build:
        assets.append({'path':f,'build_scene':f in build,'build_serialized_reachable':f in reachable,
            'in_resources':'/Resources/' in f,'components':dict(counts),'types':dict(types),
            'text_components':texts,'buttons':buttons,'canvas_scalers':scaler,'ngui_roots':ngroot,'unresolved_script_guids':sorted(missing)})

# Preserve strings while discarding comments. This is a lexical inventory, not a C# compiler.
lex=re.compile(r'@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|//[^\n]*|/\*.*?\*/',re.S)
def uncomment(s):
    return lex.sub(lambda m: ('\n'*m[0].count('\n')+' ') if m[0].startswith(('//','/*')) else m[0],s)
ngtypes={p.stem for p in files if p.suffix=='.cs' and 'NGUI/Scripts/' in rel(p) and '/Editor/' not in rel(p)}
ngpat=re.compile(r'\b('+'|'.join(sorted(ngtypes))+r')\b')
script_inventory=[]; literals=[]; calls=[]
own=('Assets/Scripts/','Assets/Scripts_New/','Assets/Scripts_Old/','Assets/MainGame/Script/','Assets/MainGame/PVP_Tester/Scripts/')
for p in files:
    f=rel(p)
    if p.suffix!='.cs' or not f.startswith(own): continue
    s=uncomment(read(p))
    type_source=lex.sub(' ',s)
    type_source=re.sub(r'KOBManager\.Localization\b',' ',type_source)
    type_source=re.sub(r'(?:MainManager|Mgrs)\.Localization\b',' ',type_source)
    # The project's LocalizationManager API/property is not NGUI's static Localization class.
    if f.startswith('Assets/Scripts_New/'):
        type_source=re.sub(r'\bLocalization\b',' ',type_source)
    ng=Counter(ngpat.findall(type_source))
    if ng: script_inventory.append({'path':f,'ngui_types':dict(ng),'serialized_owners':sorted(set(scriptowners.get(f,[]))), 'editor': '/Editor/' in f or '/Edirtor/' in f})
    for n,l in enumerate(s.splitlines(),1):
        if re.search(r'\.text\s*=|\.SetText\(|\.SetTextValue\(|ShowToast|ToastPopup|SetPopup',l) and re.search(r'"[^"\n]*[a-zA-Z가-힣][^"\n]*"',l):
            if not re.search(r'GetUILocalizedValue|GetLocalizedValue|GetStringKey',l): literals.append({'path':f,'line':n,'code':l.strip()})
    for m in re.finditer(r'Get(?:UI)?LocalizedValue2?\(\s*"([^"\n]+)"',s):
        after=s[m.end():m.end()+50].lstrip()
        calls.append({'path':f,'line':s.count('\n',0,m.start())+1,'key':m[1],'dynamic_prefix':after.startswith('+')})

loc=json.loads(read(ROOT/'Assets/Resources/Localization/LocalizationItem.json'))['LocalizationItem']
keys=Counter(r['key'] for r in loc); bykey={r['key']:r for r in loc}
langs=[x for x in loc[0] if x!='key']
coverage={l:{'filled':sum(bool(str(r.get(l,'')).strip()) for r in loc),'total':len(loc)} for l in langs}
placeholders=[]
for r in loc:
    base=set(re.findall(r'(?<!\{)\{(\d+)(?:[^{}]*)\}(?!\})',r['Kor']))
    for l in langs:
        if l=='Kor' or not r.get(l): continue
        target=set(re.findall(r'(?<!\{)\{(\d+)(?:[^{}]*)\}(?!\})',r[l]))
        if target!=base: placeholders.append({'key':r['key'],'language':l,'Kor':r['Kor'],'target':r[l],'expected':sorted(base),'actual':sorted(target)})
with (ROOT/'Assets/Resources/Localization.csv').open(encoding='utf-8-sig',newline='') as fp:
    old=list(csv.reader(fp))
locreport={'coverage':coverage,'unique_keys':len(keys),'duplicates':[k for k,n in keys.items() if n>1],
    'empty_all_languages':[r['key'] for r in loc if not any(str(r.get(l,'')).strip() for l in langs)],
    'missing_literal_keys':[c for c in calls if not c['dynamic_prefix'] and c['key'] not in keys],
    'literal_calls':calls,'placeholder_differences':placeholders,'legacy_csv':{'header':old[0],'data_rows':len(old)-1}}
save('ui-assets.json',assets);save('ngui-scripts.json',script_inventory);save('ngui-animation-clips.json',clips)
save('binary-assets-uninspected.json',[{'path':f,'build_serialized_reachable':f in reachable} for f in binary])
save('serialized-texts.json',text_inventory);save('text-code-candidates.json',literals);save('localization-data.json',locreport)
summary={'build_scenes':build,'yaml_ui_assets':len(assets),'binary_assets_uninspected':len(binary),'ngui_assets':sum(bool(a['components'].get('NGUI')) for a in assets),
    'ngui_serialized_build_assets':sum(bool(a['components'].get('NGUI')) and a['build_serialized_reachable'] for a in assets),
    'ngui_code_files':len(script_inventory),'ngui_clips':len(clips), 'ngui_types':dict(sum((Counter(a['types']) for a in assets),Counter())),
    'text_component_count':len(text_inventory),'code_text_candidates':len(literals),'fonts':dict(fonts),
    'script_groups':dict(Counter(x['path'].split('/')[1] for x in script_inventory)),
    'ui_groups':{},'localization':{k:v for k,v in locreport.items() if k in ('coverage','unique_keys','duplicates','legacy_csv')}}
for prefix in ('Assets/Scenes/','Assets/MainGame/Scene/','Assets/MainGame/PVP_Tester/Scene/','Assets/Resources/UI/','Assets/Resources/MainGame/','Assets/Resources/Prefab/','Assets/ResourcesBundle/','Assets/BundleResource/'):
    group=[a for a in assets if a['path'].startswith(prefix)]
    summary['ui_groups'][prefix]={'assets':len(group),'ngui_assets':sum(bool(a['components'].get('NGUI')) for a in group),'components':dict(sum((Counter(a['components']) for a in group),Counter()))}
save('summary.json',summary)
print(json.dumps({k:v for k,v in summary.items() if k not in ('ngui_types','fonts')},ensure_ascii=False,indent=2))
print('BUILD SCENE COMPONENTS')
for a in assets:
    if a['build_scene']: print(a['path'],a['components'],a['canvas_scalers'],a['ngui_roots'])
print('FONTS',json.dumps(summary['fonts'],ensure_ascii=False))
print('Missing keys',json.dumps(locreport['missing_literal_keys'],ensure_ascii=False))
print('Placeholder differences',len(placeholders))
