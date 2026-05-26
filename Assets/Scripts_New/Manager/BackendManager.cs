// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Transactions;
using BackEnd;
using BackendData.Base;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LitJson;


public class BackendManager : MonoBehaviour {
    // 1. 로그인씬에서 초기화 진행
    // 2. 로딩씬에서 조회 후 캐싱
    // 3. 인게임씬에서 캐싱된 데이터로 사용 및 주기적인 로직으로 갱신

    public delegate void BackendUpdate(object parm);

    //뒤끝 콘솔에 업로드한 차트 데이터만 모아놓은 클래스
    public class BackendChart {
        //이건 확정
        public readonly BackendData.Chart.AllChart ChartInfo = new(); // 모든 차트
        public readonly CharacterChart CharacterData = new();   // 캐릭터데이터 차트
        public readonly HitterLevelDataChart HitterLevelData = new();   // 캐릭터 레벨별 능력치
        public readonly PitcherLevelDataChart PitcherLevelData = new();   // 투수 레벨별 능력치
        public readonly HitterSkillDataChart HitterSkillData = new();   // 타자의 스킬 정보 차트
        public readonly SkillDataChart SkillData = new();       // 스킬데이터 차트
        public readonly GearDataChart GearData = new();                 // 장비 데이터 차트
        public readonly GearAttributesChart GearAttributes = new();     // 장비 능력치 차트
        public readonly UpgradeChart UpgradeData = new();         // 업글데이터 차트
        public readonly TrophyRoadChart TrophyRoadData = new();   // 트로피로드 데이터
        public readonly BallerTrophyRoadChart BallerTrophyRoadData = new();   // 볼러트로피로드 데이터
        public readonly AchievementChart AchievementData = new();   // 볼러 업적 데이터
        public readonly RewardDataChart RewardData = new();         //보상데이터 차트


        //이 밑으로는 빼야 될것도 있음
        public readonly ConsumeItemChart ConsumeItem = new();   // 컨슙아이템 차트
        public readonly SeasonPassRewardChart SeasonPassReward = new(); // 시즌패스 보상 차트
        public readonly SeasonPassBaseChart SeasonPassBase = new(); // 시즌패스 기본 차트
        public readonly RttsScheduleChart RttsSchedule = new();   // Rtts스케쥴
        public readonly RttsInfoChart RttsInfo = new();           // Rtts정보
        public readonly RttsTeamChart RttsTeam = new();           // Rtts팀정보
        public readonly RttsRewardChart RttsReward = new();    // 리그보상 정보
        public readonly RttsResultRewardChart RttsResultReward = new();         //리그 종료 후 보상(1등)
    }



    // 게임 정보 관리 데이터만 모아놓은 클래스
    public class BackendGameData {
        
        //public readonly KOBUserInfo KOBUserInfo = new();
        //public readonly KOBPlayerInfo KOBPlayerInfo = new();
        public readonly KOBGameData KOBGameData = new();
        public readonly KOBPublicData KOBPublicData = new();

        public readonly Dictionary<string, BackendData.Base.GameData>
            GameDataList = new Dictionary<string, GameData>();

        public BackendGameData() {
            //GameDataList.Add("유저 기본 정보", KOBUserInfo);
            //GameDataList.Add("유저 선수 정보", KOBPlayerInfo);
            GameDataList.Add("유저 게임 정보", KOBGameData);
            GameDataList.Add("퍼블릭 정보", KOBPublicData);
        }
    }

    //서버세팅
    public ServerSetting Setting = new ServerSetting();
        

    public BackendChart   Chart = new(); // 차트 모음 클래스 생성
    public BackendGameData GameData = new(); // 게임 모음 클래스 생성
    public BackendData.Rank.Manager Rank = new(); // 랭킹 관리 클래스 생성
    public BackendData.Post.Manager Post = new(); // 우편 클래스 생성

 // 게임 데이터의 저장, 조회등 일괄적으로 처리하기 위한 List

    private bool _isErrorOccured = false; // 치명적인 에러 발생 여부 

    
    // 뒤끝 매니저 초기화 함수
    public bool Init() {
        var initializeBro = Backend.Initialize(true);

        // 초기화 성공시
        if (initializeBro.IsSuccess()) {
            Debug.Log("뒤끝 초기화가 완료되었습니다.");
            CreateSendQueueMgr();
            SetErrorHandler();
            return true;
        }
        //초기화 실패시
        else {
            //StaticManager.UI.AlertUI.OpenErrorUI(GetType().Name,MethodBase.GetCurrentMethod()?.ToString(), initializeBro.ToString());
            return false;
        }
    }

    /*
    //비동기 함수를 메인쓰레드로 보내어 UI에 용이하게 접근하도록 도와주는 Poll 함수
    void Update() {
        if (Backend.IsInitialized) {
            Backend.AsyncPoll();
            Backend.ErrorHandler.Poll();
        }
    }*/

    // 모든 뒤끝 함수에서 에러 발생 시, 각 에러에 따라 호출해주는 핸들러
    private void SetErrorHandler() {
        Backend.ErrorHandler.InitializePoll(true);

        // 서버 점검 에러 발생 시
        Backend.ErrorHandler.OnMaintenanceError = () => {
            Debug.Log("점검 에러 발생!!!");
            //StaticManager.UI.AlertUI.OpenErrorUIWithText("서버 점검 중", "현재 서버 점검중입니다.\n타이틀로 돌아갑니다.");
        };
        // 403 에러 발생시
        Backend.ErrorHandler.OnTooManyRequestError = () => {
            //StaticManager.UI.AlertUI.OpenErrorUIWithText("비정상적인 행동 감지", "비정상적인 행동이 감지되었습니다.\n타이틀로 돌아갑니다.");
        };
        // 액세스토큰 만료 후 리프레시 토큰 실패 시
        Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () => {
            //StaticManager.UI.AlertUI.OpenErrorUIWithText("다른 기기 접속 감지", "다른 기기에서 로그인이 감지되었습니다.\n타이틀로 돌아갑니다.");
        };
    }

    // 로딩씬에서 할당할 뒤끝 정보 클래스 초기화
    public void InitInGameData() {

        Chart = new();
        GameData = new();
        Rank = new();
        Post = new();
    }

    //SendQueue를 관리해주는 SendQueue 매니저 생성
    private void CreateSendQueueMgr() {
        var obj = new GameObject();
        obj.name = "SendQueueMgr";
        obj.transform.SetParent(this.transform);
        obj.AddComponent<SendQueueMgr>();
    }
    
    // 일정주기마다 데이터를 저장/불러오는 코루틴 시작(인게임 시작 시)
    public void StartUpdate() {
        StartCoroutine(UpdateGameDataTransaction());
        StartCoroutine(UpdateRankScore());
        //StartCoroutine(GetAdminPostList());
    }

    // 호출 시, 코루틴 내 함수들의 동작을 멈추게 하는 함수
    public void StopUpdate() {
        Debug.Log("자동 저장을 중지합니다.");
        _isErrorOccured = false;
    }


    // 일정주기마다 내 게임정보 데이터를 묶어서 저장하는 코루틴 함수
    private IEnumerator UpdateGameDataTransaction() {
        var seconds = new WaitForSeconds(300);
        yield return seconds;

        while (_isErrorOccured) {
            UpdateAllGameData(null);

            yield return seconds;
        }
    }

    // 업데이트가 발생한 이후에 호출에 대한 응답을 반환해주는 대리자 함수
    public delegate void AfterUpdateFunc(BackendReturnObject callback, TResponseBase response);
    
    // 값이 바뀐 데이터가 있는지 체크후 바뀐 데이터들은 바로 저장 혹은 트랜잭션에 묶어 저장을 진행하는 함수
    public void UpdateAllGameData(AfterUpdateFunc afterUpdateFunc, TResponseBase response = null) {
        string info = string.Empty;


        // 바뀐 데이터가 몇개 있는지 체크
        List<GameData> gameDatas = new List<GameData>();

        foreach (var gameData in GameData.GameDataList)
        {
            if (gameData.Value.IsChangedData)
            {
                info += gameData.Value.GetTableName() + "\n";
                gameDatas.Add(gameData.Value);
            }
        }

        if (gameDatas.Count <= 0)
        {
            afterUpdateFunc(null, response); // 지정한 대리자 함수 호출

            // 업데이트할 목록이 존재하지 않습니다.
        }
        else if (gameDatas.Count == 1)
        {
            //하나라면 찾아서 해당 테이블만 업데이트
            foreach (var gameData in gameDatas)
            {
                if (gameData.IsChangedData)
                {
                    gameData.Update(callback =>
                    {
                        gameData.IsChangedData = false;
                        //성공할경우 데이터 변경 여부를 false로 변경
                        if (callback.IsSuccess())
                        {
                            gameData.LocalDataUpdate();
                        }
                        else
                        {
                            gameData.RevertData();
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + info);
                        }
                        Debug.Log($"UpdateV2 : {callback}\n업데이트 테이블 : \n{info}");
                        if (afterUpdateFunc == null)
                        {

                        }
                        else
                        {
                            afterUpdateFunc(callback, response); // 지정한 대리자 함수 호출
                        }
                    });
                }
            }
        }
        else
        {
            // 2개 이상이라면 트랜잭션에 묶어서 업데이트
            // 단 10개 이상이면 트랜잭션 실패 주의
            List<TransactionValue> transactionList = new List<TransactionValue>();

            // 변경된 데이터만큼 트랜잭션 추가
            foreach (var gameData in gameDatas)
            {
                transactionList.Add(gameData.GetTransactionUpdateValue());
            }

            SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, transactionList, callback =>
            {
                Debug.Log($"Backend.BMember.TransactionWriteV2 : {callback}");

                if (callback.IsSuccess())
                {
                    foreach (var data in gameDatas)
                    {
                        data.IsChangedData = false;
                        data.LocalDataUpdate();
                    }
                }
                else
                {
                    foreach (var data in gameDatas)
                    {
                        data.IsChangedData = false;
                        data.RevertData();  
                    }
                    SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString() + "\n" + info);
                }

                Debug.Log($"TransactionWriteV2 : {callback}\n업데이트 테이블 : \n{info}");

                if (afterUpdateFunc == null)
                {

                }
                else
                {

                    afterUpdateFunc(callback, response);  // 지정한 대리자 함수 호출
                }
            });
        }
    }

    // 일정 주기마다 랭킹 데이터 업데이트 호출
    private IEnumerator UpdateRankScore()
    {
        var seconds = new WaitForSeconds(650);

        yield return seconds;

        // 에러 발생시 true가 될때까지
        while (_isErrorOccured)
        {

            foreach (var li in Rank.List)
            {
                UpdateUserRankScore(li.uuid, null);
            }

            yield return seconds;
        }
    }

    public void UpdateUserRankScore(string uuid, AfterUpdateFunc afterUpdateFunc)
    {
        // 쓰기 비용의 부담이 클 경우에는 각 랭킹별로 Param을 업데이트 하도록 설정.(현재는 일괄 처리)
        // 바뀐 데이터가 몇개 있는지 체크
        List<GameData> gameDatas = new List<GameData>();

        foreach (var gameData in GameData.GameDataList)
        {
            gameDatas.Add(gameData.Value);

        }

        foreach (var li in Rank.List)
        {

            //업데이트하고자 하는 uuid 존재하는지 확인
            if (li.uuid.Equals(uuid))
            {

                // 랭크 리스트에 있는 테이블 이름과 현재 테이블 이름이 있는지 확인하고 존재한다면 해당 게임테이블을 전체 업데이트한다
                int index = gameDatas.FindIndex(item => item.GetTableName().Equals(li.table));
                if (index < 0)
                {
                    afterUpdateFunc?.Invoke(null, null);
                }
                SendQueue.Enqueue(Backend.URank.User.UpdateUserScore, li.uuid, li.table,
                    gameDatas[index].GetInDate(), gameDatas[index].GetParam(),
                    callback =>
                    {
                        Debug.Log($"Backend.URank.User.UpdateUserScore({li.uuid}, {li.table}, {gameDatas[index].GetInDate()}) : {callback}");
                        if (!callback.IsSuccess())
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), callback.ToString());
                        }

                        if (afterUpdateFunc != null)
                        {
                            afterUpdateFunc.Invoke(callback, null);
                        }
                    });
            }
        }
    }

    // 에러 발생시 게임로그를 삽입하는 함수
    public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
    {

        // 에러가 실패할 경우 재귀함수를 통해 최대 3번까지 호출을 시도한다.
        if (repeatCount <= 0)
        {
            return;
        }

        // 아직 로그인되지 않을 경우 뒤끝 함수 호출이 불가능하여 UI에 띄운다.
        if (string.IsNullOrEmpty(Backend.UserInDate))
        {
            //StaticManager.UI.AlertUI.SetYetLoginErrorText();
            return;
        }

        Param param = new Param();
        param.Add("className", className);
        param.Add("functionName", functionName);
        param.Add("errorPath", errorInfo);

        // [뒤끝] 로그 삽입 함수
        Backend.GameLog.InsertLog("error", param, 7, callback =>
        {
            // 에러가 발생할 경우 재귀
            if (callback.IsSuccess() == false)
            {
                SendBugReport(className, functionName, errorInfo, repeatCount - 1);
            }
        });
    }



    //차트 버전 정보
    public Dictionary<string, string> ChartLocalVersion; //로컬에 저장된 버전


    bool LocalChartUpdate = false;

    public void LoadChartVersion()
    {
        LocalChartUpdate = false;
        ChartLocalVersion = JsonHelper.LoadJsonFile<Dictionary<string, string>>(Application.persistentDataPath, "ChartVersion");
        if (ChartLocalVersion == null)
        {
            ChartLocalVersion = new Dictionary<string, string>();
            ChartLocalVersion.Clear();
        }

    }

    public void SaveChartVersion()
    {
        if (LocalChartUpdate == true)
        {
            Debug.Log("로컬 차트 정보 저장");
            string jsonData = JsonHelper.SerializeObject(ChartLocalVersion);
            JsonHelper.CreateJsonFile(Application.persistentDataPath, "ChartVersion", jsonData);
        }
    }

    public void UpdateChartVersion(string chartName, string chartid)
    {
        Debug.Log("로컬 차트 업데이트 // 차트 이름 : " + chartName);
        if (ChartLocalVersion.ContainsKey(chartName) == true)
        {
            ChartLocalVersion[chartName] = chartid;
        }
        else
        {
            ChartLocalVersion.Add(chartName, chartid);
        }
        LocalChartUpdate = true;
    }

    //백엔드 포스트 시스템

    public Dictionary<int, PostData> PostList { get; private set; } = new Dictionary<int, PostData>();

    /// <summary>
    /// 우편 리스트 가져오기
    /// </summary>
    /// <param name="postType"></param>
    /// <param name="Update"></param>
    public void PostListGet(PostType postType, BackendUpdate Update)
    {
        Backend.UPost.GetPostList(postType, callback =>
        {
            if (callback.IsSuccess())
            {
                LitJson.JsonData jsonData = callback.GetFlattenJSON()["postList"];

                if (jsonData.Count <= 0)
                {
                    Debug.LogWarning("우편함이 비어있습니다.");
                }
                PostList.Clear();

                for(int i = 0; i< jsonData.Count; ++i)
                {
                    int Key = i;
                    PostData post = new PostData();        
                    post.idx = Key;
                    post.title = jsonData[i]["title"].ToString();
                    post.content = jsonData[i]["content"].ToString();
                    post.inDate = jsonData[i]["inDate"].ToString();
                    post.expirationDate = jsonData[i]["expirationDate"].ToString();

                    post.postReward.Clear();
                    foreach ( LitJson.JsonData itemJson in jsonData[i]["items"])
                    {
                        if (itemJson["chartName"].ToString() == "AdminPostReward")
                        {
                            int amount = int.Parse(itemJson["itemCount"].ToString());
                            AdminPostReward reward = new AdminPostReward(itemJson["item"], amount);
                            post.postReward.Add(reward);
                            post.isCanReceive = true;
                        }
                    }

                    PostList.Add(Key, post);
                }

                Update(null);
            }
            else
            {
                Debug.LogError($"우편 불러오기중 에러가 발생. : {callback}");
            }
        });
    }


    /// <summary>
    /// 우편 받기
    /// </summary>
    /// <param name="postType"></param>
    /// <param name="index">-1인경우 전체 받기</param>
    /// <param name="Update"></param>
    public void PostReceive(PostType postType, int index, BackendUpdate Update)
    {
        if (PostList.Count > 0)
        {
            if (index <= -1) //-1을 넣으면 전체
            {
                Backend.UPost.ReceivePostItemAll(postType, callback =>
                {
                    if (callback.IsSuccess() == true)
                    {
                        Update((object)PostList);
                        PostList.Clear();
                    }
                    else
                    {
                        Debug.LogError($"우편 수령 중 에러가 발생 {callback}");
                    }
                });
            }
            else
            {
                if (index < PostList.Count)
                {
                    Backend.UPost.ReceivePostItem(postType, PostList[index].inDate, callback =>
                    {
                        if (callback.IsSuccess() == true)
                        {
                            Update((object)PostList[index]);
                            PostList.Remove(index);
                        }
                        else
                        {
                            Debug.LogError($"우편 수령 중 에러가 발생 {callback}");
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("해당 우편이 존재하지 않음");
                }
            }
        }
        else
        {
            Debug.LogWarning("받을수 있는 우편이 존재하지 않음");
        }
    }


}