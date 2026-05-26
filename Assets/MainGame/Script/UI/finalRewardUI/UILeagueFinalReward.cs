using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UILeagueFinalReward : MonoBehaviour
    {
        public FinalResultUI finalRewardMain;
        public GameObject _active;

        public GameObject next;
        public GameObject league, race;

        public UILabel rankLabel;
        public UILabel teamPowerLabel;
        public UILabel wdlLabel;
        public UILabel coinLabel;
        public UISprite leagueLogoSpr;

        public Transform grid;
        public GameObject noReward;

        private bool bPressNext = false;

        //private bool bSeason;

        // Use this for initialization
        public void initSeasonReward()
        {
            
        }

        public void initRaceReward()
        {
            next.SetActive(false);
            RacePlayLobbyInfo info = finalRewardMain.getRaceInfo();
            Dictionary<int, int> lgItems = info.annInfo.lgItems;

            race.SetActive(true);

            int teamPower = finalRewardMain.getTeamPower();
            int leagueLevel = info.leagueLev;
            int ranking = info.annInfo.lgRanking;
            int coin = info.annInfo.lgGold;

            int index = Mathf.Clamp(ranking - 1, 0, 10);
            RacePlayTeamRecordInfo teamRecordInfo = info.annInfo.finalTeamRanks[index];
            wdlLabel.text = "(" + teamRecordInfo.win + "승 " + teamRecordInfo.draw + "무 " + teamRecordInfo.lose + "패)";

            StartCoroutine(init(lgItems, ranking, teamPower, coin, leagueLevel));
        }

        private IEnumerator init(Dictionary<int, int> rwdItems, int ranking, int teamPower, int coin, int leagueLevel)
        {
            //텍스트 세팅
            rankLabel.text = ranking.ToString();
            teamPowerLabel.text = string.Format("{0:n0}", teamPower);
            coinLabel.text = string.Format("{0:n0}", 0);
            leagueLogoSpr.spriteName = "league_" + leagueLevel;
            //아이템이 없는 경우
            if (rwdItems == null) noReward.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(0.5f);            
            finalRewardMain.fadeIn();
            yield return new WaitForSeconds(0.2f);            
            _active.SetActive(true);
            
            yield return new WaitForSeconds(0.5f);

            //아이템 세팅
            if (rwdItems != null)
            {
                if (rwdItems.Count == 0)
                {
                    noReward.gameObject.SetActive(true);
                }
                else
                {
                    setItem(rwdItems);
                }
                yield return new WaitForSeconds(1.0f);
            }

            //골드 세팅
            int curGold = 0;
            float gab = 20;
            while (true)
            {
                coinLabel.text = string.Format("{0:N0}", (int)(curGold));
                yield return new WaitForEndOfFrame();
                curGold += (int)gab;
                gab *= 1.1f;
                if (curGold > coin)
                {
                    curGold = coin;
                    break;
                }
            }
            coinLabel.text = string.Format("{0:N0}", (int)(coin));

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
           
        }


        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                //Debug.Log("==========================>> next");
                StartCoroutine(deActive());

                //finalRewardMain.deActive();

                if (finalRewardMain.curType == FinalResultUI.FinalRewardType.PenentEndSeasonEnd)
                {
                    //페넌트 종료와 시즌종료 동시에
                    finalRewardMain.changeScene();
                    finalRewardMain.leagueFinalTitle.InitSeasonTitle();
                }
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceDayAndWeekEnd)
                {
                    //쟁탈 주간 보상 -> 주간보상 상태
                    finalRewardMain.changeScene();
                    finalRewardMain.totalReward.InitRaceWeekendReward();
                }
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceOnlyDayEnd)
                {
                    //쟁탈 일일 보상 -> 리그 승강 상태
                    finalRewardMain.leagueUpDown.InitRaceLeagueUpDown();
                }
                else
                {
                    finalRewardMain.deActive();
                }
            }

        }

        private IEnumerator deActive()
        {
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            _active.SetActive(false);
        }
    }
}
