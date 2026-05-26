using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultStat : MonoBehaviour
{
    [SerializeField] private CanvasGroup[] Stat;
    [SerializeField] private RectTransform[] Content;
    [SerializeField] private StatComponent StatClone;

    [SerializeField] private GameObject[] Btn; 
    bool bMyStat = true;
    public bool bInit = false;


    float alpha1 = 1;
    float alpha2 = 0;

    public void OnClickChange()
    {
        bMyStat = !bMyStat;
        Btn[0].gameObject.SetActive(bMyStat);
        Btn[1].gameObject.SetActive(!bMyStat);

        
        Stat[0].gameObject.SetActive(true);
        Stat[1].gameObject.SetActive(true);

        float curAlpha1 = alpha1;
        float curAlpha2 = alpha2;

        DOTween.To(() => curAlpha1, x => curAlpha1 = x, alpha2, 0.5f)
                   .OnUpdate(() => Stat[0].alpha = curAlpha1)
                   .OnComplete(() => 
                   {
                       alpha1 = 1 - alpha1;
                       Stat[0].gameObject.SetActive(bMyStat);
                   });

        DOTween.To(() => curAlpha2, x => curAlpha2 = x, alpha1, 0.5f)
                   .OnUpdate(() => Stat[1].alpha = curAlpha2)
                   .OnComplete(() => 
                   { 
                       alpha2 = 1 - alpha2;
                       Stat[1].gameObject.SetActive(!bMyStat);
                   });
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }


}
