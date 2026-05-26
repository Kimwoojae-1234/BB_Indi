using UnityEngine;
using System.Collections;
using WebConnector;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class runnerManager : MonoBehaviour
    {

        const int initZOrder = 8;
        public const int RunnerZOrder = 4;
        const int _NUM_BASE = 4;


        //오브젝트
        public BallPlayManager manager;
        public Pitcher pitcher;
        public Batter batter;
        public Field field;
        //runnerManager run;
        public Batting battingview;
        
        //주자 오브젝트
        public Runner[] runner = new Runner[4];
        public bool[] runnerActive = new bool[4];

        //득점 계산용
        public bool[] runnerRunScore = new bool[4];   //득점 여부 플래그
        public int[] runnerLastPos = new int[4];    //
        public CPlayer[] runnerData = new CPlayer[4];    //득점 계산용 데이터

        //인덱스
        public int runnerRegenIndex;            //리젠 인덱스
        public int nLastRunnerPos;	            //0노주자 1:1루 2:2루 3:3	
        public int nHitterRunnerIndex;			//타자주자 인덱스
        public int n2ndBaseTagIndex;	        //2루주자의 인덱스를 알기위함...
        public int arrayIndex;                  //runner 배열 관련 인덱스
        public int StealIndex, StartBase;       //도루관련  

        //러닝 관련 플래그
        public bool bHitterRunnerSafe;	        //타자주자가 세이프된 경우
        public bool bRunDownCase;               //런다운 플래그
        public bool bStealBase, bHomeSteal, bSecondBaseSteal;     //도루관련
        public bool bPickOff;
        public bool bRunnerHomeRun, bHomeRunEventOver, bRunnerFoul; //홈런 러닝
        public bool bRunnerWalk; //포볼 러닝
        public bool bHitAndRun; //히트앤드런
        public bool bOnlyOneBaseFlag;   //한베이스만 갈것
        public bool bWildPitchRunning;  //폭투 주루
        
        //베이스 러닝 관련 플래그
        public bool[] bOnBase = new bool[_NUM_BASE];	    //베이스에 사람이 있는지?
        public bool[] bOnBase2 = new bool[_NUM_BASE];	    //베이스에 사람이 있는지 2중 체크?
        public bool[] bBaseWait = new bool[_NUM_BASE];	    //대기중이거나 혹은 되돌아 가는 중
        public bool[] bOnRunning = new bool[_NUM_BASE];	    //0인경우 홈과 1루 사이
        public bool[] bOnBackRunning = new bool[_NUM_BASE];
        public bool[] bBaseTagFree = new bool[_NUM_BASE];	//배이스 택 플래그 --> true이면 주자는 베이스 택 가능
        public bool[] bForceOutFlag = new bool[_NUM_BASE];	//true 인 경우 봉살		
        public bool[] bBallOnBase = new bool[_NUM_BASE];	//볼이 베이스에 도착한 경우	//베이스를 벗어 날수 있는지 여
        public bool[] bBaseTagCheck = new bool[_NUM_BASE];  //베이스택관련
        public bool[] bBaseTagGo = new bool[_NUM_BASE];  //베이스택관련

        public HomeShobu homeShobu;

        //기타
        public bool bThirdBaseTag;  //3루주자 베이스택
        public bool bNotOutRunning; //타자주자 낫아웃러닝



        //텍스쳐 초기화
        public bool bRunnerTextureInit = false;


        //리와인드 관련
        public int rewindRunnerIndex;

        //인스턴스 초기화
        public void initInstance(Field field)
        {
            /*
            bOnBase = new bool[_NUM_BASE];	    //베이스에 사람이 있는지?
            bOnBase2 = new bool[_NUM_BASE];	    //베이스에 사람이 있는지 2중 체크?
            bBaseWait = new bool[_NUM_BASE];	    //대기중이거나 혹은 되돌아 가는 중
            bOnRunning = new bool[_NUM_BASE];	    //0인경우 홈과 1루 사이
            bOnBackRunning = new bool[_NUM_BASE];
            bBaseTagFree = new bool[_NUM_BASE];	//배이스 택 플래그 --> true이면 주자는 베이스 택 가능
            bForceOutFlag = new bool[_NUM_BASE];	//true 인 경우 봉살		
            bBallOnBase = new bool[_NUM_BASE];	//볼이 베이스에 도착한 경우	//베이스를 벗어 날수 있는지 여
            bBaseTagCheck = new bool[_NUM_BASE];  //베이스택관련
            bBaseTagGo = new bool[_NUM_BASE];  //베이스택관련

            runner = new Runner[4];
            runnerActive = new bool[4];
            runnerRunScore = new bool[4];
            runnerLastPos = new int[4];
            runnerData = new CPlayer[4]; //기록 보관용
            */

            //bRunnerTextureInit = false;
            /*this.field = field;
            manager = field.manager;
            pitcher = field.pitcher;
            batter = field.batter;
            battingview = field.battingview;
            //master = field.master;

            transform.parent = field.transform;*/
            //transform.localPosition = Vector3.zero;
        }

        ///////////////////////////////////////////////////////
        //초기화,업데이트, 파괴 관련 함수
        ///////////////////////////////////////////////////////
        //텍스쳐 로딩
        public void loadTexture()
        {
            // if (bRunnerTextureInit == false)
            {
                int index = manager.bTopInning ? InGameDebug._TOP_INNING_INDEX : InGameDebug._BOTTOM_INNING_INDEX;
                int rIndex = getAvailableIndex();
                ////UnityEngine.//Debug.Log("======================>>load Runner Texture :: index = " + index);
                AtlasAsset atlasdata = runner[rIndex].anim.skeletonDataAsset.atlasAssets[0];
                Material[] materials = atlasdata.materials;
                materials[0].mainTexture = (Texture)Resources.Load("MainGame/spineData/field/runner/pack" + index + "/runnerAtlas");
                // bRunnerTextureInit = true;
            }
        }

        //러너 매니저 초기화 함수 initInning에서만 호출
        public void initRunner()	//InitIning에서 호출
        {
            //인덱스
            runnerRegenIndex = 0;
            nHitterRunnerIndex = 0;
            nLastRunnerPos = 0;
            bHitterRunnerSafe = false;
            //배열 관련
            arrayIndex = 0;

            bNotOutRunning = false;

            for (int i = 0; i < 4; i++)
            {
                runnerActive[i] = false;
                bOnBase2[i] = false;
                bOnBase[i] = false;			//	
                bForceOutFlag[i] = false;
                bBaseTagFree[i] = false;
                bOnRunning[i] = false;
                bOnBackRunning[i] = false;
            }

        }

        //러너 매니저 초기화 함수 배팅뷰에서 필드뷰로 전환시 호출
        public void initRunner2()
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                bBallOnBase[i] = false;
                bOnRunning[i] = false;
                bBaseWait[i] = false;
                bBaseTagCheck[i] = false;
                bBaseTagGo[i] = false;
                bBaseTagFree[i] = (i == FieldParm.HOMEBASE_INDEX ? true : false);	//타자주자 태그 항상 참값
                runnerRunScore[i] = false;

                if (runnerActive[i] == true)
                {
                    runner[i].bOneBaseMore = false;

                    //runner[i].bAheadCount = 0;
                }
            }
        }

        //액티브한 주자의 초기 셋팅
        private void setRunnerTransform()
        {
            //////////UnityEngine.//Debug.Log("=================>>setRunnerMoveOnBase");
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].initSetting2();
                }
            }
        }

        //타자 주자 생성함수(initBatter에서 호출)
        public void makeHitterRunner(CPlayer player)
        {
            //////UnityEngine.//Debug.Log("=========================>>makeHitterRunner : bHitterRunnerSafe = " + bHitterRunnerSafe);
            //러너 오브젝트 생성
            GameObject _runner = Util.Load("MainGame/prefabs/FieldViewPrefab/runnerPrefab", transform, Vector3.zero, "runner" + runnerRegenIndex);

            if (bHitterRunnerSafe == false)
            {
                //////UnityEngine.//Debug.Log("=========================>>destroyRunner");
                //타자주자가 죽은경우 해당 인덱스 삭제
                destroyRunner(nHitterRunnerIndex);
            }

            nHitterRunnerIndex = runnerRegenIndex;
            
            
            arrayIndex = getAvailableIndex();
            ////UnityEngine.Debug.Log("실시간 엔진================>>arrayIndex = " + arrayIndex + "from rewind data 엔진: rewindRunnerIndex = " + rewindRunnerIndex);

            if (arrayIndex == -1)
            {
                //////UnityEngine.Debug.Log("#############   FATAL ERROR   ########################");
                //////////UnityEngine.//Debug.Log("===========================>>> Hitter Runner Make Fail");
                return;
            }

            //초기화
            runner[arrayIndex] = _runner.GetComponent<Runner>();
            runner[arrayIndex].loadRunner(player, manager.bTopInning);
            runner[arrayIndex].initSetting(player, field, nHitterRunnerIndex, (manager.bMyTurn ? 0 : 1), arrayIndex);
            runner[arrayIndex].initSpecialFlag();
            runner[arrayIndex].bChangedRunner = false;
            runner[arrayIndex].lineupCount = batter.curLineupCount;
            runnerActive[arrayIndex] = true;

            bHitterRunnerSafe = false; //타자주자 1루이상 진루여부
            //bRunnerTextureInit = false;

            //주자 특능 설정


            //초기화
            runner[arrayIndex].currentPos = FieldParm.HOMEBASE_INDEX;	//현재 홈
            runner[arrayIndex].lastPos = FieldParm.HOMEBASE_INDEX;	//현재 홈

            //플래그
            bForceOutFlag[FieldParm.FIRSTBASE_INDEX] = true;	//타자주자는 무조건 봉살임..			
            bOnBase[FieldParm.HOMEBASE_INDEX] = false;          //홈베이스 on은 false처리

            //상태 설정
            runner[arrayIndex].setStandby(BaseArriveMotion._NORMAL);
            runner[arrayIndex].bLead = false;
            runner[arrayIndex].bGrounderWaitFlag = false;

            //좌우타자 옵셋
            runner[arrayIndex].posX += (batter.batterHand == CPlayer._LEFTHAND ? 120 : -50); //우타자 좌타자 위치 offset
            runner[arrayIndex].posY += 20;


            //주루 기록관련 
            runner[arrayIndex].bLastPitcher = false;

            //주루 AI관련 

            
            //미니맵용 임시
            runner[arrayIndex].runnerName = player.getName();
            runner[arrayIndex].runnerRating = (int)(player.getSpeed());// / 10);
            //////UnityEngine.//Debug.Log("=================================================================================>>runner[index].runnerRating = " + runner[arrayIndex].runnerRating);

            IngameUI.GetFieldUI().MakeMinimapRunner(manager, runner[arrayIndex], manager.bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex);

            //리젠 인덱스 증가 -> 해당 이닝이 끝날때까지 무한 증가
            runnerRegenIndex++;

        }

        public void changeRunner(CPlayer player, int baseIndex)
        {
            int index = getRunnerIndex(baseIndex);

            runner[index].initSetting(player, field, -1, (manager.bMyTurn ? 0 : 1), index);
            runner[index].initSpecialFlag();
            runner[index].bChangedRunner = false;

            
            //미니맵용 임시
            runner[index].runnerName = player.getName();
            runner[index].runnerRating = (int)(player.getSpeed());// / 10);
            ////UnityEngine.//Debug.Log("=================================================================================>>"+player.getName()+" runner[index].runnerRating = " + runner[index].runnerRating);

            
        }

        //찬스모드에서 주자를 재생성할때 사용
        public Runner makeChanceRunner(CPlayer player, int basePosIndex)
        {
            ////UnityEngine.//Debug.Log("==========================================>> make chance runner~~!!!! basePosIndex = " + basePosIndex);
            GameObject _runner = Util.Load("MainGame/prefabs/FieldViewPrefab/runnerPrefab", transform, Vector3.zero, "runner" + runnerRegenIndex);

            arrayIndex = getAvailableIndex();
            ////UnityEngine.//Debug.Log("==========================================>> make chance runner~~!!!! arrayIndex = " + arrayIndex);
            
            if (arrayIndex == -1)
            {
                //UnityEngine.Debug.Log("#############   FATAL ERROR   ########################");
                //////////UnityEngine.//Debug.Log("===========================>>> Hitter Runner Make Fail");
                return null;
            }

            //초기화
            runner[arrayIndex] = _runner.GetComponent<Runner>();
            runner[arrayIndex].loadRunner(player, manager.bTopInning);
            runner[arrayIndex].initSetting(player, field, runnerRegenIndex, (manager.bMyTurn ? 0 : 1), arrayIndex);
            runner[arrayIndex].initSpecialFlag();
            runner[arrayIndex].bChangedRunner = false;
            runnerActive[arrayIndex] = true;

            //초기화
            runner[arrayIndex].currentPos = basePosIndex;	//현재 홈
            runner[arrayIndex].lastPos = basePosIndex;	//현재 홈

            //플래그
            bOnBase[basePosIndex] = true;          //홈베이스 on은 false처리

            //상태 설정
            runner[arrayIndex].setStandby(BaseArriveMotion._NORMAL);
            
            
            //주루 기록관련 
            runner[arrayIndex].bLastPitcher = true;

            
            //미니맵용 임시
            runner[arrayIndex].runnerName = player.getName();
            runner[arrayIndex].runnerRating = (int)(player.getSpeed());// / 10);

            IngameUI.GetFieldUI().MakeMinimapRunner(manager, runner[arrayIndex], manager.bTopInning ? SimulPlayerManager.awayTeamIndex : SimulPlayerManager.homeTeamIndex);
            
            //리젠 인덱스 증가 -> 해당 이닝이 끝날때까지 무한 증가
            runnerRegenIndex++;

            return runner[arrayIndex];

        }

        //updateFieldScene에서 초기화가 필요한 runner의 상태조정을 위해 호출한다.
        public void updateRunner()
        {
            ////UnityEngine.//Debug.Log("===========================>>UPDATE RUNNER");
            for (int i = 0; i < 4; i++)
            {
                bOnBase[i] = false;
                runnerRunScore[i] = false;  //득점 플래그 초기화
            }

            for (int i = 0; i < 4; i++)
            {
                //////UnityEngine.//Debug.Log("===========================>>runnerActive[i] = " + runnerActive[i]);
                if (runnerActive[i] == true)
                {
                    runner[i].setUpdate();
                    int curBase = runner[i].currentPos;
                    if (curBase != FieldParm.HOMEBASE_INDEX && curBase != -1)
                    {
                        bOnBase[curBase] = true;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                //bOnBase[i] = false;
                bOnRunning[i] = false;
                bOnBackRunning[i] = false;
            }
                        
            bRunnerWalk = false;
            bRunDownCase = false;
            bRunnerHomeRun = false;
            bPickOff = false;
            bWildPitchRunning = false;

            homeShobu = HomeShobu._NONE;
            bNotOutRunning = false;

        }

        //주자를 스탠바이 상태로
        public void setRunnerStandBy()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != nHitterRunnerIndex)
                    {
                        if ((int)runner[i].state >= (int)RunState.STANDBY)
                        {
                            runner[i].setStandby(BaseArriveMotion._NORMAL);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 번트 스피드 설정
        /// </summary>
        public void setBuntSpeed()
        {
            //번트시 투수 딜레이 이전으로 돌려놓음
            field.fielder[CPlayer._PITCHER].FIELD_DELAY = FieldingMechanism.getFieldDelay(500);

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].setBuntSpeed();
                }
            }
        }


        //주자 위치 초기화
        public void setRunnerInitPos()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].setRunnerInitPos();
                }
            }
        }

        //타자주자 파괴 함수 -> destroyCall이 true일때 호출
        void destroyRunner(int index)
        {
            
                //리와인드인경우 이곳으로 오지 않는다.
                ////UnityEngine.Debug.Log("=##########################################################========================>>RunnerManager destroyRunner체크");
                int destroyIndex = -1;

                for (int i = 0; i < 4; i++)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].runnerIndex == index)
                        {
                            destroyIndex = i;
                            break;
                        }
                    }
                }
                if (destroyIndex == -1)
                {
                    ////UnityEngine.Debug.Log("#############   FATAL ERROR   ########################");
                    ////////UnityEngine.//Debug.Log("===========================>>> Runner Destroy Fail");
                    return;
                }
                GameObject desObj = runner[destroyIndex].gameObject;
                Destroy(desObj);
                runnerActive[destroyIndex] = false;
                //manager.activeField.destroyRunner(destroyIndex);
        }

        //파괴할 주자가 있는지 체크
        public void checkDestroyRunner()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    //if (runner[i].destroyCall == true)
                    {
                        destroyRunner(runner[i].runnerIndex);
                        runner[i].destroyCall = false;
                    }
                }
            }
        }

        //타자주자 제외한 나머지 주자 없앰
        public void DestroyRunnerExceptHitterRunner()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != nHitterRunnerIndex)
                    {
                        destroyRunner(runner[i].runnerIndex);
                        runner[i].destroyCall = false;
                    }
                }
            }
        }

        public void runnerInit()
        {
            runnerRegenIndex = 0;
            for(int i=0;i<4;i++)
            {
                bOnBase[i] = false;
                bOnBase2[i] = false;
                bBaseWait[i] = false;
                bOnRunning[i] = false;
                bOnBackRunning[i] = false;
                bBaseTagFree[i] = false;
                bForceOutFlag[i] = false;
                bBallOnBase[i] = false;
                bBaseTagCheck[i] = false;
                bBaseTagGo[i] = false;
            }
            checkDestroyRunner();
            Util.RemoveChild(transform);
        }


        public void disableRunner()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].bRunnerActive = false;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        //주자 인덱스 관련 메쏘드
        ////////////////////////////////////////////////////////////////////////////////
        //0~3중 사용 가능한 index를 찾아 리턴하는 함수
        int getAvailableIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == false)
                {
                    return i;
                }
            }
            return -1; //DEBUG 용
        }

        //0~3중 타자주자의 array index를 찾아 리턴하는 함수
        public int getHitterRunnerArrayIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex == nHitterRunnerIndex)
                    {
                        return i;
                    }
                }
            }
            return -1; //DEBUG 용
        }

        //타자주자
        public Runner getHitterRunner()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex == nHitterRunnerIndex)
                    {
                        return runner[i];
                    }
                }
            }
            return null; //DEBUG 용
        }

        //해당 베이스에 있는 주자의 array index(0~3값)
        public int getRunnerIndex(int basePos)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkActive() == true)
                    {
                        if (runner[i].currentPos == basePos) return i;
                    }
                }
            }
            return -1;
        }
        
        //특정 루로 향하는 주자의 array Index(0~3값)
        public int getRunnerDestIndex(int destPos)
        {
            int i;
            for (i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                    {
                        //	System.out.println("f_currentPos[i] = "+f_currentPos[i]);
                        if (runner[i].destPos == destPos) return i;
                    }
                }
            }
            return -1;
        }

        //특정 루로 향하는 주자의 Runner 오브젝트값 리턴
        public Runner getDestRunner(int dest)
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].destPos == dest)
                    {
                        return runner[i];
                    }
                }
            }
            return null;
        }

        //특정 루로 향하는 주자의 Runner 오브젝트값 리턴
        public Runner getRunner(int baseIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].currentPos == baseIndex)
                    {
                        return runner[i];
                    }
                }
            }
            return null;
        }

        //선행주자가 향하는 위치를 구하는 함수
        public int getFirstRunnerDest(int posIndex, int notThisBase)
        {
            int dest = FieldParm.RELAY_INDEX;// FieldParm.FIRSTBASE_INDEX;

            int baseIndex = 0;

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                    {
                        if (runner[i].destPos < notThisBase)
                        {
                            if (runner[i].destPos > dest)
                            {
                                dest = runner[i].destPos;
                                baseIndex = i;
                            }
                        }
                    }
                }
            }

            /*
            //외야 땅볼시 일부러 1루에 던질 필요 없게 만든다.
            if (posIndex >= CPlayer._LEFTFIELDER)
            {
                if (dest == FieldParm.FIRSTBASE_INDEX)
                {
                    if (bOnRunning[baseIndex] == true && bOnBackRunning[baseIndex] == false)
                    {
                        //////////UnityEngine.//Debug.Log("========================>>외야 강제 릴레이 포지션");
                        dest = FieldParm.RELAY_INDEX;
                    }
                }
            }*/
            return dest;
        }

        //현주자의 다음 목적지에 선행주자가 머물러 있거나 아직 도착 하지 않은 경우 그 선행주자의 인덱스를 얻어오는 함수
        //그런 주자가 없는 경우 -1 리턴
        public int getForeRunnerStandbyStateIndex(int cur, int curIndex)
        {
            int fore = (cur + 1) % 4;

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != curIndex)
                    {
                        if (runner[i].currentPos == fore)// && runner[i].nState == RunningMechnism.STANDBY)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }


        //현주자의 이전 목적지에 뒷주자가 지금 현주자의 위치로 움직이는 경우 그 뒷주자의 인덱스를 얻어오는 함수
        //그런 주자가 없는 경우 -1 리턴
        public int getBackRunnerRunningStateIndex(int cur, int curIndex)
        {
            int back = (cur - 1) % 4;

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != curIndex)
                    {
                        if (runner[i].currentPos == back && 
                           ((int)runner[i].state == (int)RunState.MOVE && runner[i].bMoveForward == true))
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }


        //현재 볼이 위치한 베이스의 인덱스를 리턴 볼이 베이스위에 있지 않으면 -1 값을 리턴
        public int getCurBallBase()
        {
            for (int i = 0; i < 4; i++)
            {
                if (bBallOnBase[i] == true) return i;
            }

            return -1;
        }

        ////////////////////////////////////////////////////////////////////////////////
        //주루 AI (Before Fielding)
        ////////////////////////////////////////////////////////////////////////////////
        //루상에 존재하는 주자의 수를 리턴
        public int getHowManyRunners()
        {
            int i, count;
            count = 0;

            for (i = 0; i < 4; i++)
            {
                if (i != nHitterRunnerIndex)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                        {
                            count++;
                        }
                    }
                }

            }
            return count;
        }


        //현재 주자 상태(예:1 3루 같은거)를 숫자의 나열로 리턴
        public int getRunnerOnGround()
        {
            //1 : 1루
            //11: 12루
            //101:: 13루
            //111: 만루

            //10 : 2루
            //110 : 23루

            //100: 3루
            int runnerVal = 0;
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].currentPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        runnerVal += 1;
                    }
                    if (runner[i].currentPos == FieldParm.SECONDBASE_INDEX)
                    {
                        runnerVal += 10;
                    }
                    if (runner[i].currentPos == FieldParm.THIRDBASE_INDEX)
                    {
                        runnerVal += 100;
                    }
                }
            }

            return runnerVal;
        }


        ////////////////////////////////////////////////////////////////////////////////
        //주자의 타구 판단과 주루 상태 변화
        ////////////////////////////////////////////////////////////////////////////////
        //필딩 전환시 주자의 주루 상태를 체크 하게 만드는 함수
        //주자의 타구 판단을 관장하는 메인함수
        public void checkRunning()
        {
            //////////UnityEngine.//Debug.Log("======================>>checkRunning   flyCatchAvaiableCount = " + field.flyCatchAvaiableCount);     
            setRunnerTransform();

            bRunDownCase = false;
            bStealBase = false;         //타구가 나온 시점에서 도루는 없던 일로
            bHomeSteal = false;         //타구가 나온 시점에서 홈스틸도 없던 일로
            bPickOff = false;
            bWildPitchRunning = false;

            bThirdBaseTag = false;

            //치고 달리기 관련
            bHitAndRun = false;
            bOnlyOneBaseFlag = false;
            if (stealResult != SimulStealState.NONE)
            {
                bHitAndRun = true;
                setRunnerHitAndRun();
                if (field.bOutofInfield == false)
                {
                    bOnlyOneBaseFlag = true;
                }
            }
            if (manager.nOutCount >= 2 || batter.bBuntHit == true)
            {
                //투아웃시 무조건 진루
                if (field.ball.bFoulHomerunGuess == true || field.ball.bFairBallGuess == false)
                {
                }
                else
                {
                    if (batter.bBuntHit == true)
                    {
                        setBuntRunner();
                    }
                    setRunnerMoveOnBase();
                }
            }
            else
            {
                if (field.flyCatchAvaiableCount > 0)
                {
                    //플라이볼 시 판단 및 진루
                    setRunnerMoveWhenFlyBall(true);
                }
                else
                {
                    //그라운드 볼시 상황 판단
                    if (field.earlygrounder || field.grounder)
                    {
                        //그라운더시 판단 및 진루
                        setRunnerMoveWhenGrounder();
                    }
                    else
                    {
                        //플라이볼 시 판단 및 진루
                        setRunnerMoveWhenFlyBall(false);
                    }
                }
            }

        }

        //아웃을 시켜야 되는 베이스의 가중치
        public bool checkOutWeightWithPositionAndOutcount(int posIndex, int throwbaseIndex, int outCount)
        {
            //각종 케이스를 만든다

            //내야수의 경우
            if (posIndex < CPlayer._LEFTFIELDER)
            {
                if (posIndex <= CPlayer._SECONDBASEMAN && outCount == 2)
                {
                    if (bOnRunning[FieldParm.FIRSTBASE_INDEX] && throwbaseIndex != FieldParm.FIRSTBASE_INDEX)
                    {
                        //////////UnityEngine.//Debug.Log("============>> 투수 포수 일루수 이루수는 투아웃에 무조건 일루");
                        return false;
                    }
                }
                else
                {
                    if ((bOnRunning[FieldParm.THIRDBASE_INDEX] && bOnRunning[FieldParm.SECONDBASE_INDEX]) && throwbaseIndex == FieldParm.THIRDBASE_INDEX)
                    {
                        return false;
                    }
                    else if (bOnRunning[FieldParm.THIRDBASE_INDEX] && throwbaseIndex == FieldParm.THIRDBASE_INDEX)
                    {
                        if (outCount == 2)
                        {
                            return false;
                        }
                        else if (posIndex == CPlayer._FIRSTBASEMAN
                              || posIndex == CPlayer._SECONDBASEMAN
                              || posIndex == CPlayer._THIRDBASEMAN)
                        {
                            return false;
                        }
                    }
                    else if (bOnRunning[FieldParm.HOMEBASE_INDEX] && throwbaseIndex == FieldParm.HOMEBASE_INDEX)
                    {
                        if (outCount == 2)
                        {
                            return false;
                        }
                        else if (posIndex == CPlayer._SECONDBASEMAN
                              || posIndex == CPlayer._SHORTSTOP)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        //베이스 태그 플래그를 프리 상태로 변환 ( 주자가 베이스에 구속되지 않아도 된다)
        private void setBaseTagFree()
        {
            for (int i = 0; i < 4; i++)
            {
                bBaseTagFree[i] = true;
            }
        }

        //주자가 리드를 하게끔 runner의 상태를 세팅한다
        public void setRunnerLead()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].setLead();
                }
            }
        }

        //베이스에 있는 주자가(리드한 상태) 다음 루로 진루하게끔 runner의 상태를 세팅한다
        private void setRunnerMoveOnBase()
        {
            //////////UnityEngine.//Debug.Log("=================>>setRunnerMoveOnBase");
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].setMoveOnBase();
                }
            }
        }

        //땅볼타구시 주자의 진루 여부를 판단하게끔 runner의 상태를 세팅한다.
        private void setRunnerMoveWhenGrounder()
        {
            //////////UnityEngine.//Debug.Log("=================>>setRunnerMoveWhenGrounder");
            //일단은 ...
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].runnerIndex == nHitterRunnerIndex)
                        {
                            runner[i].setMoveOnBase();
                        }
                        else
                        {
                            //When Grounder상태  
                            runner[i].setMoveWhenGrounder();
                        }
                    }
                }
            }

            //익셉션 처리
            Runner third = getRunner(FieldParm.THIRDBASE_INDEX);
            if (third != null)
            {
                Runner second = getRunner(FieldParm.SECONDBASE_INDEX);
                if (second != null)
                {
                    if (third.bGrounderWaitFlag == true)
                    {
                        ////UnityEngine.//Debug.Log("===================================>>3루 똥차 때문에 2루 주자 못뛰는거 처리해줌");
                        second.bGrounderWaitFlag = true;
                        second.setMoveBack();
                    }
                }
            }


        }

        //플라이볼 타구시 주자의 진루 여부를 판단하게끔 runner의 상태를 세팅한다.
        private void setRunnerMoveWhenFlyBall(bool bFlyOut)
        {
            //////UnityEngine.//Debug.Log("====================>>>setRunnerMoveWhenFlyBall");
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex == nHitterRunnerIndex)
                    {
                        if (field.bFoulFlyOut == true)
                        {
                            runner[i].posX += (field.ball.firstAngle > 0 ? 140 : -140);
                        }
                        else
                        {
                            if (field.ball.bFairBallGuess == false || field.ball.bFoulHomerunGuess == true)
                            {
                                runner[i].setFlyBallWait();
                            }
                            else
                            {
                                runner[i].setMoveOnBase();
                            }
                        }
                    }
                    else
                    {
                        //wait상태  
                        if (runner[i].bStealFlag == true && bFlyOut == true)
                        {
                            //도루시 돌아가
                            runner[i].setMoveBack();
                        }
                        else
                        {
                            runner[i].setFlyBallWait();
                        }
                    }
                }
            }
        }

        //플라이 볼이 잡혔을 경우 다음과 같은 상태로 세팅
        //1. 원래 루로 돌아올지
        //2. 베이스 태그 후 진루를 할지
        //3. 벤치로 돌아올지를
        public void setRunnerAfterFlyCatch()
        {
            int index = getHitterRunnerArrayIndex();
            if (index == -1)
            {
                //////////UnityEngine.//Debug.Log("======================>>>SetFlyOut FATAL ERROR");
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (i == index)
                    {
                        runner[i].setRunnerBench(false, false, false);
                    }
                    else
                    {
                        if (manager.nOutCount >= 2)
                        {
                            runner[i].setRunnerBench(false, false, false);
                        }
                        else
                        {
                            ////////UnityEngine.//Debug.Log("===================>>setRunnerAfterFlyCatch");
                            ////////UnityEngine.//Debug.Log("================>>index: " + i + "====>>>baseTagPrepare= " + runner[i].baseTagPrepare);
                            //베이스택 준비시
                            if (runner[i].baseTagPrepare == true)
                            {
                                runner[i].setBaseTag();
                            }
                            else
                            {
                                runner[i].setMoveBack();
                            }
                        }
                    }
                }
            }
        }

        //바운드가 튀면 움직이지 않거나 대기중인 주자를 강제로 다음 루로 움직이게 한다.
        public void setRunnerMoveAfterBound()
        {
            //////////UnityEngine.//Debug.Log("=======================>>setRunnerMoveAfterBound");
            setBaseTagFree();
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != nHitterRunnerIndex) //if (i != nHitterRunnerIndex)
                    {
                        if (runner[i].bGrounderWaitFlag == false)
                        {
                            runner[i].setMove(); //makeRunnerMove(i, false);
                        }
                    }
                }
            }
        }


        //주자가 도루 상태에서 일반 주루 상태로 바꿈
        public void setRunnerHitAndRun()
        {
            //////////UnityEngine.//Debug.Log("=======================>>setRunnerMoveAfterBound");
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].bStealFlag = false;
                }
            }
        }

        //포볼시 주자의 진루를 판단하게 runner의 상태를 세팅하는 함수
        //미완성 혹은 수정이 필요함!!!!!!!!!!
        public void setRunnerWalkMove()
        {
            //int index;

            field.ball.bFairBall = true;

            int thirdBaseIndex = getRunnerIndex(FieldParm.THIRDBASE_INDEX);
            int secondBaseIndex = getRunnerIndex(FieldParm.SECONDBASE_INDEX);
            int firstBaseIndex = getRunnerIndex(FieldParm.FIRSTBASE_INDEX); 


            if(thirdBaseIndex != -1) //  if (bOnBase[FieldParm.THIRDBASE_INDEX] == true)
            {
                if(secondBaseIndex != -1 && firstBaseIndex != -1)// if (bOnBase[FieldParm.SECONDBASE_INDEX] == true && bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                {
                    //삼루주자 인덱스 얻어와 움직이게 만든다
                    //index = getRunnerIndex(FieldParm.THIRDBASE_INDEX);
                    //runner[index].setMove();
                    runner[thirdBaseIndex].setMove();
                }
            }

            if (secondBaseIndex != -1) //if (bOnBase[FieldParm.SECONDBASE_INDEX] == true)
            {
                if (firstBaseIndex != -1)// bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                {
                    //2루주자 인덱스 얻어와 움직이게 만든다
                    //index = getRunnerIndex(FieldParm.SECONDBASE_INDEX);
                    //runner[index].setMove();
                    runner[secondBaseIndex].setMove();
                }
            }

            if (firstBaseIndex != -1) //if (bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
            {
                //일루에 있는 경우 무조건 움직여
                //index = getRunnerIndex(FieldParm.FIRSTBASE_INDEX);
                //runner[index].setMove();
                runner[firstBaseIndex].setMove();
            }


            //타자주자 움직여
            runner[getHitterRunnerArrayIndex()].setMove();

        }

        //도루 상태를 무효화함
        public void setStealInvalid()
        {
            field.bFieldStealFlag = false;
            bStealBase = false;
            bHomeSteal = false;

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].bStealFlag == true)
                    {
                        runner[i].bStealFlag = false;
                    }
                }
            }
        }
        
        //번트시 주자의 진루를 판단하게 runner의 상태를 세팅하는 함수
        //미완성 혹은 수정이 필요함!!!!!!!!!!
        public void setBuntRunner()
        {

        }

        //주자 풀스피드 상태로
        public void runnerFullAccell(int dstBase)
        {
            Runner runner = getDestRunner(dstBase);
            if (runner != null)
            {
                runner.curSpeed = runner.RUNNER_SPEED;
            }
        }

        //주루 특정상태시 바로 배팅뷰로 돌아오게 세팅
        public IEnumerator setReturnview(float delay)
        {
            yield return new WaitForSeconds(delay);
            //Debug.Log("@@@@@@@@2");
            field.bReturnBattingView = false;
        }


        ////////////////////////////////////////////////////////////////////////////////
        //주루의 상태 체크
        ////////////////////////////////////////////////////////////////////////////////
        //해당 베이스는 포스 아웃 상태로 세팅하여 그 베이스로 다가오는 주자를 포스 아웃되게 만든다
        public void setForceOutRunner(int curBase)
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                    {
                        if (runner[i].destPos == curBase)
                        {
                            runner[i].setForceOut();

                        }
                    }
                }
            }
        }

        //상태 변화에 따른 루별 포스아웃 여부 체크 (매프레임 체크)
        public void checkForcedOut()
        {
            bForceOutFlag[FieldParm.FIRSTBASE_INDEX]
                = bForceOutFlag[FieldParm.SECONDBASE_INDEX]
                = bForceOutFlag[FieldParm.THIRDBASE_INDEX]
                = bForceOutFlag[FieldParm.HOMEBASE_INDEX] = false;

            if (bStealBase == false && bPickOff == false)
            {
                if (bOnRunning[FieldParm.FIRSTBASE_INDEX] || bOnBase[FieldParm.FIRSTBASE_INDEX])	//1루로 누군가가 달리는 경우
                {
                    bForceOutFlag[FieldParm.FIRSTBASE_INDEX] = true;
                    bForceOutFlag[FieldParm.SECONDBASE_INDEX] = true;

                    if (bOnRunning[FieldParm.SECONDBASE_INDEX] || bOnBase[FieldParm.SECONDBASE_INDEX])	//2루로 누군가가 달리는 경우
                    {
                        bForceOutFlag[FieldParm.THIRDBASE_INDEX] = true;
                        if (bOnRunning[FieldParm.THIRDBASE_INDEX] || bOnBase[FieldParm.THIRDBASE_INDEX])	//3루로 누군가가 달리는 경우
                        {
                            bForceOutFlag[FieldParm.HOMEBASE_INDEX] = true;
                        }
                    }
                }
            }
        }

        //상태 변화에 따른 루별 달리는 주자가 있는지 여부를 체크(매프레임 체크)
        public void chekcOnRunning()
        {
            for (int i = 0; i < 4; i++) bOnRunning[i] = false;
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].bMoving == true)
                    {
                        if (runner[i].bMoveForward == true)
                        {
                            bOnRunning[runner[i].destPos] = true;
                        }
                    }
                }
            }
        }

        //2루로 달리는 주자가 포스 아웃 상태임을 확인
        public bool check2ndBaseForceOut()
        {
            bool bFirst = false;
            ////////UnityEngine.//Debug.Log("=================>>check2ndBaseForceOut");
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].currentPos == FieldParm.FIRSTBASE_INDEX)
                    {

                        bFirst = true;
                        break;
                    }
                }
            }
            ////////UnityEngine.//Debug.Log("=================>>check2ndBaseForceOut bFirst = " + bFirst);
            return bFirst;
        }

        //3루로 달리는 주자가 포스 아웃 상태임을 확인
        public bool check3rdBaseForceOut()
        {
            ////////UnityEngine.//Debug.Log("=================>>check3rdBaseForceOut");
            bool bFirst = false;
            bool bSecond = false;

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].currentPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        bFirst = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].currentPos == FieldParm.SECONDBASE_INDEX)
                    {
                        bSecond = true;
                        break;
                    }
                }
            }

            ////////UnityEngine.//Debug.Log("=================>>check3rdBaseForceOut bFirst = " + bFirst + "  bSecond = " + bSecond);

            if (bFirst && bSecond)
            {
                ////////UnityEngine.//Debug.Log("=================>>3루는 포스 아웃 상태");
                return true;
            }
            else
            {
                return false;
            }
        }

        //루상에 타자주자를 제외한 주자가 있는지 여부를 확인
        public bool checkActiveRunnerOnBase()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkActive() == true)//nState > RunningMechnism.GO_BENCH)
                    {
                        if (runner[i].runnerIndex != nHitterRunnerIndex)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //루상에 달리거나 혹은 on base상태의 주자가 있는지 여부 확인
        public bool checkRunnerOnBase()
        {
            if ((bOnRunning[FieldParm.HOMEBASE_INDEX] || bOnBase[FieldParm.HOMEBASE_INDEX])
             || (bOnRunning[FieldParm.SECONDBASE_INDEX] || bOnBase[FieldParm.SECONDBASE_INDEX])	//2루로 누군가가 달리는 경우
             || (bOnRunning[FieldParm.THIRDBASE_INDEX] || bOnBase[FieldParm.THIRDBASE_INDEX]))	//3루로 누군가가 달리는 경우
            {
                return true;
            }

            return false;
        }

        //도루나 기타 상황에서 업데이트 된후 베이스 상태 체크
        public void checkUpdateOnBaseAfterSteal()
        {
            bOnBase[FieldParm.FIRSTBASE_INDEX] = (getRunnerIndex(FieldParm.FIRSTBASE_INDEX) != -1 ? true : false);
            bOnBase[FieldParm.SECONDBASE_INDEX] = (getRunnerIndex(FieldParm.SECONDBASE_INDEX) != -1 ? true : false);
            bOnBase[FieldParm.THIRDBASE_INDEX] = (getRunnerIndex(FieldParm.THIRDBASE_INDEX) != -1 ? true : false);

            IngameUI.GetControlRunner().SetActive(true, false);
        }


        ////////////////////////////////////////////////////////////////////////////////
        //연출
        ////////////////////////////////////////////////////////////////////////////////

        public void setRunnerCamera(bool battingView)
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    runner[i].setBvCamera(battingView);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////
        //주자 기록 관련
        ////////////////////////////////////////////////////////////////////////////////
        //주자의 득점 가산
        public void addRunnerRunStat()
        {
            //주자 득점 계산
            for (int i = 0; i < 4; i++)
            {
                if (runnerRunScore[i] == true)
                {
                    if (runnerData[i] != null)
                    {
                        //Debug.Log((currentPos + 1) + "루 주자 기록===============>>> " + pRunner.getName() + "의 " + Param.debug_stat[type] + " 가산");
                        //Debug.Log(runnerData[i].getName() + "의 득점 가산");
                        runnerData[i].setRecord(Param.ST_R);
                        if (field.bFieldDelayStealFlag == true)
                        {
                            //딜레이 스틸에 의한 홈스틸
                            //Debug.Log(runnerData[i].getName() + "의 딜레이 스틸에 의한 홈스틸 가산");
                            runnerData[i].setRecord(Param.ST_SBS);
                        }

                        /*
                        if (runnerLastPos[i] != FieldParm.HOMEBASE_INDEX)
                            SimulManager.AddGameSummuryInfo("\n-" + (runnerLastPos[i] + 1) + "루주자 " + runnerData[i].getName() + ": [ff3a3a]홈인[-]");
                         */
                    }
                    runnerRunScore[i] = false;
                }
            }
        }


        //승계주자
        public void setLastPitcher() //교체시 승계주자로 설정
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].runnerIndex != nHitterRunnerIndex)
                    {
                        runner[i].bLastPitcher = true;
                    }
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////////
        //오버런 주자
        ////////////////////////////////////////////////////////////////////////////////
        public void checkOverrunRunner(int posIndex, int nTargetIndex)
        {
            if (nTargetIndex > FieldParm.FIRSTBASE_INDEX)
            {
                Runner runner = getDestRunner(nTargetIndex);
                if (runner != null)
                {
                    if (runner.overRunFlag != SimulOverrunState.NONE || nTargetIndex == FieldParm.SECONDBASE_INDEX)
                    {
                        Fielder fielder = field.fielder[posIndex];
                        float timeLeft = field.getTimeLeftforThrow(nTargetIndex, fielder.posX, fielder.posY, 0.25f, fielder.THROW_SPEED);
                        //Debug.Log("=======================================================================================================>>timeLeft = " + timeLeft);
                        bool bRunnerOut = (runner.overRunFlag == SimulOverrunState.OUT ? true : false);
                        //Debug.Log("===========================>> 아웃여부 : " + bRunnerOut);
                        runner.setShobuRunnerSpeed(bRunnerOut, timeLeft, 1.2f);
                    }
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////////
        //도루 
        ////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 도루 체크
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public IEnumerator runnerCheckSteal(float delay)
        {
            yield return new WaitForSeconds(delay);

            bStealBase = false;
            bSecondBaseSteal = true;
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].checkSteal() == true)
                    {
                        //int index = getRunnerIndex(i);
                        if (runner[i].currentPos == FieldParm.FIRSTBASE_INDEX)
                        {
                            battingview._1stRunner.setBvRunnerMove(true);
                        }
                        else if (runner[i].currentPos == FieldParm.SECONDBASE_INDEX)
                        {
                            bSecondBaseSteal = false;
                            battingview._2ndRunner.setBvRunnerMove(true);
                        }
                        else if (runner[i].currentPos == FieldParm.THIRDBASE_INDEX)
                        {
                            bSecondBaseSteal = false;
                            ////UnityEngine.Debug.Log("%%%%%%%%%%%%%%% check home steal!!!!");
                            bHomeSteal = true;
                        }
                        bStealBase = true;
                    }
                }
            }

            if (bStealBase == false)
            {
                bSecondBaseSteal = false;
            }
            
        }

        

        /// <summary>
        /// 도루 (혹은 견제) 상태로 세팅
        /// </summary>
        /// <param name="baseindex"></param>
        /// <param name="bPickOff">true이면 견제</param>
        public void set_Steal_Pickoff(int baseindex, bool bPickOff = false)
        {
            //////UnityEngine.//Debug.Log("======================>>상태만 가능");
            if (bOnBase[baseindex] == true)
            {
                //////UnityEngine.//Debug.Log("======================>>도루 가능");
                int index = getRunnerIndex(baseindex);
                runner[index].set_Steal_Pickoff(true, bPickOff);

                if (bPickOff == false)
                {
                    /*
                    if (baseindex == FieldParm.THIRDBASE_INDEX)
                    {
                        //3루베이스시 스퀴즈 플래그 온
                        field.bSqueezeFlagOn = true;
                    }*/

                    int nextBase = (baseindex + 1) % 4;
                    if (nextBase != FieldParm.HOMEBASE_INDEX)
                    {
                        if (bOnBase[nextBase] == true)
                        {
                            set_Steal_Pickoff(nextBase, bPickOff);
                        }
                    }
                }
                else
                {
                    field.nTargetIndex = baseindex;
                }
            }
        }



        public void myControlSteal(int target)
        {
            getStealResultMyControl(target);
            //set_Steal_Pickoff(target);

            if(Mode.bPvpMode433 == true)
            {
                if(manager.bMyTurn == true)
                {
                    //발신
                    //Debug.Log("=======================>> 도루정보 발신 stealResult :" + stealResult);
                    pvpmanager.Get().SendStealInfo(stealResult, target);
                }
                else
                {
                    set_Steal_Pickoff(target);
                    pvpmanager.Get().SendStealInfoReturn(stealResult, target);
                }
                
            }
            else
            {
                set_Steal_Pickoff(target);
            }
        }


       
        /// <summary>
        /// 도루하는 놈이 향하는 베이스
        /// </summary>
        /// <returns></returns>
        public int getStealDest()
        {
            int baseIndex = FieldParm.SECONDBASE_INDEX;

            if (bOnRunning[FieldParm.THIRDBASE_INDEX]) baseIndex = FieldParm.THIRDBASE_INDEX;

            if (bOnRunning[FieldParm.HOMEBASE_INDEX]) baseIndex = FieldParm.HOMEBASE_INDEX;

            return baseIndex;

        }

        /// <summary>
        /// AI의 도루 컨트롤
        /// </summary>
        public void setAiStealControl()
        {
            if (Mode.bAutoPlay == true || manager.bMyTurn == false)
            {
                if (stealCount == (manager.nStrikeCount + manager.nBallCount))
                {
                    if (stealResult != SimulStealState.NONE)
                    {
                        //////UnityEngine.//Debug.Log("=================>>AI 도루 컨트롤");
                        if (bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                        {
                            runner[getRunnerIndex(FieldParm.SECONDBASE_INDEX)].set_Steal_Pickoff(true);
                        }

                        if (bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                        {
                            runner[getRunnerIndex(FieldParm.FIRSTBASE_INDEX)].set_Steal_Pickoff(true);
                        }
                    }

                    /* 딜레이드 홈스틸 테스트용
                    if (stealResult != SimulStealState.NONE)
                    {
                        //딜레이 스틸 케이스 테스트용
                        if (bOnBase[FieldParm.FIRSTBASE_INDEX] == true
                        && bOnBase[FieldParm.SECONDBASE_INDEX] == false
                        && bOnBase[FieldParm.THIRDBASE_INDEX] == true)
                        {
                            //
                            runner[getRunnerIndex(FieldParm.FIRSTBASE_INDEX)].set_Steal_Pickoff(true);
                        }
                    }*/

                    //스퀴즈인 경우 홈스틸
                    if (batter.buntType == SimulBuntType.SQUEEZE)
                    {                       
                        if (batter.buntResult == SpecificBuntType.SQUEEZ_FAIL)
                        {
                            if (MyMath.Half()) field.bSqueezeFieldOut = true;
                        }

                        if (field.bSqueezeFieldOut == false)
                        {
                            field.run.set_Steal_Pickoff(FieldParm.THIRDBASE_INDEX);
                        }
                    }
                }
            }
        }

        public void setPVPStealControl()
        {
            if(manager.bMyTurn == false)
            {

            }
        }


        /// <summary>
        /// 도루 결과
        /// </summary>
        public SimulStealState stealResult;
        
        /// <summary>
        /// 도루 하는 볼카운트
        /// </summary>
        public int stealCount;
        
        /// <summary>
        /// 도루하는 주자
        /// </summary>
        public Runner stealRunner;

        /// <summary>
        /// 도루상태 초기화
        /// </summary>
        public void setStealInit()
        {
            //////UnityEngine.//Debug.Log("=======================>>도루조건 초기화");
            stealResult = SimulStealState.NONE;
            pickoffState = SimulPickOffState.NONE;
        }

        /// <summary>
        /// 도루 카운트 세팅
        /// </summary>
        /// <param name="count"></param>
        public void setStealCount(int count)
        {
            stealCount = count;
        }


        /// <summary>
        /// AI가 발생한 도루 결과 가져오기
        /// </summary>
        public void getAIStealResult()
        {
            if (bOnBase[FieldParm.FIRSTBASE_INDEX] == true || bOnBase[FieldParm.SECONDBASE_INDEX] == true)
            {
                Debug.Log("AI의 도루 체크!!!   히트앤드런 체크 = " + manager.batter.aiHitandRunDecide);
                //도루의 로컬 밸런스
#if _Local_Balance
                /*if (InGameDebug._ALWAYS_STEAL == true) //가짜정보
                {
                    //일부러 항상 도루 발생
                    if (bOnBase[FieldParm.FIRSTBASE_INDEX] == true)
                        stealResult = SimulStealState.Success;
                }
                else*/
#endif
                {
                    stealCount = 10000;
                    stealRunner = null;
                    CPlayer catcher = field.fielder[CPlayer._CATCHER].pFielder;
                    int stealBase = -1;
                    int scoreGab = manager.offenseWinningGab();

                    if (getRunnerIndex(SimulParm.SECONDBASE_INDEX) != -1)   //2루에 주자가 있는 경우
                    {
                        //3루도루
                        stealBase = FieldParm.THIRDBASE_INDEX;
                        stealRunner = getRunner(FieldParm.SECONDBASE_INDEX);//.pRunner;
                    }
                    else if (getRunnerIndex(SimulParm.FIRSTBASE_INDEX) != -1) //1루에 주자가 있고
                    {
                        //2루도루
                        stealBase = FieldParm.SECONDBASE_INDEX;
                        stealRunner = getRunner(FieldParm.FIRSTBASE_INDEX);//.pRunner;
                    }

                    if (stealRunner != null)
                    {
                        if (SimulSteal.checkStealPossible(stealRunner.pRunner, catcher, manager.nInningCount, manager.nOutCount, scoreGab, bOnBase) == true)
                        {
                            stealResult = SimulSteal.getStealResult(stealRunner.pRunner, catcher, pitcher.pPitcher, (stealBase == FieldParm.SECONDBASE_INDEX ? false : true));
                            field.stealBaseTarget = stealBase;
                            setStealCount(Random.Range(0, 2));
                        }
                    }
                }
            }
            else
            {
                stealResult = SimulStealState.NONE;
            }

            if (stealResult != SimulStealState.NONE)
            {
                stealCount = Random.Range(0, 2);
                Debug.Log(stealCount+ "구째에 도루 감행");
            }


        }//stealBase

        
        /// <summary>
        /// 유저가 발생시킨 도루 결과 가져오기
        /// </summary>
        /// <param name="baseIndex"></param>
        public void getStealResultMyControl(int baseIndex)
        {
            int runnerIndex = getRunnerIndex(baseIndex);
            if(runnerIndex !=-1)
            {
                stealRunner = runner[runnerIndex];//.pRunner;
                Fielder catcher = field.fielder[CPlayer._CATCHER];

                field.stealBaseTarget = baseIndex+1;
                                
                int realBaseIndex = baseIndex;
                if (baseIndex == FieldParm.FIRSTBASE_INDEX)
                {
                    if (bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                    {
                        realBaseIndex = FieldParm.SECONDBASE_INDEX;
                    }
                }
                else if (baseIndex == FieldParm.SECONDBASE_INDEX)
                {
                    if (bOnBase[FieldParm.THIRDBASE_INDEX] == true)
                    {
                        stealResult = SimulStealState.Fail;
                        return;
                    }
                }

                field.stealBaseTarget = realBaseIndex + 1;

                stealResult = SimulStealState.NONE;

                if (realBaseIndex == FieldParm.FIRSTBASE_INDEX)
                {
                    stealResult = SimulSteal.getStealResult(stealRunner.pRunner, catcher.pFielder, pitcher.pPitcher, false);
                }
                else if (realBaseIndex == FieldParm.SECONDBASE_INDEX)
                {
                    stealResult = SimulSteal.getStealResult(stealRunner.pRunner, catcher.pFielder, pitcher.pPitcher, true);
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////////
        //견제
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 견제 결과
        /// </summary>
        public SimulPickOffState pickoffState;
        //public bool bPickOffOut;

        /// <summary>
        /// 견제 체크
        /// </summary>
        public void runnerCheckPickOff()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].bPickOffFlag == true)
                    {
                        runner[i].setPickOff();
                    }
                    else
                    {
                        if (runner[i].runnerIndex != nHitterRunnerIndex)
                        {
                            int curPos = runner[i].currentPos;
                            runner[i].setInitPos(curPos);
                        }
                        runner[i].state = RunState.STANDBY;
                    }
                }
            }
        }

        

        /// <summary>
        /// AI의 견제 컨트롤
        /// </summary>
        /// <returns></returns>
        public bool setAiPickoffControl()
        {
            //대전모드시 발동안함
            if (Mode.bPvpMode433 == true) return false; //if (Mode.bPvpMode == true) return false;

            //9회 투아웃시 발동안함
            if (Mode.b2outBaseLoadedMode == true) return false;

            //자동 플레이시 해당 사항 없음
            if (manager.bMyTurn == true || Mode.bAutoPlay == true)
            {
                //////UnityEngine.//Debug.Log("=================>>AI 견제 컨트롤");
                if ((bOnBase[FieldParm.FIRSTBASE_INDEX] == true && bOnBase[FieldParm.SECONDBASE_INDEX] == false)    //2루에 주자 없는 상황에서 1루베이스
                  || bOnBase[FieldParm.SECONDBASE_INDEX] == true)                                                    //2루에 주자 있는 상황    
                {
                    if (stealResult != SimulStealState.NONE)
                    {
                        int target = bOnBase[FieldParm.SECONDBASE_INDEX] ? FieldParm.SECONDBASE_INDEX : FieldParm.FIRSTBASE_INDEX;

                        Runner pickOffRunner = getRunner(target);
                        CPlayer pitcher = field.pitcher.pPitcher;

                        //pvp상관없음
                        pickoffState = SimulSteal.getPickOffResult(pickOffRunner.pRunner, pitcher, field.pickOffCount);

                        if (pickoffState != SimulPickOffState.NONE)
                        {
                            pickOffRunner.set_Steal_Pickoff(true, true);
                            manager.field.setPickOff(target);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 해당 타겟으로 강제 픽오프 발생하게 함
        /// </summary>
        /// <param name="target"></param>
        public void setPickOffToTarget(int target)
        {
            Runner pickOffRunner = getRunner(target);
            if (pickOffRunner != null)
            {
                pickOffRunner.set_Steal_Pickoff(true, true);
                manager.field.setPickOff(target);
            }
        }


        /// <summary>
        /// 도루 견제시 타자주자 상태 세팅
        /// </summary>
        /// <param name="gab"></param>
        public void setHitterRunnerStealPickOffSetting(int gab = 150)
        {
            int index = getHitterRunnerArrayIndex();
            runner[index].setStandby(BaseArriveMotion._NORMAL);
            runner[index].state = RunState.STANDBY;
            bOnRunning[FieldParm.FIRSTBASE_INDEX] = false;

            runner[index].posX += (batter.batterHand == CPlayer._LEFTHAND ? gab : -gab); //우타자 좌타자 위치 offset
        }

        ////////////////////////////////////////////////////////////////////////////////
        //폭투
        ////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 주자의 폭투 체크
        /// </summary>
        /// <param name="wildCase"></param>
        /// <param name="bBlock"></param>
        public void runnerWildPitch(FieldParm.WildPitchCase wildCase, bool bBlock)
        {
            bool bMustMove = false;

            if (bBlock == false)
            {
                //블록실패시 무조건 움직임
                bMustMove = true;
            }
            else
            {
                //블록성공시
                if (wildCase == FieldParm.WildPitchCase.BaseOnBall)
                {
                    //일반 포볼 움직임
                    setRunnerWalkMove();
                    return;
                }
                else if (wildCase == FieldParm.WildPitchCase.NotOut)
                {
                    //낫아웃 & 블록성공
                    if (manager.nOutCount >= 2) bMustMove = true;   //2아웃이면 무조건 진루
                    else bMustMove = false;                         //0,1아웃이면 무조건 귀루
                }
                else
                {
                    //무조건 귀루
                    bMustMove = false;
                }
            }


            if (bMustMove == true)
            {
                //무조건 움직이는 경우
                for (int i = 0; i < 4; i++)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].runnerIndex != nHitterRunnerIndex)
                        {
                            runner[i].setMoveOnBase();
                        }
                    }
                }
            }
            else
            {
                //무조건 귀루하는 경우
                for (int i = 0; i < 4; i++)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].runnerIndex != nHitterRunnerIndex)
                        {
                            runner[i].setMoveBack();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 폭투시 타자주자 세팅
        /// </summary>
        /// <param name="wildCase"></param>
        /// <param name="bBlock"></param>
        public void setHitterRunnerWildPitchSetting(FieldParm.WildPitchCase wildCase, bool bBlock)
        {
            bNotOutRunning = false;

            int index = getHitterRunnerArrayIndex();
            if (wildCase == FieldParm.WildPitchCase.NoRunner || wildCase == FieldParm.WildPitchCase.RunnerOnBase)
            {
                //그냥 볼이 빠진상태
                setHitterRunnerStealPickOffSetting(100);
            }
            else if (wildCase == FieldParm.WildPitchCase.BaseOnBall)
            {
                //볼이 빠지면서 포볼상태
                bRunnerWalk = true;
                runner[index].setMoveOnBase();
                runner[index].curTime = -1;
                manager.setFourballCount(); //포볼 기록 추가
            }
            else
            {
                //낫아웃 시츄에이션
                if (bBlock == false || getRunnerIndex(FieldParm.FIRSTBASE_INDEX) == -1 || manager.nOutCount >= 2)
                {
                    //낫아웃 가능
                    bNotOutRunning = true;
                    runner[index].setMoveOnBase();
                    runner[index].curTime = Random.Range(0.5f, 2.5f);
                    manager.setNotOutSituation(true);   //우선 삼진만 추가
                }
                else
                {
                    //타자주자 낫아웃상태 포기
                    runner[index].setBench(RunState.GO_BENCH, RunnerOutMotion._NORMAL, 1.0f);
                    manager.setNotOutSituation(false);   //낫아웃
                    field.judge.setCall(FieldParm.HOMEBASE_INDEX, CallType._OUT);
                }
            }

        }


        ////////////////////////////////////////////////////////////////////////////////
        //딜레이 스킬
        ////////////////////////////////////////////////////////////////////////////////
        /*
        //딜레이 스틸이 일어나는지 여부 체크
        public bool checkDelayHomeSteal()
        {
            Runner runner = getDestRunner(FieldParm.HOMEBASE_INDEX);
            if (runner != null)
            {
                if (runner.bDelayStealSkillOn == true)
                {
                    return true;
                }
            }
            return false;
        }*/

        
            


#if _RewindMode
        ///////////////////////////////////////////////////////////////////
        //리와인드 모드
        ///////////////////////////////////////////////////////////////////
        //리와인드 러너 무브 
        public void setRewindRunnerMove()
        {
            if (manager.battingResultData.result == SimulResultState.FlyOut)
            {
                setRunnerMoveWhenFlyBall(true);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (runnerActive[i] == true)
                    {
                        if (runner[i].runnerIndex == nHitterRunnerIndex)
                        {
                            runner[i].setMoveOnBase();
                        }
                        else
                        {
                            if (manager.battingResultData.runnerCurPos[i] > runner[i].currentPos)
                            {
                                runner[i].setMoveOnBase();
                            }
                        }

                    }
                }
            }

        }
        
        //리와인드 모드 보살 러너 인덱스
        public int getRewindAssistIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                if (runnerActive[i] == true)
                {
                    if (runner[i].bRewindOverRunOut == true)
                    {
                        return runner[i].destPos;
                    }
                }
            }
            return -1;
        }

        //리와인드 모드 땅볼 봉살 러너 인덱스
        public int getRewindGrounderForceOutIndex()
        {
            for (int i = 0; i < 4; i++)
            {
                    if(manager.battingResultData.runnerValue[i]==(int)(RunnerState.FourceOut))
                    {
                        manager.battingResultData.runnerValue[i] = (int)(RunnerState.None);
                        return manager.battingResultData.runnerCurPos[i];
                    }
            }
            return -1;
        }

        //리와인드 모드 봉살 러너 인덱스
        public int getRewindFourceOutDestIndex(int dest)
        {
            for (int i = 0; i < 4; i++)
            {
                if (manager.battingResultData.runnerCurPos[i] == dest)
                {
                    return i;
                }
            }
            return -1;

        }

        Runner first,second, third;
        int firstIndex, secondIndex, thirdIndex;
        //리와인드 러너 데이터를 엔진과 싱크
        public void SyncRunnerFromRewindData()
        {
            if (manager.bThreeOutChange == true || manager.nOutCount >= 3) return;

            ////UnityEngine.//Debug.Log("=============================>> 러너 데이터를 통해 엔진의 주자를 싱크시킨다");
            first = new Runner();
            second = new Runner();
            third = new Runner();
            firstIndex = secondIndex = thirdIndex = -1;

            for (int i = 0; i < 4; i++)
            {
                int curPos = manager.battingResultData.runnerCurPos[i];
                RunnerState curValue = (RunnerState)manager.battingResultData.runnerValue[i];

                if (curValue == RunnerState.OnBase)
                {
                    if (curPos == FieldParm.FIRSTBASE_INDEX)
                    {
                        firstIndex = i;
                        first = getRunner(curPos);
                    }
                    else if (curPos == FieldParm.SECONDBASE_INDEX)
                    {
                        secondIndex = i;
                        second = getRunner(curPos);
                    }
                    else if (curPos == FieldParm.THIRDBASE_INDEX)
                    {
                        thirdIndex = i;
                        third = getRunner(curPos);
                    }
                }
            }

            for (int i = 0; i < 4; i++) runnerActive[i] = false;

            if (firstIndex != -1)
            {
                ////UnityEngine.//Debug.Log("=============================>> 1루 주자를 싱크시킨다");
                runnerActive[firstIndex] = true;
                runner[firstIndex] = first;
                runner[firstIndex].arrayIndex = firstIndex;
            }
            if (secondIndex != -1)
            {
                ////UnityEngine.//Debug.Log("=============================>> 2루 주자를 싱크시킨다");
                runnerActive[secondIndex] = true;
                runner[secondIndex] = second;
                runner[secondIndex].arrayIndex = secondIndex;
            }
            if (thirdIndex != -1)
            {
                ////UnityEngine.//Debug.Log("=============================>> 3루 주자를 싱크시킨다");
                runnerActive[thirdIndex] = true;
                runner[thirdIndex] = third;
                runner[thirdIndex].arrayIndex = thirdIndex;
            }

        }
#endif

    }
}
