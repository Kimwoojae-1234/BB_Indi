using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BaseBall.BallPlay
{
    public class IngameUI : MonoBehaviour
    {
        public UIPanel mainPanel;

        /// <summary>
        /// 스코어보드 객체
        /// </summary>
        public UIScoreBoard scoreBoard;

        /// <summary>
        /// 플레이어 인포 객체
        /// </summary>
        public UIPlayerInfo playerInfo;

        /// <summary>
        /// 필드 콜 객체
        /// </summary>
        public UIFieldCall fieldCall;

        /// <summary>
        /// 배팅 콜 객체
        /// </summary>
        public UIBattingCall battingCall;

        /// <summary>
        /// 피치 존 UI 객체
        /// </summary>
        public PitchUI pitchZoneUI;

        /// <summary>
        /// 득점 디스플레이 객체
        /// </summary>
        public UIScoreShow scoreShow;

        /// <summary>
        /// 필드 UI객체
        /// </summary>
        public UIFieldUI fieldUI;

        /// <summary>
        /// 러너 컨트롤 객체
        /// </summary>
        public ControlRunner runnerControl;

        /// <summary>
        /// 투구 선택 객체
        /// </summary>
        public ControlPitchingSelect pitchingSelect;

        /// <summary>
        /// 이닝 체인지
        /// </summary>
        public UIChangeInning inningchange;

        /// <summary>
        /// 선수 체인지 UI
        /// </summary>
        public UIPlayerChange playerChangeUI;


        /// <summary>
        /// 공격쪽 스킬
        /// </summary>
        public skillUISetter offenseSkill;

        /// <summary>
        /// 수비쪽 스킬
        /// </summary>
        public skillUISetter defenseSkill;


        /// <summary>
        /// vs
        /// </summary>
        public vsUISetter vsSetter;


        //오프닝 이벤트
        public UIOpening opening;


        //스타팅 라인업 이벤트
        public UIStarting starting;

        
        //9회투아웃
        public UIWalkOff walkOff;


        //포즈 UI
        public UIPause pauseObj;

        //체인지 이벤트
        public UIChangeEvent changeEventObj;


        //이모티콘 채팅
        public UIChatting emoticonChatting;


        //컨펌 팝업
        public UI_PopupConfirm confirmPopup;


        private static IngameUI Instance_;

        void Awake()
        {
            Instance_ = this;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }

        /// <summary>
        /// 인스턴스
        /// </summary>
        /// <returns></returns>
        public static IngameUI GetInstance()
        {
            return Instance_;
        }

        /// <summary>
        /// 스코어보드 객체
        /// </summary>
        /// <returns></returns>
        public static UIScoreBoard GetScoreBoard()
        {
            return Instance_.scoreBoard;
        }

        /// <summary>
        /// 플레이어 인포 객체
        /// </summary>
        /// <returns></returns>
        public static UIPlayerInfo GetPlayerInfo()
        {
            return Instance_.playerInfo;
        }

        /// <summary>
        /// 필도 콜 객체
        /// </summary>
        /// <returns></returns>
        public static UIFieldCall GetFieldCall()
        {
            return Instance_.fieldCall;
        }

        /// <summary>
        /// 배팅콜 객체
        /// </summary>
        /// <returns></returns>
        public static UIBattingCall GetBattingCall()
        {
            return Instance_.battingCall;
        }


        /// <summary>
        /// 피치UI객체
        /// </summary>
        /// <returns></returns>
        public static PitchUI GetPitchUI()
        {
            return Instance_.pitchZoneUI;
        }



        /// <summary>
        /// 득점 디스플레이 객체
        /// </summary>
        public static UIScoreShow GetScoreShow()
        {
            return Instance_.scoreShow;
        }


        /// <summary>
        /// 필드 UI객체
        /// </summary>
        public static UIFieldUI GetFieldUI()
        {
            return Instance_.fieldUI;
        }


        /// <summary>
        /// 러너 컨트롤 객체
        /// </summary>
        public static ControlRunner GetControlRunner()
        {
            return Instance_.runnerControl;
        }

        /// <summary>
        /// 투구 선택 객체
        /// </summary>
        public static ControlPitchingSelect GetPitchingSelect()
        {
            return Instance_.pitchingSelect;
        }


        /// <summary>
        /// 이닝 체인지
        /// </summary>
        public static UIChangeInning GetInningChangeUI()
        {
            return Instance_.inningchange;
        }

        
        /// <summary>
        /// 오프닝
        /// </summary>
        /// <param name="manager"></param>
        public static void OpeningInit(BallPlayManager manager)
        {
            Instance_.opening.init(manager);
        }


        /// <summary>
        /// 스타팅 라인업
        /// </summary>
        /// <param name="manager"></param>
        public static void StartingLineup(BallPlayManager manager)
        {
            Instance_.starting.init(manager);
        }



        /// <summary>
        /// 9회2아웃 UI얻어오기
        /// </summary>
        /// <returns></returns>
        public static UIWalkOff GetWalkOffUI()
        {
            return Instance_.walkOff;
        }

        /// <summary>
        /// 9회2아웃 UI Active
        /// </summary>
        /// <returns></returns>
        public static void SetWalkOffActive(bool bActive)
        {
            Instance_.walkOff.gameObject.SetActive(bActive);
        }



        /// <summary>
        /// 공격쪽 스킬 UI
        /// </summary>
        /// <returns></returns>
        public static skillUISetter GetCpuSkillUI()
        {
            return Instance_.offenseSkill;
        }

        /// <summary>
        /// 수비쪽 스킬 UI
        /// </summary>
        /// <returns></returns>
        public static skillUISetter GetMySkillUI()
        {
            return Instance_.defenseSkill;
        }


        public static vsUISetter GetVsSkillUI()
        {
            return Instance_.vsSetter;
        }


        /// <summary>
        /// 포즈 UI 인스턴스
        /// </summary>
        /// <returns></returns>
        public static UIPause GetPauseUI()
        {
            return Instance_.pauseObj;
        }

        /// <summary>
        /// 체인지 이벤트 UI 인스턴스
        /// </summary>
        /// <returns></returns>
        public static UIChangeEvent GetChangeEventUI()
        {
            return Instance_.changeEventObj;
        }


        /// <summary>
        /// 선수 체인지 UI
        /// </summary>
        public static UIPlayerChange GetPlayerChangeUI()
        {
            return Instance_.playerChangeUI;
        }


        public static UIChatting GetEmoticonChatting()
        {
            return Instance_.emoticonChatting;
        }




        /// <summary>
        /// 동적 UI할당
        /// </summary>
        /// <param name="uiName"></param>
        public static void LoadDynamicUI(string uiName, float scale, float timeRemain, Vector3 pos)
        {
            GameObject uiObj = Util.Load("MainGame/prefabs/dynamicUI/" + uiName, Instance_.transform, pos);
            uiObj.transform.localScale = new Vector3(scale, scale, scale);
            Destroy(uiObj, timeRemain);
        }



        /// <summary>
        /// 원버튼 컨펌 팝업
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="callBack"></param>
        public static void SetConfirmPopupOnebutton(string title, string message, EventDelegate.Callback callBack = null)
        {
            Instance_.confirmPopup.SetIngameMode();
            Instance_.confirmPopup.SetPopup_OneBtn(title, message, callBack);
            Instance_.confirmPopup.gameObject.SetActive(true);
        }

        /// <summary>
        /// 투버튼 컨펌 팝업
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="callBack_Left"></param>
        /// <param name="callBack_Right"></param>
        public static void SetConfirmPopupTwobutton(string title, string message, EventDelegate.Callback callBack_Left = null, EventDelegate.Callback callBack_Right = null)
        {
            Instance_.confirmPopup.SetIngameMode();
            Instance_.confirmPopup.SetPopup_TwoBtn(title, message, "확인", "취소", callBack_Left, callBack_Right);
            Instance_.confirmPopup.gameObject.SetActive(true);
        }

        /// <summary>
        /// 컨펌 팝업 종료
        /// </summary>
        public static void ClosePopup()
        {
            Instance_.confirmPopup.gameObject.SetActive(false);
        }

    }
}