using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TR_PointComp2 : MonoBehaviour
{
    [SerializeField] private Image Tier;
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI Count;

    public void Init(BallerTrophyRoad data, Transform par)
    {
        transform.parent = par;
        transform.localScale = Vector3.one;
        KOBManager.Atlas.SetBallerTierSprite(Tier, data.idx);
        Title.text = string.Format("ballertier{0}", data.idx); //임시
        Count.text = data.trophy.ToString();
    }
}
