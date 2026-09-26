import unittest
from prepare_ui_bindings import prepare_file, documents

GUID='ab'*16
ORIGINAL=b'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &1
GameObject:
  m_Component:
  - component: {fileID: 2}
  - component: {fileID: 3}
  m_Name: Label
--- !u!224 &2
RectTransform:
  m_GameObject: {fileID: 1}
  m_LocalPosition: {x: 14, y: -23, z: 0}
--- !u!114 &3
MonoBehaviour:
  m_GameObject: {fileID: 1}
  m_Text: Hello
'''
BINDER=('''--- !u!114 &4
MonoBehaviour:
  m_GameObject: {fileID: 1}
  m_Script: {fileID: 11500000, guid: '''+GUID+''', type: 3}
  key: UI.Hello
  sourceText: Hello
  tmp: {fileID: 3}
  legacy: {fileID: 0}
''').encode()

class BindingPatchTests(unittest.TestCase):
    def test_unity_normalization_is_not_copied(self):
        candidate=ORIGINAL.replace(b'{x: 14, y: -23, z: 0}',b'{x: 0, y: 0, z: 0}')+BINDER
        result,details=prepare_file(ORIGINAL,candidate,[{'key':'UI.Hello'}],GUID)
        old=documents(ORIGINAL)[1];new=documents(result)[1]
        self.assertEqual(old['2'],new['2'])
        self.assertEqual(old['3'],new['3'])
        self.assertIn(b'- component: {fileID: 4}',result)
        self.assertEqual(details['bindings'],1)

    def test_missing_legacy_script_document_is_preserved(self):
        legacy=b'--- !u!114 &5\nMonoBehaviour:\n  m_Script: {fileID: 0}\n  oldData: 123\n'
        result,_=prepare_file(ORIGINAL+legacy,ORIGINAL+BINDER,[{'key':'UI.Hello'}],GUID)
        self.assertIn(legacy,result)

    def test_wrong_key_or_dangling_target_is_rejected(self):
        with self.assertRaises(ValueError):
            prepare_file(ORIGINAL,ORIGINAL+BINDER,[{'key':'UI.Wrong'}],GUID)
        with self.assertRaises(ValueError):
            prepare_file(ORIGINAL,ORIGINAL+BINDER.replace(b'tmp: {fileID: 3}',b'tmp: {fileID: 999}'),[{'key':'UI.Hello'}],GUID)

if __name__=='__main__':unittest.main()
