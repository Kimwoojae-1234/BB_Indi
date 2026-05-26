using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
namespace BaseBall.BallPlay
{
    public class UIChatting : MonoBehaviour
    {
        public GameObject origin;   //채팅창 좌표 원점

        public SkeletonAnimation leftSkel;//, 
        public SkeletonAnimation rightSkel;

        public SkeletonAnimation[] button;

        public GameObject toggleButton;

        private BallPlayManager manager;

        /// <summary>
        /// 버튼을 누를 수 있는지 여부
        /// </summary>
        private bool bSelectAvailable = true;

        /// <summary>
        /// 토글 여부
        /// </summary>
        private bool bToggleAvailable = true;

        /// <summary>
        /// 채팅창 활성화 여부
        /// </summary>
        private bool bChatActive = false;


        /// <summary>
        /// 필드에서 채팅창 켜져있는 여부
        /// </summary>
        private bool bFieldChatting;


        private IEnumerator checkTime = null;
        private bool bAlphBlend = false;


        private string emojiMessage = null;

        private void Awake()
        {
            pvpmanager.OnEmoji += OnEmoji;
            emojiMessage = null;
        }

        private void Update()
        {
            if(emojiMessage != null)
            {
                otherEmoticon(emojiMessage);
                emojiMessage = null;
            }
        }

        //이모지 이벤트
        private void OnEmoji(string message)
        {
            //otherEmoticon(message);
            emojiMessage = message;
        }


        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="_manager"></param>
        public void Init(BallPlayManager _manager)
        {
            gameObject.SetActive(true);
            manager = _manager;

            origin.transform.localPosition = new Vector3(0, -500, 0);
            bChatActive = false;
            bSelectAvailable = true;
            bToggleAvailable = true;

            bFieldChatting = true;

            checkTime = null;
            bAlphBlend = false;
            
        }

        private void resetTimer()
        {
            setBlend(origin.GetComponent<UIPanel>(), 1.0f);
            if (checkTime != null) StopCoroutine(checkTime);
            checkTime = setCheckTime();
            StartCoroutine(checkTime);
        }

        /// <summary>
        /// 아이콘 선택 버튼 이벤트
        /// </summary>
        /// <param name="obj"></param>
        public void selectIcon(GameObject obj)
        {
            if (bSelectAvailable == true)
            {
                if (Mode.bSimulationQuickPlay == true)
                {
                    resetTimer();
                }

                UITweener tween = obj.transform.Find("light").GetComponent<UITweener>();
                tween.gameObject.SetActive(true);
                tween.ResetToBeginning();
                tween.PlayForward();

                string name = obj.name;

                bSelectAvailable = false;

                //Debug.Log("right = " + rightSkel);
                float scaleX = 150;
                float scaleY = 150;
                if (Mode.bSimulationQuickPlay == false)
                {
                    //액션 모드시
                    if (manager.playState == PlayState.PLAY_FIELDING_VIEW)
                    {
                        //필드뷰시
                        //scaleX = scaleY = 150;
                        leftSkel.transform.localPosition = new Vector3(-628, 101, 0);
                    }
                    else
                    {
                        //배팅뷰시
                        leftSkel.transform.localPosition = new Vector3(-350, 70, 0);
                    }
                }
                else
                {
                    //시뮬모드시
                    leftSkel.transform.localPosition = new Vector3(-240, -195, 0);
                }
                leftSkel.transform.localScale = new Vector3(scaleX, scaleY, 100);

                startAnim(leftSkel, name + "_l");


                /*
                if (Mode.bPvpMode == true)
                {
                    PvpManager.GetInstance().SendMessage(name);
                }*/

                if (Mode.bPvpMode433 == true)
                {
                    pvpmanager.Get().SendEmojiInfo(name);
                }


                Invoke("buttonInit", 2.0f); //1초후에 누를 수 있음

            }
        }

        
        private IEnumerator setCheckTime()
        {
            bAlphBlend = false;
            /*yield return new WaitForSeconds(3.0f);
            bAlphBlend = true;
            UIPanel panel = origin.GetComponent<UIPanel>();

            float alpha = 1.0f;
            while (alpha > 0.65f)
            {
                alpha -= 0.02f;
                setBlend(panel, alpha);
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(6.5f);*/


            yield return new WaitForSeconds(5.0f);
            chattingDisable();
        }


        private void setBlend(UIPanel panel, float alpha)
        {
            panel.alpha = alpha;
            for(int i = 0; i < button.Length; i++)
            {
                button[i].skeleton.A = alpha;
            }
        }


        /// <summary>
        /// 네트워크로 부터 받아온 상대방 메세지
        /// </summary>
        /// <param name="message"></param>
        public void otherEmoticon(string message)
        {
            float scaleX = 150;
            float scaleY = 150;
            //Debug.Log("message = " + message);
            //Debug.Log("right = " + rightSkel);
            if (Mode.bSimulationQuickPlay == false)
            {
                //액션 모드시
                if (manager.playState == PlayState.PLAY_FIELDING_VIEW)
                {
                    //필드뷰시
                    //scaleX = scaleY = 150;
                    rightSkel.transform.localPosition = new Vector3(628, 101, 0);
                }
                else
                {
                    //배팅뷰시
                    rightSkel.transform.localPosition = new Vector3(350, 70, 0);
                }
                
            }
            else
            {
                //시뮬 모드시
                rightSkel.transform.localPosition = new Vector3(240, -195, 0);
            }
            rightSkel.transform.localScale = new Vector3(scaleX, scaleY, 100);
            startAnim(rightSkel, message + "_r");

        }

        /// <summary>
        /// 채팅 토글 버튼 선택
        /// </summary>
        public void selectChattingToggle()
        {
            if (bToggleAvailable == true)
            {
                //Debug.Log("============================>>채팅 토글");
                if (bChatActive == true)
                {
                    if (Mode.bSimulationQuickPlay == false)
                    {
                        if (manager.playState == PlayState.PLAY_FIELDING_VIEW) bFieldChatting = false;
                    }   
                    chattingDisable();
                }
                else
                {                    
                    if (Mode.bSimulationQuickPlay == false)
                    {
                        if (manager.playState == PlayState.PLAY_FIELDING_VIEW) bFieldChatting = true;
                    }
                    else
                    {
                        resetTimer();
                    }
                    chattingEnable();
                }
                bToggleAvailable = false;
                Invoke("toggleInit", 0.2f);
            }
        }


        public void chattingDisable(bool bToggle = false)
        {
            bForceDisable = false;
            TweenPosition.Begin(origin, 0.2f, new Vector3(0, -500, 0));
            bChatActive = false;
            bSelectAvailable = false;
            if (bToggle == true)
            {
                toggleActive(false);
            }
        }

        /// <summary>
        /// 강제
        /// </summary>
        /// <param name="bToggle"></param>
        bool bForceDisable = false;
        public void forceChatDisable(bool bToggle = false)
        {
            if (bChatActive == true)
            {
                chattingDisable(bToggle);
                bForceDisable = true;
            }
            else
            {
                if (bToggle == true)
                {
                    toggleActive(false);
                }
            }
        }

        public void chattingEnable(bool bToggle = false)
        {            
            TweenPosition.Begin(origin, 0.2f, new Vector3(0, -360, 0));
            bChatActive = true;
            bSelectAvailable = true;
            if (bToggle == true)
            {
                toggleActive(true);
            }
        }


        public void forceChatEnable()
        {
            if (bForceDisable == true)
            {
                chattingEnable(true);
            }
            else
            {
                toggleActive(true);
            }
        }


        public void fieldviewSetting()
        {
            if (bFieldChatting == true)
            {                
                chattingEnable(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bActive"></param>
        public void toggleActive(bool bActive)
        {
            toggleButton.SetActive(bActive);
        }


        public void simulResetTimer()
        {
            if (bChatActive == true)
            {
                resetTimer();
            }
        }
        

        /// <summary>
        /// 버튼 초기화
        /// </summary>
        private void buttonInit()
        {
            bSelectAvailable = true;
        }

        /// <summary>
        /// 토글버튼 초기화
        /// </summary>
        private void toggleInit()
        {
            bToggleAvailable = true;
        }

        /// <summary>
        /// 애니메이션
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="strAnim"></param>
        private void startAnim(SkeletonAnimation anim, string strAnim)
        {
            anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, strAnim, false);
        }


    }
}