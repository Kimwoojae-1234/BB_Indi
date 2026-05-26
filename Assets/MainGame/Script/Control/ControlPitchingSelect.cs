using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class ControlPitchingSelect : MonoBehaviour
    {
        public GameObject _active;
        public pitchingSelectButton[] button;


        private BallPlayManager manager = null;
        private PitchingArsenal selectBallType;
        private int totalSlotNum;


        public void Init(BallPlayManager _manager)
        {
            manager = _manager;
        }


        public void SetActive(bool bActive)
        {
            if (bActive == true)
            {
                if (_active.activeSelf == false)
                {
                    if (Mode.bPvpMode433 == true)
                    {
                        //채팅창을 닫는다.
                        IngameUI.GetEmoticonChatting().chattingDisable();
                        IngameUI.GetScoreBoard().SetPitchTimerActive(true); //PVP모드에서 피치 타이머

                        //타자쪽 타이머 설정
                        pvpmanager.Get().SendPitchTimer();

                    }
                    _active.SetActive(true);
                    PitcherBallInit();
                }
            }
            else
            {
                _active.SetActive(false);
            }
        }

        public void PitcherBallInit()
        {
            //bUserSelect = false;

            CPlayer pitcher = manager.pitcher.pPitcher;
            PitchingArsenal[] ballType = pitcher.getBallType();

            int index = 0;
            int max = 5;

            for (int i = 0; i < max; i++) button[i].gameObject.SetActive(false);

            for (int i = 0; i < max; i++)
            {
                if (ballType[i] != PitchingArsenal.NONE)
                {
                    button[index].gameObject.SetActive(true);
                    button[index].setInit(pitcher, ballType[i], index);
                    index++;
                }
            }
            totalSlotNum = index;


        }

        /*
        private IEnumerator aiPitching;
        private bool bUserSelect = false;
        private IEnumerator startAiPitching()
        {
            yield return new WaitForSeconds(5);
            //Debug.Log("===============>>bUserSelect = " + bUserSelect);
            if (bUserSelect == false)
            {
                
                manager.pitcher.aiCourseSelect();
                PvpManager.GetInstance().SendPitchingInfo();
                yield return new WaitForSeconds(1);
                SetActive(false);
                yield return new WaitForSeconds(1.0f);
                manager.pitcher.getSign();//0.5f);
                StartCoroutine(manager.setBattingViewState(0.01f));
            }
        }*/


        public bool autoBallSelect()
        {
            if (_active.activeSelf == true)
            {
                PitchingArsenal[] ballType = manager.pitcher.pPitcher.getBallType();
                int slot = Random.Range(0, totalSlotNum);
                manager.pitcher.setBallSelect(ballType[slot]);
                for (int i = 0; i < totalSlotNum; i++)
                {
                    button[i].setRelease(slot, i);
                }
                if (manager.bMyTurn == false)
                {
                    Debug.Log("========================>>> 오토볼 셀렉트!!");
                    IngameUI.GetControlRunner().SetActive(false, true);
                    if (Mode.bPvpMode433 == true) pvpmanager.Get().SendPitchSelect();
                }
                return true;
            }
            else
            {
                return false;
            }
        }



        public void SetBallType(int slot, PitchingArsenal selectBall)
        {
            /*살려살려
            if (Mode.bPvpMode == true)
            {
                if (manager.bMyTurn == false)
                {
                    bUserSelect = true;
                    if(aiPitching!=null)
                    {
                        StopCoroutine(aiPitching);
                    }     
                }
            }*/
            if (Mode.gameMode == Mode.GamePlayMode.Pvp)
            {
                IngameUI.GetEmoticonChatting().forceChatDisable(true);   //볼선택시 채팅창 강제닫음
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.PitchSelect);
            }
            else if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                IngameUI.GetEmoticonChatting().forceChatDisable(true);   //볼선택시 채팅창 강제닫음
                //PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.PitchSelect);
            }

            IngameUI.GetScoreBoard().TopUIActive(false);
            pushButton(slot);
            selectBallType = selectBall;
            manager.pitcher.setBallSelect(selectBall); //구종 선택
        }


        private void pushButton(int selected)
        {         
            StartCoroutine(setActiveDelay(selected));
        }

        private IEnumerator setActiveDelay(int selected)
        {
            for (int i = 0; i < totalSlotNum; i++)
            {
                button[i].setRelease(selected, i);
            }
            yield return new WaitForSeconds(0.5f + 0.45f);
            
            if (manager.bMyTurn == false)
            {
                //Debug.Log("========================>>> 볼 셀렉트!!");
                IngameUI.GetControlRunner().SetActive(false, true);
                if (Mode.bPvpMode433 == true) pvpmanager.Get().SendPitchSelect();
            }                   
            yield return new WaitForSeconds(0.5f);
            manager.bReadyFinish2 = true;    
            SetActive(false);
        }

        public PitchingArsenal GetSelectBall()
        {
            return selectBallType;
        }


    }
}