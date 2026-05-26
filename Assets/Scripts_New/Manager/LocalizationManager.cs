using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour//, ManagerEventHandler
{
    //private Dictionary<string, LocalizationDataRecord> dic_localizedData = null;
    private string missingTextString = "String Empty";
    private Font SelectFontData = null;
    private string TempLocalizationItem = string.Empty;


    public LocalizationManager()
    {
        //dic_localizedData = new Dictionary<string, LocalizationDataRecord>();
        
    }

    private void Awake()
    {
        GameConfig.ChangeLanguage();
        //InitFont(); //추후 살려
    }


    public void InitFont()
    {
        SelectFontData = LoadFont(GameConfig.CurrentLanguage);
    }

    LocalizationDataRecord localizationData = null;

    public LocalizationDataRecord LoadLocalizeData()
    {
        if(localizationData == null)
        {
            TextAsset txtAsset = Resources.Load("Localization/LocalizationItem") as TextAsset;
            string jsonData = txtAsset.text;
            localizationData = JsonHelper.DeserializeObject<LocalizationDataRecord>(jsonData);
        }
        return localizationData;

    }



    public Font LoadFont(GameDefine.eLanguage language)
    {
        Font loadFont = null;
        string fontName = null;
        
        switch(language)
        {
            case GameDefine.eLanguage.Korea:
                fontName = "BlackHanSans-Regular";
                break;
            case GameDefine.eLanguage.Japan:
                fontName = "NotoSansJP-Bold";
                break;
            case GameDefine.eLanguage.China_Simplified:
                fontName = "OPPOSans-B-2";
                break;
            case GameDefine.eLanguage.China_Traditional:
                fontName = "OPPOSans-B-2";
                break;            
            default:
                fontName = "Multicolore Pro";
                break;
        }

        if (loadFont == null)
        {
            loadFont = Resources.Load<Font>(string.Format("Font/{0}", fontName));            
        }

        return loadFont;
    }



    /// <summary>
    /// 추후 이걸쓸것
    /// </summary>
    /// <param name="TextComponent"></param>
    public void SetFont2(TMPro.TextMeshProUGUI TextComponent)
    {
        if (TextComponent == null || TextComponent.font == SelectFontData)
            return;

        
        //새로만들것

    }


    public string GetLocalizedValue2(GameDefine.eLanguage languageType, string key, TMPro.TextMeshProUGUI TextComponent = null)
    {
        string result = missingTextString;
        //SetFont(TextComponent); //추후
        LocalizationDataRecord dataRecord = LoadLocalizeData();
        if (dataRecord == null)
        {
            ///여기서 다운로드 팝업을 띄어주던가 하면될듯
            return string.Empty;
        }

        for (int i = 0; i < dataRecord.LocalizationItem.Length; i++)
        {
            if (string.Compare(dataRecord.LocalizationItem[i].key, key) == 0)
            {
                string localText = string.Empty;
                switch (languageType)
                {
                    case GameDefine.eLanguage.English:
                        localText = dataRecord.LocalizationItem[i].Eng;
                        break;
                    case GameDefine.eLanguage.Korea:
                        localText = dataRecord.LocalizationItem[i].Kor;
                        break;
                    case GameDefine.eLanguage.Spain:
                        localText = dataRecord.LocalizationItem[i].Esp;
                        break;
                    case GameDefine.eLanguage.Japan:
                        localText = dataRecord.LocalizationItem[i].Jpn;
                        break;
                    case GameDefine.eLanguage.China_Traditional:
                        localText = dataRecord.LocalizationItem[i].ChnT;
                        break;
                    case GameDefine.eLanguage.China_Simplified:
                        localText = dataRecord.LocalizationItem[i].ChnS;
                        break;
                    case GameDefine.eLanguage.France:
                        localText = dataRecord.LocalizationItem[i].Fra;
                        break;
                    case GameDefine.eLanguage.Germany:
                        localText = dataRecord.LocalizationItem[i].Deu;
                        break;
                    case GameDefine.eLanguage.Indonesia:
                        localText = dataRecord.LocalizationItem[i].Idn;
                        break;
                    case GameDefine.eLanguage.Italy:
                        localText = dataRecord.LocalizationItem[i].Ita;
                        break;
                    case GameDefine.eLanguage.Portugal:
                        localText = dataRecord.LocalizationItem[i].Prt;
                        break;
                    case GameDefine.eLanguage.Russia:
                        localText = dataRecord.LocalizationItem[i].Rus;
                        break;
                    case GameDefine.eLanguage.Thailand:
                        localText = dataRecord.LocalizationItem[i].Tha;
                        break;
                    case GameDefine.eLanguage.Turkey:
                        localText = dataRecord.LocalizationItem[i].Tur;
                        break;
                    case GameDefine.eLanguage.Vietnam:
                        localText = dataRecord.LocalizationItem[i].Vnm;
                        break;
                    default:
                        localText = dataRecord.LocalizationItem[i].Eng;
                        break;
                }
                result = localText;
                break;
            }
        }
        result = result.Replace("\\n", "\n");
        return result;
    }


    /// <summary>
    /// 이제 이걸 쓰면됨
    /// </summary>
    /// <param name="key"></param>
    /// <param name="TextComponent"></param>
    /// <returns></returns>
    public string GetUILocalizedValue2(string key, TMPro.TextMeshProUGUI TextComponent = null)
    {
        return GetLocalizedValue2(GameConfig.CurrentLanguage, key, TextComponent);
    }










    public string GetLocalizedValue(GameDefine.eLanguage languageType, string key, UnityEngine.UI.Text TextComponent = null)
    {
        string result = missingTextString;
        SetFont(TextComponent);
        LocalizationDataRecord dataRecord = LoadLocalizeData();
        if (dataRecord == null)
        {
            ///여기서 다운로드 팝업을 띄어주던가 하면될듯
            return string.Empty;
        }

        for (int i = 0; i < dataRecord.LocalizationItem.Length; i++)
        {
            if (string.Compare(dataRecord.LocalizationItem[i].key, key) == 0)
            {
                string localText = string.Empty;
                switch (languageType)
                {
                    case GameDefine.eLanguage.English:
                        localText = dataRecord.LocalizationItem[i].Eng;
                        break;
                    case GameDefine.eLanguage.Korea:
                        localText = dataRecord.LocalizationItem[i].Kor;
                        break;
                    case GameDefine.eLanguage.Spain:
                        localText = dataRecord.LocalizationItem[i].Esp;
                        break;
                    case GameDefine.eLanguage.Japan:
                        localText = dataRecord.LocalizationItem[i].Jpn;
                        break;
                    case GameDefine.eLanguage.China_Traditional:
                        localText = dataRecord.LocalizationItem[i].ChnT;
                        break;
                    case GameDefine.eLanguage.China_Simplified:
                        localText = dataRecord.LocalizationItem[i].ChnS;
                        break;
                    case GameDefine.eLanguage.France:
                        localText = dataRecord.LocalizationItem[i].Fra;
                        break;
                    case GameDefine.eLanguage.Germany:
                        localText = dataRecord.LocalizationItem[i].Deu;
                        break;
                    case GameDefine.eLanguage.Indonesia:
                        localText = dataRecord.LocalizationItem[i].Idn;
                        break;
                    case GameDefine.eLanguage.Italy:
                        localText = dataRecord.LocalizationItem[i].Ita;
                        break;
                    case GameDefine.eLanguage.Portugal:
                        localText = dataRecord.LocalizationItem[i].Prt;
                        break;
                    case GameDefine.eLanguage.Russia:
                        localText = dataRecord.LocalizationItem[i].Rus;
                        break;
                    case GameDefine.eLanguage.Thailand:
                        localText = dataRecord.LocalizationItem[i].Tha;
                        break;
                    case GameDefine.eLanguage.Turkey:
                        localText = dataRecord.LocalizationItem[i].Tur;
                        break;
                    case GameDefine.eLanguage.Vietnam:
                        localText = dataRecord.LocalizationItem[i].Vnm;
                        break;
                    default:
                        localText = dataRecord.LocalizationItem[i].Eng;
                        break;
                }
                result = localText;
                break;
            }
        }
        result = result.Replace("\\n", "\n");
        return result;
    }


    public string GetUILocalizedValue(string key, UnityEngine.UI.Text TextComponent)
    {
        return GetLocalizedValue(GameConfig.CurrentLanguage, key, TextComponent);
    }


    public void SetFont(UnityEngine.UI.Text TextComponent)
    {
        if (TextComponent == null || TextComponent.font == SelectFontData)
            return;

        if (GameConfig.CurrentLanguage == GameDefine.eLanguage.English || GameConfig.CurrentLanguage == GameDefine.eLanguage.Spain)
        {
            if (TextComponent.font != null)
            {
                if (TextComponent.font.name.Contains("Multicolore Pro"))
                {
                    return;
                }
            }
        }

        TextComponent.font = SelectFontData;

        //임시
        if (TextComponent.font == null)
        {
            Font SelectFont = LoadFont(GameConfig.CurrentLanguage);
            //TextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            TextComponent.font = SelectFont;
            this.SelectFontData = SelectFont;
        }
        //
    }

}