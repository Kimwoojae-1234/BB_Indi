using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RttsTrophyComponent : MonoBehaviour
{

    [Header("[팀 정보]")]
    [SerializeField] private GameObject LogoOrigin;
    [SerializeField] private GameObject BackOrigin;
    [SerializeField] private GameObject blow;

    [Header("[트로피로드 정보]")]
    [SerializeField] private GameObject TrophyUI;
    [SerializeField] private GameObject TrophyUIBack;
    [SerializeField] private RectTransform Mask;
    [SerializeField] private RttsTrophyScroll TrophyScroll;


    private GameObject logoObj;
    private GameObject origin;



    public void Open(int League, bool bFirstTry, GameObject _logoObj, GameObject _logoOrigin)
    {        

        float size = 1440 * Screen.width / Screen.height;
        float right = size - (900 + 2345 - 191);
        Debug.Log("right = " + right);
        Mask.offsetMax = new Vector2(right, 0);


        logoObj = _logoObj;
        origin = _logoOrigin;
        BackOrigin.transform.position = _logoOrigin.transform.position;

        logoObj.transform.parent = LogoOrigin.transform;

        logoObj.transform.DOLocalMove(Vector3.zero, 0.5f);
        TrophyUIBack.transform.DOLocalMove(Vector3.zero, 0.5f);

        blow.gameObject.SetActive(true);
        TrophyUI.gameObject.SetActive(true);


        

        TrophyScroll.gameObject.SetActive(true);
        TrophyScroll.InitUI(League, bFirstTry);

    }




    public void Close()
    {
        logoObj.transform.parent = BackOrigin.transform;


        logoObj.transform.DOLocalMove(Vector3.zero, 0.5f);
        TrophyUIBack.transform.DOLocalMove(new Vector2(3000, 0), 0.3f);
        blow.gameObject.SetActive(false);

        Invoke("closeEnd", 0.5f);
    }

    private void closeEnd()
    {
        logoObj.transform.parent = origin.transform;
        logoObj.transform.localPosition = Vector3.zero;
        TrophyUI.gameObject.SetActive(false);
    }
}
