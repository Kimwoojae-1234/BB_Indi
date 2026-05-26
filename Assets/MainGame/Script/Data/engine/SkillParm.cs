
using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public enum SkillCheck
    {
        Active = 0,
        Success = 1,
        Parm3 = 2,
        Parm4 = 3
    }

    public enum VsSkillResult
    {
        OffensWin,
        DefenseWin
    }

    public enum SkillBuffType
    {
        None,
        PitcherUP,
        PitcherDown,
        PitcherSpecial,
        DoctorK,
        BatterUP,        
        BatterDown,
        BatterSpecial,
        SkillInvalidity //스킬 무효
    }


    public class SkillParm
    {
        public static SkillBuffType GetBuffType(SkillID id)
        {
            if (id == SkillID.chu_gyeog_bon_neung ||   //추격본능
                id == SkillID.so_nyun_ga_jang ||   //소년가장
               id == SkillID.bul_kkot_tu_hon ||         //불꽃투혼
               id == SkillID.kang_sim_jang ||       //강심장
               id == SkillID.pil_seung_eu_ji)        //필승의지
            {
                //투수 버프
                return SkillBuffType.PitcherUP;
            }
            else if (id == SkillID.ta_ja_ei_ab || //타자위압
                    id == SkillID.chul_gun_sin ||       //출근의신
                    id == SkillID.so_nyun_jang_sa ||    //소년장사
                    id == SkillID.gum_gang_bul_goe ||   //금강불괴
                    id == SkillID.god_ei_ab)            //신의위압    
            {
                //투수 디버프
                return SkillBuffType.PitcherDown;
            }
            else if (id == SkillID.hoe_sim_il_gyeog ||  //회심
                     id == SkillID.dol_jik_gu || //돌직구
                     id == SkillID.mea_hog ||     //매혹
                     id == SkillID.sun_cream ||   //선크림
                     id == SkillID.beam_jik_gu)   //뱀직구
                        
            {
                //투수 특수
                return SkillBuffType.PitcherSpecial;
            }
            else if (id == SkillID.chance_man ||    //찬스맨
                    id == SkillID.little_cuba ||    //리틀쿠바
                    id == SkillID.flower_bunho)     //꽃범호
            {
                //타자버프
                return SkillBuffType.BatterUP;
            }
            else if (id == SkillID.sun_du_ta_ja ||  //선두타자
                     id == SkillID.too_soo_wi_ab || //투수위압
                     id == SkillID.ni_nu_nim ||     //니느님
                     id == SkillID.do_bal_ggun ||     //도발꾼
                     id == SkillID.pung_gi_mul_ran || //풍기문란
                     id == SkillID.gab_dragon)       //갑드래곤
            {
                //타자디버프
                return SkillBuffType.BatterDown;
            }
            else if (id == SkillID.mea_nun ||       //매의눈
                     id == SkillID.yong_nun ||      //용의눈
                     id == SkillID.song_gol_mea ||  //송골매
                     id == SkillID.sniper ||        //스나이퍼
                     id == SkillID.gang_seup_ta_gu || //강습타구
                     id == SkillID.batting_machine || //타격기계
                     id == SkillID.man_se_ta_bub  ||  //만세타법
                     id == SkillID.bunt_sin ||        //번트의신
                     id == SkillID.young_gyu_play ||    //용규놀이
                     id == SkillID.tteun_geum_po ||     //뜬금포
                     id == SkillID.park_bang_po ||      //박뱅포
                     id == SkillID.big_boy ||           //빅보이
                     id == SkillID.guk_min_ta_ja)       //국민타자
            {
                //타자 특수능력 발동
                return SkillBuffType.BatterSpecial;
            }



            return SkillBuffType.None;
        }

        ////////////////////////////////////////////////////////////////////////////////////
        //필드 스킬 발동에 따른 각종 레이트(확정)
        ////////////////////////////////////////////////////////////////////////////////////
        //(투포제외) [슬라이딩] 스킬에 따른 범위 옵셋
        const float DIVING_RANGE_PERLEVEL = 0.0048f;//-> 포핸드 백핸드 캐치 레벨당 레인지 증가율
        public static float getSlidingRangeOffset(int curLevel)
        {
            return (curLevel * DIVING_RANGE_PERLEVEL);
        }

        //(투포제외) [타구판단] 스킬에 따른 딜레이 옵셋
        public static float getDecisionOffset(int curLevel)
        {
            //타구판단 레벨이 오를 수록 줄어드는 딜레이...
            return -(0.04f + (curLevel * 0.0024f)); //최신버전 //최소 0.04 ~ 0.208      //70기준
        }

        //(투포제외) [수비반경] 스킬에 따른 내야 범위 
        const float FORE_BACK_RANGE_PERLEVEL = 0.00152f;//0.00409f;//0.0057f; // (0.4/70) //-> 포핸드 백핸드 캐치 레벨당 레인지 증가율
        public static float getInfieldRangeOffset(int curLevel)
        {
            //옵셋은 레벨이 증가함에 따라 같이 증가
            return (0.114f + (curLevel * FORE_BACK_RANGE_PERLEVEL)); //최신버전 0.114 ~ 0.2204 //0.00152
        }

        //(투포제외) [수비반경] 스킬에 따른 외야 범위 
        const float OUTFIELD_RANGE_PERLEVEL = 0.0011f; //0.002f;//0.00306f;//0.0043f; // (0.4/70) //-> 포핸드 백핸드 캐치 레벨당 레인지 증가율
        public static float getOutfieldRangeOffset(int curLevel)
        {
            //옵셋은 레벨이 증가함에 따라 같이 증가
            return (0.086f + (curLevel * OUTFIELD_RANGE_PERLEVEL));//현재버전 0.086-0.163 // 0.0011f;
        }

        const int JUMPCATCH_HEIGHT_RANGE_PERLEVEL = 1; //max 70 증가 ->점핑캐치 높이 최대영역 레벨별 증가율        
        public static int getJumpHeightRange(int curLevel)
        {
            return curLevel * JUMPCATCH_HEIGHT_RANGE_PERLEVEL;
        }

        const float JUMPCATCH_SLOPE_RANGE_PERLEVEL = 0.0057f; //max 0.4증가 ->점핑캐치 슬로프 최대영역 레벨별 증가율
        public static float getJumpSlopeRange(int curLevel)
        {
            return curLevel * JUMPCATCH_SLOPE_RANGE_PERLEVEL;
        }

        //(내야수) [스페셜송구]스킬의 [스핑송구]시 추가 레인지 확보
        public static float getAddRangeFore(float posX, float dstX, int level)
        {
            if (level > 0 && posX < dstX)
            {
                float offset = 0.1f + (level * 0.002143f);
                return offset;
            }

            return 0;
        }

        //(내야수) [스페셜송구]스킬의 [스핑송구]시 강도 비율 구하기
        public static float getSpinThrowRate(int level)
        {
            return (0.85f + (level * 0.005f));
        }

        //(내야수) [스페셜송구]스킬의 [점핑송구]시 추가 레인지 확보
        public static float getAddRangeBack(float posX, float dstX, int level)
        {
            if (level > 0 && posX > dstX)
            {
                float offset = 0.1f + (level * 0.002143f);
                return offset;
            }

            return 0;
        }

        //(내야수) [스페셜송구]스킬의 [점핑송구] 강도 비율 구하기
        public static float getJumpingThrowRate(int level)
        {
            return (0.65f + (level * 0.005f));
        }

        //(내야수) [퀵송구] 스킬의 딜레이 비율 구하기
        public static float getQuickThrowDelayRate(int level, bool bNormal)
        {
            //0.7 ~ 0.3
            float rate = 0.7f - (0.0057f * level);
            return (rate * (bNormal ? 0.75f : 1));
        }

        //(내야수) [퀵송구 / 대시송구] 스킬의 송구 강도 비율 구하기
        public static float getQuickThrowRate(int level)
        {
            return (0.9f + (0.0043f * level));
        }

        //(내야수) [스페셜송구] 스킬의 [대시송구] 딜레이 비율 구하기
        public static float getDashThrowDelayRate(int level)
        {
            //0.2 - 0.8
            return 0.8f - (level * 0.00857f);
        }

        //(외야수) [레이저] 스킬의 송구의 강도 얻어옴
        public static float getLaserThrow()//int level)
        {
            //최소 1750 ~ 최대 3000
            return 2500;// 2000;// 1750.0f +(level * 17.857f);
        }

        /*
        //새로운 스킬
        public static bool checkSkill(CPlayer player, SkillIndex index, SkillCheck _parm, int addBonus = 0)
        {
            CSkill curSkill = player.getSkillValue(index);

            if (curSkill != null)
            {                
                float activeRate = 100;
                float range = Random.Range(0.0f, 100.0f);

                if (range < activeRate)
                {
                    return true;
                }
            }

            return false;
        }

        //위의 함수와 같은 구조이나 레벨을 리턴 해줌
        public static int getSkillLevel(CPlayer player, SkillIndex index, SkillCheck _parm = SkillCheck.Active)
        {
            CSkill curSkill = player.getSkillValue(index);

            if (curSkill != null)
            {
                float activeRate = 100; //임시
                float range = Random.Range(0.0f, 100.0f);

                if (range < activeRate)
                {
                    return Random.Range(50, 70); //임시
                   
                }
            }

            return 0;
        }

        //위의 함수와 같은 구조이나 레벨을 리턴 해줌
        public static float getSkillValue(CPlayer player, SkillIndex index, int _parm)
        {
            CSkill curSkill = player.getSkillValue(index);

            if (curSkill != null)
            {
                int parm = _parm;  //체크

                float value = 1;
                              

                return (value * 0.01f);
            }

            return 0;
        }

        //스킬 vs 결과
        public static VsSkillResult checkVSSkill(int offenseSkillRank, int defenseSkillRank)
        {
            float offenseWinRate = (float)offenseSkillRank / (float)(offenseSkillRank + defenseSkillRank);

            float per = MyMath.PercentF();
            //Debug.Log("================== >> per = "+per +"     offenseWinRate = "+offenseWinRate);

            if (per < offenseWinRate)
            {
                ////UnityEngine.//Debug.Log("========================>> Vs 대결 주자 승리");
                return VsSkillResult.OffensWin;
            }
            else
            {
                ////UnityEngine.//Debug.Log("========================>> Vs 대결 야수 승리");
                return VsSkillResult.DefenseWin;
            }
        }*/

        /*
        ////////////////////////////////////////////////////////////////////////////////////
        //임시로 쓰이는 스킬 관련 메쏘드
        ////////////////////////////////////////////////////////////////////////////////////
        public static int StandardEye = 500;      //표준 값
        public static int StandardCatch = 500;      //표준 값
        public static int StandardThrowing = 500;
        public static int StandardRunning = 500;
        public static int CurrentDiviation = 10;    //현재 편차

        //승패
        public static bool checkVsFielderWin(int fValue, int fSkillValue, int fStandardValue, int rValue, int rSkillValue, int rStandardValue)
        {
            int fielder = (fSkillValue) + getDiviation(fValue, fStandardValue);
            int runner = (rSkillValue) + getDiviation(rValue, rStandardValue);

            int range = Random.Range(0, (fielder+runner));

            ////UnityEngine.//Debug.Log("======================================================>>야수밸류 Value =" + fielder);
            ////UnityEngine.//Debug.Log("======================================================>>주자밸류 Value =" + runner);
            ////UnityEngine.//Debug.Log("======================================================>>range Value =" + range);


            if (range < fielder)
            {
                ////UnityEngine.//Debug.Log("======================================================>>야수의 승리");
                return true;    //야수의 승리
            }
            return false;       //주자의 승리
        }


        public static bool checkSuccess(int value, int skillValue, int standardValue)
        {
            int max = 100;
            int limit = (skillValue) + getDiviation(value, standardValue); //커질수록 성공확률 증그

            int range = Random.Range(0, max);
            ////UnityEngine.//Debug.Log("=======================================================================================================>>> range : limit" + range+" : "+limit);

            if (range < limit)
            {
                ////UnityEngine.//Debug.Log("=======================================================================================================>>> 스킬성공 : "+limit+"/"+max);
                return true;//성공
            }
            return false;
        }

        //스킬의 발동여부
        public static bool checkActive(int value, int skillValue, int standardValue)
        {

            int max = 100;
            int limit = (skillValue) + getDiviation(value, standardValue); //커질수록 성공확률 증그

            //////UnityEngine.//Debug.Log("=============>>limit =" + limit);
            //////UnityEngine.//Debug.Log("=============>>skillValue =" + skillValue);


            if (Random.Range(0, max) < limit)
            {
                //////UnityEngine.//Debug.Log("=============>>발동성공");
                return true;//성공
            }
            return false;
        }

        //능력치에 따른 편차
        public static int getDiviation(int value, int standardValue)
        {
            int dValue = value - standardValue;
            int diviation = (SkillParm.CurrentDiviation * dValue) / standardValue;
            return diviation;
        }
         * 
         * 
        /// <summary>
        /// 스페셜캐치, 스페셜 송구, 타구판단, 수비반경의 레벨을 계산해주는 메쏘드
        /// 수비 능력치에 따라 발동이 안되는 경우도 있다
        /// 스킬별로 초기 발동 확률이 틀림
        /// 1-70단위의 레벨값을 도출해내기 위해 퍼센트(0~100)로 넘어온 파라메터를 치환하는 함수
        /// </summary>        
        public static int getFieldSkillLevel(int value, int level, int initSuccess)
        {
            float successPer = initSuccess + (value * 0.03f);
            if (level == 1)
            {
                successPer += 15;
            }
            else if (level >= 2)
            {
                successPer += 35;
            }

            if (MyMath.Percent() < (int)successPer)
            {
                //발동가능
                if (level == 0) return Random.Range(10, 30);
                else if (level == 1) return Random.Range(25, 50);
                else return Random.Range(50, 70);
            }
            else
            {
                //발동안함
                return 0;
            }
        }
        */


        /*
        
        public const int MAX_BATTING_SKILL = 27;

        public const int GOODBYE = 0,           //끝내기
                         CONCENTRATE = 1,       //집중    
                         TUJI1 = 2,             //투지강화1    
                         TUJI2 = 3,             //투지강화2
                         GANGHANG1 = 4,         //강행돌파1
                         GANGHANG2 = 5,         //강행돌파
                         DOBAL_DEFENSE = 6,     //도발방어
                         CHOGU = 7,             //초구공략
                         MUSANG = 8,            //무념무상
                         CLUTCH = 9,            //클러치
                         DRAG = 10,             //드래그번트
                         INFIELD_HIT = 11,      //내야안타
                         GOMU = 12,             //고무손목
                         CHECK_SWING = 13,      //체크스윙
                         JING_GUM = 14,         //징검다리
                         TABLE_SETTER = 15,     //테이블세터
                         GRAND_SLAM = 16,       //만루
                         SURPRISE_HOMERUN = 17, //뜬금
                         PITCHER_APDO1 = 18,    //투수압도1
                         PITCHER_APDO2 = 19,    //투수압도2
                         NO4_HITTER = 20,       //4번타자
                         PINCH_HITTER = 21,     //대타
                         WIDE_ANGLE = 22,       //광각타법
                         EAGLE_EYE = 23,        //이글아이
                         CROSS_KILLER = 24,     //크로스 킬러
                         CLEANUP = 25,          //중심타선
                         DH_HITTER = 26;        //지명타자

        public const int MAX_PITCHING_SKILL = 42;

        public const int MENTAL_GAP = 0,    //멘탈갑
                         STRONG_HEART = 1,  //강심장
                         GONZO = 2,         //근성
                         DOCRYU = 3,        //독려
                         BAESU_JIN = 4,     //배수의진
                         TUJANGSIM = 5,     //투쟁심
                         GYULBYUCK = 6,     //결벽
                         LUCKY_GUY = 7,     //행운아
                         GAKSUNG = 8,       //각성
                         BUNBAL = 9,        //분발
                         FULLCOUNT = 10,    //풀카운트
                         GIBUNPA = 11,      //기분파
                         SUNDO_KILLER = 12, //선두타자킬러
                         CLEANUP_KILLER = 13,   //클린업 킬러
                         SAMGU_SAMJIN = 14,     //삼구삼진
                         IRON_ARM = 15,         //무쇠팔
                         BACK_POWER = 16,       //뒷심
                         REVENGE = 17,          //복수
                         RESPONSIBILITY = 18,   //책임감   
                         QS = 19,               //퀄리티 스타트
                         GOHOME_INSTICT = 20,   //퇴근본능
                         SECOND_STARTER = 21,    //제2선발
                         PURSUER = 22,            //추격자   
                         MUSTWIN = 23,          //필승
                         TIE = 24,              //동점
                         THRILLER = 25,         //스릴러
                         SUCCESSION = 26,       //승계
                         TOUGH_SAVE = 27,       //터프세이브
                         TRUST = 28,            //신뢰
                         BULLY = 29,            //윽박
                         DOCTOR_K = 30,         //닥터K
                         STONE_BALL = 31,       //돌직구
                         APDO = 32,             //압도
                         JASINGAM = 33,         //자신감
                         SHUTUP = 34,            //셧업
                         BANSUNG = 35,          //반성
                         PIN_POINT = 36,        //핀포인트
                         SNAKE_BALL = 37,       //뱀직구
                         WE_AP = 38,       //위압
                         MAE_HOK = 39,      //매혹
                         SHARPNESS = 40,        //예리함
                         HOE_SIM = 41;          //회심의 일격
                         
        


        public const int MAX_FIELD_SKILL = 23;
        //필드스킬 인덱스
        public const int    HR_STEAL = 6, //홈런스틸
                            PITCHER_RUNNER_TIEUP = 12,  //주자속박        : 아직 정확히 구현 안됨
                            PITCHER_QUICK_MOTION = 13,  //퀵모션 
                            PITCHER_LEAD = 17,        //투수리드
                            DUNDUNHAM = 18,            //든든함
                            ANBANG_MANIM = 19,
                            FRAMING = 20,               //프레이밍
                            PLAYING_COACH = 21,          //플레잉코치
                            DOBAL = 22;                 //도발*/

        
        //SUPER_SONIC:          (도루 보너스 3, 딜레이스틸 보너스 2, 터보 성공 보너스 3, 가속시간 감소)
        //LEAD = 6,             (도루 보너스 3, 딜레이스틸 보너스 3, 병살 저지 발생 보너스 10, 광속견제 대항 (30~50))  *일반 견제사는 Random.Range(0,주력)<2인 경우
        //RUNNING_SENSE = 7,    (발동시 한베이스 더 감 2루와 3루에서만 발동)
        //RUNNING_SLIDING = 8;  (도루 보너스 4, 딜레이스틸 보너스 1, 홈돌지 성공 보너스 3,  레이저 대항 (30~50))
        //도루는 0번 딜레이스틸 1번 터보는 2번
        //도루관련 버프는 투수의 주자속박과 

        
        //스킬 vs타입
        //1. 필딩 발동 -> vs        : (홈돌진 없는) 주자블럭, (도루터보 없는) 앉아쏴, 레이저, 광속견제, 
        //2. 러닝 발동 -> vs        : (스페셜송구 없는) 터보, (주자블럭 없는) 홈돌진, 병살저지, (앉아쏴 없는) 도루터보, 딜레이 스틸
        //3. 양쪽 발동 -> vs        : 홈돌진 대 주자블럭, 도루터보 대 앉아쏴, 스페셜 송구 vs 터보 
        //4. 필딩 발동 -> 성공체크  : 스페셜 캐치, 다이빙 캐치, 홈런스틸, 투수필딩, 투수점프캐치, 투수타구반응
        //5. 필딩 기준 vs 부가 영향 : 정립 안됨
        //6. 러닝 기준 vs 부가 영향 : 리드(광속견제, 도루, 스킵동작), 슬라이딩(도루, 레이저, 야수의 태그 딜레이 유발), 수퍼소닉(주력보너스), 센스(one more base판단)


        //1,2, 4번의 경우 스킬의 순수 성공력
        //3번의 경우 양쪽 성공력의 비교
        //위의 모든 경우에 부가영향을 상황에 맞게....
       
    }
}