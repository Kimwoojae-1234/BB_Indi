using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIOpening : MonoBehaviour
    {
        public GameObject _active;
        
        //로고
        public UITexture myLogo, cpuLogo;
        public UISprite myHomeAway, cpuHomeAway;

        //모드별
        public GameObject [] SeasonRace;
        public GameObject[] LiveMatch;
        public GameObject [] WalkOff;

        //센터
        public GameObject[] center;

        //리그 로고
        public UISprite leagueLogo;
        public UISprite walkoffLogo;
        public UISprite liveMatchLogo;

        //구장
        public UILabel stadiumLabel;

        //
        public GameObject cosecutive;

        //
        public GameObject[] light;

        
        public void init(BallPlayManager _manager)
        {
            cosecutive.gameObject.SetActive(false);

            int myTeam = SimulPlayerManager.myTeamIndex;
            int cpuTeam = SimulPlayerManager.cpuTeamIndex;

            //스타디움 이름
            stadiumLabel.text = Util.getStadiumName(Mode.stadiumType);

#if _Test_Local
            myLogo.mainTexture = Util.loadBigLogo(1);
            cpuLogo.mainTexture = Util.loadBigLogo(Random.Range(2,9));
            //myLogo.MakePixelPerfect();
            //cpuLogo.MakePixelPerfect();
#else
            // DISABLED_MGRS: myLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(myTeam))));
            // DISABLED_MGRS: cpuLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(cpuTeam))));
#endif
            myHomeAway.spriteName = _manager.bMyHome ? "open_home" : "open_away";
            cpuHomeAway.spriteName = _manager.bMyHome ? "open_away" : "open_home";

            int awayIndex = _manager.bMyHome?1:0;
            int homeIndex = 1-awayIndex;
            
            /*if(Mode.gameMode == Mode.GamePlayMode.Ranking)
            {   
                center[1].SetActive(true);
                leagueLogo.gameObject.SetActive(true);
                for (int i = 0; i < 2; i++)
                {
                    Rank[i].SetActive(true);
                    Rank[i].transform.FindChild("team").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                }

#if _Test_Local

#else                
                // DISABLED_MGRS: WebConnector.RankedPlayGameInfo info = Mgrs.userData.Ingame_rankInfo;
                int leagueGrade = (_manager.bMyHome ? info.homeTeam.league : info.awayTeam.league);
                leagueLogo.spriteName = "league_" + leagueGrade;
                //포인트
                Rank[awayIndex].transform.FindChild("rank").GetComponent<UILabel>().text = info.awayTeam.point.ToString();
                Rank[homeIndex].transform.FindChild("rank").GetComponent<UILabel>().text = info.homeTeam.point.ToString();
                //승무패는 추후
                Rank[awayIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = info.awayTeam.wdl[0] + "승 " + info.awayTeam.wdl[1] + "무 " + info.awayTeam.wdl[2] + "패";
                Rank[homeIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = info.homeTeam.wdl[0] + "승 " + info.homeTeam.wdl[1] + "무 " + info.homeTeam.wdl[2] + "패";
#endif

            }
            else*/ 
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                walkoffLogo.gameObject.SetActive(true);
                

                for (int i = 0; i < 2; i++)
                {
                    WalkOff[i].SetActive(true);
                    WalkOff[i].transform.Find("team").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                }

            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                center[1].SetActive(true);
                liveMatchLogo.gameObject.SetActive(true);
                for (int i = 0; i < 2; i++)
                {
                    LiveMatch[i].SetActive(true);
                    LiveMatch[i].transform.Find("team").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                }

#if _Test_Local

#else
                // DISABLED_MGRS: WebConnector.LivePlayGameInfo info = Mgrs.userData.livePlayGmaeInfo;
                int leagueGrade =  1;// (_manager.bMyHome ? info.homeTeam.league : info.awayTeam.league);
                liveMatchLogo.spriteName = "rankmark_" + leagueGrade;
                //포인트
                LiveMatch[awayIndex].transform.FindChild("rank").GetComponent<UILabel>().text = info.awayTeam.point.ToString();
                LiveMatch[homeIndex].transform.FindChild("rank").GetComponent<UILabel>().text = info.homeTeam.point.ToString();
                //승무패는 추후
                string awayText;
                if (info.awayTeam.wdl == null) awayText = "0승 0무 0패";
                else awayText = info.awayTeam.wdl[0] + "승 " + info.awayTeam.wdl[1] + "무 " + info.awayTeam.wdl[2] + "패";
                LiveMatch[awayIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = awayText;
                string homeText;
                if (info.homeTeam.wdl == null) homeText = "0승 0무 0패";
                else homeText = info.homeTeam.wdl[0] + "승 " + info.homeTeam.wdl[1] + "무 " + info.homeTeam.wdl[2] + "패";
                LiveMatch[homeIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = homeText;
                
#endif
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {

                center[1].SetActive(true);
                liveMatchLogo.gameObject.SetActive(true);
                for (int i = 0; i < 2; i++)
                {
                    LiveMatch[i].SetActive(true);
                    LiveMatch[i].transform.Find("team").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                }
            }
            else //if (Mode.gameMode == Mode.GamePlayMode.Season //  시즌, 쟁탈)
            {

                leagueLogo.gameObject.SetActive(true);

                for (int i = 0; i < 2; i++)
                {
                    SeasonRace[i].SetActive(true);
                    SeasonRace[i].transform.Find("team").GetComponent<UILabel>().text = (i == 0 ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam);
                }

#if !_Test_Local
                if (Mode.gameMode == Mode.GamePlayMode.Season)
                {
                    // DISABLED_MGRS: WebConnector.SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;

                    leagueLogo.spriteName = "league_" + info.leagueLev;
                    
                    int myScheSeq = info.myScheNo;
                    int[] teamNo = info.schedule[myScheSeq];
                    //팀정보
                    WebConnector.SeasonTeamInfo awayTeam = info.teamInfos[teamNo[1]];
                    WebConnector.SeasonTeamInfo homeTeam = info.teamInfos[teamNo[0]];

                    //어웨이
                    SeasonRace[awayIndex].transform.FindChild("rank").GetComponent<UILabel>().text = awayTeam.ranking.ToString();
                    SeasonRace[awayIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = info.awayWdl[0] + "승 " + info.awayWdl[1] + "무 " + info.awayWdl[2] + "패";
                    //홈
                    SeasonRace[homeIndex].transform.FindChild("rank").GetComponent<UILabel>().text = homeTeam.ranking.ToString();
                    SeasonRace[homeIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = info.homeWdl[0] + "승 " + info.homeWdl[1] + "무 " + info.homeWdl[2] + "패";
                    //일차
                    if (info.gameType == WebConnector.SeasonGameType.PennantRace)
                    {
                        //페넌트레이스
                        center[0].SetActive(true);
                        // DISABLED_MGRS: int day = Mgrs.userData.seasonLobbyInfo.roundNo;// info.homeWdl[0] + info.homeWdl[1] + info.homeWdl[2]; //임시
                        center[0].transform.FindChild("Label").GetComponent<UILabel>().text = day.ToString();
                    }
                    else
                    {
                        string title;
                        if (info.gameType == WebConnector.SeasonGameType.WildCard) title = "와일드카드 ";
                        else if (info.gameType == WebConnector.SeasonGameType.SemiPlayOff) title = "준플레이오프 ";
                        else if (info.gameType == WebConnector.SeasonGameType.PlayOff) title = "플레이오프 ";
                        else title = "한국시리즈 ";
                        //포스트 시즌
                        center[3].SetActive(true);
                        // DISABLED_MGRS: center[3].transform.FindChild("Label").GetComponent<UILabel>().text = title + Mgrs.userData.seasonLobbyInfo.roundNo + "차전";
                    }

                    // DISABLED_MGRS: if (Mgrs.userData.GetUserGameMode() == DefineEnum.EGameMode.SeasonConsecutive)
                    {
                        //cosecutive.gameObject.SetActive(true);
                        //cosecutive.transform.FindChild("Label").GetComponent<UILabel>().text = (11-Mode.ConsecutiveNum) + "/10";
                        GameObject obj = Util.Load("MainGame/prefabs/gameUI/consectiveGamePrefab", transform, new Vector3(0, 361, 0));
                        obj.GetComponent<consectiveGameUI>().Init();
                    }

                }
                else
                {
                    center[2].SetActive(true);

                    //쟁탈은 추후고려
                    // DISABLED_MGRS: WebConnector.RacePlayGameInfo info = Mgrs.userData.raceInfo;                   
                    // DISABLED_MGRS: WebConnector.RacePlayTeamInfo homeTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.homeTeamNo);
                    // DISABLED_MGRS: WebConnector.RacePlayTeamInfo awayTeam = Mgrs.userData.ingame_raceTemaInfoManager.GetTeamInfo(info.awayTeamNo);

                    leagueLogo.spriteName = "league_" + info.leagueLev;

                    //어웨이
                    SeasonRace[awayIndex].transform.FindChild("rank").GetComponent<UILabel>().text = awayTeam.ranking.ToString();
                    SeasonRace[awayIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = awayTeam.win + "승 " + awayTeam.draw + "무 " + awayTeam.lose + "패";
                    //홈
                    SeasonRace[homeIndex].transform.FindChild("rank").GetComponent<UILabel>().text = homeTeam.ranking.ToString();
                    SeasonRace[homeIndex].transform.FindChild("wdl").GetComponent<UILabel>().text = homeTeam.win + "승 " + homeTeam.draw + "무 " + homeTeam.lose + "패";

                }
#endif
            }
            FieldCrowdManager.SetCrowdActiveAll(false);

            StartCoroutine(deActive(_manager));
        }

        //임시
        private IEnumerator deActive(BallPlayManager manager)
        {
            if (Mode.bPvpMode433 == true)
            {
                //퀵시뮬레이터 초기화
                manager.simulator.init(manager);
                manager.simulator.gameObject.SetActive(false);
                manager.setFieldBack(); 
            }
            else
            {  
                //게임 시작
                manager.startGame();
                //모든 리소스 로딩되었는지 체크
                while (manager.bFielderLoadComp == false)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }

            //yield return new WaitForSeconds(0.3f);
            _active.SetActive(true);
            yield return new WaitForEndOfFrame();
            manager.destroyLoadingObj();
            yield return new WaitForEndOfFrame();
            Animator anim = _active.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("openning"));            
            yield return new WaitForSeconds(0.5f);
            light[0].SetActive(true);
            light[1].SetActive(true);            
            yield return new WaitForSeconds(2.5f);
            if (Mode.bPvpMode433 == true)
            {
                pvpmanager.Get().SendGameReadyInfo();
                Debug_UI.SetNetwork(true);
            }
            else
            {
                manager.bUpdate = true;
            }
            //오프닝 연출 마감
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);            
            Destroy(gameObject);
            
        }

        /*
        private IEnumerator deActive(BallPlayManager manager)
        {
            //
            //_active.SetActive(true);
            //Animator anim = gameObject.GetComponent<Animator>();
            //anim.enabled = true;
            //anim.Rebind();
            //anim.Play(Animator.StringToHash("openning"));

            yield return new WaitForEndOfFrame();
            _active.SetActive(true);
            yield return new WaitForSeconds(3.0f);

            if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                Debug_UI.SetNetwork(true);
                yield return new WaitForSeconds(1.0f); //yield return new WaitForSeconds(2.0f);
#if _Test_Local
                while (true)
                {
                    bool bReady = PvpManager.GetInstance().IsReady();
                    if (bReady == true)
                    {
                        break;
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                }
#endif
                manager.startGame();
                //yield return new WaitForSeconds(2.0f);
                Debug_UI.SetNetwork(false);
                TweenAlpha.Begin(gameObject, 0.5f, 0);
                yield return new WaitForSeconds(0.5f);                
                Destroy(gameObject);
            }
            else
            {
                
                while (manager.bFielderLoadComp == false)
                {
                    yield return new WaitForSeconds(0.3f);
                }
                TweenAlpha.Begin(gameObject, 0.5f, 0);
                yield return new WaitForSeconds(0.5f);
                Destroy(gameObject);
            }
        }*/

    }
}
