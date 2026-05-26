using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private GameObject Popup;
    [SerializeField] private GameObject FrontUI;

    public static bool LoginSuccess = false;
    [SerializeField] private string RandomID = string.Empty;
    private string Password = "Zeratul";


    private void Awake()
    {
        KOBManager.Popup.Init();
        KOBManager.FrontUI.Init();
        DontDestroyOnLoad(Popup);
        DontDestroyOnLoad(FrontUI);
    }

    // Start is called before the first frame update
    void Start()
    {        
        LoginSuccess = false;
        if (KOBManager.Backend.Init() == true)
        {
            LoginWithTheBackendToken();
        }
        else
        {

        }

        //서버 세팅 
        KOBManager.Backend.Setting.InitFromServer();

        //최신차트 버전
        //KOBManager.Backend.LatestChartVersion();

        //로컬 차트
        KOBManager.Backend.LoadChartVersion();


        /*
        var broString = Backend.Chart.GetLocalChartData("85999");
        JsonData chartJson = JsonMapper.ToObject(broString);
        JsonData chartJson3 = BackendReturnObject.Flatten(chartJson)["rows"];

        BackendReturnObject bro =  BackEnd.Backend.Chart.GetChartContents("85999");
        JsonData chartJson2 = bro.FlattenRows();*/

        LoginSuccess = true;
    }


    private void LoginWithTheBackendToken()
    {
        //백엔드 토큰으로 로그인
        BackendReturnObject bro = Backend.BMember.LoginWithTheBackendToken();
        if (bro.IsSuccess())
        {
            BackendReturnObject bro2 = Backend.BMember.IsAccessTokenAlive();
            if (bro2.IsSuccess())
            {
                Debug.Log("액세스 토큰이 살아있습니다 : " + bro2);
                BackendReturnObject bro3 = Backend.BMember.RefreshTheBackendToken();
                Debug.Log("토큰 리프레시 여부 : " + bro3);
            }
        }
        else
        {
            // 뒤끝 토큰 로그인 실패             
            logInProcess();
        }
        //logInProcess();
    }

    private void logInProcess()
    {
#if UNITY_EDITOR
        customLogIn();
#else
#if UNITY_ANDROID
        googleLogoIn();
#elif UNITY_IOS
        appleLogin();
#else
        customLogIn();
#endif
#endif
    }


    private void customLogIn()
    {        
        if (RandomID == null)
        {
            return;
        }
        
        //사인업 시도
        BackendReturnObject bro = Backend.BMember.CustomSignUp(RandomID, Password);
        if (bro.IsSuccess())
        {
            Debug.Log("새로운 계정 생성");
            //사인업
            /*Debug.Log("Sign up Success // bro " + bro.ToString());
            string _RandomNickName = RandomNickName();
            Debug.Log("RandomNickName : " + _RandomNickName);
            BackendReturnObject bro3 = Backend.BMember.CreateNickname(_RandomNickName);
            if (bro3.IsSuccess())
            {
                Debug.Log("랜덤 닉네임 생성 성공 " + bro3.ToString());
            }
            else
            {
                Debug.Log("랜덤 닉네임 생성 실패 " + bro3.ToString());
            }*/
        }
        else
        {
            Debug.Log("이미 있는 계정");
            //사인업할 필요 없는 경우 로그인 시도
            Debug.Log("Log in Suceess // bro " + bro.ToString());
            BackendReturnObject bro2 = Backend.BMember.CustomLogin(RandomID, Password);
            if (bro2.IsSuccess())
            {
                Debug.Log("bro2 " + bro2.ToString());
            }
            else
            {
                Debug.Log("bro2 " + bro2.ToString());
            }
        }
    }


    private string RandomNickName()
    {
        int RandomCode = Random.Range(0, 999999);
        return string.Format("G{0:000000}", RandomCode);
    }

#if UNITY_EDITOR
    public void LogOut()
    {
        BackendReturnObject bro = Backend.BMember.Logout();
        if(bro.IsSuccess())
        {
            Debug.Log("로그아웃 성공");
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            LogOut();
        }
    }
#endif
}
