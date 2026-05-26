using UnityEngine;
using System.Collections;
using WebConnector;
using System.Collections.Generic;
using System;

namespace BaseBall.BallPlay
{
    public class CPlayer
    {   
        //HAND
        public const int _LEFTHAND = 0;// Left Hand Index
        public const int _RIGHTHAND = 1;// Right Hand Index  		 
        public const int _SWITCHHAND = 2;
        //Pitcher Type
        public const int _OVERTHROW = 0;//	
        public const int _SIDEARM = 1;//
        public const int _UNDERHAND = 2;
        //POSITION
        public const int _PITCHER = 0;	//
        public const int _CATCHER = 1;
        public const int _FIRSTBASEMAN = 2;
        public const int _SECONDBASEMAN = 3;
        public const int _THIRDBASEMAN = 4;
        public const int _SHORTSTOP = 5;
        public const int _OUTFIELDER = 6;
        public const int _LEFTFIELDER = 6;
        public const int _CENTERFIELDER = 7;
        public const int _RIGHTFIELDER = 8;
        public const int _DH = 9;
        public const int _BENCH = 10;

        
        /// <summary>
        /// 카드 인포
        /// </summary>
        private GameCardInfo card;
        //private UserData.NetworkData_GameCardInfo netCardInfo;
        private card playerdata;
        private player playerInfo;

        /// <summary>
        /// 기본 정보
        /// </summary>
        private bool bPitcher;	            //투수여부
        private string m_strName;		    //선수 이름
        private int pos;
        private int order;
        private int curPos;

        private DefineEnum.EPlayerBody body;
        private DefineEnum.EPlayerColor skin;        
        private int face;

        /// <summary>
        /// 타자 능력치
        /// </summary>        
        private int eyeValue, contactValue, powerValue, tandoValue; //타자 기본 능력
        private int fieldValue, throwValue, runningValue;           //야수 기본 능력
        
        /// <summary>
        /// 타자 보너스 능력치
        /// </summary>        
#if _Test_Local 
        public int bonusValue;         //타자만 쓰임, 매타석시 초기화
        public int fieldBonusValue;    //타자만 쓰임, 매타석시 초기화
        public int pileupValue;        //처음만 초기화 되고 특정이벤트시 변동, 투타 다 쓰임
        public int debuffValue;        //디버프 밸류 매타석시 초기화 , 투타 다 쓰임
#else
        private int bonusValue;         //타자만 쓰임, 매타석시 초기화
        private int fieldBonusValue;    //타자만 쓰임, 매타석시 초기화
        private int pileupValue;        //처음만 초기화 되고 특정이벤트시 변동, 투타 다 쓰임
        private int debuffValue;        //디버프 밸류 매타석시 초기화 , 투타 다 쓰임
#endif

        /// <summary>
        /// 타자 평균 능력치
        /// </summary>
        private int offenseAvg, defensAvg, buntPower;

        /// <summary>
        /// 투수 보유 구종
        /// </summary>
        private PitchingArsenal[] ballType = new PitchingArsenal[5];    //구종
        private int[] ballValue = new int[5];    //구위 밸류

        /// <summary>
        /// 투수 평균 능력치
        /// </summary>
        private int pitcherAvg;

        //투수 구위 보너스
        //private int guweeBonus;
        private Dictionary<SkillIndex, int> guweeBonus = new Dictionary<SkillIndex,int>();


        /// <summary>
        /// 구속 가중치
        /// </summary>
        private int velocityValue;
                
        /// <summary>
        /// 투수 스태미너 및 상태
        /// </summary>
        private int staminaValue;                                   //투수 스태미너 능력치  
#if _Test_Local
        public float curStamina, staminaReduceRate;                //현재 스태미너와 스태미너 감소율
        public FatigueStep faitgueStep;                            //지침 스텝
        public PinchStep pinchState;                               //핀치 스텝        
        public int pinchScore;                                     //핀치 점수
#else
        private float curStamina, staminaReduceRate;                //현재 스태미너와 스태미너 감소율
        private FatigueStep faitgueStep;                            //지침 스텝
        private PinchStep pinchState;                               //핀치 스텝        
        private int pinchScore;                                     //핀치 점수
#endif

        /// <summary>
        /// 타자 및 야수의 교체 근거로 사용되는 가치값
        /// </summary>
        private int vsLeftPowerValue, vsRightPowerValue;
        private int runnerPowerValue;
        private int fielderPowerValue;

        /// <summary>
        /// 기록
        /// </summary>        
        const int MAX_RECORD = 44;
        private byte[] record = new byte[MAX_RECORD];   //기록 정보        
        private int[] detailRecord = new int[13];       //마지막은 타점 
        private string resultStr;                       //배팅결과를 string 형태로          
        const int MAX_HITTYPE = 6;
        private byte[] hitType = new byte[MAX_HITTYPE]; //타구의 타입
        //누적 기록
        private GameRecordPitcher pRecord = null;
        private GameRecordHitter bRecord = null;

        //스킬 리스트
        private List<CSkill> skillList = new List<CSkill>();

#if _Test_Local
        //사진 인덱스
        public int picIndex;        
        //public int lineupPlayed, changedOrder; //플레이한 라인업 카운트와, 교체된 순서
#endif
        private int tHand, bHand;
        private int pType;        
        private int bType;      //나중에 지워


        //기타
        public int originLineup;    //출전시 라인업

        public bool bChangeIn;      //교체투입여부
        
        private bool bMissMatch;    //미스매치 여부


        /// <summary>
        /// 카드 정보를 설정 
        /// </summary>
        /// <param name="card"></param>
        public void setCard(GameCardInfo _card)
        {
            this.card = _card;
            // DISABLED_MGRS: playerdata = Mgrs.GameData.GameDB_FindCardByCardID(card.cardId);

            setBonusInit();     //스킬 부가능력 초기화
            setRecordInit();    //기록 초기화
            setSkillInit();     //스킬 초기화

            ////Debug.Log("============>> card id = " + card.cardId);

            skillList.Clear();
            
        }

        

#if _Test_Local        
        //로컬에서 무작위로 인종선택
        private void tempRace(bool bPitcher, int index)
        {
            if (bPitcher == true)
            {
                body = DefineEnum.EPlayerBody.NORMAL;

                /*int range = UnityEngine.Random.Range(1, 10);
                if (range > 3)
                {
                    skin = DefineEnum.EPlayerColor.YELLOW;
                }
                else
                {
                    skin = (DefineEnum.EPlayerColor)range;
                }                                
                if (skin == DefineEnum.EPlayerColor.YELLOW)
                {
                    face = UnityEngine.Random.Range(1, 12);
                }
                else if (skin == DefineEnum.EPlayerColor.WHITE)
                {
                    face = 1101;
                }
                else if (skin == DefineEnum.EPlayerColor.BLACK)
                {
                    face = 1201;
                }*/

                //pvp 임시
                if (index == 0)
                {
                    skin = DefineEnum.EPlayerColor.WHITE;
                    face = 1101;
                }
                else if (index == 2)
                {
                    skin = DefineEnum.EPlayerColor.BLACK;
                    face = 1201;
                }
                else
                {
                    skin = DefineEnum.EPlayerColor.YELLOW;
                    face = 1;
                }
            }
            else
            {
                /*int range = UnityEngine.Random.Range(1, 10);
                if (range > 3)
                {
                    skin = DefineEnum.EPlayerColor.YELLOW;
                }
                else
                {
                    skin = (DefineEnum.EPlayerColor)range;
                }

                if (skin == DefineEnum.EPlayerColor.YELLOW)
                {
                    body = (DefineEnum.EPlayerBody)(UnityEngine.Random.Range(1, 4));
                    if (body == DefineEnum.EPlayerBody.NORMAL)
                    {
                        face = UnityEngine.Random.Range(1, 12);
                    }
                    else if (body == DefineEnum.EPlayerBody.MUSCLE)
                    {
                        face = UnityEngine.Random.Range(1001, 1005);
                    }
                    else if (body == DefineEnum.EPlayerBody.FAT)
                    {
                        face = UnityEngine.Random.Range(2001, 2004);
                    }
                }
                else if (skin == DefineEnum.EPlayerColor.WHITE)
                {
                    body = DefineEnum.EPlayerBody.MUSCLE;
                    face = 1101;
                }
                else if (skin == DefineEnum.EPlayerColor.BLACK)
                {
                    body = DefineEnum.EPlayerBody.MUSCLE;
                    face = 1201;
                }*/


                //pvp 임시
                if (index == 2)
                {
                    skin = DefineEnum.EPlayerColor.BLACK;
                    body = DefineEnum.EPlayerBody.MUSCLE;
                    face = 1201;
                }
                else if (index == 7)
                {
                    skin = DefineEnum.EPlayerColor.WHITE;
                    body = DefineEnum.EPlayerBody.MUSCLE;
                    face = 1101;
                }
                else
                {
                    //임시                    
                    if (index == 3 || index == 5 || index == 6)
                    {
                        body = DefineEnum.EPlayerBody.FAT; //3 5 6
                        if (index == 3) face = 2001;
                        else if(index == 5) face = 2002;
                        else  face = 2003;
                    }
                    else if (index == 4)
                    {
                        body = DefineEnum.EPlayerBody.MUSCLE; //2 4 7
                        face = 1004;
                    }
                    else
                    {
                        body = DefineEnum.EPlayerBody.NORMAL; //0 1 8
                        face = index;
                    }
                    skin = DefineEnum.EPlayerColor.YELLOW;
                    
                }
            }
        }

        /// <summary>
        /// 로컬에서 선수정보 세팅
        /// </summary>
        /// <param name="card"></param>
        public void setIdentity(string name, int pos, int lineup, int throwHand, int hitHand, int batterType, int pitcherType, int curPos, int index)
        {
            
            m_strName = name;
            bPitcher = pos == CPlayer._PITCHER ? true : false;	            //투수여부
            this.pos = pos;
            this.order = lineup;
            this.tHand = throwHand;
            this.bHand = hitHand;
            //this.bType = batterType;
            this.pType = pitcherType;
            this.curPos = curPos;

            //Debug.Log("=============>> tempRace index = " + index);
            tempRace(bPitcher, index);            
            setSkillTemp();
            bType = UnityEngine.Random.Range(0, 7);
            pType = 0;// UnityEngine.Random.Range(0, 3);

            originLineup = order;

            //테스트용
            this.tHand = _RIGHTHAND;
            this.bHand = _RIGHTHAND;

            if (bPitcher == true)
            {
                if (index % 2 ==1) this.tHand = _LEFTHAND;
            }
            else
            {
                if(index == 2 || index == 3 || index == 6)
                {
                    this.tHand = _LEFTHAND;
                    this.bHand = _LEFTHAND;
                }
                else
                {
                    this.tHand = _RIGHTHAND;
                    this.bHand = _RIGHTHAND;
                }
            }

            velocityValue = 100;

        }
#else
        /// <summary>
        /// 얼굴및 피부색 체형 세팅
        /// </summary>
        private void setFace()
        {
            int colorValue = (int)playerdata.PlayerColorType;
            skin = (DefineEnum.EPlayerColor)Mathf.Clamp(colorValue, 1, 3);

            int bodyValue = (int)playerdata.PlayerBodyType;
            body = (DefineEnum.EPlayerBody)Mathf.Clamp(bodyValue, 1, 3);

            face = playerdata.face;


            if (bPitcher == true)
            {
                body = DefineEnum.EPlayerBody.NORMAL;
            }

            //얼굴 예외 처리
            if (body == DefineEnum.EPlayerBody.NORMAL)
            {
                if(bPitcher == true)
                {
                    if(face == 1101 || face == 1201)
                    {
                        //백인, 흑인
                    }
                    else
                    {
                        if(face < 1 || face > 11) face = 1;
                    }
                }
                else
                {
                    if(face < 1 || face > 11) face = 1;
                }
            }
            else if (body == DefineEnum.EPlayerBody.MUSCLE)
            {
                if (face == 1101 || face == 1201)
                {
                    //백인, 흑인
                }
                else
                {
                    if (face < 1001 || face > 1004) face = 1001;
                }
            }
            else if (body == DefineEnum.EPlayerBody.FAT)
            {
                if(face < 2001 || face > 2003) face = 2001;
            }
        }

        /// <summary>
        /// 손 세팅
        /// </summary>
        private void setHand()
        {
            tHand = _RIGHTHAND;
            bHand = _RIGHTHAND;
            pType = _OVERTHROW;

            //임시
            bType = UnityEngine.Random.Range(0, 7);

            if (bPitcher == true)
            {
                if (playerdata.eMainHander == DefineEnum.EMainHander.RORH
                  || playerdata.eMainHander == DefineEnum.EMainHander.ROLH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LORH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LOLH
                  || playerdata.eMainHander == DefineEnum.EMainHander.ROSH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LOSH)
                {
                    //오버
                    pType = _OVERTHROW;

                }
                else if (playerdata.eMainHander == DefineEnum.EMainHander.RURH
                  || playerdata.eMainHander == DefineEnum.EMainHander.RULH
                  || playerdata.eMainHander == DefineEnum.EMainHander.RUSH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LURH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LULH
                  || playerdata.eMainHander == DefineEnum.EMainHander.LUSH)
                {
                    pType = _UNDERHAND;
                }
                else
                {
                    pType = _SIDEARM;
                }
            }

            //던지는 손 (0~1)
            if (playerdata.eMainHander == DefineEnum.EMainHander.ROLH         //HanderType.RP_RH)   //우투우타
              || playerdata.eMainHander == DefineEnum.EMainHander.ROLH         //HanderType.RP_LH    //우투좌타 
              || playerdata.eMainHander == DefineEnum.EMainHander.ROSH          //HanderType.RP_BH)   //우투양타
              || playerdata.eMainHander == DefineEnum.EMainHander.RURH          //HanderType.RU_RH)   //우언우타
              || playerdata.eMainHander == DefineEnum.EMainHander.RULH          //HanderType.RU_LH)   //우언좌타
              || playerdata.eMainHander == DefineEnum.EMainHander.RUSH          //HanderType.RU_BH)   //우언양타
              || playerdata.eMainHander == DefineEnum.EMainHander.RSRH         //HanderType.RS_RH    //우사우타
              || playerdata.eMainHander == DefineEnum.EMainHander.RSLH         //HanderType.RS_LH    //우사좌타
              || playerdata.eMainHander == DefineEnum.EMainHander.RSSH)       //HanderType.RS_BH)   //우사양타
            {
                tHand = _RIGHTHAND;
            }
            else
            {
                tHand = _LEFTHAND;
            }

            //배팅하는 손
            if (playerdata.eMainHander == DefineEnum.EMainHander.RORH             //HanderType.RP_RH)   //우투우타
              || playerdata.eMainHander == DefineEnum.EMainHander.LORH             //HanderType.LP_RH)   //좌투우타
              || playerdata.eMainHander == DefineEnum.EMainHander.RURH             //HanderType.RU_RH)   //우언우타
              || playerdata.eMainHander == DefineEnum.EMainHander.RSRH            //HanderType.RS_RH)   //우사우타
              || playerdata.eMainHander == DefineEnum.EMainHander.LURH            //HanderType.LU_RH)   //좌언우타
              || playerdata.eMainHander == DefineEnum.EMainHander.LSRH)           //HanderType.LS_RH)   //좌사우타
            {
                bHand = _RIGHTHAND;
            }
            else if (playerdata.eMainHander == DefineEnum.EMainHander.ROLH        //HanderType.RP_LH)   //우투좌타
                   || playerdata.eMainHander == DefineEnum.EMainHander.LOLH        //HanderType.LP_LH)   //좌투좌타
                   || playerdata.eMainHander == DefineEnum.EMainHander.RULH        //HanderType.RU_LH)   //우언좌타
                   || playerdata.eMainHander == DefineEnum.EMainHander.RSLH       //HanderType.RS_LH)   //우사좌타
                   || playerdata.eMainHander == DefineEnum.EMainHander.LULH       //HanderType.LU_LH)   //좌언좌타
                   || playerdata.eMainHander == DefineEnum.EMainHander.LSLH)      //HanderType.LS_LH)   //좌사좌타
            {
                bHand = _LEFTHAND;
            }
            else
            {
                bHand = _SWITCHHAND;
            }  

        }

        /// <summary>
        /// 서버로부터 선수정보 세팅
        /// </summary>
        /// <param name="card"></param>
        public void setIdentity(int orderIndex)
        {
            if (Utils.CardUtils.detectCardTypeFrom(card.cardId) == CardType.Legend)
            {
                m_strName = string.Format("{0}", playerdata.name);
            }
            else
            {
                m_strName = string.Format("{0}{1}", playerdata.name, playerdata.year.ToString().Substring(2));// playerInfo.name + " " + playerdata.year.ToString().Substring(2, 2);
            }
            //투수여부 세팅
            bPitcher = (card.PlayerType == PlayerType.Pitcher ? true : false); 
            //보직 설정
            if (bPitcher == false)
            {
                ////Debug.Log("===============>>타자 " + m_strName + " 오더 " + orderIndex);
                order = orderIndex;// card.odr - 101;
                pos = (int)(playerdata.ePosition - DefineEnum.EPosition.C) + 1;
                if (order < 9)
                {
                    if (card.lineup == Lineup.DH)
                    {
                        curPos = CPlayer._DH;
                    }
                    else
                    {
                        curPos = (card.lineup - Lineup.C) + 1;
                    }
                }
                else
                {
                    curPos = CPlayer._BENCH;
                }

                originLineup = order;

            }
            else
            {                
                pos = _PITCHER;
                ////Debug.Log("===============>>투수 " + m_strName + " 오더 " + orderIndex + "라인업 = " + card.lineup);
                
                //투수일때는 보직순 (1~5선발, 6~7필승조, 8~9패전조, 10 셋업, 11 마무리)
                if (card.lineup == Lineup.SP)
                {
                    order = (int)PitcherPosotion.STARTER;
                }                
                else if (card.lineup == Lineup.CP)
                {
                    order = (int)PitcherPosotion.SAVE;
                }
                else
                {
                    //이후에 능력치로 정렬
                    if (orderIndex == 10)
                    {
                        order = (int)PitcherPosotion.SETUP;
                    }
                    else if (orderIndex == 6 || orderIndex == 7)
                    {
                        order = (int)PitcherPosotion.RELIEF;
                    }
                    else
                    {
                        order = (int)PitcherPosotion.CHASE;
                    }
                }
                originLineup = orderIndex;
                curPos = CPlayer._PITCHER;
            }
            //구속가중치 초기화
            velocityValue = 100;

            //교체투입여부 초기화
            bChangeIn = false;

            //미스매치 초기화
            bMissMatch = false;

            //스킬세팅
            skillSetting();

            //얼굴세팅 및 예외처리
            setFace();

            //던지는 손 처리
            setHand();
        }
#endif



        /// <summary>
        /// 보너스값 초기화
        /// </summary>
        /// <param name="card"></param>
        public void setBonusInit()
        {
            //각종 보너스값 초기화
            setBonusValue(0);
            setPileupValue(0);
            setFieldBounsValue(0);
            setDebuffValue(0);
        }

        
        /// <summary>
        /// 포지션 세팅
        /// </summary>
        /// <param name="position"></param>
        public void setPosition(int position)
        {
            this.pos = position;
        }

        
        /// <summary>
        /// 오더 세팅
        /// </summary>
        /// <param name="index"></param>
        public void setOrder(int index)
        {
            order = index;
        }

        /// <summary>
        /// 현재 포지션을 세팅한다
        /// </summary>
        /// <param name="pos"></param>
        public void setCurPos(int pos)
        {
            curPos = pos;
        }

        /// <summary>
        /// 미스매치 여부
        /// </summary>
        /// <param name="value"></param>
        public void setMissMatch(bool value)
        {
            bMissMatch = value;
        }


        
        /// <summary>
        /// 카드값을 얻어온다.
        /// </summary>
        /// <returns></returns>
        public GameCardInfo getCard()
        {
            return card;
        }


        public card getPlayerData()
        {
            return playerdata;
        }
                
        /// <summary>
        /// 이름을 얻어온다
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return m_strName;
        }
                
        /// <summary>
        /// 주 포지션 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getPosition()
        {
            return pos;
        }
                
        /// <summary>
        /// 타순 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getOrder()
        {
            //타자일때는 타순(1~9번, 10~14번 벤치), 
            return order;//(card.odr - 1);
        }
                
        /// <summary>
        /// 현재 포지션 얻어옴
        /// </summary>
        /// <returns></returns>
        public int getCurPos()
        {
            return curPos;
        }

        /// <summary>
        /// 미스매치 여부
        /// </summary>
        /// <returns></returns>
        public bool getMissMatch()
        {
            return bMissMatch;
        }
                
        /// <summary>
        /// 투수의 보직을 얻어온다
        /// </summary>
        /// <returns></returns>
        public int getPitcherPosition()
        {
            //투수일때는 보직순 (1: 1선발, 2: 2선발, 3:3선발, 4: 4선발, 5: 5선발, 6: 필승조1, 7: 필승조2, 8:패전조1, 9: 패전조2, 10: 셋업, 11: 마무리)
            return order;
        }
                
        /// <summary>
        /// 던지는 손 얻어온다
        /// </summary>
        /// <returns></returns>
        public int getThrowHand()
        {
            return tHand;
            
        }
                
        /// <summary>
        /// 치는 손 얻어온다
        /// </summary>
        /// <returns></returns>
        public int getHitHand()
        {
            //때리는 손 (0~2)
            return bHand;            
        }
                
        /// <summary>
        /// 타격 폼을 얻어온다
        /// </summary>
        /// <returns></returns>
        public int getBattingType()
        {
            //타자타입 (0~?)
            return bType;
        }
                
        /// <summary>
        /// 피처 타입 얻어온다
        /// </summary>
        /// <returns></returns>
        public int getPitchingType()
        {
            return pType;
            
        }
                
        /// <summary>
        /// 스타일을 얻어온다
        /// </summary>
        /// <returns></returns>
        public DefineEnum.EPlayerColor getSkin()
        {
            return skin;
        }


        public DefineEnum.EPlayerBody getBody()
        {
            return body;
        }

        public int getFace()
        {
            return face;
        }


#if _Test_Local
        /// <summary>
        /// 로컬에서의 타자 능력치 세팅
        /// </summary>        
        public void setBatterAbility(int eye, int contact, int power, int tando, int _catch, int _throw, int _speed)
        {
            eyeValue = eye;
            contactValue = contact;
            powerValue = power;
            tandoValue = tando;

            fieldValue = _catch; 
            throwValue = _throw; //
            runningValue = _speed; //

            if (pos > CPlayer._SHORTSTOP)
            {
                if (throwValue < 800) throwValue = 800;
            }

            offenseAvg = (contactValue + eyeValue + powerValue) / 30;
            defensAvg = (fieldValue + throwValue) / 20;
            buntPower = (contactValue + eyeValue + runningValue) / 30;

        }
#else
        /// <summary>
        /// 서버로부터 얻어온 정보에 의해 타자 능력치 세팅
        /// </summary>       
        public void setBatterAbility()
        {
            if (bPitcher == false)
            {
                foreach (KeyValuePair<CardAbCode, int[]> value in card.abilities)
                {
                    CardAbCode key = value.Key;
                    int[] bValue = value.Value;

                    int realValue = 0;// (bValue[0] + bValue[1] + bValue[2] + bValue[3]) * 10;
                    for (int i = 0; i < bValue.Length; i++) realValue += (bValue[i] * 10);

                    if (key == CardAbCode.TJ)
                    {
                        // 타구각
                        tandoValue = realValue;
                    }
                    else if (key == CardAbCode.CT)
                    {
                        // 컨텍
                        contactValue = realValue;
                    }
                    else if (key == CardAbCode.BE)
                    {
                        // 선구
                        eyeValue = realValue;
                    }
                    else if (key == CardAbCode.TW)
                    {
                        // 송구
                        throwValue = Mathf.Clamp((realValue), 300, 2000);
                    }
                    else if (key == CardAbCode.FD)
                    {
                        // 수비
                        fieldValue = Mathf.Clamp((realValue), 300, 2000); 
                    }
                    else if (key == CardAbCode.RN)
                    {
                        // 주력
                        runningValue = Mathf.Clamp((realValue), 300, 2000);
                    }
                    else if (key == CardAbCode.PW)
                    {
                        // 파워
                        powerValue = realValue;
                    }                    
                }

                offenseAvg = (contactValue + eyeValue + powerValue) / 30;
                defensAvg = (fieldValue + throwValue) / 20;
                buntPower = (contactValue + eyeValue + runningValue) / 30;

                //Debug.Log("[타자능력]====>선구: " + eyeValue + "====>컨택: " + contactValue + "====>파워: " + powerValue + "====>탄도: " + tandoValue);
                //Debug.Log("[필드능력]====>수비: " + fieldValue + "====>송구: " + throwValue + "====>주력: " + runningValue);
            }
            else
            {
                //투수 기본 능력치 셋팅
                eyeValue = 100;
                contactValue = 100;
                powerValue = 100;
                tandoValue = 100;
                fieldValue = 500;
                throwValue = 500;
                runningValue = 500;

                offenseAvg = 30;
                defensAvg = 70;
            }
            
        }
#endif


        /// <summary>
        /// 타력 보너스값 세팅
        /// </summary>        
        public void setBonusValue(int value)
        {
            bonusValue = value * 10;
        }

        /// <summary>
        /// 타력 보너스값 추가
        /// </summary>   
        public void setPileupValue(int value)
        {
            pileupValue += (value * 10);
            if (pileupValue < 0) pileupValue = 0;
        }

        /// <summary>
        /// 필딩 보너스값 추가
        /// </summary>   
        public void setFieldBounsValue(int value)
        {
            fieldBonusValue = value * 10;
        }

        public void setDebuffValue(int value)
        {
            debuffValue = value * 10;
            if (debuffValue > 0) debuffValue = -debuffValue;
        }

        /// <summary>
        /// 타자 타석에서의 보너스 밸류
        /// </summary>
        /// <returns></returns>
        public int getBonusValue()
        {
            return (bonusValue + pileupValue + debuffValue);
        }

        /// <summary>
        /// 야수 필드에서의 보너스 밸류
        /// </summary>
        /// <returns></returns>
        public int getFieldBonusValue()
        {
            return fieldBonusValue;
        }
              
        
        /// <summary>
        /// 선구 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getEye()
        {
            return (int)(BattingMechanism.EYE_VALUE * eyeValue);
        }

        
        /// <summary>
        /// 컨택 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getContact()
        {
            return (int)(BattingMechanism.CONTACT_VALUE * contactValue);
        }

        
        /// <summary>
        /// 파워 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getPower()
        {
            return (int)(BattingMechanism.POWER_VALUE * powerValue);
        }

        
        /// <summary>
        /// 탄도 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getTando()
        {
            return (int)(BattingMechanism.TANDO_VALUE * tandoValue);
        }

        
        /// <summary>
        /// 수비 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getFielding()
        {
            /*if (bMissMatch == true)
            {
                //30%감소
                return (fieldValue * 7) / 10;
            }
            else*/
            {
                return fieldValue;
            }
        }

        /// <summary>
        /// 송구 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getThrowing()
        {
            /*if (bMissMatch == true)
            {
                //30%감소
                return (throwValue * 7) / 10;
            }
            else*/
            {
                return throwValue;
            }
        }
                
        /// <summary>
        /// 주루 얻어오기
        /// </summary>
        /// <returns></returns>
        public int getSpeed()
        {
            return runningValue;
        }

        /// <summary>
        /// 공격 평균값
        /// </summary>
        /// <returns></returns>
        public int getOffenseAvg()
        {
            return offenseAvg;
        }

        /// <summary>
        /// 번트파워
        /// </summary>
        /// <returns></returns>
        public int getBuntPower()
        {
            return buntPower;
        }

        /// <summary>
        /// 수비 평균값
        /// </summary>
        /// <returns></returns>
        public int getDefenseAvg()
        {
            return defensAvg;
        }

        
#if _Test_Local        
        /// <summary>
        /// 로컬에서 투수 능력치 세팅
        /// </summary>
        public void setPitcherAbility(int spd, int con, int stm, int hard, int sharp, int team, int index)
        {
            //볼스피드, 컨트롤, 스태미너
            //ballSpeedValue = spd;
            //controlValue = con;
            staminaValue = stm;

            if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
            {
                //구종
                ballType[0] = pvpmanager.Get().pitch1;// PitchingArsenal.FASTBALL; //(PitchingArsenal)UnityEngine.Random.Range(1, 6); //PitchingArsenal.FASTBALL;
                                                      //변화구 구종&구위
                ballType[1] = pvpmanager.Get().pitch2;//PitchingArsenal.CURVE;// SINKING_FAST;// .CURVE; //(PitchingArsenal)UnityEngine.Random.Range(6, 11); //PitchingArsenal.CURVE;
                ballType[2] = pvpmanager.Get().pitch3;//PitchingArsenal.CIRCLE;// SLIDER; //(PitchingArsenal)UnityEngine.Random.Range(18, 23); //PitchingArsenal.SLIDER;
                ballType[3] = pvpmanager.Get().pitch4;//PitchingArsenal.SLIDER;// .FORK; //(PitchingArsenal)UnityEngine.Random.Range(13, 18); //PitchingArsenal.FORK;
                ballType[4] = pvpmanager.Get().pitch5;//PitchingArsenal.SINKER;//.FORK;// SLOW_CURVE;//.CIRCLE;

                //구위
                ballValue[0] = pvpmanager.Get().PITCHING_STAT + UnityEngine.Random.Range(0, 100); // UnityEngine.Random.Range(500, 800);
                ballValue[1] = pvpmanager.Get().PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[2] = pvpmanager.Get().PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[3] = pvpmanager.Get().PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[4] = pvpmanager.Get().PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
            }
            else
            {
                ballType[0] = PitchingArsenal.FASTBALL; //(PitchingArsenal)UnityEngine.Random.Range(1, 6); //PitchingArsenal.FASTBALL;
                                                      //변화구 구종&구위
                ballType[1] = PitchingArsenal.CURVE;// SINKING_FAST;// .CURVE; //(PitchingArsenal)UnityEngine.Random.Range(6, 11); //PitchingArsenal.CURVE;
                ballType[2] = PitchingArsenal.CIRCLE;// SLIDER; //(PitchingArsenal)UnityEngine.Random.Range(18, 23); //PitchingArsenal.SLIDER;
                ballType[3] = PitchingArsenal.SLIDER;// .FORK; //(PitchingArsenal)UnityEngine.Random.Range(13, 18); //PitchingArsenal.FORK;
                ballType[4] = PitchingArsenal.SINKER;//.FORK;// SLOW_CURVE;//.CIRCLE;

                //구위
                ballValue[0] = tempSelectPage.PITCHING_STAT + UnityEngine.Random.Range(0, 100); // UnityEngine.Random.Range(500, 800);
                ballValue[1] = tempSelectPage.PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[2] = tempSelectPage.PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[3] = tempSelectPage.PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
                ballValue[4] = tempSelectPage.PITCHING_STAT + UnityEngine.Random.Range(0, 100); //UnityEngine.Random.Range(500, 800);
            }

            pitcherAvg = 0;
            int pcount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (ballType[i] != PitchingArsenal.NONE)
                {
                    pitcherAvg += (ballValue[i] / 10);
                    pcount++;
                }
            }
            if (pcount > 0)
            {
                pitcherAvg = pitcherAvg / pcount;
            }
            
            //스태미너 세팅
            staminaInit(getPitcherPosition() == 0 ? true : false, pcount);
        }
#else        
        /// <summary>
        /// 서버로부터 얻어온 정보로 투수 능력치 세팅
        /// </summary>
        public void setPitcherAbility()
        {
            //서버로부터 투수의 능력치 세팅
            if (bPitcher == true)
            {
                for (int i = 0; i < 5; i++) ballType[i] = PitchingArsenal.NONE; //초기화
                
                foreach (KeyValuePair<CardAbCode, int[]> value in card.abilities)
                {
                    int index;
                    CardAbCode key = value.Key;
                    int[] pValue = value.Value;

                    int realValue = 0;// //int realValue = (pValue[0] + pValue[1]) * 10;
                    for (int i = 0; i < pValue.Length; i++) realValue += (pValue[i] * 10);

                    if (key == CardAbCode.SM)
                    {
                        // 체력
                        staminaValue = realValue;
                    }
                    else if (key == CardAbCode.VC)
                    {
                        // 구속 가중치
                        velocityValue = pValue[0];
                    }
                    else if (key == CardAbCode.FF)
                    {
                        //직구류
                        index = (int)PitchType.FASTBALL;
                        if(card.pitchTypes.ContainsKey(key) == false)
                        {
                            ballType[index] = PitchingArsenal.FASTBALL;
                        }
                        else
                        {
                            if (card.pitchTypes == null || card.pitchTypes[key] == null)
                            {
                                ballType[index] = PitchingArsenal.FASTBALL;
                            }
                            else
                            {
                                ballType[index] = (PitchingArsenal)Enum.Parse(typeof(PitchingArsenal), card.pitchTypes[key]); //
                            }
                        }
                        ballValue[index] = realValue;
                    }
                    else if (key == CardAbCode.CU)
                    {
                        // 체인지업류
                        index = (int)PitchType.CHANGEUP;
                        if (card.pitchTypes.ContainsKey(key) == false)
                        {
                            ballType[index] = PitchingArsenal.CHANGEUP;
                        }
                        else
                        {
                            if (card.pitchTypes == null || card.pitchTypes[key] == null)
                            {
                                ballType[index] = PitchingArsenal.CHANGEUP;
                            }
                            else
                            {
                                ballType[index] = (PitchingArsenal)Enum.Parse(typeof(PitchingArsenal), card.pitchTypes[key]); //ballType[index] = PitchingArsenal.CHANGEUP; 
                            }
                        }
                        ballValue[index] = realValue;
                    }
                    else if (key == CardAbCode.SD)
                    {
                        // 슬라이더류
                        index = (int)PitchType.SLIDER;
                        if (card.pitchTypes.ContainsKey(key) == false)
                        {
                            ballType[index] = PitchingArsenal.SLIDER;
                        }
                        else
                        {
                            if (card.pitchTypes == null || card.pitchTypes[key] == null)
                            {
                                ballType[index] = PitchingArsenal.SLIDER;
                            }
                            else
                            {
                                ballType[index] = (PitchingArsenal)Enum.Parse(typeof(PitchingArsenal), card.pitchTypes[key]); //ballType[index] = PitchingArsenal.SLIDER; 
                            }
                        }
                        ballValue[index] = realValue;
                    }
                    else if (key == CardAbCode.CV)
                    {
                        // 커브류
                        index = (int)PitchType.CURVE;
                        if (card.pitchTypes.ContainsKey(key) == false)
                        {
                            ballType[index] = PitchingArsenal.CURVE;
                        }
                        else
                        {
                            if (card.pitchTypes == null || card.pitchTypes[key] == null)
                            {
                                ballType[index] = PitchingArsenal.CURVE;
                            }
                            else
                            {
                                ballType[index] = (PitchingArsenal)Enum.Parse(typeof(PitchingArsenal), card.pitchTypes[key]); //ballType[index] = PitchingArsenal.CURVE; 
                            }
                        }
                        ballValue[index] = realValue;
                    }
                    else if (key == CardAbCode.FB)
                    {
                        // 포크볼류
                        index = (int)PitchType.FORK;
                        if (card.pitchTypes.ContainsKey(key) == false)
                        {
                            ballType[index] = PitchingArsenal.FORK;
                        }
                        else
                        {
                            if (card.pitchTypes == null || card.pitchTypes[key] == null)
                            {
                                ballType[index] = PitchingArsenal.FORK;
                            }
                            else
                            {
                                ballType[index] = (PitchingArsenal)Enum.Parse(typeof(PitchingArsenal), card.pitchTypes[key]); //ballType[index] = PitchingArsenal.FORK; 
                            }
                        }
                        ballValue[index] = realValue;
                    }
                }

                pitcherAvg = 0;
                int pcount = 0;
                for (int i = 0; i < 5; i++)
                {
                    if (ballType[i] != PitchingArsenal.NONE)
                    {
                        pitcherAvg += (ballValue[i] / 10);
                        pcount++;
                    }
                }
                if (pcount > 0)
                {
                    pitcherAvg = pitcherAvg / pcount;
                }
                //스태미너 세팅 
                staminaInit(getPitcherPosition() == 0 ? true : false, pcount);

                //Debug.Log("[투수능력]====>체력: " + staminaValue + "====>직구: " + ballValue[0] + "====>커브: " + ballValue[1]);
                //Debug.Log("[투수능력]====>첸졉: " + ballValue[2] + "====>슬라: " + ballValue[3] + "====>포크: " + ballValue[4]);
            }
            else
            {
                //기본 세팅
                pitcherAvg = 20;
                staminaValue = 300;   //스태미너
                for (int i = 0; i < 5; i++)
                {
                    ballType[i] = (i == 0 ? PitchingArsenal.FASTBALL : PitchingArsenal.NONE);
                    ballValue[i] = 300;
                }
            }

        }
#endif

        /// <summary>
        /// 투수 보너스 세팅
        /// </summary>
        public void setPitcherBonus(SkillIndex skill, int bonus)
        {
            //볼스피드, 컨트롤, 스태미너
            int value = bonus * 10;
            if (guweeBonus.ContainsKey(skill) == true)
            {
                guweeBonus[skill] = value;
            }
            else
            {
                guweeBonus.Add(skill, value);
            }
        }

        /// <summary>
        /// 구위 보너스 얻어오기
        /// </summary>
        public int getGuweeBonus()
        {
            int bouns = 0;
            foreach (KeyValuePair<SkillIndex, int> value in guweeBonus)
            {
                bouns += value.Value;
            }
            //모든 보너스 값을 다 더해서 리턴해줌
            return (bouns + pileupValue + debuffValue);
        }

        /// <summary>
        /// 투수 평균 구위값
        /// </summary>
        /// <returns></returns>
        public int getPitcherAvg()
        {
            return pitcherAvg;
        }
                
        /// <summary>
        /// 스태미너 초기화
        /// </summary>
        private void staminaInit(bool bStarter, int numPitch)
        {
            faitgueStep = FatigueStep.STAMINA_NORMAL;
            curStamina = PitchingMechanism.INIT_STAMINA;

            float H = ((PitchingMechanism.INIT_STAMINA*10) / (float)staminaValue);
            float A = (bStarter ? 1.7f : 6.0f);
            float B;
            if(numPitch == 5) B = 1.0f;
            else if(numPitch == 4) B = 1.25f;
            else B = 1.5f;
            staminaReduceRate = H * A * B;

            if (skillAvailable(SkillIndex.IronArm) == true)
            {
                int scopeRate = getSkillScopeRate(SkillIndex.IronArm);
                staminaReduceRate = (staminaReduceRate * (100 + scopeRate)) / 100.0f;
            }


            ////Debug.Log("===============>>staminaValue = " + staminaValue);
            ////Debug.Log("===============>>staminaReduceRate = " + staminaReduceRate);
        }
                
        /// <summary>
        /// 스태미너 감소
        /// </summary>
        /// <param name="rate"></param>
        public void setStamina()
        {
            //////UnityEngine.//Debug.Log("================>> 투수 체력 감소 증감 비율 :" + addRate);
            /*if (pinchState == PinchStep.Pinch)
            {
                curStamina -= (staminaReduceRate * 2.0f);   //핀치사황 2배 감소
            }
            else
            {
                curStamina -= (staminaReduceRate);
            }
            

            if (faitgueStep == FatigueStep.STAMINA_NORMAL)
            {
                if (curStamina < 50.0f) faitgueStep = FatigueStep.STAMINA_FATIGUE;
            }
            else if (faitgueStep == FatigueStep.STAMINA_FATIGUE)
            {
                //지침
                if (curStamina < 35.0f) faitgueStep = FatigueStep.STAMINA_VERY_FATIGUE;
            }
            else if (faitgueStep == FatigueStep.STAMINA_VERY_FATIGUE)
            {
                //탈진
                if (curStamina < 20.0f) faitgueStep = FatigueStep.STAMINA_EXUSTED;
            }
            else
            {
                //방전
                if (curStamina < 0) curStamina = 0;
            }*/

            //지침상태 삭제
            faitgueStep = FatigueStep.STAMINA_NORMAL;
        }

        /// <summary>
        /// 스태미너 강제세팅
        /// </summary>
        /// <param name="stamina"></param>
        public void setCurrentStamina(float stamina)
        {
            curStamina = stamina;
        }
        
        /// <summary>
        /// //핀치상태 세팅
        /// </summary>
        /// <param name="state"></param>
        public void setPinchState(PinchStep state)
        {
            pinchState = state;
        }
        
        
        /// <summary>
        /// //핀치값 세팅
        /// </summary>
        /// <param name="score"></param>
        public void setPinchScore(int score)
        {
            pinchScore = score;
        }

        
        /// <summary>
        /// //핀치값 감소
        /// </summary>
        /// <param name="parm"></param>
        public void setPinchScoreReduce(int parm)
        {
            pinchScore -= parm;
            if (pinchState == PinchStep.Pinch)
            {
                if (pinchScore <= 0)
                {
                    pinchState = PinchStep.Normal;
                }
            }
        }


        /// <summary>
        /// 스태미나 능력치
        /// </summary>
        /// <returns></returns>
        public int getStaminaValue()
        {
            return staminaValue;
        }


        /// <summary>
        /// 구속 가중치
        /// </summary>
        /// <returns></returns>
        public int getVelocityValue()
        {
            return velocityValue;
        }

        
        /// <summary>
        /// 해당 구종의 구위 얻어오기
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fastballCon"></param>
        /// <returns></returns>
        public int getBallValue(PitchingArsenal index)
        {
            int curIndex = (int)PitchingMechanism.getBallType(index);
            return (int)(PitchingMechanism.GUWEE_VALUE * ballValue[curIndex]);

        }

        public int getBallValue2(int index)
        {
            return (int)(PitchingMechanism.GUWEE_VALUE * ballValue[index]);
        }

        /// <summary>
        /// 인덱스별 구종 타입
        /// </summary>
        public PitchingArsenal [] getBallType()
        {
            return ballType;
        }
                
       

        /// <summary>
        /// 현재 스태미나 얻어오기
        /// </summary>
        public int getCurrentStamina()
        {
            return (int)curStamina;
        }

        
        /// <summary>
        /// 현재 피로도 스텝 얻어오기
        /// </summary>
        public FatigueStep getFatigueStep()
        {
            return faitgueStep;
        }


        /// <summary>
        /// 현재 핀치 스텝 얻어오기
        /// </summary>
        public PinchStep getPinchState()
        {
            return pinchState;
        }

        /// <summary>
        /// 핀치 스코어 얻어오기
        /// </summary>
        public int getPinchScore()
        {
            return pinchScore;
        }

        /// <summary>
        /// 기록 초기화
        /// </summary>
        public void setRecordInit()
        {
            for (int i = 0; i < MAX_RECORD; i++)
            {
                record[i] = 0;
            }
            for (int i = 0; i < MAX_HITTYPE; i++)
            {
                hitType[i] = 0;
            }
            for (int i = 0; i < 13; i++)
            {
                detailRecord[i] = 0;
            }
            resultStr = "오늘 기록 없음";
        }
        
        /// <summary>
        /// 기록 카운트
        /// </summary>
        public void setRecord(int parm, int num = 1)
        {
            record[parm] += (byte)num;
        }

        /// <summary>
        /// 상세 기록
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="inning"></param>
        public void setDetailRecord(Param.DetailRecord parm, int inning)
        {
            detailRecord[inning - 1] = (int)parm;
        }

        /// <summary>
        /// 상세 기록 투수
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="inning"></param>
        public void setDetailRecord2(int index, int value)
        {
            detailRecord[index] = value;
        }

        /// <summary>
        /// 타점체크
        public void setRbiRecord(int rbi)
        {
            detailRecord[12] += rbi;
        }

        /// <summary>
        /// 상세 기록
        /// </summary>
        /// <param name="parm"></param>
        /// <param name="inning"></param>
        public int [] getDetailRecord()
        {
            return detailRecord;
        }

        /// <summary>
        /// 투수의 누적 레코드 세팅
        /// </summary>
        /// <param name="record"></param>
        public void setPitcherRecord(GameRecordPitcher record)
        {
            pRecord = record;
        }

        /// <summary>
        /// 투수의 누적 레코드 가져오기
        /// </summary>
        /// <returns></returns>
        public GameRecordPitcher getPitcherRecord()
        {
            return pRecord;
        }

        /// <summary>
        /// 타자의 누적 레코드 세팅
        /// </summary>
        /// <param name="record"></param>
        public void setBatterRecord(GameRecordHitter record)
        {
            bRecord = record;
        }

        /// <summary>
        /// 타자의 누적 레코드 가져오기
        /// </summary>
        /// <returns></returns>
        public GameRecordHitter getBatterRecord()
        {
            return bRecord;
        }

                
        /// <summary>
        /// 투수의 업적 세팅(승,무,패,완투,완봉)
        /// </summary>
        public void setPitcherAchieve(int parm, int value)
        {
            record[parm] = (byte)value;
        }
                
        /// <summary>
        /// 타자의 타구 기록(플라이, 그라운더, 라이너)
        /// </summary>
        public void setHitType(int parm, int num = 1)
        {
            hitType[parm] += (byte)num;
        }

        /// <summary>
        /// 오늘의 기록을 텍스트로 저장
        /// </summary>
        public void setResultStr(string str)
        {
            if (record[Param.ST_PA] < 2)
            {
                resultStr = str;
            }
            else
            {
                resultStr += "-" + str;
            }

        }
                
        /// <summary>
        /// 해당 인덱스의 스탯 얻어오기
        /// </summary>
        public int getStat(int parm)
        {
            return (int)record[parm];
        }
                
        /// <summary>
        /// 해당 종류의 타구 카운트 얻어오기(몇개의 라이너 혹은 플라이 혹은 그라운더)
        /// </summary>
        public int getHitType(int parm)
        {
            return (int)hitType[parm];
        }
                
        /// <summary>
        /// 오늘 게임의 결과를 텍스트로 얻어오기
        /// </summary>
        public string getResultStr()
        {
            return resultStr;
        }

        /// <summary>
        /// 교체시 이용되는 타자의 공수주 파워 밸류 세팅, 생성시 호출
        /// </summary>
        public void makePowerValue()
        {
            //좀더 복잡하게 계산할것
            bool bLeftBatter = (getHitHand() == CPlayer._LEFTHAND ? true : false);

            int leftCoef = bLeftBatter ? 90 : 110;
            int rightCoef = bLeftBatter ? 110 : 90;

            //타자
            vsLeftPowerValue = (leftCoef * getContact()) + (150 * getPower()) + (leftCoef * getEye());
            vsRightPowerValue = (rightCoef * getContact()) + (150 * getPower()) + (rightCoef * getEye());
            //주자
            runnerPowerValue = (100 * getSpeed());
            //수비
            fielderPowerValue = (120 * getFielding()) + (100 * getThrowing()) + (40 * getSpeed());            
        }
                
        /// <summary>
        /// 선수의 타격 가치값 얻어오기(좌우투수 구분)
        /// </summary>
        public int getBatterPowerValue(bool bLeftPitcher)
        {
            return bLeftPitcher ? vsLeftPowerValue : vsRightPowerValue;
        }
                
        /// <summary>
        /// 선수의 주루 가치값
        /// </summary>
        public int getRunnerPowerValue()
        {
            return runnerPowerValue;
        }
                
        /// <summary>
        /// 선수의 수비 가치값
        /// </summary>
        public int getFielderPowerValue()
        {
            return fielderPowerValue;
        }






        ///////////////////////////////////////////////////////////////////////////////// 
        //선수 스킬 관련 세팅
        /////////////////////////////////////////////////////////////////////////////////  
        /// <summary>
        /// 선수의 세부 스킬 효과
        /// </summary>
        private Dictionary<SkillIndex, CSkill> playerSkills { get; set; } //세부스킬

        
#if _Test_Local

        /// <summary>
        /// 스킬 초기화
        /// </summary>
        public void setSkillInit()
        {
            playerSkills = new Dictionary<SkillIndex, CSkill>();
        }

        //스킬 세팅 - 로컬용
        private void skillAdd(SkillIndex index)
        {
            playerSkills.Add(index, new CSkill((int)index / 10, index, bPitcher));
        }


        public void setSkillTemp()
        {
            if (bPitcher == true)
            {
                if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
                {
                    if (pvpmanager.Get().SKILL_TYPE == 1)
                    {
                        skillAdd(SkillIndex.PitcherBuntFielding);
                        skillAdd(SkillIndex.PitcherJumpCatch);
                        skillAdd(SkillIndex.PitcherReaction);
                    }
                    else if (pvpmanager.Get().SKILL_TYPE == 2)
                    {
                        //견제왕
                        /*    skillAdd(SkillIndex.LaserPickOff);
                            skillAdd(SkillIndex.PitcherQuickMotion);

                            skillAdd(SkillIndex.PitcherBuntFielding);
                            skillAdd(SkillIndex.PitcherJumpCatch);
                            skillAdd(SkillIndex.PitcherReaction);


                            if (Mode.bBattingSPMode == true)
                            {


                            }*/
                    }

                    //견제왕
                    //skillAdd(SkillIndex.LaserPickOff);
                    //skillAdd(SkillIndex.PitcherQuickMotion);

                    //선두타자승부
                    //skillAdd(SkillIndex.SunduKiller);

                    //추격본능
                    //skillAdd(SkillIndex.ChaseInstinct);

                    //불꽃투혼
                    //skillAdd(SkillIndex.FrameFight);

                    //강심장
                    //skillAdd(SkillIndex.SteelHeart);

                    //회심의 일격
                    //skillAdd(SkillIndex.TenderStroke);

                    //매혹
                    //skillAdd(SkillIndex.Charm);

                    //위압
                    //skillAdd(SkillIndex.PitcherOverwhelming);

                    //강철어깨
                    //skillAdd(SkillIndex.IronArm);

                    //카리스마
                    //skillAdd(SkillIndex.Charisma);

                    //닥터K
                    //skillAdd(SkillIndex.DoctorK);

                    //필승의지
                    //skillAdd(SkillIndex.WinSpirit);

                    //제5의 내야수
                    //skillAdd(SkillIndex.PitcherBuntFielding);
                    //skillAdd(SkillIndex.PitcherJumpCatch);
                    //skillAdd(SkillIndex.PitcherReaction);

                }
            }
            else
            {
                if (Mode.gameMode == Mode.GamePlayMode.Pvp433)
                {
                    if (pvpmanager.Get().SKILL_TYPE == 1)
                    {
                        //병살저지
                        //skillAdd(SkillIndex.RunnerDoublePlayBreaker);

                        if (pos == CPlayer._CATCHER)
                        {
                            //수비형포수
                            //skillAdd(SkillIndex.CatcherRunnerBlocking); //주자블로킹
                        }
                        else if (pos <= CPlayer._SHORTSTOP)
                        {
                            //특급송구
                            if (pos > CPlayer._FIRSTBASEMAN) skillAdd(SkillIndex.SpecialThrow);
                            //철벽수비
                            skillAdd(SkillIndex.SpecialCatch);
                        }
                        else if (pos > CPlayer._SHORTSTOP)
                        {
                            skillAdd(SkillIndex.DivingCatch); //다이빙캐치                
                                                              //skillAdd(SkillIndex.OutfieldRange); //수비반경 
                            skillAdd(SkillIndex.HomerunSteal); //홈런스틸
                        }

                        //질주본능
                        //skillAdd(SkillIndex.RunnerHomeRush);   //홈돌진 
                    }
                    else if (pvpmanager.Get().SKILL_TYPE == 2)
                    {
                        //병살저지
                        /*    skillAdd(SkillIndex.RunnerDoublePlayBreaker);
                            skillAdd(SkillIndex.RunnerTurbo);   //터보

                            if (pos == CPlayer._CATCHER)
                            {
                                //수비형포수
                                skillAdd(SkillIndex.CatcherSitThrow);//앉아쏴                
                                skillAdd(SkillIndex.CatcherRunnerBlocking); //주자블로킹
                                skillAdd(SkillIndex.CatcherBallBlocking); //투구블로킹                
                            }
                            else if (pos <= CPlayer._SHORTSTOP)
                            {
                                //특급송구
                                skillAdd(SkillIndex.SpecialThrow);
                                //철벽수비
                                skillAdd(SkillIndex.SpecialCatch);
                            }
                            else if (pos > CPlayer._SHORTSTOP)
                            {
                                //레이저
                                skillAdd(SkillIndex.Laser);
                                skillAdd(SkillIndex.DivingCatch); //다이빙캐치                
                                skillAdd(SkillIndex.OutfieldRange); //수비반경                
                                skillAdd(SkillIndex.HomerunSteal); //홈런스틸
                            }

                            //질주본능
                            skillAdd(SkillIndex.RunnerStealMaster);//대도
                            skillAdd(SkillIndex.RunnerHomeRush);   //홈돌진 
                            skillAdd(SkillIndex.RunnerSliding);    //슬라이딩 

                            if (Mode.bBattingSPMode == true)
                            {
                                //번트의신
                                if (order == 0) skillAdd(SkillIndex.GodOfBunt);

                                //매의눈
                                if (order == 1) skillAdd(SkillIndex.FalconEye);

                                //타자위압
                                if (order == 2) skillAdd(SkillIndex.BatterOverwhelming);

                                //강습타구
                                if (order == 3) skillAdd(SkillIndex.AssaultBall);

                                //찬스맨
                                if (order == 4) skillAdd(SkillIndex.ChanceMan);

                                //뜬금포
                                if (order == 5) skillAdd(SkillIndex.Unexpected);

                            }*/

                    }
                }


                //주루센스
                //skillAdd(SkillIndex.RunnerLead);//리드
                //skillAdd(SkillIndex.RunnerDoublePlayBreaker);//병살저지
                //skillAdd(SkillIndex.RunnerTurbo);   //터보

                //레이저
                //skillAdd(SkillIndex.Laser);

                //매의눈
                //skillAdd(SkillIndex.FalconEye);

                //타자위압
                //skillAdd(SkillIndex.BatterOverwhelming);

                //강습타구
                //skillAdd(SkillIndex.AssaultBall);

                //찬스맨
                //skillAdd(SkillIndex.ChanceMan);

                //번트의신
                //skillAdd(SkillIndex.GodOfBunt);

                //뜬금포
                //skillAdd(SkillIndex.Unexpected);

                //특급송구
                //skillAdd(SkillIndex.SpecialThrow);

                //철벽수비
                //skillAdd(SkillIndex.SpecialCatch);

                //수비형포수
                //skillAdd(SkillIndex.CatcherSitThrow);//앉아쏴                
                //skillAdd(SkillIndex.CatcherBallBlocking); //투구블로킹                
                //skillAdd(SkillIndex.CatcherRunnerBlocking); //주자블로킹

                //도발꾼
                //skillAdd(SkillIndex.CatcherProvoke);
                //skillAdd(SkillIndex.CatcherMeatJil);

                //쇠그물수비
                //skillAdd(SkillIndex.DivingCatch); //다이빙캐치                
                //skillAdd(SkillIndex.OutfieldRange); //수비반경                
                //skillAdd(SkillIndex.HomerunSteal); //홈런스틸

                //질주본능
                //skillAdd(SkillIndex.RunnerStealMaster);//대도
                //skillAdd(SkillIndex.RunnerHomeRush);   //홈돌진 
                //skillAdd(SkillIndex.RunnerSliding);    //슬라이딩 

            }
        }

#else
        /// <summary>
        /// 스킬 초기화
        /// </summary>
        private void setSkillInit()
        {   
            playerSkills = new Dictionary<SkillIndex, CSkill>();
            playerSkills.Clear();
        }

        /// <summary>
        /// 스킬을 인게임에서 쓸수 있도로 세팅
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="index"></param>
        /// <param name="Rank"></param>
        private void skillAdd(int ID, SkillIndex index, int Rank)
        {
            if (playerSkills.ContainsKey(index) == false)
            {
                playerSkills.Add(index, new CSkill(ID, index, Rank, bPitcher));
            }
        }

        /// <summary>
        /// 서버에서 넘어온 ID와 Rank값으로 선수의 스킬을 세팅해준다
        /// </summary>
        private void skillSetting()
        {
            //테스트 -> 지워지워
            //테스트로 특정스킬의 효과 삽입시 이걸 이용
            //skillAdd((int)SkillID.doctor_kwang, SkillIndex.DoctorK, 5);            
            //테스트 -> 지워지워

            if (card.skills != null)
            {
                int count = card.skills.Count;

                for (int i = 0; i < count; i++)
                {
                    int id = card.skills[i].skillId;
                    int rank = card.skills[i].rank;
                    //UnityEngine.//Debug.Log("===============>>> 전설 스킬 아이디 : ID = " + id +" ===============>>> 전설여부 : "+((id / 100) % 100));
                    if (SimulParm.GetCommon().SkillsMap.ContainsKey(id) == true)
                    {
                        List<int?> skillEffect = SimulParm.GetCommon().SkillsMap[id].effects;
                        if (skillEffect != null)
                        {
                            int effectCount = skillEffect.Count;
                            for (int j = 0; j < effectCount; j++)
                            {
                                SkillIndex effectID = (SkillIndex)skillEffect[j];
                                ////Debug.Log("==============================>> effectID = " + effectID);
                                skillAdd(id, effectID, rank);
                            }
                        }
                        else
                        {
                            //혹시 skillEffect가 null로 넘어오면
                            SkillIndex effectID = SimulParm.GetSkillEffect(id);// (SkillIndex)((id * 10) + 1);
                            skillAdd(id, effectID, rank);
                        }
                    }
                }
            }

            
        }
#endif


        /// <summary>
        /// 해당 키의 스킬이 존재하는지 여부
        /// </summary>
        public bool skillAvailable(SkillIndex skill)
        {



            return playerSkills.ContainsKey(skill);
        }


        /// <summary>
        /// 해당 키의 스킬이 존재하는지 여부
        /// </summary>
        public bool skillAvailable_id(SkillIndex skill, int id)
        {
            if (skillAvailable(skill) == true)
            {
                CSkill curSkill = getSkillValue(skill);
                if (curSkill.ID == id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 해당 키의 스킬을 얻어온다
        /// </summary>
        public CSkill getSkillValue(SkillIndex skill)
        {
            return playerSkills[skill];            
        }

        /// <summary>
        /// 필드 스킬 성공여부
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool fieldSkillSuccess(SkillIndex skill)
        {
            if (skillAvailable(skill) == true)
            {
                CSkill curSkill = getSkillValue(skill);
                int per = MyMath.Percent();
                ////Debug.Log("==========================>> 스킬 : " + curSkill.effectIndex + "  발동 per : " + per + " / 발동률 : " + curSkill.invokeRate);
                if (per < pvpmanager.Get().SP_PER) //if (per < curSkill.invokeRate)
                {
                    return true;
                }
            }
            return false;
        }

        public bool fieldSkillSuccessPVP(SkillIndex skill, bool success)
        {
            if (skillAvailable(skill) == true)
            {
                if (success == true) //if (per < curSkill.invokeRate)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 배팅뷰에서의 스킬 성공여부
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="useCount"></param>
        /// <returns></returns>
        public CSkill bvSkillSuccess(SkillIndex skill, Dictionary<SkillID, int> skillUseCount)
        {
            if (skillAvailable(skill) == true)
            {
                CSkill curSkill = getSkillValue(skill);

                skillEffectMap info = SimulParm.GetSkillInfo(curSkill.ID);
                if (info != null)
                {
                    Restriction_Type restriction = info.restriction;
                    int restrictionCount = info.restrictionCount;

                    if (curSkill.bPitcherSkill == bPitcher && restriction != Restriction_Type.Field)
                    {
                        int useCount = skillUseCount[(SkillID)curSkill.ID];
                        if (useCount < restrictionCount || restriction == Restriction_Type.NoRestriction)
                        {
                            int per = MyMath.Percent();
                            ////Debug.Log("==========================>> 스킬 : " + curSkill.effectIndex + "  발동 per : " + per + " / 발동률 : " + curSkill.invokeRate);
                            if (per < curSkill.invokeRate)
                            {
                                return curSkill;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 중첩 스킬 효과
        /// </summary>
        /// <param name="skill">스킬 인덱스</param>
        /// <param name="max">최대 중첩 수</param>
        /// <param name="bActive">true인경우 중첩, false인경우 중첩 해제</param>
        /// <returns>중첩효과 성공시 true리턴</returns>
        public bool setPiledupSkill(SkillIndex skill, int max, bool bActive)
        {
            if (skillAvailable(skill) == true)
            {
                CSkill curSkill = getSkillValue(skill);
                if (bActive == true)
                {
                    if (curSkill.pileupCount < max)
                    {
                        curSkill.pileupCount++;
                        setPileupValue(curSkill.effectValue);
                        return true;
                    }
                }
                else
                {                    
                    setPileupValue(-(curSkill.effectValue * curSkill.pileupCount));
                    curSkill.pileupCount = 0;
                }
            }
            return false;
        }


        /// <summary>
        /// 중첩여부 체크
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool checkPiledEffect(SkillIndex skill, int id)
        {
            if (skillAvailable(skill) == true)
            {
                CSkill curSkill = getSkillValue(skill);
                if (curSkill.ID == id)
                {
                    if (curSkill.pileupCount > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 스킬 랭크 얻어오기
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public int getSkillRank(SkillIndex skill)
        {
            if (skillAvailable(skill) == true)
            {
                return getSkillValue(skill).rank;
            }
            return 0;
        }

        public int getSkillScopeRate(SkillIndex skill)
        {
            if (skillAvailable(skill) == true)
            {
                return getSkillValue(skill).scopeRate;
            }
            return 0;
        }


        public int getSkillEffectValue(SkillIndex skill)
        {
            if (skillAvailable(skill) == true)
            {
                return getSkillValue(skill).effectValue;
            }
            return 0;
        }

        /// <summary>
        /// 발동되어 있는 스킬을 저장해놓은 리스트 얻어옴
        /// </summary>
        /// <returns></returns>
        public List<CSkill> getSkillList()
        {
            return skillList;
        }

        /// <summary>
        /// 해당 스킬이 발동 여부 체크 (효과로 체크)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool checkSkillInvoke(SkillIndex index)
        {
            if (skillList.Count > 0)
            {
                for (int i = 0; i < skillList.Count; i++)
                {
                    if (skillList[i].effectIndex == index)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 해당 스킬이 발동 여부 체크 (아이디로 체크)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool checkSkillInvoke(int ID)
        {
            if (skillList.Count > 0)
            {
                for (int i = 0; i < skillList.Count; i++)
                {
                    if (skillList[i].ID == ID)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        

    }
}






