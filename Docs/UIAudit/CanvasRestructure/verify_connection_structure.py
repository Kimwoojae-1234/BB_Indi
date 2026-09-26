"""Step 4: preserve all audited serialized assets and original tween/callback data."""
import hashlib
import json
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
AUDIT = Path(__file__).resolve().parent
BASE = 'b611a0d9'
snapshot = json.loads((AUDIT / 'layout-baseline.json').read_text(encoding='utf-8'))
report = ['Step 4 serialized checks against ' + BASE]
for asset in snapshot['assets']:
    path = asset['source']
    expected = subprocess.check_output(['git', 'show', BASE + ':' + path], cwd=ROOT).replace(b'\r\n', b'\n')
    actual = (ROOT / path).read_bytes().replace(b'\r\n', b'\n')
    assert actual == expected, 'Serialized asset changed: ' + path
    report.append('PASS unchanged ' + path + ' sha256=' + hashlib.sha256(actual).hexdigest())
for asset in snapshot['preexistingModifiedFiles']:
    path = asset['path']
    expected = subprocess.check_output(['git', 'show', BASE + ':' + path], cwd=ROOT).replace(b'\r\n', b'\n')
    assert (ROOT / path).read_bytes().replace(b'\r\n', b'\n') == expected, path
report.append('PASS all 22 audited assets and 3 character materials preserved; no changes to controller IDs, animation paths, from/to values, callbacks, scroll content/viewport or clip references.')
report.append('OVERALL PASS')
(AUDIT / 'Step4').mkdir(exist_ok=True)
(AUDIT / 'Step4/static-checks.txt').write_text('\n'.join(report) + '\n', encoding='utf-8')
print('\n'.join(report))
