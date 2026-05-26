using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RttsTrophyScroll : MonoBehaviour
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
    //private bool bAutoSlideState = false;

    int CurrentWin;

    //트로피 인디케이터의 현재 위치
    float trophyPos;
    float scrollSize;
    float firstScrollPos;


    const int SIZE = 380;
    const int GAB = 200;
    const int LEFTPADDING = 800;

    bool bInit = false;

    public void InitUI(int League, bool bFirstTry)
    {
        if (bInit == false)
        {
            //League = 2; //지워지워-> 테스트용

            bInit = true;
            //bAutoSlideState = false;

            for (int i = 0; i < group.Length; i++)
            {
                group[i].padding = new RectOffset(LEFTPADDING, 0, 0, 0);
                group[i].spacing = GAB;
            }

#if false
            CurrentWin = 0;
            //트로피 차트
            RttsReward rewardInfo = KOBManager.Backend.Chart.RttsReward.Dictionary[League];
            Dictionary<int, TeamRecord> LeagueTeamRecord =  KOBManager.MyInfo.GameData.RttsInfo.LeagueTeamRecord;
            int TotalGame = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League).TotalGame;

            if (LeagueTeamRecord != null)
            {
                if(LeagueTeamRecord.ContainsKey(0) == true) CurrentWin = LeagueTeamRecord[0].Win;
            }            

            int Step = 0;
            int CurrentKey = 0;
            int CurValue = 0;
            int NextValue = 5;


            int Key = 1;//
            Dictionary<int, RttsTrophyRoad> rttsChart = new Dictionary<int, RttsTrophyRoad>();
            //--> 이것만 만들면 끝남 -> 순서가 엇갈리므로 wins로 오름차순 정렬 필요 -> 딕셔너리 정렬사용하지 말고 for문 돌려 순차적으로 할것(키값때문에)

            int[] small = rewardInfo.small_reward_pos;
            int[] big = rewardInfo.big_reward_pos;
            int bigValue = big[0];
            int smallValue = small[0];
            int cardValue = -100;
            int[] cardReward = new int[2];

            if (rewardInfo.card_reward != null)
            {
                if (rewardInfo.card_reward.Count > 0)
                {
                    foreach (KeyValuePair<int, int[]> card in rewardInfo.card_reward)
                    {
                        cardValue = card.Key;
                        cardReward = card.Value;
                        break;
                    }
                }
            }


            //이 작업을 로비 진입시 하여 글로벌에 보관한다!!!!!
            for (int i = 0; i<TotalGame;i++)
            {
                int count = i + 1;
                
                if (count == TotalGame)
                {
                    //최종
                    Debug.Log("최종 보상 : " + count);
                    rttsChart.Add(Key, new RttsTrophyRoad(Key, count, rewardInfo.winall_reward[0], rewardInfo.winall_reward[1]));
                    Key++;
                }
                else
                {
                    bool bCardGet = false;
                    if(bFirstTry == true && cardValue > 0)
                    {
                        if (count == cardValue)
                        {
                            bCardGet = true;
                            Debug.Log("선수 카드 보상 : " + count +"   부여받는 카드 번호 "+ cardReward[0]);
                            rttsChart.Add(Key, new RttsTrophyRoad(Key, count, KOBReward.Card, cardReward[0], cardReward[1]));
                            Key++;
                            foreach (KeyValuePair<int, int[]> card in rewardInfo.card_reward)
                            {
                                if (card.Key > cardValue)
                                {
                                    cardValue = card.Key;
                                    cardReward = card.Value;
                                    break;
                                }
                            }
                        }
                    }


                    if (bCardGet == false)
                    {
                        if (count == bigValue)
                        {
                            Debug.Log("빅 보상 : " + count);                            
                            rttsChart.Add(Key, new RttsTrophyRoad(Key, count, big[2], big[3]));
                            Key++;
                            bigValue += big[1];
                        }
                        else if (count == smallValue)
                        {
                            Debug.Log("스몰 보상 : " + count);                            
                            rttsChart.Add(Key, new RttsTrophyRoad(Key, count, small[2], small[3]));
                            Key++;
                            smallValue += small[1];
                        }
                    }
                }
            }
#endif
            CurrentWin = KOBManager.Rtts.CurrentWinDrawLose(0)[0];

            int Step = 0;
            int CurrentKey = 0;
            int CurValue = 0;
            int NextValue = 5;


            int Key = 1;//
            Dictionary<int, RttsTrophyRoad> rttsChart = new Dictionary<int, RttsTrophyRoad>();
            foreach (KeyValuePair<int, int[]> pair in KOBManager.Rtts.RttsRewardList)
            {
                int count = pair.Key;
                int[] value = pair.Value;
                KOBReward _type = (KOBReward)pair.Value[0];
                if (_type == KOBReward.None)
                {
                    rttsChart.Add(Key, new RttsTrophyRoad(Key, count, value[1], value[2]));
                }
                else// if (_type == KOBReward.Card)
                {
                    rttsChart.Add(Key, new RttsTrophyRoad(Key, count, _type, value[1], value[2]));
                }
                Key++;
            }


            foreach (KeyValuePair<int, RttsTrophyRoad> item in rttsChart)
            {
                GameObject reward = GameObject.Instantiate(Clone[0], Clone[0].transform.position, Quaternion.identity) as GameObject;

                TR_RewardComp.TrophyRewartState State = TR_RewardComp.TrophyRewartState.NotAvailable;
                if (item.Value.wins <= CurrentWin)
                {
                    State = TR_RewardComp.TrophyRewartState.Acquired; //rtts 트로피에서는 강제로 획득됨!!!
                }
                reward.GetComponent<TR_RewardComp>().Init3(item.Value, item.Key, rewardTrans.transform, State);


                GameObject point = GameObject.Instantiate(Clone[1], Clone[1].transform.position, Quaternion.identity) as GameObject;
                point.transform.parent = pointTrans.transform;
                point.transform.localScale = Vector3.one;
                point.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = item.Value.wins.ToString();

                if (Step == 0)
                {
                    if (CurrentWin >= item.Value.wins)
                    {
                        if (CurrentWin < rttsChart[item.Key + 1].wins)
                        {
                            CurrentKey = item.Key;
                            CurValue = item.Value.wins;
                            Debug.Log("첫노드 찾음 CurrentKey : " + CurrentKey);
                            Step = 1;
                        }
                    }
                }
                else if (Step == 1)
                {
                    NextValue = item.Value.wins;
                    Debug.Log("두번째노드 찾음 NextValue : " + NextValue);
                    Step = 2;
                }
            }



            Clone[0].gameObject.SetActive(false);
            Clone[1].gameObject.SetActive(false);


            //슬라이더 사이즈 구하기
            scrollSize = (SIZE + GAB) * rttsChart.Count + LEFTPADDING; //슬라이더의 총길이
            ScrollContent.sizeDelta = new Vector2(scrollSize, ScrollContent.sizeDelta.y);
            SliderBar.sizeDelta = new Vector2(scrollSize, SliderBar.sizeDelta.y);
            Slider slider = SliderBar.GetComponent<Slider>();

            //슬라이더 밸류 구하기
            float curPos = (SIZE + GAB) * (CurrentKey - 1) + LEFTPADDING + GAB / 2;
            float nextPos = (SIZE + GAB) * (CurrentKey) + LEFTPADDING + GAB / 2;
            float posGab = ((nextPos - curPos) * (CurrentWin - CurValue)) / (NextValue - CurValue);
            trophyPos = (curPos + 50 + posGab); //50은 위치 어긋나는 버그 때문에 설정해줌
            float sliderValue = trophyPos / scrollSize;
            slider.value = sliderValue;


            Debug.Log("trophyPos : " + trophyPos);
            //인디케이터 설정하기
            TropyTxt.text = string.Format("{0} Wins", CurrentWin);
            Indicator.anchoredPosition = new Vector2(trophyPos, Indicator.anchoredPosition.y);

            //초기 스크롤 위치
            firstScrollPos = 1280 - curPos - 250;
            if (firstScrollPos > 0) firstScrollPos = 0;
            //bInitUI = true;
        }
        ScrollContent.anchoredPosition = new Vector2(firstScrollPos, ScrollContent.anchoredPosition.y); //열때마다 위치 초기화

    }
}


public class RttsTrophyRoad
{
    public int idx { get; private set; }
    public int wins { get; private set; }
    public KOBRewardInfo rewardInfo { get; private set; }

    public RttsTrophyRoad(int Key, int Win, int _refID, int _pIndex) //박스 보상 설정
    {
        idx = Key;
        wins = Win;
        rewardInfo = new KOBRewardInfo(_refID, _pIndex, KOBRewardFrom.SeasonTrophyRoad);
    }

    public RttsTrophyRoad(int Key, int Win, KOBReward _type, int _pindex, int _amount) //카드 보상 설정
    {
        idx = Key;
        wins = Win;
        rewardInfo = new KOBRewardInfo();
        rewardInfo.reward = _type;
        rewardInfo.rewardFrom = KOBRewardFrom.SeasonTrophyRoad;
        rewardInfo.pindex = _pindex;
        rewardInfo.amount = _amount;
        
    }
}