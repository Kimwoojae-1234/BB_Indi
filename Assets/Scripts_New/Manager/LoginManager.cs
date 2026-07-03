using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using BackEnd;
using LitJson;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private GameObject Popup;
    [SerializeField] private GameObject FrontUI;

    public static bool LoginSuccess = false;
    [SerializeField] private string RandomID = string.Empty;
    private const string Password = "Zeratul";
    private const string CustomIdPrefsKey = "KOB_BACKEND_CUSTOM_ID_V2";
    private const string CustomIdPrefix = "KG";


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
            Debug.LogError("Backend initialize failed.");
        }
    }

    private void LoginWithTheBackendToken()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string hash = Backend.Utils.GetGoogleHash();
        Debug.Log("Google Hash : " + hash);
#endif

        BackendReturnObject bro = Backend.BMember.LoginWithTheBackendToken();
        if (bro.IsSuccess())
        {
            BackendReturnObject bro2 = Backend.BMember.IsAccessTokenAlive();
            if (bro2.IsSuccess())
            {
                Debug.Log("Access token is alive : " + bro2);
                BackendReturnObject bro3 = Backend.BMember.RefreshTheBackendToken();
                Debug.Log("Refresh backend token : " + bro3);
            }

            OnLoginSuccess();
        }
        else
        {
            Debug.LogWarning("Backend token login failed. Start custom login. " + bro);
            logInProcess();
        }
    }

    private void logInProcess()
    {
        customLogIn();
    }


    private void customLogIn()
    {
        RandomID = GetCustomId();
        if (string.IsNullOrEmpty(RandomID))
        {
            Debug.LogError("Custom login id is empty.");
            return;
        }

        if (!TryCustomLoginWithId(RandomID))
        {
            Debug.LogWarning("Custom login failed with saved id. Regenerate guest id and retry once.");
            RandomID = CreateAndSaveCustomId(System.Guid.NewGuid().ToString("N"));
            TryCustomLoginWithId(RandomID);
        }
    }

    private bool TryCustomLoginWithId(string customId)
    {
        Debug.Log("Try custom login id : " + customId);

        BackendReturnObject signUpBro = Backend.BMember.CustomSignUp(customId, Password);
        LogBackendResult("CustomSignUp", signUpBro);

        BackendReturnObject bro = Backend.BMember.CustomLogin(customId, Password);
        LogBackendResult("CustomLogin", bro);
        if (bro.IsSuccess())
        {
            OnLoginSuccess();
            return true;
        }

        return false;
    }

    private string GetCustomId()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(RandomID))
        {
            return RandomID.Trim();
        }
#endif

        string savedId = PlayerPrefs.GetString(CustomIdPrefsKey, string.Empty);
        if (IsValidCustomId(savedId))
        {
            return savedId;
        }

        return CreateAndSaveCustomId(SystemInfo.deviceUniqueIdentifier);
    }

    private string CreateAndSaveCustomId(string seed)
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        string newId = BuildCustomId(string.IsNullOrWhiteSpace(seed) ? deviceId : seed);

        PlayerPrefs.SetString(CustomIdPrefsKey, newId);
        PlayerPrefs.Save();
        return newId;
    }

    private string BuildCustomId(string seed)
    {
        string source = string.IsNullOrWhiteSpace(seed) ? System.Guid.NewGuid().ToString("N") : seed;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(source));
            StringBuilder builder = new StringBuilder(CustomIdPrefix, 20);
            for (int i = 0; i < 9; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    private bool IsValidCustomId(string customId)
    {
        if (string.IsNullOrWhiteSpace(customId))
        {
            return false;
        }

        if (customId.Length != 20 || !customId.StartsWith(CustomIdPrefix))
        {
            return false;
        }

        for (int i = CustomIdPrefix.Length; i < customId.Length; i++)
        {
            char c = customId[i];
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
            {
                return false;
            }
        }

        return true;
    }

    private void LogBackendResult(string label, BackendReturnObject bro)
    {
        if (bro.IsSuccess())
        {
            Debug.Log(label + " success // bro " + bro);
        }
        else
        {
            Debug.LogWarning(label + " failed // bro " + bro);
        }
    }

    private void OnLoginSuccess()
    {
        Debug.Log("Backend login success. Start server setting load.");
        KOBManager.Backend.Setting.InitFromServer();
        Debug.Log("Server setting load finished. Start chart version load.");
        KOBManager.Backend.LoadChartVersion();
        LoginSuccess = true;
        Debug.Log("LoginSuccess set true.");
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
        if (bro.IsSuccess())
        {
            Debug.Log("Logout success");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogOut();
        }
    }
#endif
}
