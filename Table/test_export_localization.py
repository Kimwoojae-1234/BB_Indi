import contextlib
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import export_localization as exporter

HEADERS = ['key', *exporter.LANGUAGES, 'AllowEmpty']

class Cell:
    def __init__(self, value, kind='s'): self.value, self.data_type = value, kind

class Sheet:
    def __init__(self, rows): self.rows = rows
    def iter_rows(self): return iter([[Cell(x) for x in HEADERS], *self.rows])

class Workbook:
    sheetnames = ['Public']
    def __init__(self, rows): self.sheet = Sheet(rows)
    def __getitem__(self, name): return self.sheet
    def close(self): pass

def row(key='UI.Test', english='Score: {0}', korean='점수: {0}', allow='FALSE'):
    values = {'key':key,'Eng':english,'Kor':korean,'AllowEmpty':allow}
    return [Cell(values.get(h,'')) for h in HEADERS]

class ExportTests(unittest.TestCase):
    def read(self, rows):
        with patch.object(exporter, 'load_workbook', return_value=Workbook(rows)):
            return exporter.read_rows(Path('test.xlsx'))

    def test_real_master_matches_generated_data(self):
        rows, warnings = exporter.read_rows(exporter.DEFAULT_INPUT)
        self.assertEqual(exporter.DEFAULT_OUTPUT.read_bytes(), exporter.encode(rows))
        self.assertTrue(rows)
        self.assertFalse(any(item['key'].startswith('TXT_') for item in rows))
        self.assertIn('Common.Action.Confirm', {item['key'] for item in rows})
        self.assertFalse(any(item['Eng'] == 'grigrigri' for item in rows))
        self.assertEqual(len(warnings), 35)

    def test_duplicates_are_rejected(self):
        with self.assertRaisesRegex(ValueError, 'duplicate key'): self.read([row(),row()])

    def test_opaque_hash_key_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'descriptive key'):
            self.read([row(key='TXT_2DF81A77B973')])

    def test_formulas_are_rejected(self):
        cells=row();cells[1]=Cell('=A1','f')
        with self.assertRaisesRegex(ValueError, 'formulas'): self.read([cells])

    def test_english_is_required(self):
        with self.assertRaisesRegex(ValueError, 'English is required'): self.read([row(english='')])

    def test_korean_in_english_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'contains Korean'): self.read([row(english='점수: {0}')])

    def test_optional_empty_description_is_explicit(self):
        rows,_=self.read([row(english='',korean='',allow='TRUE')])
        self.assertTrue(rows[0]['allowEmpty'])
        with self.assertRaisesRegex(ValueError, 'intentionally empty'): self.read([row(allow='TRUE')])

    def test_translation_placeholder_mismatch_uses_english_fallback(self):
        rows,warnings=self.read([row(korean='점수: {1}')])
        self.assertEqual(rows[0]['Kor'],'')
        self.assertEqual(len(warnings),1)
        self.assertEqual(rows[0]['Eng'],'Score: {0}')

    def test_newlines_braces_and_alignment(self):
        rows,_=self.read([row(english=r'{{Score}}\n{0,5:N0}',korean=r'{{점수}}\n{0,5:N0}')])
        self.assertEqual(rows[0]['Eng'],'{{Score}}\n{0,5:N0}')
        self.assertEqual(exporter.placeholders(rows[0]['Eng']),{0})

    def test_invalid_format_rejected(self):
        with self.assertRaises(ValueError): self.read([row(english='Score: {name}')])
        with self.assertRaises(ValueError): self.read([row(english='Score: {0')])

    def test_failed_export_preserves_existing_output(self):
        with tempfile.TemporaryDirectory() as temp:
            output=Path(temp)/'data.json';output.write_text('original')
            with patch.object(exporter, 'read_rows', side_effect=ValueError('duplicate key')):
                with patch('sys.argv',['export_localization.py','--output',str(output)]), contextlib.redirect_stderr(io.StringIO()):
                    self.assertEqual(exporter.main(),1)
            self.assertEqual(output.read_text(),'original')

if __name__ == '__main__': unittest.main()
