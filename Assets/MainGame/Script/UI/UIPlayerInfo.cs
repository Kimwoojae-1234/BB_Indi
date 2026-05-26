using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIPlayerInfo : MonoBehaviour
    {
        public GameObject _active;
        public infoCard PitcherObj;
        public infoCard BatterObj;


        //private bool bInit = false;
        private int leftBuffUISetting, rightBuffUISetting;


        // Use this for initialization
        void Start()
        {
            //bInit = false;            
            _active.SetActive(false);
            leftBuffUISetting = rightBuffUISetting = 0;
        }

        /// <summary>
        /// 활성화여부
        /// </summary>
        /// <param name="bActive"></param>
        BallPlayManager manager = null;
        private bool bInitMotion = false;
        public void SetActive(bool bActive, bool bFade = false, bool bNewPitcher = false)
        {
            UIPanel panel = GetComponent<UIPanel>();

            if (bActive == true)
            {
                _active.SetActive(true);

                if (manager.bPlayBallEvent == false)
                {
                    IngameUI.LoadDynamicUI("playballPrefab", 100, 1.5f, new Vector3(-63, 0, 0));
                    manager.bPlayBallEvent = true;
                }

                panel.alpha = 1;

                bool bNewBatter = bFade;

                if (bNewBatter == false)
                {
                    if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                    {
                        PitcherObj.gameObject.SetActive(false);
                        BatterObj.transform.localPosition = new Vector3(470, 138, 0);
                    }
                    else
                    {
                        if (bInitMotion == false)
                        {
                            PitcherObj.transform.localPosition = new Vector3(-476, 138, 0);
                            BatterObj.transform.localPosition = new Vector3(470, 138, 0);
                        }
                        bInitMotion = false;
                    }
                    
                }
                else
                {
                    bInitMotion = true;
                    //투수 초기화
                    int pitcherIndex = manager.bTopInning ? SimulPlayerManager.homeTeamIndex : SimulPlayerManager.awayTeamIndex;
                    int batterIndex = manager.bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex;
                    if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
                    {
                        PitcherObj.gameObject.SetActive(false);
                        //타자 초기화
                        int lineupCount = SimulPlayerManager.GetLineupCount(manager.bMyTurn ? 0 : 1) + 1;
                        BatterObj.initBatter(manager.batter.pBatter, batterIndex, lineupCount, new Vector3(795, 138, 0));
                        BatterObj.start();
                    }
                    else
                    {
                        if (manager.bMyTurn == false)
                        {
                            if (bNewPitcher == true)
                            {
                                PitcherObj.initPitcher(manager.pitcher.pPitcher, pitcherIndex, new Vector3(-795, 138, 0));
                                PitcherObj.start();
                            }
                            else
                            {
                                PitcherObj.transform.localPosition = new Vector3(-795, 138, 0);
                                PitcherObj.preSet(manager.pitcher.pPitcher);
                            }
                            //타자 초기화
                            int lineupCount = SimulPlayerManager.GetLineupCount(manager.bMyTurn ? 0 : 1) + 1;
                            BatterObj.initBatter(manager.batter.pBatter, batterIndex, lineupCount, new Vector3(795, 138, 0));
                            BatterObj.start();
                        }
                        else
                        {
                            if (bNewPitcher == true)
                            {
                                BatterObj.initPitcher(manager.pitcher.pPitcher, pitcherIndex, new Vector3(795, 138, 0));
                                BatterObj.start();
                            }
                            else
                            {
                                BatterObj.transform.localPosition = new Vector3(795, 138, 0);
                                BatterObj.preSet(manager.pitcher.pPitcher);
                            }
                            //타자 초기화
                            int lineupCount = SimulPlayerManager.GetLineupCount(manager.bMyTurn ? 0 : 1) + 1;
                            PitcherObj.initBatter(manager.batter.pBatter, batterIndex, lineupCount, new Vector3(-795, 138, 0));
                            PitcherObj.start();
                        }
                        UpdatePitcherSkill();
                    }
                }

#if GIRL_PLAY
                _active.SetActive(false);
#endif
                
                //StartCoroutine(deActiveCard(2.0f));
                Invoke("deActiveCard", 2.0f);
            }
            else
            {
                if(bFade == true)
                    StartCoroutine(deActive(panel));
                else
                    _active.SetActive(false);
            }

        }

        public void Active()
        {
            gameObject.GetComponent<UIPanel>().alpha = 1.0f;
            _active.SetActive(true);

            //버프 UI
            if (leftBuffUISetting != 0)
            {
                PitcherObj.setBuffUI((SkillID)leftBuffUISetting, true);
                leftBuffUISetting = 0;
            }
            if (rightBuffUISetting != 0)
            {
                BatterObj.setBuffUI((SkillID)rightBuffUISetting, false);
                rightBuffUISetting = 0;
            }
        }


        public void InfoInitPos()
        {
            //Debug.Log("=======================>>위치 초기화");
            PitcherObj.transform.localPosition = new Vector3(-795, 138, 0);
            BatterObj.transform.localPosition = new Vector3(795, 138, 0);
            PitcherObj.origin.SetActive(false);
            PitcherObj.origin.SetActive(false);
        }



        public void Init(BallPlayManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// 비활성화
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        private IEnumerator deActive(UIPanel panel)
        {
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.6f);
            _active.SetActive(false);
        }

        /// <summary>
        /// 카드만 비활성화
        /// </summary>
        /// <returns></returns>
        /*private bool bInfoGone = false;
        private IEnumerator deActiveCard(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (bInfoGone == false)
            {
                ControlManager.InfoGone();
                bInfoGone = true;
            }

        }*/

        private void deActiveCard()
        {
            ControlManager.InfoGone();
        }



        public void SetPitchNum(CPlayer pitcher)
        {
            //pitchNum.text = "투구 " + pitcher.getStat(Param.ST_PNP);
        }



        public void SetBuffUI(SkillID id, bool bMyUI)
        {
            SkillBuffType type = SkillParm.GetBuffType(id);            
            bool myUI = bMyUI;
            if (type == SkillBuffType.PitcherDown || type == SkillBuffType.BatterDown) myUI = !bMyUI; //반대

            if (myUI)
            {
                leftBuffUISetting = (int)id;
                SetActivateSkill(id, bMyUI);
            }
            else
            {
                if (type == SkillBuffType.PitcherSpecial || type == SkillBuffType.BatterSpecial)
                {
                    //특수능력 안나타내줘
                    rightBuffUISetting = 0;
                }
                else
                {
                    rightBuffUISetting = (int)id;
                    SetActivateSkill(id, bMyUI);
                }
            }

            
        }


        public void SetBuffUIDirect(SkillID id, bool bMyUI)
        {
            SkillBuffType type = SkillParm.GetBuffType(id);
            bool myUI = bMyUI;
            if (type == SkillBuffType.PitcherDown || type == SkillBuffType.BatterDown) myUI = !bMyUI; //반대

            if (myUI)
            {
                PitcherObj.setBuffUI(id, true);
            }
            else
            {
                if (type == SkillBuffType.PitcherSpecial || type == SkillBuffType.BatterSpecial)
                {
                    //특수능력 안나타내줘
                }
                else
                {
                    BatterObj.setBuffUI(id, false);
                }
            }
        }

        public void SetSkillInvalidity(bool bMyUI)
        {
            if (bMyUI)
            {
                PitcherObj.setSkillInvalidityUI(bMyUI);
            }
            else
            {
                BatterObj.setSkillInvalidityUI(bMyUI);
            }
        }


        public void SetActivateSkill(SkillID id, bool bMyUI)
        {
            //Debug.Log("=========================>>액티베이트 뉴 스킬");
            if (bMyUI == true)
            {
                CPlayer player = manager.bMyTurn ? manager.batter.pBatter : manager.pitcher.pPitcher;
                PitcherObj.activateSkill(player, (int)id);
            }
            else
            {
                CPlayer player = manager.bMyTurn ? manager.pitcher.pPitcher : manager.batter.pBatter;
                BatterObj.activateSkill(player, (int)id);
            }
        }

        public void UpdatePitcherSkill()
        {
            //Debug.Log("=========================>>업데이트 피처 스킬");
            CPlayer pitcher = manager.pitcher.pPitcher;
            if (pitcher != null)
            {
                if (manager.bMyTurn == true)
                {
                    BatterObj.updatePitcher(pitcher);
                }
                else
                {
                    PitcherObj.updatePitcher(pitcher);
                }
            }
        }
    }

}