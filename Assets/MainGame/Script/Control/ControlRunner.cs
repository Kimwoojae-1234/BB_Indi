using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class ControlRunner : MonoBehaviour
    {   
        //
        public GameObject _active;
        public GameObject [] baseObj;
        public GameObject intWalkButton;

        
        //
        private BallPlayManager manager;
        private bool[] pressActive = new bool[3];
        private GameObject[] onBaseObj = new GameObject[3];
        private bool bPressAvail = false;

        public bool bUpdateNeed = false;

        public bool bActiveAvailble;

       
        // Use this for initialization
        void Start()
        {
            bActiveAvailble = true;
            for (int i = 0; i < 3; i++)
            {
                pressActive[i] = false;
                onBaseObj[i] = baseObj[i].transform.Find("onbase").gameObject;
            }
        }

        


        //초기화
        public void Init(BallPlayManager _manager)
        {
            this.manager = _manager;
            bUpdateNeed = true;
            
            //intWalkButton.SetActive(!manager.bMyTurn); //이게 진짜였는데 없어짐
            intWalkButton.SetActive(false);     //영원히

            for (int i = 0; i < 3; i++)
            {
                UISprite cur = onBaseObj[i].transform.Find("steal").gameObject.GetComponent<UISprite>();
                if (manager.bMyTurn == true)
                {
                    cur.spriteName = "steal_1";
                }
                else
                {
                    cur.spriteName = "pickoff_1";
                }
            }
        }

        public void UpdateState()
        {
            if (bUpdateNeed == true)
            {
                for (int i = 0; i < 3; i++)
                {
                    pressActive[i] = false;
                    if (manager.field.run.bOnBase[i] == true)
                    {
                        onBaseObj[i].SetActive(true);                        
                        setRunner(onBaseObj[i], manager.field.run.getRunner(i));
                    }
                    else
                    {
                        onBaseObj[i].SetActive(false);
                    }
                }
            }
        }


        //활성화
        public void SetActive(bool bActive, bool fade)
        {
            //2사만루 모드 비활성화
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut) return;

            if ((manager.field.run.bOnBase[0] == true || manager.field.run.bOnBase[1] == true || manager.field.run.bOnBase[2] == true)
                && bActiveAvailble == true)
                //&& Mode.bAutoPlay == false)
            {
                UIPanel panel = GetComponent<UIPanel>();
                if (bActive == true)
                {                 
                    bPressAvail = true;
                    _active.SetActive(true);
                    panel.alpha = 1.0f;
                }
                else
                {
                    if (fade == true)
                    {
                        StartCoroutine(deActive(panel));
                    }
                    else
                    {
                        _active.SetActive(false);
                    }
                }
            }
            else
            {
                _active.SetActive(false);
            }

        }

        private IEnumerator deActive(UIPanel panel)
        {
            bPressAvail = false;
            float alpha = 1.0f;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                alpha -= 0.015f;
                panel.alpha = alpha;
                if (alpha < 0)
                {
                    break;
                }
            }
            _active.SetActive(false);
        }


        //주자 정보
        private void setRunner(GameObject runnerObj, Runner runner)
        {
            runnerObj.GetComponent<UISprite>().spriteName = "runnercon_onbase";
            runnerObj.transform.Find("skillIcon").gameObject.SetActive(false);
            runnerObj.transform.Find("light").gameObject.SetActive(false);
            int overRallValue = (runner.pRunner.getSpeed() / 10);
            UILabel overRallLabel = runnerObj.transform.Find("overall").gameObject.GetComponent<UILabel>();
            Color overRallColor = new Color(0.38f, 0.45f, 0.84f); //최저
            if (overRallValue >= 100) overRallColor = new Color(0.74f, 0.15f, 0.89f);
            else if (overRallValue >= 80) overRallColor = new Color(0.96f, 0.16f, 0.16f);
            else if (overRallValue >= 60) overRallColor = new Color(0.16f, 0.58f, 1);
            overRallLabel.color = overRallColor;
            overRallLabel.text = overRallValue.ToString();
            runnerObj.transform.Find("steal").gameObject.GetComponent<UISprite>().spriteName = (manager.bMyTurn == true ? "steal_1" : "pickoff_1");
         
            //도루 스킬
            if (runner.pRunner.skillAvailable(SkillIndex.RunnerStealMaster) == true)
            {
                runnerObj.transform.Find("skillIcon").gameObject.SetActive(true);
            }

        }

        //버튼 활성화 여부
        private void setState(GameObject runnerObj)
        {
            runnerObj.GetComponent<UISprite>().spriteName = "runnercon_steal";
            runnerObj.transform.Find("light").gameObject.SetActive(true);
            runnerObj.transform.Find("steal").gameObject.GetComponent<UISprite>().spriteName = (manager.bMyTurn == true ? "steal_2" : "pickoff_2");
        }


        //////////////////////////////////////////////////////////////////////////////////////
        //버튼 메세지
        //////////////////////////////////////////////////////////////////////////////////////
        public void pushFirstBase()
        {
            ////Debug.Log("===================>>pushFirstBase"); //도루
            if (Mode.bAutoPlay == false)
            {
                if (bPressAvail == true)
                {
                    //자동 플레이시 해당 사항 없음
                    if (manager.bMyTurn == true)
                    {
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW)
                        {
                            if (Mode.bPvpMode == true)
                            {
                                //PVP 모드에서 도루 정보
                                PvpManager.GetInstance().SendStealInfo(FieldParm.FIRSTBASE_INDEX);
                            }

                            //버튼설정 : 1루도루시 다음 베이스 까지 고려
                            setState(onBaseObj[FieldParm.FIRSTBASE_INDEX]);
                            if (manager.field.run.bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                            {
                                setState(onBaseObj[FieldParm.SECONDBASE_INDEX]);
                                if (manager.field.run.bOnBase[FieldParm.THIRDBASE_INDEX] == true)
                                {
                                    //ControlBattingUI.SetSqueezeButtonOn();
                                    setState(onBaseObj[FieldParm.THIRDBASE_INDEX]);
                                }
                            }
                            //도루상태 세팅
                            setBaseSteal(FieldParm.FIRSTBASE_INDEX);
                            bUpdateNeed = true;
                        }
                    }
                    else
                    {
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW_READY)
                        {
                            /*if (Mode.bPvpMode == true)
                            {
                                bActiveAvailble = false; //한번견제후 견제UI 없앰
                                //PVP 모드에서 견제 정보
                                PvpManager.GetInstance().SendPickOffInfo(FieldParm.FIRSTBASE_INDEX);
                            }*/
                            if (Mode.bPvpMode433 == true) return;
                            ////Debug.Log("===================>>push Pick Off state : " + manager.playState);
                            //버튼설정
                            setState(onBaseObj[FieldParm.FIRSTBASE_INDEX]);
                            //견제상태
                            setPickoff(FieldParm.FIRSTBASE_INDEX);
                            bUpdateNeed = true;
                        }
                        
                    }
                }
            }
        }




        public void pushSecondBase()
        {
            ////Debug.Log("===================>>pushSecondBase");
            if (Mode.bAutoPlay == false)
            {
                if (bPressAvail == true)
                {                    
                    //자동 플레이시 해당 사항 없음
                    if (manager.bMyTurn == true)
                    {
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW)
                        {                            
                            if (Mode.bPvpMode == true)
                            {
                                //PVP 모드에서 도루 정보
                                PvpManager.GetInstance().SendStealInfo(FieldParm.SECONDBASE_INDEX);
                            }

                            //버튼설정
                            setState(onBaseObj[FieldParm.SECONDBASE_INDEX]);
                            if (manager.field.run.bOnBase[FieldParm.THIRDBASE_INDEX] == true)
                            {
                                //ControlBattingUI.SetSqueezeButtonOn();
                                setState(onBaseObj[FieldParm.THIRDBASE_INDEX]);
                            }
                            //도루상태 세팅
                            setBaseSteal(FieldParm.SECONDBASE_INDEX);
                            bUpdateNeed = true;
                        }
                    }
                    else
                    {
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW_READY)
                        {
                            /*if (Mode.bPvpMode == true)
                            {
                                bActiveAvailble = false; //한번견제후 견제UI 없앰
                                //PVP 모드에서 견제 정보
                                PvpManager.GetInstance().SendPickOffInfo(FieldParm.SECONDBASE_INDEX);
                            }*/
                            if (Mode.bPvpMode433 == true) return;
                            //버튼설정
                            setState(onBaseObj[FieldParm.SECONDBASE_INDEX]);
                            //견제상태
                            setPickoff(FieldParm.SECONDBASE_INDEX);
                            bUpdateNeed = true;



                        }                        
                    }
                }
            }
        }



        public void pushThirdBase()
        {
            ////Debug.Log("===================>>pushThirdBase");
            if (Mode.bAutoPlay == false)
            {                
                if (bPressAvail == true)
                {                    
                    //자동 플레이시 해당 사항 없음
                    if (manager.bMyTurn == true)
                    {
                        //ControlBattingUI.SetSqueezeButtonOn();                        
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW)
                        {
                            if (Mode.bPvpMode == true)
                            {
                                //PVP 모드에서 도루 정보
                                PvpManager.GetInstance().SendStealInfo(FieldParm.THIRDBASE_INDEX);
                            }

                            //버튼설정
                            setState(onBaseObj[FieldParm.THIRDBASE_INDEX]);
                            //도루상태 세팅
                            setBaseSteal(FieldParm.THIRDBASE_INDEX);

                            bUpdateNeed = true;
                        }
                    }
                    else
                    {
                        if (manager.playState == PlayState.PLAY_BATTING_VIEW_READY)
                        {
                            /*if (Mode.bPvpMode == true)
                            {
                                bActiveAvailble = false; //한번견제후 견제UI 없앰
                                //PVP 모드에서 견제 정보
                                PvpManager.GetInstance().SendPickOffInfo(FieldParm.THIRDBASE_INDEX);
                            }*/
                            if (Mode.bPvpMode433 == true) return;
                            //버튼설정
                            setState(onBaseObj[FieldParm.THIRDBASE_INDEX]);
                            //견제상태
                            setPickoff(FieldParm.THIRDBASE_INDEX);

                            bUpdateNeed = true;

                        }
                        
                    }
                }
            }
        }


        /// <summary>
        /// 도루 상태 세팅
        /// </summary>
        /// <param name="target"></param>
        public void setBaseSteal(int target)
        {
            //if (Mode.bPvpMode == true) Random.InitState(PvpManager.RandomSeed);

            manager.field.run.myControlSteal(target);
        }

        /// <summary>
        /// 견제상태 세팅
        /// </summary>
        /// <param name="target"></param>
        public void setPickoff(int target)
        {
            //if (Mode.bPvpMode == true) Random.InitState(PvpManager.RandomSeed);

            manager.field.run.set_Steal_Pickoff(target, true);
            manager.field.setPickOff(target);
        }

    }
}
