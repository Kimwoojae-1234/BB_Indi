using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class SimulPlayer
    {
        public const int PLAYING_PLAYER = 25;
        public const int NUM_FIELDER = 14;
        public const int NUM_PITCHER = 11;
        public const int NUM_LINEUP = 10;
        public const int NUM_BATTER = 9;
        public const int PLAYING_TEAM = 2;

        /////////////////////////////////////////////////////////////////
        //플레이어 정보
        /////////////////////////////////////////////////////////////////
        //시즌 팀 정보
        private bool bMyHome;
        private int myStaterIndex, otherStarterIndex;

        //팀스탯
        //private TeamStat teamStat;

        //플레이어 정보
        private CPlayer[,] pFielder; //야수
        private CPlayer[,] pPithcer; //투수
        //플레이어 교체 아웃 여부 (교체 아웃시 true로 설정)
        private bool[,] fielderOut;
        private bool[,] pitcherOut;
        //야수 정보
        private int[] lineupCount; //해당팀의 현재 타순
        private int[] battingCycle; //해당팀의 현재 배팅 사이클
        private int[,] currentPosition; //해당팀 해당선수의 현재 포지션
        //야수 정보
        private int[] starterIndex; //선발의 인덱스
        private int[] pitcherIndex; //현재 등판중인 투수 인덱스


        //투수 교체 리스트
        private List<int>[] pitcherList = new List<int>[2];

        //타자교체 리스트
        private List<CPlayer>[] batterList = new List<CPlayer>[2];

        /////////////////////////////////////////////////////////////////
        //시합 결과 정보
        /////////////////////////////////////////////////////////////////
        //스탯
        private TeamStat gameStat;

        ////////////////////////////////////////////////////////////////////
        //외부 호출용 메쏘드
        ////////////////////////////////////////////////////////////////////      
#if _Test_Local
        //MakePlayer의 로컬 버전 (실게임에서는 사용하지 않고 로컬 테스트용으로만 사용)
        public void MakePlayerLocal(int step)
        {
            makePlayerLocal(step);
        }
#else
        //서버로 받아온 게임 시즌 정보를 통해 선수 데이터를 구축한다.
        public void MakePlayer(bool isMyhome, GameLineup lineup, int homeStarterIndex, int awayStaterIndex)
        {
            this.bMyHome = isMyhome;
            this.myStaterIndex = (bMyHome ? homeStarterIndex : awayStaterIndex);
            this.otherStarterIndex = (bMyHome ? awayStaterIndex : homeStarterIndex);
            makePlayer(lineup);
            setPlayerIndex();

        }

        public void MakePlayerWalkOff(WalkoffPlayGameInfo info)
        {
            this.bMyHome = true;
            myStaterIndex = 1;
            otherStarterIndex = 1;
            makePlayerWalkOff(info);
            setPlayerIndex();
        }


#endif
        

        //현재 배팅하고 있는 타자의 정보를 얻어옴
        public CPlayer GetBatter(int team)
        {
            int index = GetLineupCount(team);
            return pFielder[team, index];
        }

        public void SavePlayerData()
        {
            savePlayerData();
        }

        //다음 타자 정보를 얻어옴
        //public CPlayer GetNextBatter(int team, int next = 1)
        public CPlayer GetNextBatter(int team, int next)
        {
            int index = (GetLineupCount(team) + next) % 9;
            return pFielder[team, index];
        }

        //해당 팀과 인덱스의 야수 데이터를 얻어옴
        public CPlayer GetFielder(int team, int index, bool bSaved) //bool bSaved = false
        {
            if (bSaved == false)
            {
                return pFielder[team, index];
            }
            else
            {
                return pSavedFielder[team, index];
            }

        }

        // 해당 인덱스의 야수를 교체
        // inPlayer: 교체들어오는 선수, outPlayer: 교체 아웃되는 선수
        public void SetFielderChange(int team, int inPlayer, int outPlayer, int changeType)
        {
            setFielderChange(team, inPlayer, outPlayer, changeType);
        }

        //해당 팀과 인덱스의 타자(야수)를 세팅한다(SeasonGameInfo 데이터로부터 야수의 정보를 초기화 할때 호출)
        public void SetBatter(CPlayer player, int team, int index)
        {
            setBatter(player, team, index);
        }

        // 해당 팀과 인덱스의 타자(야수)가 이미 출전했는지 여부를 세팅한다.(교체시 필요)
        public void SetFielderOut(int team, int index, bool bOut) //bool bOut = true
        {
            fielderOut[team, index] = bOut;
        }

        // 해당 팀과 인덱스의 타자(야수)가 이미 출전했는지 여부값을 가져온다
        public bool GetFielderOut(int team, int index, bool bSaved) //bool bSaved = false
        {
            if (bSaved == false)
            {
                return fielderOut[team, index];
            }
            else
            {
                return savedFielderOut[team, index];
            }
        }

        // 해당 팀의 현재 타순값을 가져온다.
        public int GetLineupCount(int team)
        {
            return lineupCount[team];
        }

        // 해당 팀의 현재 타순을 세팅한다.
        public void SetLineupCount(int team)
        {
            setLineupCount(team);
        }

        //현재 타순을 강제로 세팅
        public void SetLineUp(int team, int count)
        {
            lineupCount[team] = count;
        }

        // 해당 팀의 타순 사이클을 가져온다
        public int GetCycle(int team)
        {
            return battingCycle[team];
        }

        // 해당 팀의 타순별 현재 수비 포지션 값을 가져온다
        public int GetCurPosition(int team, int index)
        {
            return currentPosition[team, index];
        }

        //카드 시퀀스로부터 인덱스를 얻어온다
        public int GetFielderIndexFromCard(int team, long seq)
        {
            for (int i = 0; i < NUM_FIELDER; i++)
            {
                if (GetFielder(team, i,false).getCard().cardSeq == seq)
                {
                    return i;
                }
            }
            return -1;
        }


        // 해당 팀의 타순별 현재 수비 포지션 값을 세팅한다. (초기화 메쏘드)
        public void SetCurPosition(int team, int index, int pos)
        {
            currentPosition[team, index] = pos;
        }

        // 현재 등판 중인 투수의 데이터를 가져온다.
        public CPlayer GetPitcher(int team)
        {
            int index = GetPitcherIndex(team);
            return pPithcer[team, index];
        }

        // 해당 인덱스의 투수 데이터를 가져온다
        public CPlayer GetPitcher(int team, int index, bool bSaved)
        {
            if (bSaved == false)
            {
                return pPithcer[team, index];
            }
            else
            {
                return pSavedPithcer[team, index];
            }
        }

        public int GetPitcherIndexFromCard(int team, long seq)
        {
            for (int i = 0; i < NUM_PITCHER; i++)
            {
                GameCardInfo card = GetPitcher(team, i, false).getCard();
                if (card != null)
                {
                    if (card.cardSeq == seq)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        // 해당 팀과 인덱스의 투수를 세팅한다(SeasonGameInfo 데이터로부터 투수의 정보를 초기화 할때 호출)
        public void SetPitcher(CPlayer player, int team, int index)
        {
            setPitcher(player, team, index);
        }

        // 해당 팀의 현재 출전중인 투수의 인덱스를 설정한다. (초기화 혹은 교체시 호출)
        public void SetCurrentPitcherIndex(int team, int index, bool bStarer) //bool bStarer = false
        {
            if (bStarer == true)
            {
                starterIndex[team] = index;
            }
            pitcherIndex[team] = index;

            addPitcherChange(index, team);
        }

        // 해당 팀의 투수가 이미 출전했는지 여부를 세팅한다.
        public void SetPitcherOut(int team, int index, bool bOut) //bool bOut = true
        {
            pitcherOut[team, index] = bOut;
        }

        // 해당 팀의 투수가 이미 출전했는지 여부값을 가져온다.
        public bool GetPitcherOut(int team, int index)
        {
            return pitcherOut[team, index];
        }

        // 선발투수의 인덱스를 리턴해준다
        public int GetStarterIndex(int team)
        {
            return starterIndex[team];
        }

        // 현재 투수의 인덱스를 리턴해준다
        public int GetPitcherIndex(int team)
        {
            return pitcherIndex[team];
        }

        // 지금 현재 투수가 선발투수 여부인지를 알려줌
        public bool IsStartPitcher(int team)
        {
            if (starterIndex[team] == pitcherIndex[team]) return true;
            else return false;
        }

        //선수 데이터 초기화 함수
        public void init()
        {
            pFielder = new CPlayer[PLAYING_TEAM, NUM_FIELDER];
            pPithcer = new CPlayer[PLAYING_TEAM, NUM_PITCHER];

            fielderOut = new bool[PLAYING_TEAM, NUM_FIELDER];
            pitcherOut = new bool[PLAYING_TEAM, NUM_PITCHER];

            //batterIndex = new int[PLAYING_TEAM, NUM_FIELDER];
            lineupCount = new int[PLAYING_TEAM];
            battingCycle = new int[PLAYING_TEAM];
            currentPosition = new int[PLAYING_TEAM, NUM_LINEUP];


            starterIndex = new int[PLAYING_TEAM];
            pitcherIndex = new int[PLAYING_TEAM];

            lineupCount[0] = 0;
            battingCycle[0] = 0;
            lineupCount[1] = 0;
            battingCycle[1] = 0;
            //teamStat = new TeamStat();

            for(int i=0;i<2;i++)
            {
                pitcherList[i] = new List<int>();
                pitcherList[i].Clear();
                batterList[i] = new List<CPlayer>();
                batterList[i].Clear();
            }
        }


        // 유저의 홈 어웨이 여부를 리턴
        public bool isMyHome()
        {
            return this.bMyHome;
        }

        /////////////////////////////////////////////////////////////////////////
        //선수 데이터 생성 관련 메쏘드
        /////////////////////////////////////////////////////////////////////////

#if _Test_Local
        
     

        //makePlayer의 local버전 (실게임에서 사용안함)
        private void makePlayerLocal(int step)
        {
            if (step == 0)
            {
                for (int i = 0; i < NUM_FIELDER; i++)
                {
                    CPlayer player = new CPlayer();
                    //기아타자
                    player.setBonusInit();
                    player.setRecordInit();
                    player.setSkillInit();
                    tempPlayerData.makeFielderData(player, 1, i);
                    SetBatter(player, 1, i);
                }
            }
            else if (step == 1)
            {
                for (int i = 0; i < NUM_FIELDER; i++)
                {
                    CPlayer player = new CPlayer();
                    //삼성타자
                    player.setBonusInit();
                    player.setRecordInit();
                    player.setSkillInit();
                    tempPlayerData.makeFielderData(player, 0, i);
                    SetBatter(player, 0, i);
                }
            }
            else if (step == 2)
            {
                //상대팀
                int starterIndex = Random.Range(0, 5);
                if(Mode.bPvpMode433 == true)
                {
                    starterIndex = pvpmanager.Get().pitcherIndex[1];
                }
                SetCurrentPitcherIndex(1, starterIndex, true);
                for (int i = 0; i < NUM_PITCHER; i++)
                {
                    //Debug.Log("step2 : pitcher index : " + i);
                    CPlayer player = new CPlayer();
                    //기아투수
                    player.setBonusInit();
                    player.setRecordInit();
                    player.setSkillInit();
                    tempPlayerData.makePitcherData(player, 1, i);
                    SetPitcher(player, 1, i);
                }
            }
            else if (step == 3)
            {
                //우리팀
                int starterIndex = Random.Range(0, 5);
                if (Mode.bPvpMode433 == true)
                {
                    starterIndex = pvpmanager.Get().pitcherIndex[0];
                }
                SetCurrentPitcherIndex(0, starterIndex, true);
                for (int i = 0; i < NUM_PITCHER; i++)
                {
                    //Debug.Log("step3 : pitcher index : " + i);
                    CPlayer player = new CPlayer();
                    //삼성투수
                    player.setBonusInit();
                    player.setRecordInit();
                    player.setSkillInit();
                    tempPlayerData.makePitcherData(player, 0, i);
                    SetPitcher(player, 0, i);
                }
            }
            else if (step == 4)
            {
                setPlayerIndex();
            }
        }

#else
        //서버로 부터 전달 받은 SeasonGameInfo데이터를 인게임 선수데이터로 변환하는 메쏘드
        private CPlayer TranslateData(GameCardInfo card, int orderIndex)
        {
            CPlayer player = new CPlayer();
            player.setCard(card);
            player.setIdentity(orderIndex);
            player.setBatterAbility();
            player.setPitcherAbility();
            return player;
        }

        //9회2아웃 전용 플레이어 만들기
        private void makePlayerWalkOff(WalkoffPlayGameInfo info)
        {
            int myIndex = 0;
            int cpuIndex = 1;

            int bCount = 0;
            int pCount = 0;

            GameCardInfo myHitter = info.myHitter;
            GameCardInfo pitcherCard = null;

            int count = info.otherLineup.Count; 
                        
            //상대 선수 세팅
            for (int i = 0; i < count; i++)
            {
                GameCardInfo card = info.otherLineup[i];
                bool bPitcher = (card.PlayerType == PlayerType.Pitcher ? true : false);

                if (bPitcher == false)
                {                    
                    CPlayer player = TranslateData(card, bCount);
                    player.setBonusInit();
                    player.setRecordInit();
                    setBatter(player, cpuIndex, bCount);
                    if (player.getCurPos() == CPlayer._PITCHER) player.setCurPos(CPlayer._DH);
                    ////Debug.Log("===========>> 상대타자 세팅 " + bCount +" ===>> 이름 " + player.getName() + "포지션 " + player.getCurPos());
                    bCount++;
                }
                else
                {
                    if (pCount == 0)
                    {
                        //투수카드 세팅
                        pitcherCard = card;
                        for (int j = 0; j < NUM_PITCHER; j++)
                        {
                            ////Debug.Log("===========>> 상대투수 세팅 " + pCount);
                            CPlayer player = TranslateData(pitcherCard, pCount);
                            player.setBonusInit();
                            player.setRecordInit();
                            setPitcher(player, cpuIndex, pCount);
                            pCount++;
                        }
                    }
                }
            }

            for (int i = bCount; i < NUM_FIELDER; i++)
            {
                //타자 쓰레기값 세팅
                CPlayer player = TranslateData(myHitter, i);
                player.setBonusInit();
                player.setRecordInit();
                setBatter(player, cpuIndex, i);
            }

            
            bCount = 0;
            pCount = 0;

            

            //내 선수 세팅
            for (int i = 0; i < PLAYING_PLAYER; i++)
            {
                if (i < NUM_FIELDER)
                {
                    //내타자 세팅
                    CPlayer player = TranslateData(myHitter, bCount);
                    player.setBonusInit();
                    player.setRecordInit();
                    if (i < 9) player.setCurPos(i + 1);
                    ////Debug.Log("===========>> 내타자 세팅 " + bCount + " ===>> 이름 " + player.getName() + "포지션 " + player.getCurPos());
                    setBatter(player, myIndex, bCount);
                    bCount++;
                }
                else
                {
                    //내투수 세팅 - 쓰레기값 세팅
                    ////Debug.Log("===========>> 내투수 세팅 " + pCount);
                    CPlayer player = TranslateData(pitcherCard, pCount);
                    player.setBonusInit();
                    player.setRecordInit();
                    setPitcher(player, myIndex, pCount);
                    pCount++;
                }
            }


            SetCurrentPitcherIndex(cpuIndex, 0, true);
            SetCurrentPitcherIndex(myIndex, 0, true);

        }

        //서버로 받아온 SeasonGameInfo 데이터를 통해 선수 데이터를 구축하는 메쏘드
        private void makePlayer(GameLineup lineup)
        {
            List<GameCardInfo> myBatter = new List<GameCardInfo>();
            List<GameCardInfo> myPitcher = new List<GameCardInfo>();
            List<GameCardInfo> cpuBatter = new List<GameCardInfo>();
            List<GameCardInfo> cpuPitcher = new List<GameCardInfo>();
           
            if(bMyHome == true)
            {
                //홈팀일 경우
                myBatter = lineup.homeHitters;
                myPitcher = lineup.homePitchers;
                cpuBatter = lineup.awayHitters;
                cpuPitcher = lineup.awayPitchers;
            }
            else
            {
                //원정일 경우
                myBatter = lineup.awayHitters;
                myPitcher = lineup.awayPitchers;
                cpuBatter = lineup.homeHitters;
                cpuPitcher = lineup.homePitchers;
            }

            int myBatterNum = myBatter.Count;
            int myPitcherNum = myPitcher.Count;
            int cpuBatterNum = cpuBatter.Count;
            int cpuPitcherNum = cpuPitcher.Count;


            int starterIndex;
            int myIndex = 0;// (bMyHome == false ? 0 : 1);
            int cpuIndex = 1;// (bMyHome == false ? 1 : 0);
            int pCount = 0;

            //내 타자 세팅
            for (int i = 0; i < myBatterNum; i++)
            {
                GameCardInfo card = myBatter[i];
                CPlayer player = TranslateData(card, i);
                player.setBonusInit();
                player.setRecordInit();
                
                setBatter(player, myIndex, i);
            }
            //내 투수 세팅
            starterIndex = myStaterIndex - 1;            
            ////UnityEngine.//Debug.Log("================>>우리팀 선발 인덱스: " + starterIndex);
            pCount = 0;
            SetCurrentPitcherIndex(myIndex, starterIndex, true);

            for (int i = 0; i < NUM_PITCHER; i++)
            {
                bool bCheck = false;
                if (i < 5)
                {
                    if (i == starterIndex)
                    {
                        //선발 세팅
                        //////UnityEngine.//Debug.Log("================>> 선발세팅");
                        bCheck = true;
                    }
                }
                else
                {
                    //구원 세팅
                    bCheck = true;
                }

                if (bCheck == true)
                {
                    ////Debug.Log("=========================>> pCount = " + pCount);
                    GameCardInfo card = myPitcher[pCount];
                    //////UnityEngine.//Debug.Log("================>> 내 투수 SEQ " + (i + 1) + "번: " + card.cardSeq);
                    CPlayer player = TranslateData(card, i);
                    //////UnityEngine.//Debug.Log("====================>>투수 이름 : " + player.getName());
                    player.setBonusInit();
                    player.setRecordInit();
                    
                                      
                    setPitcher(player, myIndex, i);              
                }
                else
                {
                    //쓰레기값 (출격하지 않는 선발의 인덱스)
                    //////UnityEngine.//Debug.Log("================>> 내투수 쓰레기값 index = " + (i + 1) + "번");
                    CPlayer player = new CPlayer();
                    player.setBonusInit();
                    player.setRecordInit();
                    
                    setPitcher(player, myIndex, i);
                }
                pCount++;
            }

            //cpu 타자 세팅
            for (int i = 0; i < cpuBatterNum; i++)
            {
                GameCardInfo card = cpuBatter[i];
                CPlayer player = TranslateData(card, i);
                //////UnityEngine.//Debug.Log("====================>>타자 이름 : " + player.getName());
                player.setBonusInit();
                player.setRecordInit();
                

                setBatter(player, cpuIndex, i);
            }


            //cpu 투수 세팅
            starterIndex = otherStarterIndex - 1;            
            ////UnityEngine.//Debug.Log("================>>상대팀 선발 인덱스: " + starterIndex);
            pCount = 0;
            SetCurrentPitcherIndex(cpuIndex, starterIndex, true);

            for (int i = 0; i < NUM_PITCHER; i++)
            {
                bool bCheck = false;
                if (i < 5)
                {
                    if (i == starterIndex)
                    {
                        //선발 세팅
                        bCheck = true;
                    }
                }
                else
                {
                    //구원 투수 세팅
                    bCheck = true;
                }

                if (bCheck == true)
                {
                    GameCardInfo card = cpuPitcher[pCount];
                    //////UnityEngine.//Debug.Log("================>> CPU 투수 SEQ " + (i + 1) + "번: " + card.cardSeq);
                    CPlayer player = TranslateData(card, i);
                    //////UnityEngine.//Debug.Log("====================>>투수 이름 : " + player.getName());
                    player.setBonusInit();
                    player.setRecordInit();
                    
                    setPitcher(player, cpuIndex, i);             
                }
                else
                {
                    //쓰레기값 (출격하지 않는 선발의 인덱스)
                    //////UnityEngine.//Debug.Log("================>> 내투수 쓰레기값 index = " + (i + 1) + "번");
                    CPlayer player = new CPlayer();
                    player.setBonusInit();
                    player.setRecordInit();
                    
                    setPitcher(player, cpuIndex, i);
                }
                pCount++;
            }
        }

#endif

        // 각종 인덱스를 설정해준다.
        private void setPlayerIndex()
        {
            for (int i = 0; i < 2; i++)
            {
                currentPosition[i, 9] = CPlayer._PITCHER;   //투수 자리에 설정
                for (int j = 0; j < NUM_FIELDER; j++)
                {
                    bool bOut = false;
                    if (j < 9)
                    {
                        bOut = true;
                        addBatterChange(pFielder[i,j], i);
                    }
                    SetFielderOut(i, j, bOut);
                }
                for (int j = 0; j < NUM_PITCHER; j++)
                {
                    SetPitcherOut(i, j, false);
                }
            }
        }



        /////////////////////////////////////////////////////////////////////////
        //타자 관련 메쏘드
        /////////////////////////////////////////////////////////////////////////
        // 해당 인덱스의 야수를 교체
        // inPlayer: 교체들어오는 선수, outPlayer: 교체 아웃되는 선수
        private void setFielderChange(int team, int inIndex, int outIndex, int changeType)
        {
            ////Debug.Log("================================================================================>>야수교체");

            //임시 버퍼 저장
            CPlayer inPlayer = pFielder[team, inIndex];
            CPlayer outPlayer = pFielder[team, outIndex];
            int curPos = outPlayer.getCurPos();

            addBatterChange(inPlayer, team);

            //오더 체인지
            inPlayer.setOrder(outIndex);
            outPlayer.setOrder(inIndex);

            if (curPos != inPlayer.getPosition())
            {
                //미스매치 세팅 (수비 어깨 감소)
                inPlayer.setMissMatch(true);
            }

            //대타시만 이거 플래그 활성화
            if(changeType < 200) inPlayer.bChangeIn = true;

            inPlayer.setCurPos(curPos);
            outPlayer.setCurPos(CPlayer._BENCH);
            //////UnityEngine.//Debug.Log("====================>> 교체 인 플레이어 " + inPlayer.getName() + "의 라인업 플레이 인덱스 설정 = " + inPlayer.lineupPlayed);

            //데이터 체인지
            pFielder[team, inIndex] = outPlayer;
            pFielder[team, outIndex] = inPlayer;

            //더이상 쓸수 없음
            fielderOut[team, inIndex] = true;
            fielderOut[team, outIndex] = true;
        }

        //해당 팀과 인덱스의 타자(야수)를 세팅한다(SeasonGameInfo 데이터로부터 야수의 정보를 초기화 할때 호출)
        private void setBatter(CPlayer player, int team, int index)
        {
#if _Test_Local  
            //테스트용 - 나중에 지워
            player.setPosition(team == 1 ? tempPlayerData._kiaBatter[index, 0] : tempPlayerData._samsungBatter[index, 0]);
            player.picIndex = index;
#endif

            pFielder[team, index] = player;
            pFielder[team, index].makePowerValue();
            if (index < 9)
            {
                //player.lineupPlayed = index;
                //player.changedOrder = 0;
                if (player.getCurPos() <= CPlayer._DH)
                {
                    SetCurPosition(team, index, player.getCurPos());
                }
            }
            else
            {
                //player.changedOrder = -1;
                //player.lineupPlayed = -1;
            }
        }

        // 해당 팀의 현재 타순을 세팅한다.
        private void setLineupCount(int team)
        {
            lineupCount[team]++;
            //배팅 싸이클
            if (lineupCount[team] > 8)
            {
                lineupCount[team] = 0;
                battingCycle[team]++;	//배팅 사이클도 증가
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //투수 관련 메쏘드
        /////////////////////////////////////////////////////////////////////////
        // 해당 팀과 인덱스의 투수를 세팅한다(SeasonGameInfo 데이터로부터 투수의 정보를 초기화 할때 호출)
        public void setPitcher(CPlayer player, int team, int index)
        {
#if _Test_Local
            player.picIndex = index;
#endif

            player.setPosition(CPlayer._PITCHER);
            //player.changedOrder = 0;
            pPithcer[team, index] = player;
            
        }


        /////////////////////////////////////////////////////////////////////////
        //리와인드 플레이시 저장 정보용
        /////////////////////////////////////////////////////////////////////////
        //플레이어 정보(리와인드 플레이시 필요)
        private CPlayer[,] pSavedFielder; //야수
        private CPlayer[,] pSavedPithcer; //투수
        private bool[,] savedFielderOut;
        private void savePlayerData()
        {
            pSavedFielder = new CPlayer[PLAYING_TEAM, NUM_FIELDER];
            pSavedPithcer = new CPlayer[PLAYING_TEAM, NUM_PITCHER];
            savedFielderOut = new bool[PLAYING_TEAM, NUM_FIELDER];


            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < NUM_FIELDER; j++)
                {
                    //pSavedFielder[i, j] = new CPlayer();
                    pSavedFielder[i, j] = pFielder[i, j];

                    //savedFielderOut[i, j] = new bool();
                    savedFielderOut[i, j] = fielderOut[i, j];
                }
                for (int j = 0; j < NUM_PITCHER; j++)
                {
                    //pSavedPithcer[i, j] = new CPlayer();
                    pSavedPithcer[i, j] = pPithcer[i, j];
                }
            }

        }




        ////////////////////////////////////////////////////////////////////
        //시즌 모드에서 투수 기록 결과
        ////////////////////////////////////////////////////////////////////
        //서버에 보내기 위한 투수 결과 얻어오기
        public List<GameRecordPitcher> getPitcherResult(int team)
        {
            List<GameRecordPitcher> pitcherResult = new List<GameRecordPitcher>();
            for (int j = 0; j < NUM_PITCHER; j++)
            {
                CPlayer player = GetPitcher(team, j,false);
                if (player.getStat(Param.ST_PNP) > 0) //투구수 0보다 큰경우
                {
#if _Test_Local 
                    
#else               //add                    
                    pitcherResult.Add(setPitcherRecord(player));
#endif
                }
            }
            return pitcherResult;
        }


        //투수의 기록을 세팅
        private GameRecordPitcher setPitcherRecord(CPlayer player)
        {
            GameRecordPitcher p1 = new GameRecordPitcher();
            //SEQ
            p1.cardSeq = player.getCard().cardSeq;
            //id
            p1.cardId = player.getCard().cardId;
            p1.cardPw = Utils.TeamPowerUtils.calCardPower(player.getCard());
            //이닝
            p1.pOC = player.getStat(Param.ST_IP);
            /// 삼진
            p1.pSO = player.getStat(Param.ST_PSO);
            /// 볼넷
            p1.pBB = player.getStat(Param.ST_PBB);
            /// 피안타
            p1.pH = player.getStat(Param.ST_PH);
            /// 실점
            p1.pRA = player.getStat(Param.ST_PR);
            /// 자책
            p1.pER = player.getStat(Param.ST_PER);
            /// 에러
            p1.pE = player.getStat(Param.ST_PE);
            /// 와일드피치
            p1.pWP = player.getStat(Param.ST_PWP);
            /// 힛바이피치
            p1.pHBP = player.getStat(Param.ST_PHBP);
            /// 피홈런
            p1.pHR = player.getStat(Param.ST_PHR);
            /// 피2루타
            p1.p2B = player.getStat(Param.ST_P2B);
            /// 피3루타
            p1.p3B = player.getStat(Param.ST_P3B);
            /// 투구수
            p1.pNP = player.getStat(Param.ST_PNP);
            /// 피타수
            p1.pTBF = player.getStat(Param.ST_TBF);
            /// 승
            p1.pW = (player.getStat(Param.ST_PW) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            /// 패
            p1.pL = (player.getStat(Param.ST_PL) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            /// 홀드
            p1.pHLD = (player.getStat(Param.ST_HLD) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            /// 세이브
            p1.pSV = (player.getStat(Param.ST_SV) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            /// 블론세이브
            p1.pBS = (player.getStat(Param.ST_BS) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            //완투
            p1.pCG = (player.getStat(Param.ST_CG) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);
            //완봉
            p1.pSHO = (player.getStat(Param.ST_SHO) == Param.P_ACHIEVE_COMPLETE ? 1 : 0);

            //땅볼    
            p1.pGO = player.getHitType(Param.ST_GROUNDER);
            /// 플라이
            p1.pAO = player.getHitType(Param.ST_FLY);
            // 라이터
            p1.pLO = player.getHitType(Param.ST_LINER);

            return p1;
        }


        ////////////////////////////////////////////////////////////////////
        //시즌 모드에서 타자 기록 결과
        ////////////////////////////////////////////////////////////////////
        //서버에 보내기 위한 타자의 결과 얻어오기
        public List<GameRecordHitter> getHitterResult(int team)
        {
            List<GameRecordHitter> hitterResult = new List<GameRecordHitter>();
            for (int j = 0; j < NUM_FIELDER; j++)
            {
                ////////UnityEngine.Debug.Log("====> i = " + i + " ====>>j = " + j);
                CPlayer player = GetFielder(team, j, false);
                if (GetFielderOut(team, j, false) == true)
                {
                    /*필드, 주루 기록 체크용
                    Debug.Log(player.getName() + "포지션 "+ player.getPosition() + " ====> 득점:" + player.getStat(Param.ST_R) + " ====> 병살:" + player.getStat(Param.ST_DP) + " ====> 도루:" + player.getStat(Param.ST_SBS)
                        + " ====> 도루자:" + player.getStat(Param.ST_SBF) + " ====> 자살:" + player.getStat(Param.ST_PO) + " ====> 보살:" + player.getStat(Param.ST_A)
                        + " ====> 도루허용:" + player.getStat(Param.ST_SBA) + " ====> 도루저지:" + player.getStat(Param.ST_CS));
                    */
#if _Test_Local     //

#else               
                    //ADD
                    hitterResult.Add(setHitterRecord(player));
#endif
                }
            }
            return hitterResult;
        }

        //타자의 기록을 세팅
        private GameRecordHitter setHitterRecord(CPlayer player)
        {
            GameRecordHitter h1 = new GameRecordHitter();
            /// 타자 카드 아이디 (내팀인 경우 cardSeq, 상대팀인 경우 cardId)
            ///SEQ
            h1.cardSeq = player.getCard().cardSeq;
            ///ID
            h1.cardId = player.getCard().cardId;
            h1.cardPw = Utils.TeamPowerUtils.calCardPower(player.getCard());
            /// 타석
            h1.hPA = player.getStat(Param.ST_PA);
            /// 타수
            h1.hAB = player.getStat(Param.ST_AB);
            /// 1루타 -> 1루타 수가 아닌 안타수
            h1.hH = player.getStat(Param.ST_H);
            // 2루타
            h1.h2B = player.getStat(Param.ST_2B);
            // 3루타
            h1.h3B = player.getStat(Param.ST_3B);
            // 홈런
            h1.hHR = player.getStat(Param.ST_HR);
            /// 타점
            h1.hRBI = player.getStat(Param.ST_RBI);
            /// 득점
            h1.hR = player.getStat(Param.ST_R);
            /// 볼넷
            h1.hBB = player.getStat(Param.ST_BB);
            /// 사구
            h1.hHBP = player.getStat(Param.ST_HBP);
            /// 삼진
            h1.hSO = player.getStat(Param.ST_SO);
            /// 병살타
            h1.hGDP = player.getStat(Param.ST_DP);
            /// 도루성공
            h1.hSB = player.getStat(Param.ST_SBS);
            /// 도루실패
            h1.hCS = player.getStat(Param.ST_SBF);
            /// 자살
            h1.fPO = player.getStat(Param.ST_PO);
            /// 보살
            h1.fA = player.getStat(Param.ST_A);
            /// 에러
            h1.fE = player.getStat(Param.ST_E);
            /// 도루허용(포수)
            h1.fSBA = player.getStat(Param.ST_SBA);
            /// 도루저지(포수)
            h1.fCS = player.getStat(Param.ST_CS);

            //힛타입
            //땅볼    
            h1.hGO = player.getHitType(Param.ST_GROUNDER);
            /// 플라이
            h1.hAO = player.getHitType(Param.ST_FLY);
            // 라이터
            h1.hLO = player.getHitType(Param.ST_LINER);
            /// 땅볼 Hit
            h1.hGB = player.getHitType(Param.ST_GROUNDERHIT);
            /// 플라이 Hit
            h1.hFB = player.getHitType(Param.ST_FLYHIT);
            // 라이터 Hit
            h1.hLB = player.getHitType(Param.ST_LINERHIT);

            return h1;
        }


        ////////////////////////////////////////////////////////////////////
        //시즌 모드에서 팀 스탯
        ////////////////////////////////////////////////////////////////////
        public TeamStat getTeamStat()
        {
            return gameStat;
        }

        //서버로 전송하기전 팀 스탯을 저장
        public void setTeamStat(SimulGameInfo info, int curInning, bool topInning)
        {
            int inning = curInning;// SimulManager.GetInstance().currentInning;
            bool bTopInning = topInning;// SimulManager.GetInstance().bTopInning;

            gameStat = new TeamStat();
            //////UnityEngine.//Debug.Log("============>>팀스탯 저장");
            for (int i = 0; i < 2; i++)
            {
                gameStat.score[i] = info.run[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 득점: " + stat.score[i]);
                gameStat.hitCount[i] = info.hit[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 안타: " + stat.hitCount[i]);
                gameStat.errorCount[i] = info.error[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 에러: " + stat.errorCount[i]);
                gameStat.hrCount[i] = info.homerun[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 홈런: " + stat.hrCount[i]);
                gameStat.stealCount[i] = info.steal[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 도루: " + stat.stealCount[i]);
                gameStat.kCount[i] = info.strikeout[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 삼진: " + stat.kCount[i]);
                gameStat.dpCount[i] = info.doubleplay[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 병살: " + stat.dpCount[i]);
                gameStat.bbCount[i] = info.fourBall[i];
                ////UnityEngine.Debug.Log("[팀득점]============>>" + (i + 1) + "팀 포볼: " + stat.bbCount[i]);

                for (int j = 0; j < SimulGameInfo.MAX_INNING; j++)
                {
                    gameStat.inningScore[i, j] = info.inningScore[i, j];
                }
            }

            info.DEBUG_COUNTER_RESULT();

        }


        public void addPitcherChange(int pIndex, int team)
        {
            pitcherList[team].Add(pIndex);
        }

        public List<int> getPitcherChangeList(int team)
        {
            return pitcherList[team];
        }


        public void addBatterChange(CPlayer batter, int team)
        {
            batterList[team].Add(batter);
        }

        public List<CPlayer> getBatterChangeList(int team)
        {
            return batterList[team];
        }


    }
}
