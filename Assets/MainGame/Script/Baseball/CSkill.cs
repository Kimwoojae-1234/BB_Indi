using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    [System.Serializable]
    public class CSkill
    {
        /// <summary>
        /// 투수 스킬 여부
        /// </summary>
        public bool bPitcherSkill;

        /// <summary>
        /// 스킬 아이디
        /// </summary>
        public int ID;

        /// <summary>
        /// 효과 아이디
        /// </summary>
        public SkillIndex effectIndex;

        /// <summary>
        /// 스킬의 레벨
        /// </summary>
        public int rank;

        

        /// <summary>
        /// 스킬의 발동 확률
        /// </summary>
        public int invokeRate;


        /// <summary>
        /// 범위형 스킬의 범위효과
        /// </summary>
        public int scopeRate;

        /// <summary>
        /// 스킬의 value형 효과
        /// </summary>
        public int effectValue;


        /// <summary>
        /// 중첩 카운트
        /// </summary>
        public int pileupCount;
        

#if _Test_Local
        
        public CSkill(int _ID, SkillIndex _effectIndex, bool bPitcher)//, 
            //Effect_InvokeCondition _invokeCondition = Effect_InvokeCondition.Field, 
            //Effect_Validity _effectValidity = Effect_Validity.Field, 
            //Restriction_Type _restriction = Restriction_Type.NoRestriction, 
            //int _restrictionCount = 100000, 
            //List<SkillIndex> counterList = null)
        {            
            bPitcherSkill = bPitcher;
            ID = _ID;
            effectIndex = _effectIndex;
            rank = 3;
            //invokeCondition = _invokeCondition;
            //effectValidity = _effectValidity;
            //restrictionCount = _restrictionCount;
            invokeRate = 100;// 30;// 50;// bPitcher ? 50 : 100;
            scopeRate = 40;//30;// 30;
            effectValue = 40;//30;// 20;
            pileupCount = 0;
            //restriction = _restriction;
            //counter = counterList;
        }

#else
        public CSkill(int _ID, SkillIndex _effectIndex, int _rank, bool bPitcher)
        {
            //분류
            bPitcherSkill = bPitcher;

            //기본값
            ID = _ID;


            effectIndex = _effectIndex;
            rank = _rank;
            
            //중첩값 초기화
            pileupCount = 0;

            //
            WebConnector.Skill curSkill = SimulParm.GetCommon().SkillsMap[ID];
            int? value1 = curSkill.invokeRate[rank - 1];
            int? value2 = curSkill.scopeRate[rank - 1];
            int? value3 = curSkill.value[rank - 1];
            invokeRate = (value1 == null ? 0 : (int)value1);
            scopeRate = (value2 == null ? 0 : (int)value2);
            effectValue = (value3 == null ? 0 : (int)value3);


            //테스트 시 invokeRate값을 조정해 확률조작가능
        }
#endif

    }

    public enum SkillID
    {
        None = -1,

        /// <summary>
        /// 제5의 내야수
        /// </summary>
        je_5_nea_ya_su = 10001,
        /// <summary>
        /// 견제왕
        /// </summary>
        gyeun_we_wang = 10002,
        /// <summary>
        /// 선두타자
        /// </summary>
        sun_du_ta_ja = 10003,
        /// <summary>
        /// 추격본능
        /// </summary>
        chu_gyeog_bon_neung = 10004,
        /// <summary>
        /// 소년가장
        /// </summary>
        so_nyun_ga_jang = 11104,
        /// <summary>
        /// 불꽃투혼
        /// </summary>
        bul_kkot_tu_hon = 10005,
        /// <summary>
        /// 강심장
        /// </summary>
        kang_sim_jang = 10006,
        /// <summary>
        /// 회심의일격
        /// </summary>
        hoe_sim_il_gyeog = 10007,
        /// <summary>
        /// 돌직구
        /// </summary>
        dol_jik_gu = 11107,
        /// <summary>
        /// 매혹
        /// </summary>
        mea_hog = 10008,
        /// <summary>
        /// 썬크림
        /// </summary>
        sun_cream = 11108,
        /// <summary>
        /// 뱀직구
        /// </summary>
        beam_jik_gu = 11208,
        /// <summary>
        /// 투수위압
        /// </summary>
        too_soo_wi_ab = 10009,
        /// <summary>
        /// 니느님
        /// </summary>
        ni_nu_nim = 11109,
        /// <summary>
        /// 강철어깨
        /// </summary>
        gang_chul_shoulder = 10010,
        /// <summary>
        /// 카리스마
        /// </summary>
        chrisma = 10011,        
        /// <summary>
        /// 닥터K
        /// </summary>
        doctor_k = 10012,
        /// <summary>
        /// 닥터광
        /// </summary>
        doctor_kwang = 11112,
        /// <summary>
        /// 필승의지
        /// </summary>
        pil_seung_eu_ji = 10013,



        /// <summary>
        /// 철벽수비
        /// </summary>
        chul_byuk_su_bi = 20001,
        /// <summary>
        /// 특급송구
        /// </summary>
        tueck_geup_song_gu = 20002,
        /// <summary>
        /// 평화송구
        /// </summary>
        pyung_hwa_song_gu = 21102,
        /// <summary>
        /// 쇠그물 수비
        /// </summary>
        seo_gu_mul_su_bi = 20003,
        /// <summary>
        /// 레이저 송구
        /// </summary>
        laser = 20004,
        /// <summary>
        /// 도발꾼
        /// </summary>
        do_bal_ggun = 20005,
        /// <summary>
        /// 풍기문란
        /// </summary>
        pung_gi_mul_ran = 21105,
        /// <summary>
        /// 갑드래곤
        /// </summary>
        gab_dragon = 21205,
        /// <summary>
        /// 수비형포수
        /// </summary>
        su_bi_hyung_po_su = 20006,
        /// <summary>
        /// 안방마님
        /// </summary>
        an_bang_ma_nim = 21106,
        /// <summary>
        /// 질주본능
        /// </summary>
        jil_ju_bon_neung = 20007,
        /// <summary>
        /// 바람의 아들
        /// </summary>
        baram_son = 21107,
        /// <summary>
        /// 주루센스
        /// </summary>
        ju_lu_sense = 20008,
        /// <summary>
        /// 매의눈
        /// </summary>
        mea_nun = 20009,
        /// <summary>
        /// 용의눈
        /// </summary>
        yong_nun = 21109,
        /// <summary>
        /// 송골매
        /// </summary>
        song_gol_mea =21209,
        /// <summary>
        /// 스나이퍼
        /// </summary>
        sniper = 21309,
        /// <summary>
        /// 타자위압
        /// </summary>
        ta_ja_ei_ab = 20010,
        /// <summary>
        /// 출근의 신
        /// </summary>
        chul_gun_sin = 21110,
        /// <summary>
        /// 소년장사
        /// </summary>
        so_nyun_jang_sa = 21210,
        /// <summary>
        /// 금강불괴
        /// </summary>
        gum_gang_bul_goe =21310,
        /// <summary>
        /// 신의위압
        /// </summary>
        god_ei_ab = 21410,
        /// <summary>
        /// 강습타구
        /// </summary>
        gang_seup_ta_gu = 20011,
        /// <summary>
        /// 타격기계
        /// </summary>
        batting_machine = 21111,
        /// <summary>
        /// 만세타법
        /// </summary>
        man_se_ta_bub = 21211,
        /// <summary>
        /// 찬스맨
        /// </summary>
        chance_man = 20012,
        /// <summary>
        /// 리틀쿠바
        /// </summary>
        little_cuba = 21112,
        /// <summary>
        /// 꽃범호
        /// </summary>
        flower_bunho = 21212,
        /// <summary>
        /// 번트의신
        /// </summary>
        bunt_sin = 20013,
        /// <summary>
        /// 용규놀이
        /// </summary>
        young_gyu_play = 21113,
        /// <summary>
        /// 뜬금포
        /// </summary>
        tteun_geum_po = 20014,
        /// <summary>
        /// 박뱅포
        /// </summary>
        park_bang_po = 21114,
        /// <summary>
        /// 빅보이
        /// </summary>
        big_boy = 21214,
        /// <summary>
        /// 국민타자
        /// </summary>
        guk_min_ta_ja = 21314

    }



  

    public enum SkillIndex
    {
        ///////////////////////////////////////////////
        //타자스킬
        ///////////////////////////////////////////////
        //철벽수비
        SpecialCatch = 200011,            //스페셜 캐치
        InfieldRange = 200012,            //수비반경  

        //특급송구
        SpecialThrow = 200021,            //스페셜 송구        

        //쇠그물수비
        DivingCatch = 200031,             //다이빙캐치
        OutfieldRange = 200032,           //수비반경
        HomerunSteal = 200033,            //홈런스틸

        //레이저송구
        Laser = 200041,                   //레이져송구

        //도발꾼
        CatcherMeatJil = 200051,          //도발꾼
        CatcherProvoke = 200052,          //도발

        //수비형포수
        CatcherBallBlocking = 200061,     //든든함
        CatcherSitThrow = 200062,         //앉아쏴
        CatcherRunnerBlocking = 200063,   //주자 블록

        //질주본능
        RunnerStealMaster = 200071,       //대도
        RunnerHomeRush = 200072,          //홈돌진
        RunnerSliding = 200073,           //슬라이딩

        //주루센스
        RunnerSense = 200081,             //주루센스
        RunnerDoublePlayBreaker = 200082, //병살저지
        RunnerTurbo = 200083,             //터보엔진
        RunnerLead = 200084,              //리드
        
        //매의눈
        FalconEye = 200091,               //매의눈

        //타자위압
        BatterOverwhelming = 200101,      //타자위압

        //강습타구
        AssaultBall = 200111,             //강습타구

        //찬스맨
        ChanceMan = 200121,               //찬스맨

        //번트의신
        GodOfBunt = 200131,               //번트의 신
        //Buntist = 200132,                 //능숙한 번트

        //뜬금포
        Unexpected = 200141,              //뜬금포
        
        ///////////////////////////////////////////////
        //투수스킬
        ///////////////////////////////////////////////
        //제5의내야수
        PitcherBuntFielding = 100011,     //번트수비
        PitcherJumpCatch = 100012,        //점프캐치
        PitcherReaction = 100013,         //반사신경

        //견제왕
        PitcherQuickMotion = 100021,      //퀵모션	
        LaserPickOff = 100022,            //광속견제        

        //선두타자승부
        SunduKiller = 100031,             //선두타자킬러

	    //추격본능
        ChaseInstinct = 100041,           //추격본능	

        //불꽃투혼
        FrameFight = 100051,              //불꽃 투혼

        //강심장
        SteelHeart = 100061,

        //회심의 일격
        TenderStroke = 100071,            //회심의 일격

        //매혹
        Charm = 100081,                   //매혹

        //위압
        PitcherOverwhelming = 100091,     //투수위압

        //강철어깨
        IronArm = 100101,                 //강철어깨

        //카리스마
        Charisma = 100111,                //카리스마

        //닥터K
        DoctorK = 100121,                 //닥터K

        //필승의지
        WinSpirit = 100131,               //필승의지



        //None = 0
    }


    public enum Restriction_Type
    {
        Game  = 0,
        Inning = 1,
        Batter = 2,
        Field = 3,
        NoRestriction = 4

    }

    public enum Effect_InvokeCondition
    {        
        GameStart,
        InningStart,
        BattingStart,
        PitchStart,
        Passive,
        Field,
        ScoringPosition,
        Crisis,
        NoRunner,
        ExceptionCase
    }

    public enum Effect_Validity
    {        
        GameEnd,
        InningEnd,
        BattingEnd,
        PitchEnd,        
        Field,
        ExceptionCase
    }

    /// <summary>
    /// 나중에 지워
    /// </summary>
    public class skillEffectMap
    {
#if _Test_Local
        public skillEffectMap(string _skillName, 
            Restriction_Type _restriction = Restriction_Type.NoRestriction,
            Effect_InvokeCondition _invokeCondition = Effect_InvokeCondition.ExceptionCase,
            Effect_Validity _effectValidity = Effect_Validity.ExceptionCase,
            int _restrictionCount = 100000,
            List<int?> _counter = null)
        {
            skillName = _skillName;
            restriction = _restriction;
            invokeCondition = _invokeCondition;
            effectValidity = _effectValidity;
            restrictionCount = _restrictionCount;
            counter = _counter;

    #if _Local_Balance
            if (InGameDebug._SKILL_UNLIMITED == true)
            {
                restrictionCount = 100000;
            }
    #endif
        }
#endif

        /// <summary>
        /// 스킬이름
        /// </summary>
        public string skillName;

        /// <summary>
        /// 스킬 제약
        /// </summary>
        public Restriction_Type restriction;

        /// <summary>
        /// 발동조건
        /// </summary>
        public Effect_InvokeCondition invokeCondition;

        /// <summary>
        /// 발동 기간
        /// </summary>
        public Effect_Validity effectValidity;

        /// <summary>
        /// 해당스킬 사용 제한 회수
        /// </summary>
        public int restrictionCount;

        /// <summary>
        /// 카운터 스킬
        /// </summary>
        public List<int?> counter = null;


    }

}
