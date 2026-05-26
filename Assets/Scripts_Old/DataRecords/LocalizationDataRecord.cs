[System.Serializable]
public class LocalizationDataRecord
{
    public LocalizationItem[] LocalizationItem;
}

[System.Serializable]
public class LocalizationItem
{
    
    public string key;
    [UnityEngine.TextArea]
    public string Eng;
    [UnityEngine.TextArea]
    public string Kor;
    [UnityEngine.TextArea]
    public string Esp;
    [UnityEngine.TextArea]
    public string Jpn;
    [UnityEngine.TextArea]
    public string ChnT;
    [UnityEngine.TextArea]
    public string ChnS;
    [UnityEngine.TextArea]
    public string Fra;
    [UnityEngine.TextArea]
    public string Deu;
    [UnityEngine.TextArea]
    public string Idn;
    [UnityEngine.TextArea]
    public string Ita;
    [UnityEngine.TextArea]
    public string Prt;
    [UnityEngine.TextArea]
    public string Rus;
    [UnityEngine.TextArea]
    public string Tha;
    [UnityEngine.TextArea]
    public string Tur;
    [UnityEngine.TextArea]
    public string Vnm;
}