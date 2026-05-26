using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BackEnd;
using System.Reflection;
using LitJson;

public class UI_Loading : MonoBehaviour
{
    [SerializeField] LoginManager Login = null;
    [SerializeField] GameObject LogoCanvas = null;
    [SerializeField] Slider Slider = null;
    [SerializeField] TextMeshProUGUI Text = null;
    [SerializeField] TextMeshProUGUI StateText = null;


    private int _maxLoadingCount; // 총 뒤끝 함수를 호출할 갯수

    private int _currentLoadingCount; // 현재 뒤끝 함수를 호출한 갯수

    private delegate void BackendLoadStep();
    private readonly Queue<BackendLoadStep> _initializeStep = new Queue<BackendLoadStep>();

    private void Awake()
    {
        if (LogoCanvas != null)
        {
            //로고로 부터 로딩
            DontDestroyOnLoad(LogoCanvas);
        }
        else
        {
            //게임에서 로비로 
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 게임 시작 시 로딩 (첫로딩)
    /// </summary>
    public void StartLoading()
    {
        bServerConnectComplete = false;
        Init();
        // 뒤끝 데이터 초기화
        KOBManager.Backend.InitInGameData();
        //Queue에 저장된 함수 순차적으로 실행
        NextStep(true, string.Empty, string.Empty, string.Empty);
        StartCoroutine(startLoadingProcess());
    }


    /// <summary>
    /// 백엔드 및 차트 초기화 (첫로딩시)
    /// </summary>
    void Init()
    {
        _initializeStep.Clear();
        // 트랜잭션으로 불러온 후, 안불러질 경우 각자 Get 함수로 불러오는 함수 *중요*
        _initializeStep.Enqueue(() => { ShowDataName("Loading User Data"); TransactionRead(NextStep); });

        // 차트정보 불러오기 함수 Insert
        _initializeStep.Enqueue(() => { ShowDataName("Loading All Chart Data"); KOBManager.Backend.Chart.ChartInfo.BackendLoad(NextStep); }); //모든 차트 정보 수집
        //기본 차트 정보 -> 버전업이 아닌경우 로컬에 저장하여 사용
        _initializeStep.Enqueue(() => { ShowDataName("Loading Character Data Info"); KOBManager.Backend.Chart.CharacterData.BackendChartAndSave(NextStep); }); //선수데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Hitter Level Data Info"); KOBManager.Backend.Chart.HitterLevelData.BackendChartAndSave(NextStep); }); //선수레벨별 능력치
        _initializeStep.Enqueue(() => { ShowDataName("Loading Pitcher Level Data Info"); KOBManager.Backend.Chart.PitcherLevelData.BackendChartAndSave(NextStep); }); //투수레벨별 능력치
        _initializeStep.Enqueue(() => { ShowDataName("Loading Character Skill Info"); KOBManager.Backend.Chart.HitterSkillData.BackendChartAndSave(NextStep); }); //선수 스킬 정보
        _initializeStep.Enqueue(() => { ShowDataName("Loading Skill Data Info"); KOBManager.Backend.Chart.SkillData.BackendChartAndSave(NextStep); }); //스킬데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Gear Data Info"); KOBManager.Backend.Chart.GearData.BackendChartAndSave(NextStep); }); //장비데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Gear Attribute"); KOBManager.Backend.Chart.GearAttributes.BackendChartAndSave(NextStep); }); //장비능력치
        _initializeStep.Enqueue(() => { ShowDataName("Loading Data Info"); KOBManager.Backend.Chart.UpgradeData.BackendChartAndSave(NextStep); }); //업글데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Trophy Data"); KOBManager.Backend.Chart.TrophyRoadData.BackendChartAndSave(NextStep); }); //트로피로드데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Baller Reputation Data"); KOBManager.Backend.Chart.BallerTrophyRoadData.BackendChartAndSave(NextStep); }); //볼러트로피로드데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Baller Achevement Data"); KOBManager.Backend.Chart.AchievementData.BackendChartAndSave(NextStep); }); //볼러업적데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Reward Data"); KOBManager.Backend.Chart.RewardData.BackendChartAndSave(NextStep); }); //보상정보
        //이 위로 확정


        //이 밑으로 뺄수도 있음
        _initializeStep.Enqueue(() => { ShowDataName("Loading Consume Item Info"); KOBManager.Backend.Chart.ConsumeItem.BackendChartAndSave(NextStep); }); //소비성데이터
        _initializeStep.Enqueue(() => { ShowDataName("Loading Season Pass Info"); KOBManager.Backend.Chart.SeasonPassBase.BackendChartAndSave(NextStep); }); //시즌패스
        _initializeStep.Enqueue(() => { ShowDataName("Loading Season Reward Info"); KOBManager.Backend.Chart.SeasonPassReward.BackendChartAndSave(NextStep); }); //시즌패스보상
        //Rtts
        _initializeStep.Enqueue(() => { ShowDataName("Loading Rtts Schedule Info"); KOBManager.Backend.Chart.RttsSchedule.BackendChartAndSave(NextStep); }); //Rtts 스케쥴
        _initializeStep.Enqueue(() => { ShowDataName("Loading Rtts Data Info"); KOBManager.Backend.Chart.RttsInfo.BackendChartAndSave(NextStep); }); //Rtts 인포
        _initializeStep.Enqueue(() => { ShowDataName("Loading Rtts Team Info"); KOBManager.Backend.Chart.RttsTeam.BackendChartAndSave(NextStep); }); //Rtts 팀
        _initializeStep.Enqueue(() => { ShowDataName("Loading Rtts Reward"); KOBManager.Backend.Chart.RttsReward.BackendChartAndSave(NextStep); }); //Rtts 리그보상
        _initializeStep.Enqueue(() => { ShowDataName("Loading Rtts Result Reward"); KOBManager.Backend.Chart.RttsResultReward.BackendChartAndSave(NextStep); }); //Rtts 리그 종료후보상


        // 랭킹 정보 불러오기 함수 Insert
        //_initializeStep.Enqueue(() => { ShowDataName("랭킹 정보 불러오기"); StaticManager.Backend.Rank.BackendLoad(NextStep); });
        // 우편 정보 불러오기 함수 Insert
        //_initializeStep.Enqueue(() => { ShowDataName("관리자 우편 정보 불러오기"); StaticManager.Backend.Post.BackendLoad(NextStep); });
        //_initializeStep.Enqueue(() => { ShowDataName("랭킹 우편 정보 불러오기"); StaticManager.Backend.Post.BackendLoadForRank(NextStep); });

        //다음 씬으로 넘어가는 함수 Insert

        //게이지 바 지정
        _maxLoadingCount = _initializeStep.Count;
        _currentLoadingCount = 0;
    }

    private void NextStep(bool isSuccess, string className, string funcName, string errorInfo)
    {
        if (isSuccess)
        {
            _currentLoadingCount++;
            //loadingSlider.value = _currentLoadingCount;

            if (_initializeStep.Count > 0)
            {
                _initializeStep.Dequeue().Invoke();
            }
            else
            {
                bServerConnectComplete = true;
            }
        }
        else
        {
            //StaticManager.UI.AlertUI.OpenErrorUI(className, funcName, errorInfo);
            Debug.LogError("Loading Error // className: " + className + "  // funcName: " + funcName + " // errorInfo : " + errorInfo);
        }
    }

    private void TransactionRead(BackendData.Base.Normal.AfterBackendLoadFunc func)
    {
        bool isSuccess = false;
        string className = GetType().Name;
        string functionName = MethodBase.GetCurrentMethod()?.Name;
        string errorInfo = string.Empty;

        //트랜잭션 리스트 생성
        List<TransactionValue> transactionList = new List<TransactionValue>();

        // 게임 테이블 데이터만큼 트랜잭션 불러오기
        foreach (var gameData in KOBManager.Backend.GameData.GameDataList)
        {
            transactionList.Add(gameData.Value.GetTransactionGetValue());
        }

        // [뒤끝] 트랜잭션 읽기 함수
        SendQueue.Enqueue(Backend.GameData.TransactionReadV2, transactionList, callback => {
            try
            {
                Debug.Log($"Backend.GameData.TransactionReadV2 : {callback}");

                // 데이터를 모두 불러왔을 경우
                if (callback.IsSuccess())
                {
                    JsonData gameDataJson = callback.GetFlattenJSON()["Responses"];

                    int index = 0;

                    foreach (var gameData in KOBManager.Backend.GameData.GameDataList)
                    {

                        _initializeStep.Enqueue(() => {
                            ShowDataName(gameData.Key);
                            // 불러온 데이터를 로컬에서 파싱
                            gameData.Value.BackendGameDataLoadByTransaction(gameDataJson[index++], NextStep);
                        });
                        _maxLoadingCount++;

                    }
                    // 최대 작업 개수 증가
                    //loadingSlider.maxValue = _maxLoadingCount;
                    isSuccess = true;
                }
                else
                {
                    // 트랜잭션으로 데이터를 찾지 못하여 에러가 발생한다면 개별로 GetMyData로 호출
                    foreach (var gameData in KOBManager.Backend.GameData.GameDataList)
                    {
                        _initializeStep.Enqueue(() => {
                            ShowDataName(gameData.Key);
                            // GetMyData 호출
                            gameData.Value.BackendGameDataLoad(NextStep);
                        });
                        _maxLoadingCount++;
                    }
                    // 최대 작업 개수 증가
                    //loadingSlider.maxValue = _maxLoadingCount;
                    isSuccess = true;
                }
            }
            catch (Exception e)
            {
                errorInfo = e.ToString();
            }
            finally
            {
                func.Invoke(isSuccess, className, functionName, errorInfo);
            }
        });
    }


    private void ShowDataName(string loadingText)
    {
        StateText.text = loadingText;
    }

    bool bServerConnectComplete = false;

    /// <summary>
    /// 첫로딩의 로딩 프로세스
    /// </summary>
    /// <returns></returns>
    IEnumerator startLoadingProcess()
    {        
        yield return null;
        //서버 접속
        //우선 가라로 처리
        float value = 0;
        float lastValue = -1;
        while (!bServerConnectComplete)
        {
            yield return null;
            value = (float)_currentLoadingCount / (2.0f * _maxLoadingCount);
            if (lastValue != value)
            {
                Slider.value = value;
                string Percent = string.Format("{0}%", (int)(value * 100));
                Text.text = Percent;
                lastValue = value;
            }
            if(_currentLoadingCount>= _maxLoadingCount)
            {
                break;
            }
        }

        //로컬 차트 정보 저장
        KOBManager.Backend.SaveChartVersion();

        yield return null;

        //KOB 상수 초기화
        KOBConstant.InitConstant();

        yield return null;

        
        AsyncOperation async = null;
        if (KOBManager.Tuto.IsTuroialComplete(TutorialManager.TutoStep.FirstTuto) == false) //튜토리얼 진입
        {
            ShowDataName("Entering Tutorial");
            //신로딩
            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("TutorialScene");
        }
        else if (KOBManager.Tuto.IsTuroialComplete(TutorialManager.TutoStep.NickNameSetting) == false) //이름
        {
            ShowDataName("Entering Nickname Setting");
            //신로딩
            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("NicknameScene");
        }
        else
        {
            ShowDataName("Entering the Main Lobby");
            //신로딩
            async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainLobby");
        }

        if (async != null)
        {
            while (!async.isDone)
            {
                yield return null;
                value = (0.5f + async.progress * 0.5f);
                Slider.value = value;
                string Percent = string.Format("{0}%", (int)(value * 100));
                Text.text = Percent;
            }            
            yield return new WaitForSeconds(0.5f);            
            DG.Tweening.DOTweenAnimation animator = gameObject.GetComponent<DG.Tweening.DOTweenAnimation>();
            animator.DOPlay();
        }
    }


    /// <summary>
    /// 게임에서 로비로 돌아감 (백투로비)
    /// </summary>
    public void BackToLobby()
    {
        StartCoroutine(backToLobbyProcess());
    }

    /// <summary>
    /// 백투로비 프로세스
    /// </summary>
    /// <returns></returns>
    private IEnumerator backToLobbyProcess()
    { 
        ShowDataName("Entering the Main Lobby");
        Slider.value = 0;
        yield return new WaitForSeconds(0.25f);
        //신로딩
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainLobby");
        while (!async.isDone)
        {
            yield return null;
            Slider.value = async.progress;
            string Percent = string.Format("{0}%", (int)(async.progress * 100));
            Text.text = Percent;
        }
        
        //yield return new WaitForSeconds(0.5f);
        DG.Tweening.DOTweenAnimation animator = gameObject.GetComponent<DG.Tweening.DOTweenAnimation>();
        animator.DOPlay();
    }


    /// <summary>
    /// 로딩 완료시
    /// </summary>
    public void LoadingComplete()
    {
        if (LogoCanvas != null)
        {
            //첫로딩시는 로고 캔버스 삭제 (이게 부모이므로)
            Destroy(LogoCanvas);
        }
        else
        {
            //백투로비시는 자신 삭제 (부모는 frontUI)
            Destroy(gameObject);
        }
    }
}
