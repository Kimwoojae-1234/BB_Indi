"""Keep the completed NGUI converters as source records; retain native regression tools."""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[2]; editor=root/'Assets/Editor'
archive=root/'Docs/UIAudit/LegacyConverterSources';archive.mkdir(parents=True,exist_ok=True)
names=['IntegratedUGUIConverter.cs','IntegratedUGUIChecks.cs','RemainingUGUIMigration.cs','RemainingUGUIConversions.cs','NGUIBitmapFontConverter.cs']
for name in names:
    source=editor/name
    for path in (source,Path(str(source)+'.meta')):
        if path.exists(): (archive/(path.name+'.txt')).write_bytes(path.read_bytes())

def method(text,signature):
    start=text.index(signature);brace=text.index('{',start);depth=1;end=brace+1
    while depth:
        depth+=(text[end]=='{')-(text[end]=='}');end+=1
    return text[start:end]

converter=(editor/'IntegratedUGUIConverter.cs').read_text(encoding='utf8')
schema=converter[converter.index('    public const string Folder'):converter.index('    private static readonly Dictionary<UIAtlas')]
schema=re.sub(r'^.*currentSourceGuid;\n','',schema,flags=re.M)
native=method(converter,'    public static Material NativeMaterial(')
(editor/'IntegratedUGUIConverter.cs').write_text('#if UNITY_EDITOR\nusing System;\nusing UnityEditor;\nusing UnityEngine;\n// Native migration records and material lookup used by regression checks.\n// Completed conversion implementation is preserved in Docs/UIAudit/LegacyConverterSources.\npublic static class IntegratedUGUIConverter\n{\n'+schema+native+'\n}\n#endif\n',encoding='utf8',newline='\n')

checks=(editor/'IntegratedUGUIChecks.cs').read_text(encoding='utf8')
checks=checks[:checks.index('    [MenuItem("Tools/UI Migration/Integrated/Check Candidates")]')]
checks=checks.replace('typeof(UIWidget).IsAssignableFrom(b.type) || b.type == typeof(UIPanel)','RemainingUGUIChecks.IsLegacyUIType(b.type)')
checks+='    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }\n}\n#endif\n'
(editor/'IntegratedUGUIChecks.cs').write_text(checks,encoding='utf8',newline='\n')

migration=(editor/'RemainingUGUIMigration.cs').read_text(encoding='utf8')
header=migration[:migration.index('    public static void Command(')]
capture=method(migration,'    static void CaptureResult(')
capture='\n'.join(line for line in capture.splitlines() if 'GetComponentsInChildren<UIPanel>' not in line and 'GetComponentsInChildren<UIWidget>' not in line)
command='''    public static void Command(string command)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required.");
        if (command == "check") { RemainingUGUIChecks.Check(); return; }
        if (command == "play-controls") { RemainingUGUIControlChecks.Start(); return; }
        if (command == "capture-after") { CaptureResult("after"); return; }
        if (command == "dependencies")
        {
            var report = new List<string>();
            foreach (string path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && !p.StartsWith("Assets/NGUI/") && !p.StartsWith("Assets/Editor/") && !AssetDatabase.IsValidFolder(p)))
                foreach (string dependency in AssetDatabase.GetDependencies(path, false).Where(p => p.StartsWith("Assets/NGUI/"))) report.Add(path + " -> " + dependency);
            File.WriteAllLines(Output + "/remaining-plugin-dependencies.txt", report); return;
        }
        throw new ArgumentException(command);
    }
'''
(editor/'RemainingUGUIMigration.cs').write_text(header+command+capture+'\n}\n#endif\n',encoding='utf8',newline='\n')
for name in ['RemainingUGUIConversions.cs','NGUIBitmapFontConverter.cs']:
    for path in (editor/name,editor/(name+'.meta')):
        if path.exists():
            assert path.resolve().is_relative_to(editor.resolve()) and (archive/(path.name+'.txt')).read_bytes()==path.read_bytes()
            path.unlink()

command=(editor/'IntegratedUGUICommand.cs').read_text(encoding='utf8')
command=command.replace('            if (command == "build-candidates") IntegratedUGUIConverter.BuildCandidates();\n            else if (command == "refresh")','            if (command == "refresh")')
command=command.replace('            else if (command == "check-candidates") IntegratedUGUIChecks.Run();\n','')
(editor/'IntegratedUGUICommand.cs').write_text(command,encoding='utf8',newline='\n')
play=(editor/'IntegratedUGUIPlayChecks.cs').read_text(encoding='utf8')
play=play.replace('.GetComponent<UIAtlas>()','.GetComponent<GameUIAsset>()').replace('IntegratedUGUIConverter.NativeAtlasMaterial(atlas)','atlas != null && atlas.sprites != null ? atlas.sprites.material : null')
(editor/'IntegratedUGUIPlayChecks.cs').write_text(play,encoding='utf8',newline='\n')
print('Archived completed converters and retained native verification commands.')
