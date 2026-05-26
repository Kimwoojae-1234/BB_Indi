using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
#if _Skill_Display
    //연출테스트용
    public enum pSkillDisplay
    {
        NoSkill,
        Sun_Du_Ta_Ja,
        Chu_Gyeog_Bon_Neung,
        Bul_Kkot_Tu_Hon,
        Kang_Sim_Jang,
        Hoe_Sim_Il_Gyeog,
        Mea_Hog,
        Too_Soo_Wi_Ab,
        Chrisma,
        Doctor_K,
        Pil_Seung_Ui_Ji
    }

    public enum bSkillDisplay
    {
        NoSkill,
        Mea_Noon,
        Ta_Ja_Wi_Ab,
        Gang_Seub_Ta_Gu,
        Chance_Man,
        Bunt_Sin,
        Tteun_Geum_Po
    }
#endif


    public class tempPlayerData
    {
#if _Test_Local 
        const int BATTER_VALUE = 600;
        const int PITCHER_VALUE = 600;
        const int FIELD_VALUE = 600;
        const int TANDO_VALUE = 600;
        const int STAMINA_VALUE = 400;


        //타자
        const int TEST_CONTACT = BATTER_VALUE;
        const int TEST_EYE = BATTER_VALUE;
        const int TEST_POWER = BATTER_VALUE;
        const int TEST_TANDO = TANDO_VALUE;
        const int TEST_SPEED = FIELD_VALUE;//450;//FIELD_VALUE;
        const int TEST_THROW = FIELD_VALUE;//800;//FIELD_VALUE;
        const int TEST_FIELD = FIELD_VALUE;
        //투수
        const int TEST_BALLSPEED = 700;// PITCHER_VALUE;
        const int TEST_BALLCONTROL = PITCHER_VALUE;
        const int TEST_HARDNESS = PITCHER_VALUE;
        const int TEST_SHARPNESS = PITCHER_VALUE;
        const int TEST_STAMINA = STAMINA_VALUE;
        



        const int _R = 1;
        const int _L = 0;

        const int STARTER = 0;
        const int CHASE = 1;
        const int RELIEF = 2;
        const int SETUP = 3;
        const int SAVE = 4;

        const int na = -1;

        //0번카테고리 0
        const int fourseam = 0;
        //1번카테고리 { 1, 2, 3, 4, 5 };
        const int curve = 1; //커브
        const int slowcurve = 2; //슬로 커브
        const int powercurve = 3;//파워커브
        const int _12_6curve = 4;//12-6커브
        const int knucklecurve = 5;//너클 커브
        //2번카테고리 { 7,8,9,10 };
        const int slider = 7; //슬라이더     7    
        const int vslider = 8;//V슬라이더    8
        const int hslider = 9;//H슬라이더     9
        const int slurve = 10;//슬러브      10        
        //3번카테고리 { 16, 17, 18, 19,20 };
        const int fork = 16;//포크            16
        const int changeup = 17;//체인지업        17          
        const int palm = 19;//팜              19
        const int knuckle = 20;//너클            20    
        //4번 카테고리 { 6,11,13 };
        const int circle = 18;//서클           18
        const int sinker = 11; //싱커        11 
        const int screw = 6;//스크류 6
        //5번 카테고리  { 12,13,14,15 };
        const int sff = 12;//SFF           12
        const int sikingfast = 13;//싱킹패스트     13
        const int twoseam = 14;//투심            14
        const int cutter = 15;//컷              15

        public static int[,] _samsungBatter = new int[14, 9]
        {
                   //컨택	선구안	파워	탄도	주력	수비	송구
            {3,	_R,	620,	1000,	520,	510,	600,	550,	800}, //나바로         3   999
            {8, _L,	708,	490,	410,	230,	920,	600,	410}, //박한이         8   650
            {6, _R,	680,	550,	620,	480,	980,	620,	500}, //박석민         4   380
            {2, _L,	750,	800,	690,	500,	350,	470,	460}, //최형우         6
            {2,	_L,	430,	320,	590,	670,	330,	420,	400}, //이승엽         2    
            {4, _L,	685,	560,	540,	390,	350,	540,	410}, //채태인         2
            {7, _R,	630,	500,	420,	300,	250,	500,	430}, //진갑용         1
            {1, _L,	590,	470,	300,	100,	920,	460,	900}, //박해민         7
            {5, _R,	550,	480,	360,	150,	980,	700,	500}, //김상수         5
            {3, _R,	500,	450,	290,	120,	600,	520,	430}, //조동찬         3
            {1, _R,	480,	480,	650,	160,	330,	450,	410}, //이지영         1
            {4, _R,	540,	460,	250,	140,	360,	410,	400}, //김태완         4
            {7, _L,	300,	1000,	520,	115,	800,	550,	490}, //정형식         7
            {6, _R,	410,	410,	270,	155,	400,	460,	420}  //김현곤         6
        };

        

        /*
        public static string[] _samsungBatterName = new string[14]
        {
            "나바로 14",
            "박한이 14",
            "박석민 14",
            "최형우 14",
            "이승엽 14",
            "채태인 14",
            "진갑용 14",
            "박해민 14", 
            "김상수 14",
            "조동찬 14",
            "이지영 14",
            "김태완 14",
            "정형식 14",
            "김현곤 14"
        };*/

        public static int[,] _kiaBatter = new int[14, 9]
        {
            {7,	0,	460,	410,	330,	250,	850,	330,	450}, //이대형
            {3,	1,	580,	550,	500,	390,	850,	350,	450}, //김주찬
            {4,	1,	530,	540,	510,	350,	500,	400,	420}, //이범호
            {6,	1,	550,	380,	688,	540,	300,	300,	330}, //나지완
            {2,	1,	590,	510,	710,	400,	450,	400,	500}, //브렛필
            {8,	1,	610,	535,	540,	390,	800,	510,	800}, //안치홍
            {6,	0,	540,	480,	510,	350,	510,	350,	880}, //신종길
            {5,	1,	310,	330,	350,	350,	300,	500,	600}, //차일목
            {1,	1,	535,	500,	380,	330,	670,	410,	750}, //김선빈
            {8,	0,	600,	750,	410,	400,	200,	200,	250}, //이종환
            {4,	1,	460,	470,	370,	350,	250,	400,	310}, //박기남
            {6,	1,	450,	420,	400,	380,	800,	250,	270}, //김다원
            {5,	0,	390,	400,	350,	260,	300,	280,	280}, //강한울
            {4,	1,	420,	430,	380,	330,	550,	310,	300}  //김민우
        };


        /*
        public static string[] _kiaBatterName = new string[14]
    {
        "이대형 14",
        "김주찬 14",
        "이범호 14",
        "나지완 14",
        "브렛필 14",
        "안치홍 14",
        "신종길 14",
        "차일목 14", 
        "김선빈 14",
        "이종환 14",
        "박기남 14",
        "김다원 14",
        "강한울 14",
        "김민우 14"
    };*/

        public static int[,] _samsungPitcher = new int[11, 12]
        {
            {STARTER,	_R,		148,	750,	600, fourseam,	powercurve, na,	        changeup,   na,     na ,0},//벤덴헐크
            {STARTER,	_R,		140,	560,    540, fourseam,	_12_6curve,	na,	        changeup,	na,     na ,0},//윤성환	
            {STARTER,	_L,		142,	580,	550, fourseam,	slowcurve,	slider,	    na,         circle,	na ,0},//장원삼	
            {STARTER,	_R,		143,	460,	500, fourseam,	na,         hslider,	changeup,   na,     na ,1},//마틴	
            {STARTER,	_R,		138,	450,	480, fourseam,	curve,  	slider, 	na,         circle, na ,0},//배영수	        
            {CHASE,	_L,		139,	390,	400, fourseam,	curve,      vslider,    changeup,	na,     na ,2},//백정현        	
            {CHASE,	_L,		142,	410,	300, fourseam,	na,         slider,     na,         na,     twoseam ,0},//권혁	        
	        {RELIEF,	_R,		141,	480,	330, fourseam,	na,     	slider, 	fork,       na,     sikingfast ,1},//심창민
	        {RELIEF,	_L,		143,	430,	370, fourseam,	na,         na,         changeup,   sinker, twoseam ,0},//차우찬
            {SETUP,	_R,		144,	500,	390, fourseam,	curve,      hslider,    fork,	    na,     na ,0},//안지만	
            {SAVE,	_R,		147,	490,	350, fourseam,	na,     	hslider, 	fork,       na,     sikingfast ,1}//임창용	
        };

        
        /*
        public static string[] _samsungPitcherName = new string[11]
    {
        "벤덴헐크 14",
        "윤성환 14",
        "장원삼 14",
        "마틴 14",
        "배영수 14",
        "백정현 14",
        "권혁 14", 
        "심창민 14",
        "차우찬 14",
        "안지만 14",
        "임창용 14"
    };*/

        public static int[,] _kiaPitcher = new int[11, 12]
        {
            {STARTER,	_L,		148,	850,	700, fourseam,	curve,      slider,	    changeup,   na,     na ,0},//양현종
            {STARTER,	_R,		140,	470,    490, fourseam,	powercurve,	slider,     na,     	na,     na ,0},//김진우	
            {STARTER,	_L,		141,	500,	500, fourseam,	curve,      na,     	changeup,   na,     twoseam ,0},//토마스	
            {STARTER,	_R,		144,	440,	460, fourseam,	na,         hslider,   	na,         circle, twoseam ,0},//송은범	
            {STARTER,	_L,		139,	450,	480, fourseam,	curve,  	slider, 	na,         circle, na ,0},//임준섭	        	
            {CHASE,	    _R,		135,	420,	400, fourseam,	na,         hslider,    changeup,	na,     sikingfast ,2},//김병현	        	
            {CHASE,	    _R,		137,	400,	350, fourseam,	na,         slider,     na,         na,     twoseam ,1},//신창호	    
            {RELIEF,	_R,		134,	500,	280, fourseam,	curve,     	slider,     na,         na,     na ,0},//김태영
            {RELIEF,	_R,		133,	550,	250, fourseam,	curve,     	slider, 	fork,       na,     twoseam ,0},//최영필
            {SETUP,	_L,		142,	460,	300, fourseam,	na,         slider,     fork,	    na,     na ,0},//심동섭	
            {SAVE,	_R,		148,	500,	360, fourseam,	na,     	hslider, 	na,         circle, twoseam ,0}//어센시오	
        };

        /*
        public static string[] _kiaPitcherName = new string[11]
    {
        "양현종 14",
        "김진우 14",
        "토마스 14",
        "송은범 14",
        "임준섭 14",
        "김병현 14",
        "신창호 14",
        "김태영 14",
        "최영필 14",
        "신동섭 14",
        "어센시오 14"
    };*/
#if GIRL_PLAY
        public static string[,] _batterName = new string[10, 14]
        {
            {"Kai Wave","Luna Tide","Rex Splash","Mina Coral","Jake Sunny","Rio Drift","Nami Blue","Terry Sand","Lika Shell","Dean Surf","Yuna Breeze","Coco Palm","Finn Coast","Rina Bay"},
            {"Ace Harbor","Sia Lagoon","Leo Current","Momo Tide","Zane Reef","Nina Wave","Haru Salt","Bella Foam","Ken Splash","Yuki Shore","Noah Breeze","Lily Coast","Aiden Surf","Mia Coral"},
            {"Riku Ocean","Sara Pearl","Toma Wave","Lena Drift","Kira Blue","Evan Bay","Nora Shell","Kai Reef","Sena Splash","Luca Sand","Rin Sunny","Theo Harbor","Amy Tide","Dino Palm"},
            {"Rex Coral","Yui Splash","Milo Surf","Nami Coast","Dean Lagoon","Lina Wave","Toby Reef","Hana Drift","Kyle Breeze","Mina Harbor","Ryo Blue","Sora Palm","Finn Tide","Ruby Shell"},
            {"Jett Sand","Luna Foam","Kai Current","Rika Surf","Noel Splash","Yuna Bay","Ares Coral","Sia Drift","Toma Breeze","Lio Reef","Nina Harbor","Ken Tide","Rex Coast","Mika Palm"},
            {"Leo Blue","Rin Wave","Cody Lagoon","Yuri Shell","Jake Drift","Nami Coral","Finn Breeze","Aki Surf","Lina Bay","Dean Reef","Sora Splash","Theo Palm","Amy Harbor","Kai Sunny"},
            {"Terry Tide","Mina Coast","Rio Splash","Nora Surf","Riku Harbor","Yui Drift","Jett Wave","Momo Bay","Noah Shell","Kira Breeze","Aiden Reef","Ruby Coral","Luca Palm","Haru Foam"},
            {"Sena Lagoon","Leo Surf","Lina Splash","Finn Current","Ryo Coast","Mika Tide","Kai Reef","Amy Drift","Dean Coral","Nina Harbor","Toby Breeze","Lily Palm","Rex Wave","Yuna Bay"},
            {"Noel Splash","Sora Surf","Kira Tide","Jake Coast","Rin Reef","Luca Harbor","Yui Drift","Theo Breeze","Momo Shell","Finn Palm","Riku Wave","Sara Coral","Dean Bay","Amy Lagoon"},
            {"Kai Splash","Lina Surf","Noah Tide","Ruby Coast","Ares Reef","Nami Drift","Leo Breeze","Mika Coral","Toma Harbor","Sia Palm","Rin Bay","Dean Foam","Finn Current","Yuna Wave"},
        };

        public static string[,] _pitcherName = new string[10, 11]
        {
            {"Storm Kai","Wave Rex","Tide Dean","Coral Finn","Splash Rio","Drift Leo","Surf Jake","Blue Noah","Reef Ares","Harbor Theo","Palm Cody"},
            {"Lagoon Riku","Foam Terry","Bay Luca","Sunny Milo","Coast Jett","Current Noel","Shell Toma","Breeze Dean","Coral Rex","Wave Finn","Surf Kai"},
            {"Tide Leo","Drift Noah","Splash Theo","Reef Jake","Palm Luca","Harbor Cody","Bay Terry","Wave Milo","Foam Riku","Sunny Dean","Surf Jett"},
            {"Coral Kai","Blue Finn","Lagoon Rex","Current Leo","Tide Jake","Shell Noah","Wave Cody","Palm Theo","Drift Terry","Harbor Milo","Splash Luca"},
            {"Surf Dean","Reef Kai","Bay Finn","Foam Jake","Coral Theo","Wave Noah","Sunny Rex","Palm Leo","Lagoon Terry","Tide Cody","Drift Luca"},
            {"Splash Milo","Harbor Dean","Current Kai","Reef Noah","Blue Jake","Shell Finn","Palm Terry","Surf Theo","Wave Cody","Coral Luca","Tide Rex"},
            {"Foam Leo","Drift Kai","Bay Noah","Lagoon Jake","Splash Finn","Palm Cody","Current Theo","Surf Terry","Reef Milo","Harbor Luca","Wave Dean"},
            {"Sunny Kai","Coral Noah","Wave Leo","Tide Finn","Surf Jake","Palm Rex","Drift Theo","Shell Cody","Harbor Terry","Reef Dean","Splash Luca"},
            {"Blue Milo","Foam Kai","Lagoon Noah","Current Finn","Wave Jake","Palm Dean","Tide Terry","Surf Cody","Coral Theo","Reef Luca","Harbor Rex"},
            {"Drift Leo","Splash Kai","Bay Finn","Palm Noah","Sunny Jake","Wave Terry","Surf Luca","Coral Cody","Reef Theo","Lagoon Dean","Tide Milo"}
        };
#else

        public static string[,] _batterName = new string[10, 14]
        {
            {"김상수 15","박한이 15","나바로 15","최형우 15","박석민 15","이승엽 15","채태인 15","이지영 15","박해민 15","진갑용 15","구자욱 15","우동균 15","박찬도 15","김정혁 15"},
            {"서건창 15","이택근 15","유한준 15","박병호 15","윤석민 15","김민성 15","스나이더 15","김하성 15","박동원 15","박헌도 15","문우람 15","고종욱 15","서동욱 15","김지수 15"},
            {"김종호 15","이종욱 15","나성범 15","테임즈 15","모창민 15","이호준 15","손시헌 15","김태군 15","박민우 15","지석훈 15","김성욱 15","조영훈 15","최재원 15","노진혁 15"},
            {"오지환 15","김용의 15","박용택 15","이병규 15","정성훈 15","이병규 15","이진영 15","최경철 15","손주인 15","박지규 15","양석환 15","정의윤 15","유강남 15","채은성 15"},
            {"이명기 15","김강민 15","최정 15","브라운 15","박정권 15","이재원 15","정상호 15","박계현 15","김성현 15","조동화 15","임훈 15","박진만 15","나주환 15","박재상 15"},
            {"민병헌 15","정수빈 15","김재환 15","김현수 15","양의지 15","홍성흔 15","오재원 15","최주환 15","김재호 15","루츠 15","정진호 15","최재훈 15","고영민 15","허경민 15"},
            {"아두치 15","손아섭 15","황재균 15","최준석 15","강민호 15","김문호 15","정훈 15","김대우 15","문규현 15","오승택 15","강동수 15","안중열 15","김민하 15","임재철 15"},
            {"김주찬 15","신종길 15","필 15","나지완 15","이범호 15","김원섭 15","최용규 15","이홍구 15","강한울 15","최희섭 15","김다원 15","오준혁 15","박기남 15","이성우 15"},
            {"이용규 15","정근우 15","김경언 15","김태균 15","최진행 15","모건 15","김회성 15","조인성 15","권용관 15","강경학 15","한상훈 15","이성열 15","송광민 15","정범모 15"},
            {"이대형 15","김민혁 15","하준호 15","김상현 15","장성우 15","마르테 15","박경수 15","신명철 15","박기혁 15","용덕한 15","심우준 15","김사연 15","박용근 15","조중근 15"},

        };

        public static string[,] _pitcherName = new string[10, 11]
        {
            {"윤성환 15","피가로 15","클로이드 15","차우찬 15","장원삼 15","심창민 15","신용운 15","박근홍 15","백정현 15","안지만 15","임창용 15"},
            {"밴헤켄 15","한현희 15","피어밴드 15","문성현 15","송신영 15","김영민 15","김동준 15","마정길 15","이상민 15","조상우 15","손승락 15"},
            {"찰리 15","손민한 15","해커 15","이태양 15","이재학 15","고창성 15","임정호 15","최금강 15","민성기 15","이민호 15","임창민 15"},
            {"소사 15","루카스 15","우규민 15","류제국 15","임지섭 15","임정우 15","정찬헌 15","김선규 15","유원상 15","이동현 15","봉중근 15"},
            {"김광현 15","켈리 15","밴와트 15","윤희상 15","채병용 15","전유수 15","문광은 15","박종훈 15","백인식 15","정우람 15","윤길현 15"},
            {"니퍼트 15","마야 15","유희관 15","장원준 15","진야곱 15","이현호 15","김강률 15","함덕주 15","오현택 15","이재우 15","윤명준 15"},
            {"린드블럼 15","이상화 15","심수창 15","레일리 15","송승준 15","이명우 15","홍성민 15","심규범 15","이정민 15","김성배 15","김승회 15"},
            {"양현종 15","험버 15","스틴슨 15","서재응 15","문경찬 15","최영필 15","한승혁 15","홍건희 15","박준표 15","심동섭 15","윤석민 15"},
            {"탈보트 15","유먼 15","안영명 15","배영수 15","송은범 15","송창식 15","임준섭 15","이동걸 15","정대훈 15","박정진 15","권혁 15"},
            {"옥스프링 15","어윈 15","정대현 15","시스코 15","엄상백 15","이창재 15","최원재 15","심재민 15","고영표 15","김민수 15","장시환 15"}
        };
#endif
        public static int[,] _batterStat = new int[10, 6]
        {
            //con  eye   pow   spd   cat   thr
            {1100,  750,  750,  750,  750,  750 }, //SAMSUNG
            {1100,  900,  900,  900,  900,  900 }, //Nexen
            {1100,  500,  500,  700,  700,  700 }, //NC
            {1100,  900,  900,  900,  900,  900 }, //LG
            {1100,  1000,  1000,  1000,  1000,  1000 }, //SK
            {1180,  950,  950,  950,  950,  950 }, //DOOSAN
            {1100,  700,  700,  700,  700,  700 }, //Lotte
            {1100,  800,  800,  800,  800,  800 }, //KIA
            {1100,  850,  850,  850,  850,  850 }, //HANHWA
            {1100,  600,  600,  700,  700,  700 }, //KT
        };


        public static void makeFielderData(CPlayer fielder, int team, int index)
        {
            int curIndex = (team == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex);
            ////Debug.Log("===============> team = " + team + "====>curIndex = " + curIndex + "====>index = " + index);
            string name = _batterName[curIndex-1, index];

            int pos = (team == 1 ? _kiaBatter[index, 0] : _samsungBatter[index, 0]);
            int secondPos = pos;
            int throwHand = (team == 1 ? _kiaBatter[index, 1] : _samsungBatter[index, 1]);
            int hitHand = throwHand;
            int batterType = 0;
            int pitcherType = 0;// (team == 1 ? _kiaPitcher[index, 11] : _samsungPitcher[index, 11]);

            fielder.setIdentity(name, pos, index, throwHand, hitHand, batterType, pitcherType, (index < 9 ? tempPlayerData._currentPosition[team, index] : 9), index);

            int contact = _batterStat[curIndex - 1, 0];// tempSelectPage.CONTACT_STAT;
            int eye = _batterStat[curIndex - 1, 1];//tempSelectPage.BATTING_STAT;
            int power = _batterStat[curIndex - 1, 2];//tempSelectPage.BATTING_STAT;
            int tando = 400;
            int _speed = _batterStat[curIndex - 1, 3];// tempSelectPage.RUNNING_STAT;
            int _catch = _batterStat[curIndex - 1, 4];// tempSelectPage.FIELDING_STAT;
            int _throw = _batterStat[curIndex - 1, 5];// tempSelectPage.THROW_STAT;

            if (index == 3) tando = 700;
            else if (index == 4) tando = 600;
            else if (index == 2 || index == 5 || index ==7) tando = 400;
            else tando = 300;

            //_catch = 480;
            //_throw = 480;
            //_speed = team == 0?1000:300;
            fielder.setBatterAbility(eye, contact, power, tando, _catch, _throw, _speed);
            
        }



        public static void makePitcherData(CPlayer pitcher, int team, int index)
        {
            int curIndex = (team == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex);
            string name = _pitcherName[curIndex-1, index];

            int pos = 0;
            int secondPos = (team == 1 ? _kiaPitcher[index, 0] : _samsungPitcher[index, 0]);
            int throwHand = (team == 1 ? _kiaPitcher[index, 1] : _samsungPitcher[index, 1]);
            int hitHand = throwHand;
            int batterType = 0;
            int pitcherType = (team == 1 ? _kiaPitcher[index, 11] : _samsungPitcher[index, 11]);

            pitcher.setIdentity(name, pos, secondPos, throwHand, hitHand, batterType, pitcherType, CPlayer._PITCHER, index);

            int contact = 100;
            int eye = 100;
            int power = 100;
            int tando = 100;
            int _speed = 500;
            int _catch = 600;
            int _throw = TEST_BALLSPEED;
            
            pitcher.setBatterAbility(eye, contact, power, tando, _catch, _throw, _speed);
            
            int spd = TEST_BALLSPEED;//
            int con = 800;// 
            int stm = 1000;// 
            int hard = 600;// 
            int sharp = 600;//

            pitcher.setPitcherAbility(spd, con, stm, hard, sharp, team, index);

        }


        //////////////////////////////////////////////////////////////////////
        //기타(이중에 많은 것들이 사라질 예정
        //////////////////////////////////////////////////////////////////////
        

        //임시 이름
        public static string[,] _name = new string[2, 10] 
        {
            {"이대형 14","김주찬 14","이범호 14","나지완 14","브렛필 14","안치홍 14","신종길 14","차일목 14","김선빈 14","양현종 14"},
            {"나바로 14","박한이 14","박석민 14","최형우 14","이승엽 14","채태인 14","진갑용 14","박해민 14","김상수 14","벤덴헐크 14"},
        };

        //임시 사용손
        public static int[,] _hand = new int[2, 10] 
        {
            {0,1,1,1,0,1,1,1,1,0},
            {1,0,1,0,0,0,1,1,1,0},
        };

        //임시 포지션
        public static int[,] _currentPosition = new int[2, 10] 
        {        
            {3,8,6,2,9,4,7,1,5,0},  //삼성
            {7,3,4,9,2,8,6,5,1,0},  //기아
        };

#if GIRL_PLAY
        public static string[] _teamName = new string[10]
        {
            "Beach Breakers",
            "Sandstorm Sluggers",
            "Wave Runners",
            "Sunset Batters",
            "Coral Pirates",
            "Tidal Smash",
            "Blue Lagoon Nine",
            "Palm Hitters",
            "Seaside Storm",
            "Orca Beach Club"
        };
#else
        public static string[] _teamName = new string[10]
        {
            "삼성라이온즈","넥센히어로즈","NC다이노스","LG트윈스","SK와이번즈","두산베어즈","롯데자이언츠","기아타이거즈","한화이글스","kt위즈"
        };
#endif

#endif

    }
}