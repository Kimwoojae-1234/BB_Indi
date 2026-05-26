using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BaseBall.BallPlay;

public class pvpmanager : MonoBehaviour {

    public const byte CONTACT_INFO = 0;
    public const byte EMOJI_INFO = 1;

    public const byte GAME_READY = 2;

    public const byte BATTER_SYNC = 10;

    public const byte PITCH_INFO1 = 11;
    public const byte PITCH_INFO2 = 12;
        
    public const byte NO_HIT_INFO = 13;
    public const byte BATTING_INFO = 14;
    public const byte FIELDING_INFO = 15;
    public const byte FIELD_RESULT_INFO = 16;
    public const byte ONEMORE_INFO = 18;

    public const byte FIELDING_SYNC = 20;
    public const byte THROWING_SYNC = 21;

    public const byte STEAL_INFO = 30;
    public const byte STEAL_INFO_RETURN = 31;

    public const byte PITCH_SELECT = 40;
    public const byte PITCH_TIMER = 41;


    public const byte FIELD_RESULT_SYNC = 50;

    public const byte RANDOMSEED_INFO = 100;



    private static pvpmanager Instance_;

    public int SKILL_TYPE = 0;
    public int ERROR_PER = 0;
    public int SP_PER = 0;

    public int FIELDING_STAT = 500;
    public int THROW_STAT = 500;
    public int RUNNING_STAT = 500;
    public int PITCHING_STAT = 500;
    public int BATTING_STAT = 500;
    public int CONTACT_STAT = 500;


    public PitchingArsenal pitch1 = PitchingArsenal.FASTBALL;
    public PitchingArsenal pitch2 = PitchingArsenal.CURVE;
    public PitchingArsenal pitch3 = PitchingArsenal.FORK;
    public PitchingArsenal pitch4 = PitchingArsenal.SLIDER;
    public PitchingArsenal pitch5 = PitchingArsenal.CHANGEUP;


    
    //유저 ID
    public string[] UserID = new string[2];
    //팀 코드
    public WebConnector.TeamCode[] teamCode = new WebConnector.TeamCode[2];
    //구장
    public int[] stadiumIndex = new int[2];
    //선발투수
    public int[] pitcherIndex = new int[2];
    //타자 타순
    public int[] batterOrder = new int[2];



    //이벤트
    //접속관련 이벤트
    public delegate void OnConnectEvent(PvpUserInfo info);
    public static event OnConnectEvent OnContact;
    //이모지 관련 이벤트
    public delegate void OnEmojiEvent(string massege);
    public static event OnEmojiEvent OnEmoji;
    //게임 플레이
    public delegate void OnPlayEvent(byte eventcode, object content);
    public static event OnPlayEvent OnPlay;


    public static PVP_Check pvpCheck = PVP_Check.None;

    // Use this for initialization
    void Awake () {
        Instance_ = this;

        //PhotonNetwork.OnEventCall += this.OnEvent;
        DontDestroyOnLoad(gameObject);
	}

    
    void OnDestroy()
    {
        //PhotonNetwork.OnEventCall -= this.OnEvent;
        Instance_ = null;
    }

    private void Start()
    {
        pvpCheck = PVP_Check.None;
        UserID[0] = UserID[1] = "Nobody";
        teamCode[0] = teamCode[1] = WebConnector.TeamCode.HANWHA;
        stadiumIndex[0] = stadiumIndex[1] = 1;
        pitcherIndex[0] = pitcherIndex[1] = 0;
        batterOrder[0] = batterOrder[1] = 3;
    }


    private void Update()
    {
        
    }

    /// <summary>
    /// 인스턴트 리턴
    /// </summary>
    /// <returns></returns>
    public static pvpmanager Get()
    {
        return Instance_;
    }

    /// <summary>
    /// event 수신
    /// </summary>
    /// <param name="eventcode"></param>
    /// <param name="content"></param>
    /// <param name="senderid"></param>
    private void OnEvent(byte eventcode, object content, int senderid)
    {
        if (eventcode == CONTACT_INFO)
        {
            //초기 컨택 정보
            GetInitInfo(content);
        }
        else if (eventcode == EMOJI_INFO)
        {
            //이모지 정보
            GetEmojiInfo(content);
        }
        else
        {
            GetPlayInfo(eventcode, content);
        }
    }

    /// <summary>
    /// 초기 전송 정보
    /// </summary>
    public void SendInitInfo()
    {
        //Debug.Log("초기 정보 상대에게 보냄");
        //정보 세팅
        stadiumIndex[0] = (Random.Range(0, 600) % 6) + 1;
        pitcherIndex[0] = Random.Range(0, 5);
        batterOrder[0] = Random.Range(0, 6);
        PvpUserInfo sendInfo = new PvpUserInfo();
        sendInfo.UserName = UserID[0];
        sendInfo.teamCode = teamCode[0];
        sendInfo.stadiumIndex = stadiumIndex[0];
        sendInfo.pitcherIndex = pitcherIndex[0];
        sendInfo.batterOrder = batterOrder[0];
        //json 시리얼라이즈
        string eventContent = Utils.JsonUtils.Serialize<PvpUserInfo>(sendInfo);
        //send
        //PhotonNetwork.RaiseEvent(CONTACT_INFO, (object)eventContent, true, null);
    }

    /// <summary>
    /// 이 정보를 수신하면 게임 시작
    /// </summary>
    /// <param name="eventcode"></param>
    /// <param name="content"></param>
    /// <param name="senderid"></param>
    private void GetInitInfo(object content)
    {
        //Debug.Log("초기 정보 수신");
        string recieveCode = (string)(content);
        PvpUserInfo recieveInfo = Utils.JsonUtils.Deserialize<PvpUserInfo>(recieveCode);
        UserID[1] = recieveInfo.UserName;
        teamCode[1] = recieveInfo.teamCode;
        stadiumIndex[1] = recieveInfo.stadiumIndex;
        pitcherIndex[1] = recieveInfo.pitcherIndex;
        batterOrder[1] = recieveInfo.batterOrder;
        pvpCheck = PVP_Check.None;
        //컨택 이벤트 호출
        if (OnContact!=null) OnContact(recieveInfo);        
    }


    /// <summary>
    /// 이모지 전송 정보
    /// </summary>
    public void SendEmojiInfo(string name)
    {
        //Debug.Log("Send Init Info");
        //정보 세팅
        //send
        //PhotonNetwork.RaiseEvent(EMOJI_INFO, (object)name, true, null);
    }

    /// <summary>
    /// 이모지 정보 수신
    /// </summary>
    /// <param name="eventcode"></param>
    /// <param name="content"></param>
    /// <param name="senderid"></param>
    private void GetEmojiInfo(object content)
    {
        //이모지 정보 수신
        string emojiInfo = (string)content;
        if(OnEmoji != null) OnEmoji(emojiInfo);
    }



    /// <summary>
    /// 게임 준비 완료 정보 보내기
    /// </summary>
    public void SendGameReadyInfo()
    {
        Debug.Log("게임 레디 보내기");
        string eventContent = "game_ready";
        //PhotonNetwork.RaiseEvent(GAME_READY, (object)eventContent, true, null);
    }

    /// <summary>
    /// 타자 동기화 정보 보내기
    /// </summary>
    /// <param name="manager"></param>
    public void SendBatterSync(BallPlayManager manager)
    {
        //Debug.Log("타자 동기화 발신");
        //Debug.Log("2222222222222222222");

        manager.Pvp_spcatch = (MyMath.Percent() < 45 ? true : false);
        manager.Pvp_spthrow = (MyMath.Percent() < 45 ? true : false);
        manager.Pvp_diving = (MyMath.Percent() < 45 ? true : false);
        manager.Pvp_hrsteal = (MyMath.Percent() < 45 ? true : false);

        PvpBatterSync batterSync = new PvpBatterSync();
        int randomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(randomSeed);
        int randomSeed2 = (int)(Time.time * 100f);

        //카운트
        batterSync.randSeed = randomSeed;
        batterSync.randSeed2 = randomSeed2;
        batterSync.ballCount = manager.nBallCount;
        batterSync.strikeCount = manager.nStrikeCount;
        batterSync.outCount = manager.nOutCount;

        //베이스
        //for (int i = 0; i < 3; i++) batterSync.bBaseOn[i] = manager.field.run.bOnBase[i];

        //특수능력 동기화
        batterSync.spcatch = manager.Pvp_spcatch;
        batterSync.spthrow = manager.Pvp_spthrow;
        batterSync.spdiving = manager.Pvp_diving;
        batterSync.sphrsteal = manager.Pvp_hrsteal;

        string eventContent = Utils.JsonUtils.Serialize<PvpBatterSync>(batterSync);
        //PhotonNetwork.RaiseEvent(BATTER_SYNC, (object)eventContent, true, null);

        //Debug.Log("발신 특수능력 발동상황 Pvp_spcatch =" + manager.Pvp_spcatch + "   Pvp_spthrow =" + manager.Pvp_spthrow + "   Pvp_diving =" + manager.Pvp_diving + "   Pvp_hrsteal =" + manager.Pvp_hrsteal);
    }

    /// <summary>
    /// 피칭 정보 보내기
    /// </summary>
    /// <param name="manager"></param>
    public void SendPitchInfo(Pitcher pitcher)
    {
        PvpPitchInfo pitchInfo = new PvpPitchInfo();

        //피치 정보
        pitchInfo.curBallSpeed = (int)pitcher.curBallSpeed;
        pitchInfo.selectBallType = (int)pitcher.selectedBallIndex;
        pitchInfo.userControlValue = (int)pitcher.userControlValue;
        //로케이션 정보

        //투수뷰
        /*float realX = pitcher.courseX2 + pitcher.preHenkaX;
        float realY = pitcher.courseY2 + pitcher.preHenkaY;
        //UnityEngine.Debug.Log("보내는 값 courseX = " + realX + "    ====  courseY = " + realY);
        pitchInfo.courseX = -realX * Zone.STRIKE_ZONE_WIDTH / Zone.STRIKE_ZONE_WIDTH_PV;
        pitchInfo.courseY = realY * Zone.STRIKE_ZONE_HEIGHT / Zone.STRIKE_ZONE_HEIGHT_PV;*/
        //UnityEngine.Debug.Log("보내는 변환된 값 courseX = " + courseX + "    ====  courseY = " + courseY);

        //타자뷰
        pitchInfo.courseX = pitcher.courseX2 + pitcher.preHenkaX;
        pitchInfo.courseY = pitcher.courseY2 + pitcher.preHenkaY;

        //폭투정보
        pitchInfo.bMissControl = pitcher.bMissControl;
        pitchInfo.hitByPitchStep = pitcher.hitByPitchStep;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpPitchInfo>(pitchInfo);
        //PhotonNetwork.RaiseEvent(PITCH_INFO1, (object)eventContent, true, null);
    }

    /// <summary>
    /// 피치인포2
    /// </summary>
    public void SendPitchInfo2(BallPlayManager manager)
    {
        PvpPitchInfo2 pitchInfo2 = new PvpPitchInfo2();

        //카운트 동기화 정보
        pitchInfo2.ballCount = manager.nBallCount;
        pitchInfo2.strikeCount = manager.nStrikeCount;
        pitchInfo2.outCount = manager.nOutCount;

        //에러정보
        for (int i = 0; i < 9; i++)
        {
            pitchInfo2.bCatchError[i] = manager.field.fielder[i].bCatchErrorFlag;
            pitchInfo2.bThrowError[i] = manager.field.fielder[i].bThrowErrorFlag;
        }

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpPitchInfo2>(pitchInfo2);
        //PhotonNetwork.RaiseEvent(PITCH_INFO2, (object)eventContent, true, null);
    }


    /// <summary>
    /// 피치 셀렉트
    /// </summary>
    public void SendPitchSelect()
    {
        //Debug.Log("==========>> 피치셀렉트 정보 전달");
        //보내기
        string eventContent ="select";
        //PhotonNetwork.RaiseEvent(PITCH_SELECT, (object)eventContent, true, null);
    }

    /// <summary>
    /// 피치 타이머 보내기
    /// </summary>
    public void SendPitchTimer()
    {
        //Debug.Log("==========>> 피치 타이머 정보 전달");
        //보내기
        string eventContent = "timer";
        //PhotonNetwork.RaiseEvent(PITCH_TIMER, (object)eventContent, true, null);
    }

    /// <summary>
    /// 노 힛 인포 보내기
    /// </summary>
    /// <param name="manager"></param>
    public void SendNoHitInfo(BallPlayManager manager, NoHitStatus type)
    {
        PvpNoHitInfo nohitInfo = new PvpNoHitInfo();
        //PvpNo
        nohitInfo.noHitType = type;
        nohitInfo.bWildPitch = manager.pitcher.bWildPitch;
        if (type == NoHitStatus.NoSwing)
        {
            nohitInfo.bStrikeCheck = manager.bStrikeCheck;
        }
        else if (type == NoHitStatus.HutSwing)
        {
            nohitInfo.TimingPoint = manager.batter.timingCheckPVP();
            nohitInfo.bStrikeCheck = true;
            //Debug.Log("체크스윙 여부==============>>" + manager.batter.bCheckSwingActivate);
            //nohitInfo.bCheckSwing = manager.batter.bCheckSwingActivate;
        }
        else if (type == NoHitStatus.CheckSwing)
        {
            nohitInfo.TimingPoint = BattingTiming.PERFECT;
            nohitInfo.bStrikeCheck = false;
        }
        else if(type == NoHitStatus.BuntSwing)
        {
            nohitInfo.bStrikeCheck = true;
        }


        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpNoHitInfo>(nohitInfo);
        //PhotonNetwork.RaiseEvent(NO_HIT_INFO, (object)eventContent, true, null);
    }


    /// <summary>
    /// 도루 정보 보내기
    /// </summary>
    /// <param name="result"></param>
    /// <param name="target"></param>
    public void SendStealInfo(BaseBall.BallPlay.SimulStealState result, int target)
    {
        Debug.Log("도루정보 발신 result = " + result + "   target = " + target);
        PvpStealInfo stealInfo = new PvpStealInfo();

        stealInfo.stealResult = result;
        stealInfo.stealTarget = target;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpStealInfo>(stealInfo);
        //PhotonNetwork.RaiseEvent(STEAL_INFO, (object)eventContent, true, null);
    }

    /// <summary>
    /// 도루정보 되돌려주기
    /// </summary>
    /// <param name="result"></param>
    /// <param name="target"></param>
    public void SendStealInfoReturn(BaseBall.BallPlay.SimulStealState result, int target)
    {
        //Debug.Log("도루리턴 정보 발신   target = " + target);
        PvpStealInfo stealInfo = new PvpStealInfo();

        stealInfo.stealResult = result;
        stealInfo.stealTarget = target;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpStealInfo>(stealInfo);
        //PhotonNetwork.RaiseEvent(STEAL_INFO_RETURN, (object)eventContent, true, null);
    }

    /// <summary>
    /// 배팅 정보 보내기
    /// </summary>
    /// <param name="field"></param>
    public void SendBattingInfo(Field field)
    {
        //Debug.Log("=============>>배팅인포 송신");
        PvpBattingInfo battingInfo = new PvpBattingInfo();
        //타구 정보

        battingInfo.ballPower = field.ballPower;
        battingInfo.angleZ = field.ball.firstAngleZ;  //field.ball.angleZ;
        battingInfo.angle = field.ball.firstAngle; //field.ball.angle;
        //Debug.Log("배팅인포 송신정보  ballPower = "+ battingInfo.ballPower+ "   angleZ = "+ battingInfo.angleZ + "    angle = " + battingInfo.angle);

        battingInfo.angleHookSlice = field.ball.angleHookSlice;
        battingInfo.bHookorSlice = field.ball.bHookorSlice;
        battingInfo.bTopSpin = field.ball.bTopSpin;
        
        //번트정보
        battingInfo.bBunt = field.batter.bBunt;
        //        manager.batter.bBuntHit = true;
        battingInfo.buntType = field.batter.buntType;
        battingInfo.buntResult = field.batter.buntResult;
        battingInfo.buntFielder = field.batter.buntFielder;

        //플라이볼
        for (int i = 0; i < 9; i++)
        {
            battingInfo.possibleDis[i] = field.manager.Pvp_possibleDis[i];
            battingInfo.distanceToBall[i] = field.manager.Pvp_distanceToBall[i];
        }

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpBattingInfo>(battingInfo);
        //PhotonNetwork.RaiseEvent(BATTING_INFO, (object)eventContent, true, null);
    }

    /// <summary>
    /// 랜덤 시드 동기화 용
    /// </summary>
    /// <param name="field"></param>
    public void SendRandomSeedInfo(int seed)
    {
        //Debug.Log("=============>>랜덤시드 송신");
        //보내기
        string eventContent = seed.ToString();
        //PhotonNetwork.RaiseEvent(RANDOMSEED_INFO, (object)eventContent, true, null);
    }

    /// <summary>
    /// 주루정보 보내기
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="oneMore"></param>
    /// <param name="moreSkill"></param>
    public void SendOnemorebaseInfo(int dst, bool oneMore, SimulOverrunState moreSkill)
    {
        //Debug.Log("=============>>한베이스 더 정보 송신");
        PvpOnemoreInfo oneMoreInfo = new PvpOnemoreInfo();

        //
        oneMoreInfo.dst = dst;
        oneMoreInfo.oneMore = oneMore;
        oneMoreInfo.moreSkill = moreSkill;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpOnemoreInfo>(oneMoreInfo);
        //PhotonNetwork.RaiseEvent(ONEMORE_INFO, (object)eventContent, true, null);
    }



    /// <summary>
    /// 필딩 싱크 정보 보내기
    /// </summary>
    /// <param name="manager"></param>
    public void SendFieldingSyncInfo(BallPlayManager manager)
    {
        //Debug.Log("필드 동기화 정보 발신");
        PvpFieldingSyncInfo fieldSyncInfo = new PvpFieldingSyncInfo();

        for(int i =0; i<9;i++)
        {
            fieldSyncInfo.groundTimeH[i] = manager.Pvp_GroundTimeH[i];
            fieldSyncInfo.groundTimeF[i] = manager.Pvp_GroundTimeF[i];
            //fieldSyncInfo.possibleDis[i] = manager.Pvp_possibleDis[i];
            //fieldSyncInfo.distanceToBall[i] = manager.Pvp_distanceToBall[i];
        }

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpFieldingSyncInfo>(fieldSyncInfo);
        //PhotonNetwork.RaiseEvent(FIELDING_SYNC, (object)eventContent, true, null);
    }

    /// <summary>
    /// 송구 동기화
    /// </summary>
    /// <param name="index"></param>
    /// <param name="target"></param>
    public void SendThrowingSyncInfo(int index, int target)
    {
        //Debug.Log("송구 동기화 정보 발신");
        PvpThrowingSyncInfo throwInfo = new PvpThrowingSyncInfo();
        throwInfo.index = index;
        throwInfo.target = target;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpThrowingSyncInfo>(throwInfo);
        //PhotonNetwork.RaiseEvent(THROWING_SYNC, (object)eventContent, true, null);
    }



    /// <summary>
    /// 필딩 결과 동기화 정보
    /// </summary>
    /// <param name="manager"></param>
    public void SendFieldResultSync(BallPlayManager manager)
    {
        //Debug.Log("필드 결과 동기화 정보 발신");
        PvpFieldResultSync resultSync = new PvpFieldResultSync();
        for(int i=0;i<3;i++)
        {
            resultSync.bOnBase[i] = manager.field.run.bOnBase[i];
        }
        resultSync.myScore = manager.nGameScore[1];
        resultSync.otherScore = manager.nGameScore[0];
        resultSync.outCount = manager.nOutCount;
        resultSync.bThreeOut = manager.bThreeOutChange;
        resultSync.bGoodBye = manager.bGoodByeHitCall;

        //보내기
        string eventContent = Utils.JsonUtils.Serialize<PvpFieldResultSync>(resultSync);
        //PhotonNetwork.RaiseEvent(FIELD_RESULT_SYNC, (object)eventContent, true, null);
        
    }






    /// <summary>
    /// 게임 플레이 정보 수신
    /// </summary>
    /// <param name="eventcode"></param>
    /// <param name="content"></param>
    private void GetPlayInfo(byte eventcode, object content)
    {
        if (OnPlay != null) OnPlay(eventcode, content);
    }



    

    /// <summary>
    /// 플레이게임
    /// </summary>
    public void PlayGame()
    {
        SKILL_TYPE = 1;
        ERROR_PER = 4;
        SP_PER = 40;

        FIELDING_STAT = 650;
        THROW_STAT = 750;
        RUNNING_STAT = 650;
        PITCHING_STAT = 850;
        BATTING_STAT = 750;
        CONTACT_STAT = 850;

        pitch1 = PitchingArsenal.FASTBALL;
        pitch2 = PitchingArsenal.SLURVE;
        pitch3 = PitchingArsenal.TWOSEAM;
        pitch4 = PitchingArsenal.SINKER;
        pitch5 = PitchingArsenal.CIRCLE;

        AsyncOperation async = SceneManager.LoadSceneAsync("MainLoading");
    }
}
