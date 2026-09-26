"""Build minimal localization-only Unity YAML patches from disposable Unity candidates.

Existing document bytes, IDs, transforms, text, materials and event references are
preserved. Only LocalizedText documents, required stripped identity records, and
component attachment entries may be added. Originals are written only with --apply
after the entire plan has passed validation and all original hashes still match.
"""
import argparse
import collections
import hashlib
import json
from pathlib import Path
import re
import shutil

ROOT=Path(__file__).resolve().parent.parent
PLAN=ROOT/'Docs/Localization/ui-binding-plan.json'
WORK=ROOT/'Library/Localization'

def sha(data): return hashlib.sha256(data).hexdigest()

def documents(data):
    text=data.decode('utf-8-sig')
    matches=list(re.finditer(r'(?m)^--- !u!(\d+) &(-?\d+)([^\r\n]*)\r?\n',text))
    if not matches: raise ValueError('Not a text-serialized Unity asset')
    docs={}
    for index,m in enumerate(matches):
        end=matches[index+1].start() if index+1<len(matches) else len(text)
        if m[2] in docs: raise ValueError('Duplicate fileID')
        docs[m[2]]={'kind':m[1],'stripped':'stripped' in m[3],'raw':text[m.start():end]}
    return text[:matches[0].start()],docs

def references(raw): return set(re.findall(r'\{fileID: (-?\d+)\}',raw))-{'0'}

def added_section(raw, required=True):
    match=re.search(r'(?m)^    m_AddedComponents:(?: \[\])?\r?\n(?:    -[^\r\n]*\r?\n(?: {6,}[^\r\n]*\r?\n)*)*',raw)
    if not match and required: raise ValueError('PrefabInstance has no supported m_AddedComponents section')
    return match

def prepare_file(source,candidate,expected,guid,inherited=frozenset()):
    preamble,old=documents(source);_,new=documents(candidate)
    # Unity cannot save prefabs containing missing scripts. Those scripts may be
    # absent from the disposable copy; their original documents are retained verbatim below.
    # Obsolete GUILayer (92) is also dropped by modern Unity. Keep its original bytes.
    if any(old[i]['kind'] not in {'114','92'} for i in set(old)-set(new)):
        raise ValueError('Unity candidate changed existing object fileIDs')
    if any(old[i]['kind']!=new[i]['kind'] for i in set(old)&set(new)):
        raise ValueError('Unity candidate reassigned an existing object ID')
    additional=set(new)-set(old)
    binders={i for i in additional if new[i]['kind']=='114' and ('guid: '+guid+',') in new[i]['raw'] and not new[i]['stripped']}
    if len(binders)!=len(expected): raise ValueError(f'Expected {len(expected)} bindings, found {len(binders)}')
    keys=[re.search(r'(?m)^  key: (.+)\r?$',new[i]['raw'])[1].strip() for i in binders]
    if collections.Counter(keys)!=collections.Counter(e['key'] for e in expected): raise ValueError('Binding keys differ from the validated plan')
    if not inherited <= binders: raise ValueError('Inherited binding is not a planned addition')
    binders -= inherited
    needed=set(binders)
    pending=list(binders)
    while pending:
        i=pending.pop()
        for target in references(new[i]['raw']):
            if target in old or target in needed: continue
            if target not in new or not new[target]['stripped']: raise ValueError('Unexpected new object reference '+target)
            needed.add(target);pending.append(target)
    bodies={i:doc['raw'] for i,doc in old.items()}
    changes=[];attached=set()
    newline='\r\n' if b'\r\n' in source else '\n'
    def apply_change(i,start,end,replacement):
        before=bodies[i];after=before[:start]+replacement+before[end:]
        # Reversibility ensures that every existing byte outside this exact insertion stays unchanged.
        assert after[:start]+before[start:end]+after[start+len(replacement):]==before
        changes.append({'document':i,'before':before[start:end],'after':replacement})
        bodies[i]=after
    by_owner=collections.defaultdict(list)
    for i in binders:
        owner=re.search(r'(?m)^  m_GameObject: \{fileID: (-?\d+)\}',new[i]['raw'])[1]
        by_owner[owner].append(i)
    for owner,ids in by_owner.items():
        if owner not in old or old[owner]['stripped']: continue
        if old[owner]['kind']!='1': raise ValueError('Binding owner is not a GameObject')
        match=re.search(r'(?m)^  m_Component:\r?\n(?:  - component: \{fileID: -?\d+\}\r?\n)+',bodies[owner])
        if not match: raise ValueError('Unsupported component list')
        addition=''.join('  - component: {fileID: '+i+'}'+newline for i in sorted(ids))
        apply_change(owner,match.end(),match.end(),addition);attached.update(ids)
    for i,doc in old.items():
        if doc['kind']!='1001':continue
        candidate_section=added_section(new[i]['raw'])[0]
        entries=re.findall(r'(?m)^    -[^\r\n]*\r?\n(?: {6,}[^\r\n]*\r?\n)*',candidate_section)
        additions=[]
        for entry in entries:
            added=re.search(r'addedObject: \{fileID: (-?\d+)\}',entry)
            if added and added[1] in binders:
                if added[1] in attached: raise ValueError('Binding attached twice')
                additions.append(entry.replace('\r\n','\n').replace('\n',newline));attached.add(added[1])
        if additions:
            original_section=added_section(bodies[i],False)
            if original_section:
                value=original_section[0]
                if value.startswith('    m_AddedComponents: []'):value='    m_AddedComponents:'+newline
                apply_change(i,original_section.start(),original_section.end(),value+''.join(additions))
            else:
                # Older Unity assets omit empty added-component lists entirely.
                assert '  m_Modification:' in bodies[i]
                anchor=re.search(r'(?m)^  m_SourcePrefab:',bodies[i])
                if not anchor:raise ValueError('Unsupported old prefab-instance layout')
                apply_change(i,anchor.start(),anchor.start(),'    m_AddedComponents:'+newline+''.join(additions))
    if attached!=binders: raise ValueError('Unattached binding documents')
    # Every original document remains in its original order. No candidate rewrite is copied.
    text=preamble+''.join(bodies.values())
    for i in new:
        if i in needed:text+=new[i]['raw'].replace('\r\n','\n').replace('\n',newline)
    result=(b'\xef\xbb\xbf' if source.startswith(b'\xef\xbb\xbf') else b'')+text.encode('utf-8')
    _,final=documents(result)
    assert set(final)==set(old)|needed
    for i in needed:
        assert references(final[i]['raw'])<=set(final), 'Dangling local reference'
    # Remove each permitted addition in reverse order and prove exact reconstruction of every original document.
    reconstructed={i:final[i]['raw'] for i in old}
    for change in reversed(changes):
        i=change['document'];after=change['after']
        if not after or reconstructed[i].count(after)!=1: raise ValueError('Ambiguous reverse validation')
        reconstructed[i]=reconstructed[i].replace(after,change['before'],1)
    assert all(reconstructed[i]==old[i]['raw'] for i in old)
    return result,{'bindings':len(binders),'inheritedBindings':len(inherited),'identityRecords':len(needed-binders),'originalDocumentsPreserved':len(old),'attachments':len(changes)}

def inherited_bindings(candidates,guid):
    """Use the binding on a source prefab instead of adding another on its instances."""
    import yaml
    by_guid={}
    for c in candidates:
        source=c['source']
        source_guid=re.search(r'guid: (\w+)',(ROOT/(source+'.meta')).read_text())[1]
        _,docs=documents((ROOT/c['candidate']).read_bytes())
        bindings={}
        for i,doc in docs.items():
            if doc['kind']=='114' and not doc['stripped'] and ('guid: '+guid+',') in doc['raw']:
                value=yaml.safe_load(doc['raw'].split('\n',1)[1])['MonoBehaviour']
                owner=str(value['m_GameObject']['fileID'])
                bindings[owner]=(i,value['key'],value.get('sourceText',''))
        by_guid[source_guid]=(source,docs,bindings)
    suppressed=collections.defaultdict(set)
    for source,docs,bindings in by_guid.values():
        for owner,(binding_id,key,text) in bindings.items():
            owner_doc=docs[owner]
            if not owner_doc['stripped']:continue
            match=re.search(r'm_CorrespondingSourceObject: \{fileID: (-?\d+), guid: (\w+)',owner_doc['raw'])
            if not match or match[2] not in by_guid:continue
            parent=by_guid[match[2]][2].get(match[1])
            if parent is None:continue
            if parent[1:]!=(key,text):
                raise ValueError('Nested prefab needs explicit localization property overrides: '+source+' '+key)
            suppressed[source].add(binding_id)
    return suppressed

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--apply',action='store_true');args=parser.parse_args()
    plan=json.loads(PLAN.read_text(encoding='utf-8-sig'));assert not plan['errors']
    groups=collections.defaultdict(list)
    for e in plan['entries']:groups[e['asset']].append(e)
    candidates=json.loads((WORK/'candidates.json').read_text(encoding='utf-8-sig'))['files']
    if args.apply and {c['source'] for c in candidates}!=set(groups):raise ValueError('All planned candidates are required before applying')
    guid=re.search(r'guid: (\w+)',(ROOT/'Assets/Scripts_New/UIComponent/LocalizedText.cs.meta').read_text())[1]
    inherited=inherited_bindings(candidates,guid)
    prepared=[];report=[]
    for c in candidates:
        source_path=(ROOT/c['source']).resolve();candidate_path=(ROOT/c['candidate']).resolve()
        if not source_path.is_relative_to(ROOT/'Assets') or not candidate_path.is_relative_to(ROOT/'Assets/Editor/LocalizationCandidates'):raise ValueError('Path outside localization scope')
        original=source_path.read_bytes()
        if sha(original)!=c['sha256'] or any(e['sha256']!=c['sha256'] for e in groups[c['source']]):raise ValueError('Stale original: '+c['source'])
        try:
            result,details=prepare_file(original,candidate_path.read_bytes(),groups[c['source']],guid,inherited[c['source']])
        except (ValueError,AssertionError) as error:
            raise ValueError(c['source']+': '+str(error)) from error
        target=WORK/'MinimalPatches'/c['source'];target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(result)
        prepared.append((source_path,c['sha256'],result))
        report.append({'asset':c['source'],'originalSha256':sha(original),'patchedSha256':sha(result),**details})
    if args.apply:
        # No original is written until every candidate passes and all source hashes are rechecked.
        if any(sha(path.read_bytes())!=expected for path,expected,_ in prepared):raise ValueError('Original changed during preparation')
        backup=WORK/'MinimalPatchBackups'
        for path,expected,_ in prepared:
            saved=backup/path.relative_to(ROOT);saved.parent.mkdir(parents=True,exist_ok=True)
            if not saved.exists():shutil.copyfile(path,saved)
            elif sha(saved.read_bytes())!=expected:raise ValueError('Conflicting backup')
        for path,_,result in prepared:path.write_bytes(result)
    output={'status':'APPLIED' if args.apply else 'PREPARED','assets':len(report),'bindings':sum(r['bindings'] for r in report),'inheritedBindings':sum(r['inheritedBindings'] for r in report),'files':report}
    (ROOT/'Docs/Localization/minimal-binding-verification.json').write_text(json.dumps(output,ensure_ascii=False,indent=2),encoding='utf-8')
    print(output['status'],output['assets'],'assets',output['bindings'],'bindings,',output['inheritedBindings'],'inherited; all original document bytes reconstruct exactly')

if __name__=='__main__':
    raise SystemExit('Bulk binding is retired. Code-written text must not receive localization components. See Table/README.md and Docs/Localization/static-text-scope.json. Parsing helpers remain for preservation tests only.')
