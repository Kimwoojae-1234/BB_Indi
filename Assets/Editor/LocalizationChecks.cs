#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BaseBall.BallPlay.UGUI;

public static class LocalizationChecks
{
    [MenuItem("Tools/Localization/Check Catalog and Bindings")]
    public static void Run()
    {
        int checks=0;
        void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);checks++;}
        bool hadPreference=PlayerPrefs.HasKey(L10n.PreferenceKey);
        string saved=PlayerPrefs.GetString(L10n.PreferenceKey);
        var previous=L10n.Language;
        var scene=EditorSceneManager.NewPreviewScene();
        GameObject root=null;
        try
        {
            L10n.SetLanguage(GameDefine.eLanguage.English);
            var data=L10n.LoadData().LocalizationItem;
            Check(data.Length>0,"Catalog not generated.");
            Check(data.All(r=>!r.key.StartsWith("TXT_",StringComparison.Ordinal)),"Opaque localization key remains.");
            Check(data.Select(r=>r.key).Distinct().Count()==data.Length,"Duplicate keys.");
            Check(data.All(r=>r.allowEmpty||!string.IsNullOrWhiteSpace(r.Eng)),"Blank English entry.");
            Check(data.All(r=>!System.Text.RegularExpressions.Regex.IsMatch(r.Eng??"","[가-힣]")),"Korean remains in English.");
            Check(L10n.T("CharDesc_0002")=="","Intentional empty description.");
            Check(L10n.T("UI.TeamNumber",GameDefine.eLanguage.Japan)=="TEAM {0}","English fallback failed.");
            Check(L10n.F("UI.TeamNumber",12)=="TEAM 12","Formatting failed.");
            Check(KOBTextUtil.ToOrdinal(11)=="11th" && KOBTextUtil.ToOrdinal(22)=="22nd","Ordinal regression.");
            root=new GameObject("Localization test",typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(root,scene);
            var text=root.AddComponent<Text>();
            L10n.SetText(text,"UI.English");
            Check(text.text=="English" && root.GetComponent<LocalizedText>()==null,"Code assignment added an automatic binding.");
            L10n.SetLanguage(GameDefine.eLanguage.Korea);
            Check(text.text=="English","Language event overwrote code-owned text.");
            var fixedObject=new GameObject("Static label",typeof(RectTransform));fixedObject.transform.SetParent(root.transform);
            var fixedText=fixedObject.AddComponent<Text>();fixedText.text="English";
            var fixedBinding=fixedObject.AddComponent<LocalizedText>();fixedBinding.key="UI.English";fixedBinding.sourceText="English";fixedBinding.legacy=fixedText;
            fixedBinding.Refresh();
            Check(fixedText.text=="영어","Static label language refresh failed.");
            L10n.SetText(text,"UI.English");
            Check(text.text=="영어","Language refresh failed.");
            text.text="grigrigri";
            L10n.SetLanguage(GameDefine.eLanguage.English);
            Check(text.text=="grigrigri" && root.GetComponent<LocalizedText>()==null,"Nickname was localized.");
            L10n.SetLanguage(GameDefine.eLanguage.Korea);
            L10n.SetText(text,"achieve_desc_1",75);
            Check(text.text=="Record 75 hits!","Achievement key/count failed.");
            var child=new GameObject("Gameplay label");child.transform.SetParent(root.transform);
            var element=child.AddComponent<GameUIElement>();
            L10n.SetText(element,"UI.TeamNumber",3);
            Check(element.text=="TEAM 3" && child.GetComponent<LocalizedText>()==null,"Gameplay assignment attached a binding.");
            var tmpObject=new GameObject("TMP label",typeof(RectTransform));tmpObject.transform.SetParent(root.transform);
            var tmp=tmpObject.AddComponent<TextMeshProUGUI>();
            L10n.SetText(tmp,"UI.TeamNumber",4);
            Check(tmp.text=="TEAM 4" && tmpObject.GetComponent<LocalizedText>()==null,"TMP assignment attached a binding.");
            var team=new RttsTeam(LitJson.JsonMapper.ToObject("{\"idx\":1,\"Player\":\"[]\",\"Pitcher\":0,\"Level\":\"[]\",\"Pos\":\"[]\",\"Logo\":0,\"Name\":\"Server supplied name\"}"));
            Check(team.Name=="Server supplied name","Server team name was replaced by catalog text.");
            var keys=new[]{"UI.English"};
            Check(L10n.Texts(keys)[0]=="영어","Array translation failed.");
            L10n.SetLanguage(GameDefine.eLanguage.English);
            Check(L10n.Texts(keys)[0]=="English","Array language cache stale.");
            int notifications=0;Action listener=()=>notifications++;L10n.LanguageChanged+=listener;
            try{L10n.SetLanguage(GameDefine.eLanguage.Korea);L10n.SetLanguage(GameDefine.eLanguage.Korea);Check(notifications==1,"Duplicate language events.");}
            finally{L10n.LanguageChanged-=listener;}
            File.WriteAllText("Library/Localization/tests.txt","PASS "+checks+" localization behavior checks\n"+DateTime.UtcNow.ToString("O"));
        }
        finally
        {
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            EditorSceneManager.ClosePreviewScene(scene);
            L10n.SetLanguage(previous);
            if(hadPreference)PlayerPrefs.SetString(L10n.PreferenceKey,saved);else PlayerPrefs.DeleteKey(L10n.PreferenceKey);
            PlayerPrefs.Save();
        }
    }
}
#endif
