"""Record source controller references and restore source object names after staging."""
import convert_remaining as c
import json, re

plans = c.read_json(c.OUT / 'candidate-plan.json')['assets']
maps = {g:{int(k):v for k,v in m.items()} for g,m in json.loads((c.WORK/'external-maps.json').read_text()).items()}
refs=[]
names=0
for asset in plans:
    path = asset['source']
    guid = re.search(r'^guid: (\w+)', (c.ROOT/(path+'.meta')).read_text(),re.M)[1]
    source = c.blocks_from_text((c.WORK/'SourceYaml'/path).read_text(encoding='utf8'))
    target_path = c.ROOT/path
    target = c.blocks_from_text(target_path.read_text(encoding='utf8'))
    for fid,(cid,block) in source.items():
        if cid==1 and fid in target:
            old = next(line for line in block.splitlines() if line.startswith('  m_Name:'))
            k,current = target[fid]
            changed = re.sub(r'^  m_Name:.*$', lambda m:old,current,flags=re.M)
            names += changed!=current
            target[fid] = k,changed
        if cid!=114 or fid in maps[guid]: continue
        values = c.block_dict(block)
        def walk(value, prop):
            if isinstance(value,dict):
                if 'fileID' in value:
                    i=value['fileID']; g=value.get('guid',guid)
                    if not i or prop in ('m_GameObject','m_Script') or prop.startswith('m_Prefab') or prop=='m_CorrespondingSourceObject': return
                    if g==guid and i not in source: return
                    refs.append(dict(asset=path,component=fid,property=prop,guid=g,target=maps.get(g,{}).get(i,i)))
                else:
                    for k,v in value.items(): walk(v,prop+'.'+str(k) if prop else str(k))
            elif isinstance(value,list):
                for i,v in enumerate(value): walk(v,prop+'.Array.data['+str(i)+']')
        walk(values,'')
    text = c.HEADER+''.join(b for _,b in target.values())
    if text!=target_path.read_text(encoding='utf8'): target_path.write_text(text,encoding='utf8')
(c.OUT/'controller-references.json').write_text(json.dumps({'values':refs},indent=2),encoding='utf8')
print('Source names restored:',names,'; controller references:',len(refs))
