using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultItem : MonoBehaviour
{
    [SerializeField] private Slider[] slider;
    [SerializeField] private TextMeshProUGUI[] number;
    [SerializeField] private TextMeshProUGUI Content;

    public void InitItem(int [] Value, string contentName, int Max = 20)
    {
        for (int i = 0; i < Value.Length; i++)
        {
            slider[i].value = Value[i];
            slider[i].maxValue = Max;
            number[i].text = Value[i].ToString();
        }
        Content.text = contentName;
        slider[0].transform.Find("Fill").GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, Value[0] > Value[1] ? "Slider07_Fill_Yellow" : "Slider07_Fill_Blue");
        slider[1].transform.Find("Fill").GetComponent<Image>().sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, Value[1] > Value[0] ? "Slider07_Fill_Yellow" : "Slider07_Fill_Blue");
    }
}
