//#define _TEST_STATE

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIChangeInning : MonoBehaviour
    {
        public GameObject _active;

        public scoreboard board;

        public UITexture myTeam, cpuTeam;
        public UISprite myState;
        public UILabel inning;
        public UISprite topBottom;

        public UI_CardSmall[] playerCard;


        private BallPlayManager manager;
        private bool bPressPossible;

        void Update()
        {
            /*if(Input.GetMouseButtonDown(0) == true)
            {
                if (_active.activeSelf == true)
                {
                    if (bPressPossible == true)
                    {
                        bPressPossible = false;
                        StartCoroutine(deActive(_active));
                    }
                }
                if (_walkOff.activeSelf == true)
                {
                    if (bPressPossible == true)
                    {
                        bPressPossible = false;
                        StartCoroutine(deActive(_walkOff));
                    }
                }
            }*/
        }


        public void Init()
        {
            _active.SetActive(true);
            board.initScoreBoard(SimulPlayerManager.strAwayTeam, SimulPlayerManager.strHomeTeam, SimulPlayerManager.awayTeamIndex, SimulPlayerManager.homeTeamIndex);
#if _Test_Local
            //
#else
            // DISABLED_MGRS: myTeam.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.myTeamIndex))));
            // DISABLED_MGRS: cpuTeam.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.cpuTeamIndex))));
#endif
            _active.SetActive(false);
        }


        


        public void SetActive(BallPlayManager _manager)
        {
            bPressPossible = false;
            manager = _manager;
            int awayIndex = manager.bMyHome?1:0;
            int homeIndex = 1-awayIndex;

            int[] awayScore = new int[12];
            int[] homeScore = new int[12];
            for (int i = 0; i < 12; i++)
            {
                awayScore[i] = manager.nInningScore[awayIndex, i];
                homeScore[i] = manager.nInningScore[homeIndex, i];
            }
            int[] awayStat = new int[3] { manager.nGameScore[awayIndex], manager.nHitCount[awayIndex], manager.nErrorCount[awayIndex] };
            int[] homeStat = new int[3] { manager.nGameScore[homeIndex], manager.nHitCount[homeIndex], manager.nErrorCount[homeIndex] };
            
            //보드
            board.setPlaying(manager.nInningCount, manager.bTopInning,
                             awayScore,
                             homeScore,
                             awayStat,
                             homeStat);

            //인포
            myState.spriteName = (manager.bMyTurn==false ? "lineup_offence" : "lineup_defence");
            inning.text = (manager.nInningCount + (manager.bTopInning?0:1)).ToString();
            topBottom.spriteName = (manager.bTopInning==false?"inningchange_top":"inningchange_bottom");

#if _Test_Local
            //로컬
#else
            for(int i=0;i<3;i++)
            {
                int team = manager.bTopInning ? homeIndex :awayIndex ;
                int lCount = SimulPlayerManager.GetLineupCount(team) + i;
                CPlayer player = SimulPlayerManager.GetFielder(team, lCount);
                CardData data = new CardData(player.getCard());
                playerCard[i].SetCardInfo(data);
            }
#endif           
            gameObject.GetComponent<UIPanel>().alpha = 1;
            StartCoroutine(active(_active, 3.0f));

        }

        private IEnumerator active(GameObject obj, float delay)
        {
            bPressPossible = true;
            yield return new WaitForSeconds(0.1f);
            obj.SetActive(true);            
            StartCoroutine(arrowDelay());
            yield return new WaitForSeconds(delay);
            StopCoroutine(arrowDelay());
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            obj.SetActive(false);
        }


        private IEnumerator deActive(GameObject obj)
        {
            StopCoroutine("active");            
            StopCoroutine(arrowDelay());
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            obj.SetActive(false);
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                manager.nineTwoNextRoundSetting(nextCount);
            }
            else
            {
                manager.changeInningSetting();
            }            
        }



        public GameObject _walkOff;

        public UISprite wLogo;
        public UILabel wTeamLabel;
        public UILabel[] wScore;
        public UISprite wCur;
        public UILabel wTotalScore;
        public UILabel wRound;
        public GameObject wArrow;
        public UISprite[] wStrike;

        public UI_CardSmall[] wPlayerCard;


        public void InitWalkOff(BallPlayManager _manager)
        {
            manager = _manager;
            _walkOff.SetActive(true);

            Util.SetSpritePixelPerfect(wLogo, "logo_" + SimulPlayerManager.myTeamIndex);//wLogo.spriteName = "logo_" + SimulPlayerManager.myTeamIndex;
            wTeamLabel.text = SimulPlayerManager.strMyTeam;
            wTotalScore.text = "0";
            wRound.text = "1";
            _walkOff.SetActive(false);
        }

        private int nextCount;
        public void WalkOffActive(BallPlayManager manager, int getScore, int count)
        {
            nextCount = count;
            
            int round = manager.nineTwoRound;
            int score = manager.nineTwoScore;
            if (getScore >= 0 && round >= 2)
            {
                wScore[round - 2].gameObject.SetActive(true);
                wScore[round - 2].text = getScore.ToString();
                wCur.gameObject.SetActive(true);
                wCur.transform.position = wScore[round - 2].transform.position;
                wCur.transform.localPosition += new Vector3(-0.8f, -2, 0);
            }
            else
            {
                wCur.gameObject.SetActive(false);
            }

            for (int i = 0; i < 2; i++)
            {
                wStrike[i].spriteName = (i < count ? "scoreboard_strike" :"scoreboard_round");
            }
            wTotalScore.text = score.ToString();
            wRound.text = round.ToString();
            bPressPossible = true;
            gameObject.GetComponent<UIPanel>().alpha = 1;
            StartCoroutine(active(_walkOff, 2.0f));

#if _Test_Local
            //
#else
            wPlayerCard[0].SetCardInfo(new CardData(manager.walkOffBatter.getCard()));
            wPlayerCard[1].SetCardInfo(new CardData(manager.walkOffPitcher.getCard()));
#endif

        }


        private IEnumerator arrowDelay()
        {
            int count = 0;
            while (true)
            {
                if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                {
                    wArrow.transform.localPosition = new Vector3(-14 + count * 14, -150, 0);
                }
                count++;
                if (count > 2) count = 0;
                yield return new WaitForSeconds(0.2f);
            }
        }

    }
}
