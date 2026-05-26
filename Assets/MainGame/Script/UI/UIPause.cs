using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class UIPause : MonoBehaviour
    {
        public GameObject _active;
        public GameObject _waitActive;

        public GameObject normal;
        public GameObject pvpObj;


        public UILabel waitTimeLabel;

        private BallPlayManager manager;
        private int num;        
        private int curRemainTime;
        
        void Awake()
        {
            num = 3;
        }


        private IEnumerator pauseTimer;
        public bool SetPause(BallPlayManager _manager)
        {
            if (Mode.gameMode == Mode.GamePlayMode.NineInningTwoOut)
            {
                Util.Load("MainGame/prefabs/gameUI/QuitPopupPrefab", transform.parent, Vector3.zero).GetComponent<UIQuit>().init(_manager);
            }
            else
            {
                curRemainTime = 10;
                if (Mode.gameMode == Mode.GamePlayMode.Pvp)
                {
                    if (num <= 0)
                    {
                        return false;
                    }
                    pvpObj.SetActive(true);
                    IngameUI.GetEmoticonChatting().chattingDisable();       //채팅창 닫음
                    IngameUI.GetScoreBoard().SetPitchTimerActive(false);    //피치 타이머 닫음
                    if (Mode.bPvpMode == true)
                    {
                        PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChangeWait);
                    }
                    num--;
                    pvpObj.transform.Find("count").GetComponent<UILabel>().text = num + "/3";

                    pauseTimer = pauseTimerSetting();
                    StartCoroutine(pauseTimer);
                }
                else
                {
                    pauseTimer = null;
                    normal.SetActive(true);
                }
                manager = _manager;
                _active.SetActive(true);
                TweenAlpha.Begin(gameObject, 0.3f, 1);
            }
            return true;
        }


        public void pressChange()
        {            
            Debug.Log("pressChange");
            IngameUI.GetPlayerChangeUI().InitPlayerChangeUI(manager, curRemainTime);
            _active.SetActive(false);
            if (pauseTimer != null) StopCoroutine(pauseTimer);
        }


        public void pressExit()
        {
            
        }


        public void pressContinue()
        {
            Debug.Log("pressContinue");
            Mode.bPauseGame = false;
            manager.pitcher.setResume();
            _active.SetActive(false);

            if (Mode.bPvpMode == true)
            {
                PvpManager.GetInstance().SendQuickGameReplyInfo(ApplyInfo.ChangeFinish);
                if (manager.bMyTurn == false) IngameUI.GetScoreBoard().SetPitchTimerActive(true); //피치 타이머 재가동
            }
            
            if (pauseTimer != null) StopCoroutine(pauseTimer);
        }

        private IEnumerator timer;
        public void SetChangeWait(bool bActive)
        {            
            if (bActive == true)
            {
                _waitActive.gameObject.SetActive(true);
                IngameUI.GetEmoticonChatting().chattingDisable(); //채팅창 닫음
                if (timer != null)
                {
                    StopCoroutine(timer);
                    timer = null;
                }
                timer = timerSetting();
                StartCoroutine(timer);
            }
            else
            {
                if (timer != null)
                {
                    StopCoroutine(timer);
                    timer = null;
                }
                _waitActive.gameObject.SetActive(false);
            }
            
        }


        private IEnumerator pauseTimerSetting()
        {
            curRemainTime = 10;

            while (curRemainTime >= 0)
            {
                yield return new WaitForSeconds(1.0f);
                curRemainTime--;
            }
            curRemainTime = 0;
            pressContinue();
        }

        private IEnumerator timerSetting()
        {
            int time = 10;

            while (time >= 0)
            {
                waitTimeLabel.text = "대기시간 " + time + "초";
                yield return new WaitForSeconds(1.0f);
                time--;
            }
            time = 0;

            //Mode.bPauseGame = false;
            //manager.pitcher.setResume();
            //_waitActive.SetActive(false);
        }
        
    }
}