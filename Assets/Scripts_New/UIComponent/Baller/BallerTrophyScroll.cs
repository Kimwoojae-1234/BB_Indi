using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallerTrophyScroll : MonoBehaviour
{
    [SerializeField] private HorizontalLayoutGroup[] group;
    [SerializeField] private GameObject[] Clone;
    [SerializeField] private RectTransform rewardTrans;
    [SerializeField] private RectTransform pointTrans;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform ScrollContent;
    [SerializeField] private RectTransform SliderBar;

    //트로피 인디케이터
    [SerializeField] private RectTransform Indicator;
    [SerializeField] private TextMeshProUGUI TropyTxt;

    //[SerializeField] private RectTransform Origin;

    //private bool bInitUI = false;
    private bool bAutoSlideState = false;

    int CurrentTrophy;

    //트로피 인디케이터의 현재 위치
    float trophyPos;
    float scrollSize;
    float firstScrollPos;


    const int SIZE = 380;
    const int GAB = 200;
    const int LEFTPADDING = 800;

    int LastIdx = -1;

    public void InitUI(int _idx)
    {
        if (LastIdx != _idx)
        {
            LastIdx = _idx;
            bAutoSlideState = false;

            UIUtil.RemoveChild(rewardTrans);
            UIUtil.RemoveChild(pointTrans);


            for (int i = 0; i < group.Length; i++)
            {
                group[i].padding = new RectOffset(LEFTPADDING, 0, 0, 0);
                group[i].spacing = GAB;
            }

            /*
            for (int i = 0; i < side_Indicator.Length; i++)
            {
                side_Indicator[i].gameObject.SetActive(false);
            }*/


            KOBBaller baller = KOBManager.MyInfo.GameData.GetBaller(_idx);

            //트로피 차트
            IReadOnlyDictionary<int, BallerTrophyRoad> chart = KOBManager.Backend.Chart.BallerTrophyRoadData.Dictionary;



            CurrentTrophy = baller.baller_trophy;
            bool isMax = (CurrentTrophy >= KOBConstant.MAX_BALLER_FAME);

            int Step = 0;
            int CurrentKey = 0;
            int CurValue = 0;
            int NextValue = 5;

            List<int> TropyGetList = KOBManager.MyInfo.GameData.GrowthInfo.BallerTropyGetList[_idx];

            foreach (KeyValuePair<int, BallerTrophyRoad> item in chart)
            {
                GameObject reward = GameObject.Instantiate(Clone[0], Clone[0].transform.position, Quaternion.identity) as GameObject;

                TR_RewardComp.TrophyRewartState State = TR_RewardComp.TrophyRewartState.NotAvailable;
                if (item.Value.trophy <= CurrentTrophy)
                {
                    if (TropyGetList.Contains(item.Key)) State = TR_RewardComp.TrophyRewartState.Acquired;
                    else State = TR_RewardComp.TrophyRewartState.Available;
                }
                reward.gameObject.SetActive(true);  
                reward.GetComponent<TR_RewardComp>().Init2(item.Value, item.Key, rewardTrans.transform, State);


                GameObject point = GameObject.Instantiate(Clone[1], Clone[1].transform.position, Quaternion.identity) as GameObject;
                point.gameObject.SetActive(true);
                point.GetComponent<TR_PointComp2>().Init(item.Value, pointTrans.transform);

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
                            if(isMax == true)
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

            float curPos = 0;
            float posGab = 0;
            if (CurrentKey == 0)
            {
                curPos = 320;
                posGab = ((200 + 380) * (CurrentTrophy - CurValue)) / (chart[1].trophy);
            }
            else
            {
                curPos = 990 + (CurrentKey - 1) * (200 + 380); //990초기 위치, 200 갭, 380 rectTrans 넓이
                if(isMax == false)
                    posGab = ((200 + 380) * (CurrentTrophy - CurValue)) / (NextValue - CurValue);                
            }
            trophyPos = (curPos + posGab);

            if (isMax == false)
            {
                float sliderValue = trophyPos / scrollSize;
                slider.value = sliderValue;
            }
            else
            {
                slider.value = 1;
            }

            Debug.Log("trophyPos : " + trophyPos);
            //인디케이터 설정하기
            TropyTxt.text = CurrentTrophy.ToString();
            Indicator.anchoredPosition = new Vector2(trophyPos, Indicator.anchoredPosition.y);

            //초기 스크롤 위치
            firstScrollPos = 1280 - curPos - 250;
            if (firstScrollPos > 0) firstScrollPos = 0;

            //bInitUI = true;
        }
        ScrollContent.anchoredPosition = new Vector2(firstScrollPos, ScrollContent.anchoredPosition.y); //열때마다 위치 초기화

    }

    private void CheckIndicator() //혹시 포지셔닝 필요하면 이거 살려
    {
        /*if (bAutoSlideState == false)
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
        }*/
    }
}
