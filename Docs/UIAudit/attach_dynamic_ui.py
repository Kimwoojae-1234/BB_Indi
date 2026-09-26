"""Attach native host/camera/mask binding without resaving missing-script payloads."""
import convert_remaining as c
import json, re, hashlib
def guid(path): return re.search(r'^guid: (\w+)', (c.ROOT/(path+'.meta')).read_text(), re.M)[1]
element_guid = guid('Assets/MainGame/Script/UI/UGUI/Integrated/GameUIElement.cs')
dynamic_guid = guid('Assets/MainGame/Script/UI/UGUI/Integrated/GameUIDynamicUI.cs')
plans=c.read_json(c.OUT/'candidate-plan.json')['assets']
changed=[]
for plan in plans:
    path=plan['source']; file=c.ROOT/path
    blocks=c.blocks_from_text(file.read_text(encoding='utf8'))
    transforms={}; owners={}; widget_owners=[]; dynamic_owners=set()
    for fid,(cid,block) in blocks.items():
        owner=re.search(r'^  m_GameObject: \{fileID: (-?\d+)\}',block,re.M)
        if cid in (4,224):
            transforms[fid]=int(re.search(r'^  m_Father: \{fileID: (-?\d+)\}',block,re.M)[1]);owners[int(owner[1])]=fid
        if cid==114 and owner:
            if 'guid: '+element_guid in block: widget_owners.append(int(owner[1]))
            if 'guid: '+dynamic_guid in block: dynamic_owners.add(int(owner[1]))
    root_owners={}
    reverse={v:k for k,v in owners.items()}
    for owner in widget_owners:
        node=owners[owner]
        while transforms[node]: node=transforms[node]
        root_owners[reverse[node]]=node
    for owner in root_owners.keys()-dynamic_owners:
        fid=8000000000000000000+int(hashlib.sha256((path+str(owner)+'DynamicUI').encode()).hexdigest()[:14],16)
        while fid in blocks: fid+=1
        cid,block=blocks[owner]
        blocks[owner]=(cid,block.replace('  m_Component:\n','  m_Component:\n  - component: {fileID: '+str(fid)+'}\n'))
        values=dict(m_ObjectHideFlags=0,m_CorrespondingSourceObject=c.Flow(fileID=0),m_PrefabInstance=c.Flow(fileID=0),m_PrefabAsset=c.Flow(fileID=0),m_GameObject=c.Flow(fileID=owner),m_Enabled=1,m_EditorHideFlags=0,m_Script=c.Flow(fileID=11500000,guid=dynamic_guid,type=3),m_Name='',m_EditorClassIdentifier='')
        blocks[fid]=(114,c.emit(114,fid,'MonoBehaviour',values)); changed.append(dict(path=path,root=owner,component=fid))
    if root_owners.keys()-dynamic_owners: file.write_text(c.HEADER+''.join(b for _,b in blocks.values()),encoding='utf8',newline='\n')
(c.OUT/'dynamic-host-bindings.json').write_text(json.dumps(changed,indent=2),encoding='utf8')
print('Attached',len(changed),'native dynamic UI roots.')
