//#define _TEST_TYPE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class UIStarting : MonoBehaviour
    {
        //팀레벨 72 1루
        public GameObject _active;

        public GameObject _text_active;


        //탑정보
        public UITexture logo;
        public GameObject away, home;
        public UILabel teamPower;

        //선수정보
        public UI_CardSmall playerCard;
        public GameObject[] gaugeObj;
        public SkillSlot[] skillObj;
        public GearSlot[] equipObj;
        //
        

        //라인업
        public GameObject[] lineupObj;


        //탑
        public GameObject top;

        //라이트
        public UISprite light;

        private bool bQuit;
        private BallPlayManager manager;
        private int showCount;

        private int teamIndex;
        private bool bHome;

        private UISprite[] infoSpr = new UISprite[6];

        void Awake()
        {
            showCount = 0;
            //lightIndex = 0;
        }


        //private int lightIndex;
        
        void Update()
        {
            if (_active.activeSelf == true)
            {
                if (bQuit == false)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (Mode.bPvpMode == true)
                        {
                            PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.SkipAsk);
                        }

                        ////Debug.Log("=====================>>>aaa");
                        StopAllCoroutines();
                        StartCoroutine(deActive());
                    }

                    if (Mode.bPvpMode == true)
                    {
                        if (Mode.SkipAsk == true)
                        {
                            Mode.SkipAsk = false;
                            StopAllCoroutines();
                            StartCoroutine(deActive());
                        }
                    }

                }
                
            }
        }


        public void init(BallPlayManager _manager)
        {
            bQuit = false;
            manager = _manager;
            manager.playState = PlayState.NONE;
            teamIndex = (manager.bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex);
            bHome = manager.bTopInning;

            //
            for (int i = 0; i < 6; i++)
                infoSpr[i] = gaugeObj[i].transform.Find("name").GetComponent<UISprite>();

#if _Test_Local
#else          
            setPlayerInfoBox(SimulPlayerManager.GetFielder(manager.bMyTurn ? 0 : 1, 0), false);
            setTopUI();
            setLineup();

#endif      
            
            if(manager.bMyTurn == true)
            {
                _text_active.GetComponent<UILabel>().text = "선공입니다\n먼저 공격하세요!";
            }
            else
            {
                _text_active.GetComponent<UILabel>().text = "후공입니다\n무실점으로 방어하세요!";
            }
            _text_active.SetActive(true);

            /*_active.SetActive(true);
            gameObject.GetComponent<UIPanel>().alpha = 0;
            TweenAlpha.Begin(gameObject, 0.3f, 1);
            top.transform.localPosition = new Vector3(-55, 259, 0);
            top.GetComponent<UIWidget>().alpha = 0;*/
            StartCoroutine(showLineup(6.0f));
        }

        private void setTopUI()
        {
            // DISABLED_MGRS: logo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(teamIndex))));
            away.SetActive(bHome);
            home.SetActive(!bHome);
            //teamPower.text = UI_HelperCalculator.CalTeamPower(
#if _Test_Local

#else
            //팀 전력
            List<WebConnector.GameCardInfo> cards = null;
            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                //시즌 라인업 정보
                // DISABLED_MGRS: cards = bHome ? Mgrs.userData.seasonLobbyInfo.awayInfo.cards : Mgrs.userData.seasonLobbyInfo.homeInfo.cards;
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Race)
            {
                //쟁탈 라인업 정보
                // DISABLED_MGRS: cards = bHome ? Mgrs.userData.raceInfo.awayLineup : Mgrs.userData.raceInfo.homeLineup;
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Ranking)
            {
                //랭킹전 라인업 정보
                // DISABLED_MGRS: cards = bHome ? Mgrs.userData.Ingame_rankInfo.awayTeam.lineup : Mgrs.userData.Ingame_rankInfo.homeTeam.lineup;
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                //라이브 매치 라인업 정보
                // DISABLED_MGRS: cards = bHome ? Mgrs.userData.livePlayGmaeInfo.awayTeam.lineup : Mgrs.userData.livePlayGmaeInfo.homeTeam.lineup;
            }

            if (cards != null)
            {
                teamPower.text = string.Format("{0:N0}", UI_HelperCalculator.CalTeamPower(cards).total);
            }
#endif
        }


        private void setPlayerInfoBox(CPlayer player, bool bPitcher)
        {
#if _Test_Local
            for (int i = 0; i < 5; i++)
            {
                skillObj[i].SetSkillEmpty(SkillSlot.IconSIze.Medium);
            }
#else
            CardData data = new CardData(player.getCard());
            playerCard.SetCardInfo(data);

            int maxSkillCount = player.getPlayerData().max_skill_cnt;
            int skillCount = 0;
            if (player.getCard().skills != null)
            {
                skillCount = player.getCard().skills.Count;
            }

            if (bPitcher == true)
            {
                bool[] bCheck = new bool[6] { false, false, false, false, false, false };
                int count = 0;
                Dictionary<WebConnector.CardAbCode, int[]> abilities = player.getCard().abilities;
                for (int i = 0; i < 6; i++)
                {
                    gaugeSetter gauge = gaugeObj[i].GetComponent<gaugeSetter>();
                    if (abilities.ContainsKey(WebConnector.CardAbCode.SM) == true && bCheck[0] == false)
                    {
                        infoSpr[count].spriteName = "stat_stamina";
                        gauge.set(abilities[WebConnector.CardAbCode.SM], 81);
                        bCheck[0] = true;
                    }
                    else if (abilities.ContainsKey(WebConnector.CardAbCode.FF) == true && bCheck[1] == false)
                    {
                        infoSpr[count].spriteName = "stat_fourseam";
                        gauge.set(abilities[WebConnector.CardAbCode.FF], 81);
                        bCheck[1] = true;
                    }
                    else if (abilities.ContainsKey(WebConnector.CardAbCode.CU) == true && bCheck[2] == false)
                    {
                        infoSpr[count].spriteName = "stat_changeup";
                        gauge.set(abilities[WebConnector.CardAbCode.CU], 81);
                        bCheck[2] = true;
                    }
                    else if (abilities.ContainsKey(WebConnector.CardAbCode.SD) == true && bCheck[3] == false)
                    {
                        infoSpr[count].spriteName = "stat_slider";
                        gauge.set(abilities[WebConnector.CardAbCode.SD], 81);
                        bCheck[3] = true;
                    }
                    else if (abilities.ContainsKey(WebConnector.CardAbCode.CV) == true && bCheck[4] == false)
                    {
                        infoSpr[count].spriteName = "stat_curve";
                        gauge.set(abilities[WebConnector.CardAbCode.CV], 81);
                        bCheck[4] = true;
                    }
                    else if (abilities.ContainsKey(WebConnector.CardAbCode.FB) == true && bCheck[5] == false)
                    {
                        infoSpr[count].spriteName = "stat_fork";
                        gauge.set(abilities[WebConnector.CardAbCode.FB], 81);
                        bCheck[5] = true;
                    }
                    else
                    {
                        gauge.gameObject.SetActive(false);
                    }
                    count++;
                }
            }
            else
            {
                string[] sprName = new string[6] { "stat_contact", "stat_power", "stat_eye", "stat_fielding", "stat_throw", "stat_speed" };
                for (int i = 0; i < 6; i++)
                {
                    infoSpr[i].spriteName = sprName[i];
                    gaugeSetter gauge = gaugeObj[i].GetComponent<gaugeSetter>();
                    int[] value;
                    if (i == 0) value = player.getCard().abilities[WebConnector.CardAbCode.CT]; //컨택
                    else if (i == 1) value = player.getCard().abilities[WebConnector.CardAbCode.PW]; //파워
                    else if (i == 2) value = player.getCard().abilities[WebConnector.CardAbCode.BE]; //선구
                    else if (i == 3) value = player.getCard().abilities[WebConnector.CardAbCode.FD]; //수비
                    else if (i == 4) value = player.getCard().abilities[WebConnector.CardAbCode.TW]; //송구
                    else  value = player.getCard().abilities[WebConnector.CardAbCode.RN]; //주력
                    gauge.set(value, 81);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (i < maxSkillCount)
                {
                    if (i < skillCount)
                    {
                        SkillData curSkillData = new SkillData(player.getCard().skills[i]);
                        skillObj[i].SetSkillSlot(curSkillData, SkillSlot.IconSIze.Medium);
                    }
                    else
                    {
                        skillObj[i].SetLockSlot();
                    }
                }
                else
                {
                    skillObj[i].SetSkillEmpty(SkillSlot.IconSIze.Medium);
                }
            }
#endif
        }


        private void setLineup()
        {
            //타자세팅
            for (int i = 0; i < 9; i++)
            {
                setBatterBox(lineupObj[i].transform, i);
            }
            //투수세팅
            setPitcherBox(lineupObj[9].transform);
        }


        private void setBatterBox(Transform box, int count)
        {
            int curIndex = manager.bMyTurn?0:1;
            CPlayer curPlayer = SimulPlayerManager.GetFielder(curIndex,count);

#if _Test_Local
            Util.SetSpritePixelPerfect(box.Find("logo").GetComponent<UISprite>(), "logo_" + teamIndex);
#else
            Util.SetSpritePixelPerfect(box.FindChild("logo").GetComponent<UISprite>(), "logo_" + (int)curPlayer.getPlayerData().eTeam);
#endif
            //
            box.Find("num").GetComponent<UILabel>().text = (count+1).ToString();
            box.Find("lineup").GetComponent<UILabel>().text = curPlayer.getName();
            //
            /*UILabel overall = box.FindChild("rate").GetComponent<UILabel>();
            int overallNum = Utils.TeamPowerUtils.calCardPower(curPlayer.getCard().abilities);
            //overall.bitmapFont = Util.GetOverallFont(overallNum);
            overall.text = overallNum.ToString();            
            //
            box.FindChild("pos").GetComponent<UISprite>().spriteName = "position_" + (curPlayer.getCurPos() + 1).ToString();

            WebConnector.GameRecordHitter record = curPlayer.getBatterRecord();
            box.FindChild("avg").GetComponent<UILabel>().text = Util.GetCurAvg(record, 0, 0);*/
        }

        private void setPitcherBox(Transform box)
        {
            int curIndex = manager.bMyTurn ? 0 : 1;
            CPlayer curPlayer = SimulPlayerManager.GetPitcher(curIndex);
#if _Test_Local
            Util.SetSpritePixelPerfect(box.Find("logo").GetComponent<UISprite>(), "logo_" + teamIndex);
#else
            Util.SetSpritePixelPerfect(box.Find("logo").GetComponent<UISprite>(), "logo_" + (int)curPlayer.getPlayerData().eTeam);
#endif      
            //
            /*box.FindChild("lineup").GetComponent<UILabel>().text = curPlayer.getName();
            //
            UILabel overall = box.FindChild("rate").GetComponent<UILabel>();
            int overallNum = Utils.TeamPowerUtils.calCardPower(curPlayer.getCard());
            //overall.bitmapFont = Util.GetOverallFont(overallNum);
            overall.text = overallNum.ToString(); 
            //
            WebConnector.GameRecordPitcher record = curPlayer.getPitcherRecord();
            box.FindChild("avg").GetComponent<UILabel>().text = Util.GetCurErr(record, 0, 0);*/
        }


        private IEnumerator showLineup(float totalDelay)
        {
            /*for (int i = 0; i < 10; i++)
            {
                lineupObj[i].transform.GetComponent<UISprite>().spriteName = "lineup_table2";
                lineupObj[i].transform.FindChild("focus").GetComponent<UISprite>().alpha = 0;
            }
            
            yield return new WaitForSeconds(0.3f);

            TweenAlpha.Begin(top, 0.2f, 1);
            TweenPosition.Begin(top, 0.2f, new Vector3(0, 259, 0));

            yield return new WaitForSeconds(0.2f);

            CameraManager.SetPositionTo(new Vector3(4665, 1200, -200), 8.0f);

            showCount++;

            

            int curIndex = manager.bMyTurn ? 0 : 1;
            for (int i = 0; i < 10; i++)
            {
                CPlayer curPlayer = (i == 9 ? SimulPlayerManager.GetPitcher(curIndex) : SimulPlayerManager.GetFielder(curIndex, i));
                setPlayerInfoBox(curPlayer, (i == 9 ? true : false));

                GameObject focus = lineupObj[i].transform.FindChild("focus").gameObject;
                TweenAlpha.Begin(focus, 0.1f, 1);
                yield return new WaitForSeconds(0.1f);
                light.gameObject.SetActive(true);
                for (int j = 0; j < 20; j++)
                {
                    light.spriteName = "cardback_" + string.Format("{0:00000}", j);
                    yield return new WaitForEndOfFrame();
                     //yield return new WaitForSeconds(0.6f / 20.0f);
                }
                light.gameObject.SetActive(false);
                TweenAlpha.Begin(focus, 0.1f, 0);
                yield return new WaitForSeconds(0.1f);
            }*/

            Debug_UI.SetNetwork(false);

            showCount++;

            bQuit = true;

            //TweenAlpha.Begin(_text_active, 0.5f, 0);

            //TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            manager.playState = PlayState.PLAY_START_INNING;
            manager.setInningChangeSkip();
            yield return new WaitForEndOfFrame();
            _active.SetActive(false);

            yield return new WaitForSeconds(0.2f);
            TweenAlpha.Begin(_text_active, 0.2f, 0);
            if (showCount >= 2)
            {
                Destroy(gameObject,0.5f);
            }
            //Debug.Log("=================>> showLineup End");
        }


        private IEnumerator deActive()
        {
            UITweener tween = CameraManager.GetInstance().GetComponent<UITweener>();
            if (tween != null)
            {
                tween.enabled = false;
            }
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            manager.playState = PlayState.PLAY_START_INNING;
            manager.setInningChangeSkip();
            yield return new WaitForEndOfFrame();
            _active.SetActive(false);
            if (showCount >= 2)
            {
                Destroy(gameObject, 0.5f);
            }
            //Debug.Log("=================>> Deactive");
        }


    }
}
