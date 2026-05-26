using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BallerTrophyComponent : MonoBehaviour
{
    public enum BallerTrophyState
    {
        Trophy = 1,
        Career,
        Achivement
    }


    [Header("[볼러 정보]")]
    [SerializeField] private GameObject CharOrigin;
    [SerializeField] private GameObject BackOrigin;
    [SerializeField] private GameObject blow;
    [SerializeField] private CanvasGroup info;
    [SerializeField] private Image posPattern;
    [SerializeField] private TextMeshProUGUI posTxt;
    [SerializeField] private TextMeshProUGUI posTxt2;
    [SerializeField] private TextMeshProUGUI typeTxt;
    

    [Header("[상단 탭]")]
    [SerializeField] private TextMeshProUGUI []BtnTxt;
    [SerializeField] private GameObject[] NotiObj;
    [SerializeField] private GameObject[] TabObj;

    [Header("[트로피로드 정보]")]
    [SerializeField] private GameObject TrophyUI;
    [SerializeField] private GameObject TrophyUIBack;
    [SerializeField] private RectTransform Mask;


    [SerializeField] private BallerTrophyScroll TrophyScroll;
    [SerializeField] private BallerCareerStat CareerStatObj;
    [SerializeField] private BallerAchievement AchievementObj;


    private GameObject ballerObj;
    private GameObject origin;


    private int SelecteIdx = -1;
    private BallerTrophyState State;


    public void Open(int idx, GameObject _ballerObj, GameObject _ballerOrigin)
    {
        bool bNewUI = false;

        if (SelecteIdx != idx)
        {
            SelecteIdx = idx;
            bNewUI = true;
        }
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(idx); //고정정보 - 선수고유정보
        KOBBaller ballerInfo = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[idx];

        float size = 1440 * Screen.width / Screen.height;
        float right = size - (900 + 2345 - 191);
        Debug.Log("right = " + right);
        Mask.offsetMax = new Vector2(right, 0);


        ballerObj = _ballerObj;
        origin = _ballerOrigin;
        BackOrigin.transform.position = _ballerOrigin.transform.position;


        ballerObj.transform.parent = CharOrigin.transform;
        //ballerObj.transform.localPosition = Vector3.zero;
        //ballerObj.transform.DOMoveX(0, 0.5f);

        ballerObj.transform.DOLocalMove(Vector3.zero, 0.5f);
        TrophyUIBack.transform.DOLocalMove(Vector3.zero, 0.5f);

        blow.gameObject.SetActive(true);
        TrophyUI.gameObject.SetActive(true);


        info.gameObject.SetActive(true);

        if (bNewUI == true)
        {
            //포지션 세팅
            typeTxt.text = KOBManager.Localization.GetUILocalizedValue2(string.Format("CharType_{0:D4}", (int)ballerData.char_type));
            posTxt.text = KOBUtil.GetPosString(ballerData.position);
            posTxt2.text = KOBUtil.GetPosString2(ballerData.position);
            posPattern.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UITier, KOBUtil.GetPosPatter(ballerData.position));

        
        }
        SetState(BallerTrophyState.Trophy);

        info.DOFade(1, 0.5f);
    }



    private void SetState(BallerTrophyState _state)
    {
        State = _state;// BallerTrophyState.Trophy;

        TrophyScroll.gameObject.SetActive(false);
        CareerStatObj.gameObject.SetActive(false);
        AchievementObj.gameObject.SetActive(false);

        int index = (int)State - 1;
        for (int i = 0; i < BtnTxt.Length; i++)
        {
            BtnTxt[i].color = (i == index ? Color.white : new Color(0.29f, 0.674f, 0.968f));
            TabObj[i].gameObject.SetActive(i == index ? true : false);
        }



        if (State == BallerTrophyState.Trophy)
        {
            TrophyScroll.gameObject.SetActive(true);
            TrophyScroll.InitUI(SelecteIdx);
        }
        else if (State == BallerTrophyState.Career)
        {
            CareerStatObj.gameObject.SetActive(true);
            CareerStatObj.InitUI(SelecteIdx);
        }
        else if (State == BallerTrophyState.Achivement)
        {
            AchievementObj.gameObject.SetActive(true);
            AchievementObj.InitUI(SelecteIdx);
        }

    }


    public void OnClickTab(int arg)
    {
        SetState((BallerTrophyState)arg);
    }





    public void Close()
    {
        ballerObj.transform.parent = BackOrigin.transform;
        

        ballerObj.transform.DOLocalMove(Vector3.zero, 0.5f);
        TrophyUIBack.transform.DOLocalMove(new Vector2(3000, 0), 0.3f);
        blow.gameObject.SetActive(false);

        info.DOFade(0, 0.25f);
        Invoke("closeEnd", 0.5f);
    }

    private void closeEnd()
    {
        ballerObj.transform.parent = origin.transform;
        ballerObj.transform.localPosition = Vector3.zero;
        TrophyUI.gameObject.SetActive(false);
        info.gameObject.SetActive(false);
    }
}
