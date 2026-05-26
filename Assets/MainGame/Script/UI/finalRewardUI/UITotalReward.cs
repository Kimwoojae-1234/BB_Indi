using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    /// <summary>
    /// 시즌모드 : 타이틀 보상
    /// 쟁탈 : 주간보상
    /// 라이브 : 주간보상
    /// 9회2아웃 : 주간보상
    /// </summary>
    public class UITotalReward : MonoBehaviour
    {
        public FinalResultUI finalRewardMain;
        public GameObject _active;
        public GameObject season, race, livematch, walkoff;
        public GameObject seasonCasterAnim, liveCasterAnim, raceCasterAnim;
        public GameObject goldUI, coinUI, moneyObj;
        public Transform grid;
        public GameObject next;

        private UILabel goldLabel;

        private int top100Rulby;

        private bool bPressNext;

        /// <summary>
        /// 시즌 종합 보상 초기화
        /// </summary>
        public void InitSeasonTitleReward()
        {
            next.SetActive(false);
            WebConnector.SeasonAnnounceInfo annInfo = finalRewardMain.getLobbyInfo().annInfo;
            //골드
            goldUI.SetActive(true);
            goldLabel = goldUI.transform.Find("coinLabel").GetComponent<UILabel>();
            int gold = getTotalGold(annInfo);
            goldLabel.text = string.Format("{0:n0}", 0);
            //아이템
            Dictionary<int, int> rwdItems = annInfo.titleRwdItems;
            //season.SetActive(true);

            top100Rulby = 0;

            //초기화
            seasonCasterAnim.gameObject.SetActive(true); //시즌 캐스터
            StartCoroutine(init(rwdItems, gold, season));
        }

        /// <summary>
        /// 시즌 골드값 계산(해당 타이틀중 내가 획득한 타이틀만 골라 모두 더함)
        /// </summary>
        /// <param name="annInfo"></param>
        /// <returns></returns>
        private int getTotalGold(SeasonAnnounceInfo annInfo)
        {
            int totalGold = 0;
            SeasonTitleMvpRewardInfo batterMvp = annInfo.titleHitterMvp;
            SeasonTitleMvpRewardInfo pitcherMvp = annInfo.titlePitcherMvp;            

            if (batterMvp.teamNo == BHConst.myTeamNo)
            {
                //타자 MVP상금
                totalGold += batterMvp.rwdGold;
            }
            if (pitcherMvp.teamNo == BHConst.myTeamNo)
            {
                //투수 MVP상금
                totalGold += pitcherMvp.rwdGold;
            }
            Dictionary<MvpType, List<SeasonTitleRewardInfo>> titleInfo = annInfo.titleInfo;
            foreach (KeyValuePair<MvpType, List<SeasonTitleRewardInfo>> value in titleInfo)
            {
                MvpType key = value.Key;
                List<SeasonTitleRewardInfo> rewardTitleList = value.Value;
                for (int i = 0; i < rewardTitleList.Count; i++)
                {
                    SeasonTitleRewardInfo reward = rewardTitleList[i];
                    if (reward.teamNo == BHConst.myTeamNo)
                    {
                        //각종 순위권 상금
                        totalGold += reward.rwdGold;
                    }
                }
            }
            return totalGold;
        }


        /// <summary>
        /// 쟁탈 주간 보상 초기화
        /// </summary>
        public void InitRaceWeekendReward()
        {
            next.SetActive(false);
            WebConnector.RacePlayAnnounceInfo annInfo = finalRewardMain.getRaceInfo().annInfo;
            //race.SetActive(true);
            race.transform.Find("sesonRankLabel").GetComponent<UILabel>().text = string.Format("{0:n0}", annInfo.weekRanking);

            //골드
            int gold = 0; //골드 없음

            //아이템(루비)
            Dictionary<int, int> weekLeagueItems = annInfo.weekLeagueItems;
            if (annInfo.weekLeagueRuby > 0)
            {
                if (weekLeagueItems == null)
                {
                    weekLeagueItems = new Dictionary<int, int>();
                }
                weekLeagueItems.Add(0, annInfo.weekLeagueRuby);
            }

            //탑100루비
            top100Rulby = annInfo.weekRankingRuby;// --> (top100 루비 보상) 다른 UI에서 처리

            //초기화
            raceCasterAnim.gameObject.SetActive(true);
            StartCoroutine(init(weekLeagueItems, gold, race));
        }

        /// <summary>
        /// 9회투아웃 주간 보상 초기화
        /// </summary>
        public void InitWalkoffWeekendReward()
        {
            next.SetActive(false);
            WebConnector.WalkoffPlayAnnounceInfo annInfo = finalRewardMain.getWalkoffInfo().annInfo;
            //walkoff.SetActive(true);
            walkoff.transform.Find("sesonRankLabel").GetComponent<UILabel>().text = string.Format("{0:n0}", annInfo.weekRanking);

            //골드
            int gold = 0; //골드 없음

            /*//지워지워 테스트용
            gold = 100000; 
            goldUI.SetActive(true);
            goldLabel = goldUI.transform.Find("coinLabel").GetComponent<UILabel>();
            goldLabel.text = string.Format("{0:n0}", 0);
            //지워지워 테스트용 - 여기까지*/

            //아이템(+루비)
            Dictionary<int, int> weekLeagueItems = null;
            if (annInfo.weekRankingRuby > 0)
            {
                weekLeagueItems = new Dictionary<int, int>();
                weekLeagueItems.Add(0, annInfo.weekRankingRuby);
            }

            top100Rulby = 0;

            //초기화
            seasonCasterAnim.gameObject.SetActive(true); //시즌 캐스터 (우선 임시로)
            StartCoroutine(init(weekLeagueItems, gold, walkoff));

        }

        /// <summary>
        /// 라이브 주간 보상 초기화
        /// </summary>
        public void InitLiveMatchWeekendReward()
        {
            next.SetActive(false);
            WebConnector.LivePlayAnnounceInfo annInfo = finalRewardMain.getLiveInfo().annInfo;
            //livematch.SetActive(true);
            livematch.transform.Find("sesonRankLabel").GetComponent<UILabel>().text = string.Format("{0:n0}", annInfo.finalRank);


            //골드대신 코인
            coinUI.SetActive(true);
            goldLabel = coinUI.transform.Find("coinLabel").GetComponent<UILabel>();
            int coin = annInfo.weekLeagueCoin; //코인임
            goldLabel.text = string.Format("{0:n0}", 0);

            //아이템( +루비)
            Dictionary<int, int> weekLeagueItems = annInfo.weekLeagueItem;
            if (annInfo.weekLeagueRuby > 0)
            {
                if (weekLeagueItems == null)
                {
                    weekLeagueItems = new Dictionary<int, int>();
                }
                weekLeagueItems.Add(0, annInfo.weekLeagueRuby);
            }

            //탑100루비
            top100Rulby = annInfo.rwdRankRuby;// --> (top100 루비 보상) 다른 UI에서 처리

            //초기화
            liveCasterAnim.gameObject.SetActive(true);
            StartCoroutine(init(weekLeagueItems, coin, livematch));

        }

        /// <summary>
        /// 보상창 초기화
        /// </summary>
        private IEnumerator init(Dictionary<int, int> rwdItems, int gold, GameObject type)
        {
            if (gold == 0) goldUI.SetActive(false); //골드가 0이면 골드 UI deactive;
            //액티베이트                                 
            //finalRewardMain.changeScene();
            yield return new WaitForSeconds(0.5f);
            finalRewardMain.fadeIn();
            yield return new WaitForSeconds(0.2f);
            _active.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            type.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            //아이템 세팅
            if (rwdItems != null)
            {
                if (rwdItems.Count > 0)
                {
                    setItem(rwdItems);
                }
                yield return new WaitForSeconds(1.0f);
            }               
            //골드 세팅
            if (gold > 0)
            {
                moneyObj.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                int curGold = 0;
                float gab = 20;
                while (true)
                {
                    goldLabel.text = string.Format("{0:N0}", (int)(curGold));
                    yield return new WaitForEndOfFrame();
                    curGold += (int)gab;
                    gab *= 1.1f;
                    if (curGold > gold)
                    {
                        curGold = gold;
                        break;
                    }
                }
                goldLabel.text = string.Format("{0:N0}", (int)(gold));

            }
            yield return new WaitForSeconds(0.5f);
            next.SetActive(true);
            bPressNext = false;
        }

        /// <summary>
        /// 보상 아이템 세팅
        /// </summary>
        /// <param name="value"></param>
        private void setItem(Dictionary<int, int> items)
        {
            int itemCount = 0;
            foreach (KeyValuePair<int, int> value in items)
            {
                int id = value.Key;
                int getNum = value.Value;

                ItemSlot.ItemSize size;
                ItemSlot.ItemType type;

                if (id == 0)
                {
                    size = ItemSlot.ItemSize.BIG;
                    type = ItemSlot.ItemType.RUBY;
                }
                else
                {
                    
                }

                
            }
            grid.GetComponent<UIGrid>().enabled = true;
        }

        /// <summary>
        /// 확인 버튼
        /// </summary>
        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                ////Debug.Log("==========================>> next");
                StartCoroutine(deActive());

                if (top100Rulby > 0)
                {
                    //Debug.Log("==========================>> 탑100 루비 보상이 있다!!!!!! " + top100Rulby);
                }
                //else

                //9회투아웃 주간 보상시
                if (finalRewardMain.curType == FinalResultUI.FinalRewardType.WalkoffWeekEnd)
                {
                    //보상 프로세스 종료
                    finalRewardMain.deActive(); //이거진짜                
                    //finalRewardMain.leagueUpDown.InitTest(); //테스트용 - 이거 지움
                }
                //라이브매치 주간 보상시
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.LiveMatchWeekEnd)
                {
                    //보상 프로세스 종료
                    finalRewardMain.deActive();
                }
                //쟁탈 주간 보상
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceOnlyWeekEnd)
                {
                    //보상 프로세스 종료
                    finalRewardMain.deActive();  //이게 진짜                
                }
                //쟁탈 일일보상 + 쟁탈 주간 보상
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceDayAndWeekEnd)
                {
                    //쟁탈 리그 승강
                    finalRewardMain.leagueUpDown.InitRaceLeagueUpDown();
                }
                //시즌 모드
                else
                {
                    //시즌 리그 승강
                    finalRewardMain.leagueUpDown.InitSeasonLeagueUpDown();
                }
            }
        }

        /// <summary>
        /// 해당 UI비활성화
        /// </summary>
        /// <returns></returns>
        private IEnumerator deActive()
        {
            seasonCasterAnim.gameObject.SetActive(false);
            liveCasterAnim.gameObject.SetActive(false);
            raceCasterAnim.gameObject.SetActive(false);
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            _active.SetActive(false);
        }



    
    }
}
