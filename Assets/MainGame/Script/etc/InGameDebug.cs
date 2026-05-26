using UnityEngine;
using System.Collections;

public class InGameDebug {

    //나중에 지워야되는 놈들
    public static int _TOP_INNING_INDEX = 1;
    public static int _BOTTOM_INNING_INDEX = 2;
    public static bool _NOSOUND = false;

#if _Test_Local
    
    /////////////////////////////////////////////////////////////////////////
    //로컬 테스트시
    /////////////////////////////////////////////////////////////////////////
    //홈 어웨이
    public static bool MYHOME = true;

#endif

#if _Local_Balance
    /////////////////////////////////////////////////////////////////////////
    //로컬 밸런스시
    /////////////////////////////////////////////////////////////////////////
    public static bool _SPECIFIC_FLY_TEST = false;

    public static bool _SPECIFIC_GROUNDER_TEST = false;

    public static bool _SPECIFIC_LINER_TEST = false;

    //오직 배팅뷰 카메라
    public static bool _ONLY_BATTINGVIEW_CAMERA = false;

    //교체 없음
    public static bool _NO_CHANGE_PLAYER = false;

    //아웃카운트 증가 안함
    public static bool _NO_OUT_COUNT = false;

    //투아웃 테스트
    public static bool _TWO_OUT_TEST = false;

    //카운트 세팅 테스트
    public static bool _BALL_COUNT_SETTING = false;
    
    //액티브 항상 온
    public static bool _ALWAYS_SKILL_ON = false;

    //스킬 제한없음
    public static bool _SKILL_UNLIMITED = false;
            
    //도루 결과 결정
    public static bool _STEAL_RESULT_TEST = false;

    //무조건 도루
    public static bool _ALWAYS_STEAL = false;

    //무조건 오버런
    public static bool _ALWAYS_OVERRUN = false;

    //딜레이 스틸 상황시 도루
    public static bool _DELAY_STEAL_CASE = false;

    //무조건 견제
    public static bool _ALWAYS_PICKOFF = false;

    public static bool _ALWAYS_LASER_PICKOFF_HAPPEN = false;

    //무조건 포구 에러
    public static bool _ALWAYS_CATCH_ERROR = false;

    //캐치에러시 펌블만
    public static bool _ALWAYS_ERROR_FUMBLE = false;

    //무조건 송구 에러
    public static bool _ALWAYS_THROW_ERROR = false;


    
    //번트 테스트
    public static bool _BUNT_TEST = false;   //

    //번트 결과 
    public static bool _BUNT_SUCCESS = false;   //


    //이벤트를 거치지 않고 게임에 돌입
    public static bool EVENT_SKIP_MODE = false;

    //에러를 더 잘나오게 하는 
    public static bool CUSTOM_ERROR_MODE = false;


    //바로 결과로 돌입
    public static bool END_INNING_DIRECT_RESULT = false; //한 이닝만 끝나고 결과로

    public static bool GOODBYE_HIT_DIRECT_RESULT = false;    //한타자 끝내고 끝내기 상태로

    

#endif

#if _Skill_Display
    //연출테스트용
    public static BaseBall.BallPlay.pSkillDisplay PitcherSkill;
    public static BaseBall.BallPlay.bSkillDisplay BitcherSkill;
#endif

}
