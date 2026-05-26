using UnityEngine;
using System.Collections;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UIResultMain : MonoBehaviour
    {
        public GameObject _active;

        //게임 정보
        public scoreboard board;

        public GameObject[] teamStat;
        public GameObject[] gauge;

        public GameObject effectObj;
        private GameObject [] upDownObj = new GameObject[2];


        public void initSeason(BallPlayManager manager)
        {
            //보드 세팅
            boardSetting(manager);

#if _Test_Local
            //이전 랭킹과 현재 랭킹
            //int[] teamNo = info.schedule[info.myScheNo];
            //int myTeamNo = teamNo[manager.bMyHome ? 0 : 1];
            //int cpuTeamNo = teamNo[manager.bMyHome ? 1 : 0];

            int[] lastRank = new int[2] { 1, 1};
            int[] curRank = new int[2] { 1, 1 };

            for (int i = 0; i < 2; i++)
            {
                //팀스탯 세팅
                Transform stat = teamStat[i].transform;
                // DISABLED_MGRS: stat.Find("logo").GetComponent<UITexture>().mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(i == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex))));
                stat.Find("teamLabel").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                stat.Find("scoreLabel").GetComponent<UILabel>().text = manager.nGameScore[i].ToString();
                //
                /*
                UISprite spr = stat.FindChild("updown").GetComponent<UISprite>();
                if (curRank[i] < lastRank[i]) spr.spriteName = "result_rankup";
                else if (curRank[i] > lastRank[i]) spr.spriteName = "result_rankdown";
                else spr.spriteName = "result_rankkeep";
                spr.MakePixelPerfect();*/
                if (curRank[i] < lastRank[i])
                {
                    upDownObj[i] = stat.Find("updown").Find("up").gameObject;
                }
                else if (curRank[i] > lastRank[i])
                {
                    upDownObj[i] = stat.Find("updown").Find("down").gameObject;
                }
                else
                {
                    upDownObj[i] = stat.Find("updown").Find("even").gameObject;
                }
                upDownObj[i].transform.Find("rankLabel").GetComponent<UILabel>().text = curRank[i] + "위";

                //게이지 세팅
                setGauge(gauge[i], manager.nHitCount[i], 1);
                setGauge(gauge[i], manager.nHomerunCount[i], 2);
                setGauge(gauge[i], manager.nStealCount[i], 3);
                setGauge(gauge[i], manager.nStrikeOutCount[i], 4);
                setGauge(gauge[i], manager.nErrorCount[i], 5);
            }
#else
            //인포
            // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;
            SeasonGameEndInfo resultInfo = ResultUI.GetSeasonEndInfo();


            //이전 랭킹과 현재 랭킹
            int[] teamNo = info.schedule[info.myScheNo];
            int myTeamNo = teamNo[manager.bMyHome?0:1];
            int cpuTeamNo = teamNo[manager.bMyHome?1:0];
            
            int [] lastRank = new int[2]{info.teamInfos[myTeamNo].ranking, info.teamInfos[cpuTeamNo].ranking};
            int [] curRank = new int[2]{resultInfo.teamRankings[myTeamNo], resultInfo.teamRankings[cpuTeamNo]};

            for (int i = 0; i < 2; i++)
            {
                //팀스탯 세팅
                Transform stat = teamStat[i].transform;
                // DISABLED_MGRS: stat.FindChild("logo").GetComponent<UITexture>().mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(i==0?SimulPlayerManager.myTeamIndex:SimulPlayerManager.cpuTeamIndex))));
                stat.FindChild("teamLabel").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                stat.FindChild("scoreLabel").GetComponent<UILabel>().text = manager.nGameScore[i].ToString();
                //
                /*
                UISprite spr = stat.FindChild("updown").GetComponent<UISprite>();
                if (curRank[i] < lastRank[i]) spr.spriteName = "result_rankup";
                else if (curRank[i] > lastRank[i]) spr.spriteName = "result_rankdown";
                else spr.spriteName = "result_rankkeep";
                spr.MakePixelPerfect();*/
                if (curRank[i] < lastRank[i])
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("up").gameObject;
                }
                else if (curRank[i] > lastRank[i])
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("down").gameObject;
                }
                else
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("even").gameObject;
                }
                upDownObj[i].transform.FindChild("rankLabel").GetComponent<UILabel>().text = curRank[i] + "위";                

                //게이지 세팅
                setGauge(gauge[i], manager.nHitCount[i], 1);
                setGauge(gauge[i], manager.nHomerunCount[i], 2);
                setGauge(gauge[i], manager.nStealCount[i], 3);
                setGauge(gauge[i], manager.nStrikeOutCount[i], 4);
                setGauge(gauge[i], manager.nErrorCount[i], 5);
            }

#endif

            _active.SetActive(true);
        }

        public void initRace(BallPlayManager manager)
        {
            //보드 세팅
            boardSetting(manager);

#if _Test_Local
                   
#else
            // DISABLED_MGRS: WebConnector.RacePlayGameInfo info = Mgrs.userData.raceInfo;
            WebConnector.RacePlayEndInfo resultInfo = ResultUI.GetRaceEndInfo();

            int[] lastRank = new int[2];
            int[] curRank = new int[2];

            // DISABLED_MGRS: RacePlayTeamInfo homeTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo);
            // DISABLED_MGRS: RacePlayTeamInfo awayTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.awayTeamNo);

            //이전 순위
            curRank[0] = lastRank[0] = manager.bMyHome ? homeTeam.ranking : awayTeam.ranking;
            curRank[1] = lastRank[1] = manager.bMyHome ? awayTeam.ranking : homeTeam.ranking;

            if (resultInfo.teamUdtInfos != null)
            {
                //새 순위 머지
                // DISABLED_MGRS: Mgrs.userData.ingame_raceTemaInfoManager.MergeTeamInfos(resultInfo.teamUdtInfos);

                //새 순위
                // DISABLED_MGRS: RacePlayTeamInfo homeTeamNew = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo);
                // DISABLED_MGRS: RacePlayTeamInfo awayTeamNew = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.awayTeamNo);

                //이전 순위
                curRank[0] = manager.bMyHome ? homeTeam.ranking : awayTeam.ranking;
                curRank[1] = manager.bMyHome ? awayTeam.ranking : homeTeam.ranking;
            }


            for (int i = 0; i < 2; i++)
            {
                //팀스탯 세팅
                Transform stat = teamStat[i].transform;
                // DISABLED_MGRS: stat.FindChild("logo").GetComponent<UITexture>().mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(i == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex))));
                stat.FindChild("teamLabel").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                stat.FindChild("scoreLabel").GetComponent<UILabel>().text = manager.nGameScore[i].ToString();

                //순위
                /*UISprite spr = stat.FindChild("updown").GetComponent<UISprite>();
                stat.FindChild("rankLabel").GetComponent<UILabel>().text = curRank[i] + "위";
                if (curRank[i] < lastRank[i]) spr.spriteName = "result_rankup";
                else if (curRank[i] > lastRank[i]) spr.spriteName = "result_rankdown";
                else spr.spriteName = "result_rankkeep";
                spr.MakePixelPerfect();*/
                if (curRank[i] < lastRank[i])
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("up").gameObject;
                }
                else if (curRank[i] > lastRank[i])
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("down").gameObject;
                }
                else
                {
                    upDownObj[i] = stat.FindChild("updown").FindChild("even").gameObject;
                }
                upDownObj[i].transform.FindChild("rankLabel").GetComponent<UILabel>().text = curRank[i] + "위";    

                //게이지 세팅
                setGauge(gauge[i], manager.nHitCount[i], 1);
                setGauge(gauge[i], manager.nHomerunCount[i], 2);
                setGauge(gauge[i], manager.nStealCount[i], 3);
                setGauge(gauge[i], manager.nStrikeOutCount[i], 4);
                setGauge(gauge[i], manager.nErrorCount[i], 5);
            }
#endif
            _active.SetActive(true);
        }

        /*
        public void initRank(BallPlayManager manager)
        {
            //보드 세팅
            boardSetting(manager);

#if _Test_Local
                   
#else
            // DISABLED_MGRS: WebConnector.RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;
            WebConnector.RankedPlayGameEndInfo resultInfo = ResultUI.GetRankPlayEndInfo();

            
            for (int i = 0; i < 2; i++)
            {
                //팀스탯 세팅
                Transform stat = teamStat[i].transform;
                // DISABLED_MGRS: stat.FindChild("logo").GetComponent<UITexture>().mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(i == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex))));
                stat.FindChild("teamLabel").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                stat.FindChild("scoreLabel").GetComponent<UILabel>().text = manager.nGameScore[i].ToString();

                UISprite spr = stat.FindChild("updown").GetComponent<UISprite>();
                spr.gameObject.SetActive(false);

                //게이지 세팅
                setGauge(gauge[i], manager.nHitCount[i], 1);
                setGauge(gauge[i], manager.nHomerunCount[i], 2);
                setGauge(gauge[i], manager.nStealCount[i], 3);
                setGauge(gauge[i], manager.nStrikeOutCount[i], 4);
                setGauge(gauge[i], manager.nErrorCount[i], 5);
            }
#endif
            _active.SetActive(true);
        }*/



        /// <summary>
        /// 라이브 매치 결과 초기화
        /// </summary>
        /// <param name="manager"></param>
        public void initLiveMath(BallPlayManager manager)
        {
            //보드 세팅
            boardSetting(manager);

#if _Test_Local
                   
#else
            //WebConnector.RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;
            WebConnector.LivePlayGameEndInfo resultInfo = ResultUI.GetLiveEndInfo();

            for (int i = 0; i < 2; i++)
            {
                //팀스탯 세팅
                Transform stat = teamStat[i].transform;
                // DISABLED_MGRS: stat.FindChild("logo").GetComponent<UITexture>().mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(i == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex))));
                stat.FindChild("teamLabel").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                stat.FindChild("scoreLabel").GetComponent<UILabel>().text = manager.nGameScore[i].ToString();


                //UISprite spr = stat.FindChild("updown").GetComponent<UISprite>();
                //spr.gameObject.SetActive(false);

                //게이지 세팅
                setGauge(gauge[i], manager.nHitCount[i], 1);
                setGauge(gauge[i], manager.nHomerunCount[i], 2);
                setGauge(gauge[i], manager.nStealCount[i], 3);
                setGauge(gauge[i], manager.nStrikeOutCount[i], 4);
                setGauge(gauge[i], manager.nErrorCount[i], 5);
            }

            //재화 업데이트
            // DISABLED_MGRS: Mgrs.userData.SetUserBalances(resultInfo.balances);
#endif
            _active.SetActive(true);
        }


        public void deActive()
        {
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            Invoke("deactive", 0.6f);
        }

        private void deactive()
        {
            _active.SetActive(false);
        }
             


        private void boardSetting(BallPlayManager manager)
        {
            //보드 세팅
            board.initScoreBoard(SimulPlayerManager.strAwayTeam, SimulPlayerManager.strHomeTeam, SimulPlayerManager.awayTeamIndex, SimulPlayerManager.homeTeamIndex);

            int awayIndex = manager.bMyHome ? 1 : 0;
            int homeIndex = 1 - awayIndex;
            int[] awayScore = new int[12];
            int[] homeScore = new int[12];
            for (int i = 0; i < 12; i++)
            {
                awayScore[i] = manager.nInningScore[awayIndex, i];
                homeScore[i] = manager.nInningScore[homeIndex, i];
            }
            int[] awayStat = new int[3] { manager.nGameScore[awayIndex], manager.nHitCount[awayIndex], manager.nErrorCount[awayIndex] };
            int[] homeStat = new int[3] { manager.nGameScore[homeIndex], manager.nHitCount[homeIndex], manager.nErrorCount[homeIndex] };

            board.setResult(awayScore, homeScore, awayStat, homeStat, awayIndex);
        }
      

        private void setGauge(GameObject gauge, int value, int count)
        {
            Transform value5 = gauge.transform.Find("value" + count);
            value5.GetComponent<UILabel>().text = value.ToString();
            int size5 = Mathf.Clamp(value * 22, 20, 332);
            UISprite gaugeSpr = value5.Find("gauge").GetComponent<UISprite>();
            if (value > 0)
            {
                //gaugeSpr.SetDimensions(size5, 20);
                StartCoroutine(gagueAnim(gaugeSpr, size5));
            }
            else
            {
                gaugeSpr.gameObject.SetActive(false);
            }
        }


        public void setEffectStart(GameObject titleObj, GameObject leftObj, GameObject rightObj)
        {
            StartCoroutine(effectStart(titleObj, leftObj, rightObj));
        }


        private IEnumerator effectStart(GameObject titleObj, GameObject leftObj, GameObject rightObj)
        {
            yield return new WaitForSeconds(0.5f);
            effectObj.SetActive(true);
            if(leftObj.activeSelf) TweenPosition.Begin(leftObj, 0.15f, new Vector3(-437, 0, 0));
            if (rightObj.activeSelf) TweenPosition.Begin(rightObj, 0.15f, new Vector3(437, 0, 0));
            yield return new WaitForSeconds(0.15f);
            TweenAlpha.Begin(effectObj.transform.Find("light1").gameObject, 0.2f, 0);
            TweenAlpha.Begin(effectObj.transform.Find("light2").gameObject, 0.2f, 0);
            yield return new WaitForSeconds(0.15f);
            titleObj.SetActive(true);

            if (Mode.gameMode == Mode.GamePlayMode.Race || Mode.gameMode == Mode.GamePlayMode.Season)
            {
                yield return new WaitForSeconds(0.8f);
                upDownObj[0].SetActive(true);
                yield return new WaitForSeconds(0.3f);
                upDownObj[1].SetActive(true);
            }
        }

        private IEnumerator gagueAnim(UISprite gaugeSpr, int size)
        {
            yield return new WaitForSeconds(1);
            gaugeSpr.gameObject.SetActive(true);
            float cursize = 1;
            float dv = 1;
            while (cursize < size)
            {
                gaugeSpr.SetDimensions((int)cursize, 20);
                yield return new WaitForEndOfFrame();
                cursize += dv;
                dv += 0.05f;
                if (cursize > size)
                {
                    cursize = size;
                    break;
                }
            }

            gaugeSpr.SetDimensions(size, 20);

        }


    }
}
