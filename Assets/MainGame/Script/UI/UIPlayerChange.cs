using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIPlayerChange : MonoBehaviour
    {
        public enum PlayerChangeType
        {
            PitcherChange = 1,
            BatterChange = 2,
            FielderChange = 3,
            RunnerChange = 4
        }


        public GameObject _active;

        public GameObject defense, ofense;
        public GameObject bench, bullpen;


        public changeController2[] defensePlayer;
        public changeController2[] offensePlayer;


        public changeController[] benchCard;
        public changeController[] bullpenCard;

        public changeController outPlayer, inPlayer;

        public GameObject changeDisable;

        public GameObject batterGaugeBg, pitcherGaugeBg;
        public GameObject inPlayerGaugeObj;
        public GameObject[] outPlayerGauge;
        public GameObject[] inPlayerGauge;


        public GameObject timerObj;


        private BallPlayManager manager;
        private PlayerChangeType changeType;
        private bool bBullpenInit, bBenchInit;

        //만약 주자라면 바뀌는 베이스 인덱스
        private int outPlayerBaseIndex;

        CPlayer outPlayerCard, inPlayerCard;

        //시간
        private float remainTime;

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="_manager"></param>
        public void InitPlayerChangeUI(BallPlayManager _manager, int _remainTime = 10)
        {
            manager = _manager;

            outPlayerCard = null;
            inPlayerCard = null;

            bBullpenInit = bBenchInit = false;
            if (manager.bMyTurn == true)
            {
                offenseInit();
            }
            else
            {
                defenseInit();
            }
            inPlayerGaugeObj.SetActive(false);
            changeDisable.SetActive(true);
            if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                timerObj.SetActive(true);
                remainTime = _remainTime;
                timer = timerSetting();
                StartCoroutine(timer);
            }
            else
            {
                timerObj.SetActive(false);
                timer = null;
            }
            _active.SetActive(true);
            TweenAlpha.Begin(gameObject, 0.5f, 1);

        }

        /// <summary>
        /// 수비시 교체 UI 초기화
        /// </summary>
        private void defenseInit()
        {
            ofense.SetActive(false);
            defense.SetActive(true);

            defenseFieldInit();

            changeType = PlayerChangeType.PitcherChange;
            defensePlayer[CPlayer._PITCHER].Select();

            outPlayer.SelectChangePlayer(manager.pitcher.pPitcher);
            inPlayer.SelectNone();

            
            bullpenInit();
        }

        /// <summary>
        /// 공격시 교체 UI 초기화
        /// </summary>
        private void offenseInit()
        {
            defense.SetActive(false);
            ofense.SetActive(true);

            offenseFieldInit();

            changeType = PlayerChangeType.BatterChange;
            offensePlayer[0].Select();

            outPlayer.SelectChangePlayer(manager.batter.pBatter);
            inPlayer.SelectNone();
                        
            benchInit();
            
        }

        /// <summary>
        /// 수비 필드의 초기화
        /// </summary>
        private void defenseFieldInit()
        {
            int team = 0;
            for (int i = 0; i < BallPlayManager.NUM_LINEUP; i++)
            {
                int pos = SimulPlayerManager.GetCurPosition(team, i);
                if (pos < CPlayer._DH && pos >= CPlayer._PITCHER)
                {
                    if (pos == CPlayer._PITCHER)
                    {
                        CPlayer player = SimulPlayerManager.GetPitcher(team);
                        defensePlayer[pos].Init(this, player, pos, PlayerChangeType.PitcherChange);
                    }
                    else
                    {
                        CPlayer player = SimulPlayerManager.GetFielder(team, i);
                        defensePlayer[pos].Init(this, player, pos, PlayerChangeType.FielderChange);
                    }
                }
            }
        }

        /// <summary>
        /// 공격 필드의 초기화
        /// </summary>
        private void offenseFieldInit()
        {
            CPlayer batter = manager.batter.pBatter;
            offensePlayer[0].Init(this, batter, batter.getCurPos(), PlayerChangeType.BatterChange);

            for (int i = FieldParm.FIRSTBASE_INDEX; i <= FieldParm.THIRDBASE_INDEX; i++)
            {
                Runner _runner = manager.field.run.getRunner(i);
                if (_runner == null)
                {
                    offensePlayer[1 + i].gameObject.SetActive(false);
                }
                else
                {
                    offensePlayer[1 + i].gameObject.SetActive(true);
                    CPlayer runner = _runner.pRunner;
                    offensePlayer[1 + i].Init(this, runner, runner.getCurPos(), PlayerChangeType.RunnerChange);
                    offensePlayer[1 + i].SetBaseIndex(i);   //베이스 인덱스 설정해줌
                }
            }
        }

        /// <summary>
        /// 필드상 오브젝트(교체 아웃되는 대상)을 모두 선택되지 않게 설정
        /// </summary>
        /// <param name="type"></param>
        public void unSelectAll(PlayerChangeType type)
        {
            if (changeType != type)
            {
                if (type == PlayerChangeType.PitcherChange)
                {
                    setInPlayerNone();
                    bullpenInit();
                }
                else
                {
                    if (changeType == PlayerChangeType.PitcherChange)
                    {
                        setInPlayerNone();
                        benchInit();
                    }
                }
                changeType = type;
            }


            if (manager.bMyTurn == false)
            {
                for (int i = 0; i < 9; i++) defensePlayer[i].Unselect();
            }
            else
            {
                for (int i = 0; i < 4; i++) offensePlayer[i].Unselect();
            }
        }

        /// <summary>
        /// 벤치 혹은 불펜에 있는 카드를 모두 선택되지 않게 설정
        /// </summary>
        public void unSelectCardAll()
        {
            if (changeType == PlayerChangeType.PitcherChange)
            {
                for (int i = 0; i < bullpenCard.Length; i++) bullpenCard[i].Unselect();
            }
            else
            {
                for (int i = 0; i < benchCard.Length; i++) benchCard[i].Unselect();
            }
        }

        /// <summary>
        /// 교체 In 되는 선수를 설정
        /// </summary>
        /// <param name="player"></param>
        public void setInPlayer(CPlayer player)
        {
            //Debug.Log("==================>> 교체 인될 플레이어 선택 " + player.getName());
            inPlayerCard = player;
            inPlayerGaugeObj.SetActive(true);
            changeDisable.SetActive(false);
            inPlayer.SelectChangePlayer(player);

            if (outPlayerCard == null)
            {
                setGauge(inPlayerGauge, inPlayerCard);
            }
            else
            {
                setGaugeCompare(outPlayerGauge, inPlayerGauge, outPlayerCard, inPlayerCard);
            }
        }

        /// <summary>
        /// 교체 Out 되는 선수를 설정
        /// </summary>
        /// <param name="player"></param>
        /// <param name="baseIndex"></param>
        public void setOutPlayer(CPlayer player, int baseIndex)
        {
            //Debug.Log("==================>> 교체 아웃될 플레이어 선택 " + player.getName());
            outPlayerCard = player;
            outPlayer.SelectChangePlayer(player);

            outPlayerBaseIndex = baseIndex;

            if (inPlayerCard == null)
            {
                setGauge(outPlayerGauge, outPlayerCard);
            }
            else
            {
                setGaugeCompare(outPlayerGauge, inPlayerGauge, outPlayerCard, inPlayerCard);
            }
        }

        /// <summary>
        /// 교체 In되는 선수 초기화
        /// </summary>
        public void setInPlayerNone()
        {
            inPlayerCard = null;
            inPlayerGaugeObj.SetActive(false);
            changeDisable.SetActive(true);
            inPlayer.SelectNone();
            setGauge(outPlayerGauge, outPlayerCard);
        }

        /// <summary>
        /// 불펜 덱 초기화
        /// </summary>
        private void bullpenInit()
        {
            batterGaugeBg.SetActive(false);
            pitcherGaugeBg.SetActive(true);
            bench.SetActive(false);
            bullpen.SetActive(true);
            if (bBullpenInit == false)
            {
                bBullpenInit = true;

                for (int i = 0; i < bullpenCard.Length; i++)
                {
                    bullpenCard[i].gameObject.SetActive(false);
                }

                int count = 0;
                for (int i = 5; i < SimulPlayer.NUM_PITCHER; i++)
                {
                    if (SimulPlayerManager.GetPitcherOut(0, i) == false)
                    {
                        bullpenCard[count].gameObject.SetActive(true);
                        CPlayer bullpenPlayer = SimulPlayerManager.GetPitcher(0, i);
                        bullpenCard[count].Init(this, bullpenPlayer);
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// 벤치 덱 초기화
        /// </summary>
        private void benchInit()
        {
            batterGaugeBg.SetActive(true);
            pitcherGaugeBg.SetActive(false);
            bullpen.SetActive(false);
            bench.SetActive(true);
            if (bBenchInit == false)
            {
                bBenchInit = true;

                for (int i = 0; i < benchCard.Length; i++)
                {
                    benchCard[i].gameObject.SetActive(false);
                }

                int count = 0;
                for (int i = 0; i < SimulPlayer.NUM_FIELDER; i++)
                {
                    if (SimulPlayerManager.GetFielderOut(0, i) == false)
                    {
                        benchCard[count].gameObject.SetActive(true);
                        CPlayer benchPlayer = SimulPlayerManager.GetFielder(0, i);
                        benchCard[count].Init(this, benchPlayer);
                        count++;
                    }
                }
                
            }
        }

        /// <summary>
        /// 게이지 세팅
        /// </summary>
        /// <param name="gauge"></param>
        /// <param name="player"></param>
        public void setGauge(GameObject [] gauge, CPlayer player)
        {
            int count = gauge.Length;
            bool bPitcher = (changeType == PlayerChangeType.PitcherChange ? true : false);
            for (int i = 0; i < count; i++)
            {
                UISprite bar1 = gauge[i].transform.Find("bar1").GetComponent<UISprite>();
                UISprite bar2 = gauge[i].transform.Find("bar2").GetComponent<UISprite>();
                UILabel Label = gauge[i].transform.Find("Label").GetComponent<UILabel>();
                UILabel minus = gauge[i].transform.Find("minus").GetComponent<UILabel>();

                bar2.gameObject.SetActive(false);

                int value = 0;
                if (bPitcher ==true)
                {
                    if (i == 0) value = player.getStaminaValue();
                    else if (i == 1) value = player.getBallValue2((int)PitchType.FASTBALL);
                    else if (i == 2) value = player.getBallValue2((int)PitchType.CHANGEUP);
                    else if (i == 3) value = player.getBallValue2((int)PitchType.SLIDER);
                    else if (i == 4) value = player.getBallValue2((int)PitchType.CURVE);
                    else if (i == 5) value = player.getBallValue2((int)PitchType.FORK);       
                }
                else
                {                    
                    if (i == 0) value = player.getPower();
                    else if (i == 1) value = player.getContact();
                    else if (i == 2) value = player.getEye();
                    else if (i == 3) value = player.getSpeed();
                    else if (i == 4) value = player.getThrowing();
                    else if (i == 5) value = player.getFielding();                    
                }
                value = value / 10;
                Label.text = value.ToString();
                Label.color = Color.white;
                value = Mathf.Clamp(value, 1, 120);
                bar1.SetDimensions((value * 96) / 120, 16);
                minus.gameObject.SetActive(false);
                /*if (i >= 4 && player.getMissMatch())
                {
                    minus.gameObject.SetActive(true);
                    int minusValue = -(value * 30) /100;
                    minus.text = minusValue.ToString();
                }
                else
                {
                    minus.gameObject.SetActive(false);
                }*/
            }

        }

        /// <summary>
        /// 비교 게이지 세팅
        /// </summary>
        /// <param name="outGauge"></param>
        /// <param name="inGauge"></param>
        /// <param name="outPlayer"></param>
        /// <param name="inPlayer"></param>
        public void setGaugeCompare(GameObject[] outGauge, GameObject[] inGauge,  CPlayer outPlayer, CPlayer inPlayer)
        {
            int count = outGauge.Length;
            bool bPitcher = (changeType == PlayerChangeType.PitcherChange ? true : false);
            for (int i = 0; i < count; i++)
            {
                int smallValue = 0;
                int bigValue = 0;
                bool bOutPlayerBig = false;

                UISprite outbar1 = outGauge[i].transform.Find("bar1").GetComponent<UISprite>();
                UISprite outbar2 = outGauge[i].transform.Find("bar2").GetComponent<UISprite>();
                UILabel outLabel = outGauge[i].transform.Find("Label").GetComponent<UILabel>();
                UILabel outminus = outGauge[i].transform.Find("minus").GetComponent<UILabel>();

                UISprite inbar1 = inGauge[i].transform.Find("bar1").GetComponent<UISprite>();
                UISprite inbar2 = inGauge[i].transform.Find("bar2").GetComponent<UISprite>();
                UILabel inLabel = inGauge[i].transform.Find("Label").GetComponent<UILabel>();
                UILabel inminus = inGauge[i].transform.Find("minus").GetComponent<UILabel>();

                outbar2.gameObject.SetActive(false);
                inbar2.gameObject.SetActive(false);

                if (bPitcher == true)
                {
                    if (i == 0)
                    {
                        bOutPlayerBig = (outPlayer.getStaminaValue() < inPlayer.getStaminaValue() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getStaminaValue() : outPlayer.getStaminaValue());
                        bigValue = (bOutPlayerBig ? outPlayer.getStaminaValue() : inPlayer.getStaminaValue());
                    }
                    else if (i == 1)
                    {
                        bOutPlayerBig = (outPlayer.getBallValue2((int)PitchType.FASTBALL) < inPlayer.getBallValue2((int)PitchType.FASTBALL) ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getBallValue2((int)PitchType.FASTBALL) : outPlayer.getBallValue2((int)PitchType.FASTBALL));
                        bigValue = (bOutPlayerBig ? outPlayer.getBallValue2((int)PitchType.FASTBALL) : inPlayer.getBallValue2((int)PitchType.FASTBALL));
                    }
                    else if (i == 2)
                    {
                        bOutPlayerBig = (outPlayer.getBallValue2((int)PitchType.CHANGEUP) < inPlayer.getBallValue2((int)PitchType.CHANGEUP) ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getBallValue2((int)PitchType.CHANGEUP) : outPlayer.getBallValue2((int)PitchType.CHANGEUP));
                        bigValue = (bOutPlayerBig ? outPlayer.getBallValue2((int)PitchType.CHANGEUP) : inPlayer.getBallValue2((int)PitchType.CHANGEUP));
                    }
                    else if (i == 3)
                    {
                        bOutPlayerBig = (outPlayer.getBallValue2((int)PitchType.SLIDER) < inPlayer.getBallValue2((int)PitchType.SLIDER) ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getBallValue2((int)PitchType.SLIDER) : outPlayer.getBallValue2((int)PitchType.SLIDER));
                        bigValue = (bOutPlayerBig ? outPlayer.getBallValue2((int)PitchType.SLIDER) : inPlayer.getBallValue2((int)PitchType.SLIDER));
                    }
                    else if (i == 4)
                    {
                        bOutPlayerBig = (outPlayer.getBallValue2((int)PitchType.CURVE) < inPlayer.getBallValue2((int)PitchType.CURVE) ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getBallValue2((int)PitchType.CURVE) : outPlayer.getBallValue2((int)PitchType.CURVE));
                        bigValue = (bOutPlayerBig ? outPlayer.getBallValue2((int)PitchType.CURVE) : inPlayer.getBallValue2((int)PitchType.CURVE));
                    }
                    else if (i == 5)
                    {
                        bOutPlayerBig = (outPlayer.getBallValue2((int)PitchType.FORK) < inPlayer.getBallValue2((int)PitchType.FORK) ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getBallValue2((int)PitchType.FORK) : outPlayer.getBallValue2((int)PitchType.FORK));
                        bigValue = (bOutPlayerBig ? outPlayer.getBallValue2((int)PitchType.FORK) : inPlayer.getBallValue2((int)PitchType.FORK));
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        bOutPlayerBig = (outPlayer.getPower() < inPlayer.getPower() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getPower() : outPlayer.getPower());
                        bigValue = (bOutPlayerBig ? outPlayer.getPower() : inPlayer.getPower());
                    }
                    else if (i == 1)
                    {
                        bOutPlayerBig = (outPlayer.getContact() < inPlayer.getContact() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getContact() : outPlayer.getContact());
                        bigValue = (bOutPlayerBig ? outPlayer.getContact() : inPlayer.getContact());
                    }
                    else if (i == 2)
                    {
                        bOutPlayerBig = (outPlayer.getEye() < inPlayer.getEye() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getEye() : outPlayer.getEye());
                        bigValue = (bOutPlayerBig ? outPlayer.getEye() : inPlayer.getEye());
                    }
                    else if (i == 3)
                    {
                        bOutPlayerBig = (outPlayer.getSpeed() < inPlayer.getSpeed() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getSpeed() : outPlayer.getSpeed());
                        bigValue = (bOutPlayerBig ? outPlayer.getSpeed() : inPlayer.getSpeed());
                    }
                    else if (i == 4)
                    {
                        bOutPlayerBig = (outPlayer.getThrowing() < inPlayer.getThrowing() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getThrowing() : outPlayer.getThrowing());
                        bigValue = (bOutPlayerBig ? outPlayer.getThrowing() : inPlayer.getThrowing());
                    }
                    else if (i == 5)
                    {
                        bOutPlayerBig = (outPlayer.getFielding() < inPlayer.getFielding() ? false : true);
                        smallValue = (bOutPlayerBig ? inPlayer.getFielding() : outPlayer.getFielding());
                        bigValue = (bOutPlayerBig ? outPlayer.getFielding() : inPlayer.getFielding());
                    }
                }
                smallValue = smallValue / 10;
                bigValue = bigValue / 10;
                if (bOutPlayerBig == true)
                {
                    outLabel.text = bigValue.ToString();
                    inLabel.text = smallValue.ToString();
                    outLabel.color = new Color(1, 0.83f, 0);
                    inLabel.color = new Color(0.5f, 0.58f, 0.91f);
                }
                else
                {
                    outLabel.text = smallValue.ToString();
                    inLabel.text = bigValue.ToString();
                    outLabel.color = new Color(0.5f, 0.58f, 0.91f);
                    inLabel.color = new Color(1, 0.83f, 0);
                }

                smallValue = Mathf.Clamp(smallValue, 1, 118);
                bigValue = Mathf.Clamp(bigValue, 1, 120);

                outbar1.SetDimensions((smallValue * 96) / 120, 16);
                inbar1.SetDimensions((smallValue * 96) / 120, 16);
                if (bOutPlayerBig == true)
                {
                    outbar2.gameObject.SetActive(true);
                    outbar2.SetDimensions((bigValue * 96) / 120, 16);
                }
                else
                {
                    inbar2.gameObject.SetActive(true);
                    inbar2.SetDimensions((bigValue * 96) / 120, 16);
                }

                outminus.gameObject.SetActive(false);
                if (i >= 4 && inPlayer.getPosition() != outPlayer.getCurPos())
                {
                    //인플레이어 미스매치
                    inminus.gameObject.SetActive(true);
                    int curValue = bOutPlayerBig ? smallValue : bigValue;
                    int minusValue = -(curValue * 30) / 100;
                    inminus.text = minusValue.ToString();
                }
                else
                {
                    inminus.gameObject.SetActive(false);
                }
            }

        }


        /// <summary>
        /// 선수교체
        /// </summary>
        public void playerChange()
        {
            if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                //PVP모드에서는 시간 없으니 그냥 바꿈
                changePlayerEvent();
            }
            else
            {
                //일반 모드에서는 팝업을 띄워줌
                string message = "선수를 교체 하시겠습니까?"; //포지션이 맞는 경우
                if (inPlayerCard.getPosition() != outPlayerCard.getCurPos()) message = "포지션이 맞지 않습니다.\n정말 교체 하시겠습니까?"; //포지션이 맞지 않는경우
                IngameUI.SetConfirmPopupTwobutton("타이틀", message, this.changePlayerEvent, IngameUI.ClosePopup);
            }
        }

        private void changePlayerEvent()
        {
            IngameUI.ClosePopup();

            Debug.Log("====> 선수 교체");
            if (timer != null) StopCoroutine(timer);

            //체인지 이벤트
            int index = manager.bMyTurn ? outPlayerBaseIndex : outPlayerCard.getCurPos();

            if (Mode.bPvpMode == true)
            {   

                //PVP모드 선수교체 동기화 정보 송신
                int outIndex, inIndex;
                if (changeType == PlayerChangeType.PitcherChange)
                {
                    outIndex = outPlayerCard.originLineup;
                    inIndex = inPlayerCard.originLineup;
                }
                else
                {
                    outIndex = outPlayerCard.getOrder();
                    inIndex = inPlayerCard.getOrder();
                }
                PvpManager.GetInstance().SendChangeSyncInfo(false, changeType, outIndex, inIndex, index);
            }

            //선수교체 이벤트 호출
            IngameUI.GetChangeEventUI().InitPlayerChangeUI(true, manager, outPlayerCard, inPlayerCard, changeType, index);

            TweenAlpha.Begin(gameObject, 0.2f, 0);
            Invoke("deactive2", 0.3f);
        }



        /// <summary>
        /// 
        /// </summary>
        public void deactive()
        {
            if (timer != null) StopCoroutine(timer);
            if (Mode.bPvpMode == true)
            {                
                //선수교체
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChangeFinish);
                if (manager.bMyTurn == false) IngameUI.GetScoreBoard().SetPitchTimerActive(true); //피치 타이머 재가동
            }

            Mode.bPauseGame = false;
            manager.pitcher.setResume();
            _active.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void deactive2()
        {            
            _active.SetActive(false);
        }

        /// <summary>
        /// 타이머 세팅
        /// </summary>
        private IEnumerator timer;
        private IEnumerator timerSetting()
        {
            UILabel timerLabel = timerObj.transform.Find("Label").GetComponent<UILabel>();
            UISprite timerGauge = timerObj.transform.Find("gauge").GetComponent<UISprite>();
            while (remainTime >= 0)
            {
                yield return new WaitForEndOfFrame();
                remainTime -= Time.deltaTime;
                timerLabel.text = "[000000]TIMER   [FF0000]" + string.Format("{0:F2}", remainTime); 
                int w = (int)(285 * remainTime / 10.00f);
                timerGauge.SetDimensions(w, 6);
            }
            timerLabel.text =  "[000000]TIMER   [FF0000]0.00";
            deactive();
        }

    }
}