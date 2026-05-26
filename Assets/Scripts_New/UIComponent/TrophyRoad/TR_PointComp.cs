using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TR_PointComp : MonoBehaviour
{
    [SerializeField] private GameObject Count;
    [SerializeField] private GameObject Tier;

    private TrophyRoad Data;

    public void Init(TrophyRoad data, Transform par)
    {
        transform.parent = par;
        transform.localScale = Vector3.one;

        Data = data;

        if(Data.tier == 0)
        {
            Tier.gameObject.SetActive(false);
            Count.gameObject.SetActive(true);
            Count.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Data.trophy.ToString();            
        }
        else
        {
            Count.gameObject.SetActive(false);
            Tier.gameObject.SetActive(true);
            KOBManager.Atlas.SetTierSprite(Tier.GetComponent<Image>(), Data.tier);
            Tier.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "TIRE " + Data.tier; //임시
        }
    }
}
