using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_TrophyRoad : UIWindow
{
    [SerializeField] private GameObject[] Clone;
    [SerializeField] private RectTransform rewardTrans;
    [SerializeField] private RectTransform pointTrans;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform ScrollContent;
    [SerializeField] private RectTransform SliderBar;
    [SerializeField] private Image MaxFill;

    [SerializeField] private HorizontalLayoutGroup[] group;

    //트로피 인디케이터
    [SerializeField] private RectTransform Indicator;
    [SerializeField] private TextMeshProUGUI TropyTxt;

    //인디케이터 좌우측에 있는지 여부 판단
    [SerializeField] private GameObject [] side_Indicator;


    const int SIZE = 380;
    const int GAB = 350;
    const int LEFTPADDING = 850;


    //UI 초기화 여부
    private bool bInitUI = false;

    //트로피 인디케이터의 현재 위치
    float trophyPos;
    float scrollSize;


    // 현재 트로피
    int CurrentTrophy;
    int MaxTrophy;


    bool bAutoSlideState = false;

    public override void Initialize()
    {
        base.Initialize();
        bInitUI = false;
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        InitUI();

    }


    protected override void Update()
    {
        base.Update();
        if (bInitUI == true)
        {
            CheckIndicator();
        }
    }





    private void InitUI()
    {
        if (bInitUI == false)
        {
            bAutoSlideState = false;

            for (int i = 0; i < group.Length; i++)
            {
                group[i].padding = new RectOffset(LEFTPADDING, 0, 0, 0);
                group[i].spacing = GAB;
            }

            for(int i = 0; i < side_Indicator.Length; i++)
            {
                side_Indicator[i].gameObject.SetActive(false);
            }


            //트로피 차트
            IReadOnlyDictionary<int, TrophyRoad> chart = KOBManager.Backend.Chart.TrophyRoadData.Dictionary;



            CurrentTrophy = KOBManager.MyInfo.GameData.GrowthInfo.Trophy;
            MaxTrophy = KOBManager.MyInfo.GameData.GrowthInfo.MaxTrophy;

            bool isMax = (CurrentTrophy >= KOBConstant.MAX_TROPHY);

            int Step = 0;
            int CurrentKey = 0;
            int CurValue = 0;
            int NextValue = 5;

            int MaxCurrentKey = 0;
            int MaxCurValue = 0;
            int MaxNextValue = 5;

            List<int> TropyGetList = KOBManager.MyInfo.GameData.GrowthInfo.TropyGetList;

            foreach (KeyValuePair<int, TrophyRoad> item in chart)
            {
                GameObject reward = GameObject.Instantiate(Clone[0], Clone[0].transform.position, Quaternion.identity) as GameObject;

                TR_RewardComp.TrophyRewartState State = TR_RewardComp.TrophyRewartState.NotAvailable;
                if (item.Value.trophy <= MaxTrophy)
                {
                    if (TropyGetList.Contains(item.Key)) State = TR_RewardComp.TrophyRewartState.Acquired;
                    else State = TR_RewardComp.TrophyRewartState.Available;
                }
                reward.GetComponent<TR_RewardComp>().Init(item.Value, item.Key, rewardTrans.transform, State);


                GameObject point = GameObject.Instantiate(Clone[1], Clone[1].transform.position, Quaternion.identity) as GameObject;
                point.GetComponent<TR_PointComp>().Init(item.Value, pointTrans.transform);

                if(Step == 0)
                {
                    if(MaxTrophy >= item.Value.trophy)
                    {
                        if (chart.ContainsKey(item.Key + 1) == true)
                        {
                            if (MaxTrophy < chart[item.Key + 1].trophy)
                            {
                                MaxCurrentKey = item.Key;
                                MaxCurValue = item.Value.trophy;
                                Debug.Log("첫노드 찾음 MaxCurrentKey : " + MaxCurrentKey);
                                Step = 1;
                            }
                        }
                        else
                        {
                            if(isMax == true)
                            {
                                MaxCurrentKey = item.Key;
                                MaxCurValue = item.Value.trophy;
                            }
                        }
                    }
                }
                else if (Step == 1)
                {
                    MaxNextValue = item.Value.trophy;
                    Debug.Log("두번째노드 찾음 MaxNextValue : " + MaxNextValue);
                    Step = 2;
                }
            }

            Step = 0;
            foreach (KeyValuePair<int, TrophyRoad> item in chart)
            {                
                if (Step == 0)
                {
                    if (CurrentTrophy >= item.Value.trophy)
                    {
                        if (chart.ContainsKey(item.Key + 1) == true)
                        {
                            if (CurrentTrophy < chart[item.Key + 1].trophy)
                            {
                                CurrentKey = item.Key;
                                CurValue = item.Value.trophy;
                                Debug.Log("첫노드 찾음 CurrentKey : " + CurrentKey);
                                Step = 1;
                            }
                        }
                        else
                        {
                            if (isMax == true)
                            {
                                CurrentKey = item.Key;
                                CurValue = item.Value.trophy;
                            }
                        }
                    }
                }
                else if (Step == 1)
                {
                    NextValue = item.Value.trophy;
                    Debug.Log("두번째노드 찾음 NextValue : " + NextValue);
                    Step = 2;
                }
            }


            Clone[0].gameObject.SetActive(false);
            Clone[1].gameObject.SetActive(false);


            //슬라이더 사이즈 구하기
            scrollSize = (SIZE + GAB) * chart.Count + LEFTPADDING; //슬라이더의 총길이
            ScrollContent.sizeDelta = new Vector2(scrollSize, ScrollContent.sizeDelta.y);
            SliderBar.sizeDelta = new Vector2(scrollSize, SliderBar.sizeDelta.y);
            Slider slider = SliderBar.GetComponent<Slider>();

            float curPos = (SIZE + GAB) * (CurrentKey - 1) + LEFTPADDING + GAB / 2;
            //슬라이더 밸류 구하기
            if (isMax == false)
            {
                
                float nextPos = (SIZE + GAB) * (CurrentKey) + LEFTPADDING + GAB / 2;
                float posGab = ((nextPos - curPos) * (CurrentTrophy - CurValue)) / (NextValue - CurValue);
                trophyPos = (curPos + posGab);
                float sliderValue = trophyPos / scrollSize;
                slider.value = sliderValue;


                //맥스 슬라이더
                float maxcurPos = (SIZE + GAB) * (MaxCurrentKey - 1) + LEFTPADDING + GAB / 2;
                float maxnextPos = (SIZE + GAB) * (MaxCurrentKey) + LEFTPADDING + GAB / 2;
                float maxposGab = ((maxnextPos - maxcurPos) * (MaxTrophy - MaxCurValue)) / (MaxNextValue - MaxCurValue);
                float maxtrophyPos = (maxcurPos + maxposGab);
                float maxsliderValue = maxtrophyPos / scrollSize;
                MaxFill.fillAmount = maxsliderValue;
            }
            else
            {
                slider.value = 1;
                MaxFill.fillAmount = 1;
                trophyPos = (curPos);
            }



            Debug.Log("trophyPos : " + trophyPos);

            //인디케이터 설정하기
            TropyTxt.text = CurrentTrophy.ToString();
            Indicator.anchoredPosition = new Vector2(trophyPos, Indicator.anchoredPosition.y);

            //초기 스크롤 위치
            float firstScroll = 1280 - curPos;
            if (firstScroll > 0) firstScroll = 0;
            ScrollContent.anchoredPosition = new Vector2(firstScroll, ScrollContent.anchoredPosition.y);


            bInitUI = true;
        }

    }

    /// <summary>
    /// 인디케이터가 스크롤의 좌/우측에 있는지 여부를 판단하는 함수
    /// </summary>
    private void CheckIndicator()
    {
        if (bAutoSlideState == false)
        {
            float CurX = -ScrollContent.anchoredPosition.x;
            //if(trophyPos > CurX && trophyPos < CurX + 2560)
            if (trophyPos < CurX - 300)
            {
                if (side_Indicator[0].activeSelf == false) side_Indicator[0].SetActive(true);
            }
            else if (trophyPos > CurX + 2860)
            {
                if (side_Indicator[1].activeSelf == false) side_Indicator[1].SetActive(true);
            }
            else
            {
                if (side_Indicator[0].activeSelf == true) side_Indicator[0].SetActive(false);
                if (side_Indicator[1].activeSelf == true) side_Indicator[1].SetActive(false);
            }
        }
        else
        {
            _time += Time.deltaTime;
            scrollRect.horizontalNormalizedPosition += (dv * Time.deltaTime);
            if (_time >= 0.3f)
            {
                scrollRect.horizontalNormalizedPosition = destPos;
                bAutoSlideState = false;
            }
        }
    }


    float destPos, startPos;
    float _time, dv;

    public void OnClickIndicator()
    {
        Debug.Log("OnClickIndicator");
        //float sliderValue = (trophyPos-1280) / scrollSize;
        //scrollRect.horizontalNormalizedPosition = sliderValue;

        startPos = scrollRect.horizontalNormalizedPosition;
        destPos = (trophyPos - 1280) / scrollSize;
        _time = 0;
        dv = (destPos - startPos) / 0.3f;

        bAutoSlideState = true;
    }


    /// <summary>
    /// 트로피관련 업데이트 되면 이걸 호출해줘
    /// </summary>
    public void TrophyUpdate()
    {

    }

}
