"""Preserve completed one-off migration tools outside Unity's compilation scope."""
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
editor = ROOT / 'Assets/Editor'
archive = ROOT / 'Docs/UIAudit/LegacyConverterSources'
archive.mkdir(parents=True, exist_ok=True)
names = [p for p in editor.glob('*.cs') if p.name.startswith(('PitchSelection', 'RunnerControl', 'FieldMinimap')) or p.name in ('ScoreboardMigrationChecks.cs','ScoreboardUGUIConverter.cs')]
for source in names:
    for path in (source, Path(str(source) + '.meta')):
        if not path.exists(): continue
        assert path.resolve().is_relative_to(editor.resolve())
        destination = archive / (path.name + '.txt')
        if destination.exists(): assert destination.read_bytes() == path.read_bytes(), path
        else: destination.write_bytes(path.read_bytes())
        path.unlink()
for path in editor.glob('*.cs'):
    text = path.read_text(encoding='utf-8-sig')
    changed = text.replace('ScoreboardMigrationChecks.PrefabPath', 'UGUIMigrationPaths.PrefabPath').replace('ScoreboardMigrationChecks.OutputPath','UGUIMigrationPaths.OutputPath').replace('ScoreboardUGUIConverter.BuildFont', 'NGUIBitmapFontConverter.BuildFont')
    if path.name == 'CanvasIntegrationChecks.cs':
        changed = changed.replace('GetComponent<UILabel>', 'GetComponent<GameUIElement>').replace('GetComponentsInChildren<UILabel>','GetComponentsInChildren<GameUIElement>')
        changed = changed.replace('Legacy result still references the UGUI scoreboard.', 'Result scoreboard controller binding is missing or incorrect.')
        changed = changed.replace('"Legacy result', '"Result').replace('Incorrect legacy inning', 'Incorrect result inning')
    if changed != text: path.write_text(changed, encoding='utf-8')
print('Archived', len(names), 'completed legacy-specific conversion tools; native regression checks retained.')
