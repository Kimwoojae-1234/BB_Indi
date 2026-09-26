"""Finalize native resource links and keep the result-record popup above the summary."""
import convert_remaining as c
import json,re
def guid(path): return re.search(r'^guid: (\w+)',(c.ROOT/(path+'.meta')).read_text(),re.M)[1]
inventory=json.loads((c.OUT/'inventory.json').read_text(encoding='utf8'))['assets']
fonts=0
for asset in inventory:
    if not any(v['type']=='UIFont' for v in asset['components']): continue
    folder=c.ROOT/'Assets/MainGame/UI/CompleteUGUI/Fonts'/guid(asset['path'])
    matches=list(folder.glob('*.asset'))
    if len(matches)!=1: continue
    native=guid(matches[0].relative_to(c.ROOT).as_posix())
    file=c.ROOT/asset['path'];text=file.read_text(encoding='utf8')
    text=re.sub(r'^  bitmapFont:.*$', '  bitmapFont: {fileID: 11400000, guid: '+native+', type: 2}',text,flags=re.M)
    file.write_text(text,encoding='utf8',newline='\n');fonts+=1
path='Assets/Resources/MainGame/prefabs/resultUI/resultPrefab.prefab'
file=c.ROOT/path;blocks=c.blocks_from_text(file.read_text(encoding='utf8'))
transforms={}; owners={}; names={}
for fid,(cid,block) in blocks.items():
    if cid==1: names[fid]=c.block_dict(block)['m_Name']
    if cid in (4,224):
        data=c.block_dict(block);transforms[fid]=data['m_Father']['fileID'];owners[data['m_GameObject']['fileID']]=fid
popup=next(owners[i] for i,name in names.items() if name=='playerRecord')
panel_guid=guid('Assets/MainGame/Script/UI/UGUI/Integrated/GameUIPanel.cs')
count=0
for fid,(cid,block) in list(blocks.items()):
    if cid!=114 or 'guid: '+panel_guid not in block: continue
    data=c.block_dict(block);node=owners[data['m_GameObject']['fileID']]
    while node and node!=popup: node=transforms[node]
    if node!=popup: continue
    depth=data['mDepth']
    if depth in (20,21):
        blocks[fid]=(cid,re.sub(r'^  mDepth: \d+', '  mDepth: '+str(depth+10),block,flags=re.M));count+=1
    # Nine fixed rows occupy y=150..-228. Match the viewport to that area,
    # preserving row positions while keeping scrolling below the column header.
    if data.get('clipRegion',{}).get('z')==1110:
        cid,block=blocks[fid]
        blocks[fid]=(cid,re.sub(r'^  clipRegion:.*$', '  clipRegion: {x: 0, y: 0, z: 1110, w: 398}',block,flags=re.M))
        viewport=data['clipAnchor']['fileID'];vcid,vblock=blocks[viewport];vd=c.block_dict(vblock)
        if vd['m_AnchoredPosition']['y']==0:
            vblock=re.sub(r'^  m_AnchoredPosition:.*$', '  m_AnchoredPosition: {x: 0, y: -39}',vblock,flags=re.M)
            vblock=re.sub(r'^  m_SizeDelta:.*$', '  m_SizeDelta: {x: 1110, y: 398}',vblock,flags=re.M)
            blocks[viewport]=(vcid,vblock)
            content=vd['m_Children'][0]['fileID'];ccid,cblock=blocks[content]
            cblock=re.sub(r'^  m_AnchoredPosition:.*$', '  m_AnchoredPosition: {x: 0, y: 39}',cblock,flags=re.M)
            blocks[content]=(ccid,cblock)
file.write_text(c.HEADER+''.join(b for _,b in blocks.values()),encoding='utf8',newline='\n')
print('Font data references:',fonts,'; record popup panels placed above summary:',count)

# The old result atlas contains stale position rectangles; the gameplay atlas
# retains the correct Korean position glyphs with the same names and padding.
file=c.ROOT/'Assets/Resources/MainGame/prefabs/resultUI/resultBatterRecordBarPrefab.prefab'
blocks=c.blocks_from_text(file.read_text(encoding='utf8'))
catalog='Assets/MainGame/UI/CompleteUGUI/Atlas-49ac6bd96818296438eee15d9fbb9d78.asset'
catalog_guid=guid(catalog)
fid=764390359361921068
cid,block=blocks[fid]
data=c.block_dict(block)
blocks[fid]=(cid,re.sub(r'^  sprites:.*$', '  sprites: {fileID: 11400000, guid: '+catalog_guid+', type: 2}',block,flags=re.M))
graphic=data['graphic']['fileID'];cid,block=blocks[graphic]
sprite=re.search(r'  - name: position_3\n    sprite: \{fileID: (-?\d+)\}',(c.ROOT/catalog).read_text(encoding='utf8'))[1]
blocks[graphic]=(cid,re.sub(r'^  m_Sprite:.*$', '  m_Sprite: {fileID: '+sprite+', guid: '+catalog_guid+', type: 2}',block,flags=re.M))
file.write_text(c.HEADER+''.join(b for _,b in blocks.values()),encoding='utf8',newline='\n')
print('Batter record positions now use the verified gameplay atlas glyphs.')

catalog_text=(c.ROOT/catalog).read_text(encoding='utf8')
for name in ('resultBatterRecordBarPrefab','resultPitcherRecordBarPrefab'):
    file=c.ROOT/('Assets/Resources/MainGame/prefabs/resultUI/'+name+'.prefab')
    blocks=c.blocks_from_text(file.read_text(encoding='utf8'))
    for fid,(cid,block) in list(blocks.items()):
        if cid!=114 or not re.search(r'^  mSpriteName: position_bg_',block,re.M): continue
        data=c.block_dict(block)
        blocks[fid]=(cid,re.sub(r'^  sprites:.*$', '  sprites: {fileID: 11400000, guid: '+catalog_guid+', type: 2}',block,flags=re.M))
        sprite=re.search(r'  - name: '+data['mSpriteName']+r'\n    sprite: \{fileID: (-?\d+)\}',catalog_text)[1]
        graphic=data['graphic']['fileID'];gcid,gblock=blocks[graphic]
        blocks[graphic]=(gcid,re.sub(r'^  m_Sprite:.*$', '  m_Sprite: {fileID: '+sprite+', guid: '+catalog_guid+', type: 2}',gblock,flags=re.M))
    file.write_text(c.HEADER+''.join(b for _,b in blocks.values()),encoding='utf8',newline='\n')
