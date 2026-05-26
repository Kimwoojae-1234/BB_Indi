using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackendData.Base;

public interface ManagerEventHandler
{
    void OnReceiveEvent(GameDefine.eEvent manager_event, GameDefine.EventParam? parameter);
}


public class KOBManager : MonoBehaviour
{
    private static KOBManager instance = null;
    private static GameObject MainManagerObject = null;
    //private static List<ManagerEventHandler> ManagerEvents = null;    

    private void OnDestroy()
    {
        if (instance != null)
        {
            Databasemanager = null;
            Myinfomanager = null;
            Localizationmanager = null;
            Resourcemanager = null;            
            uiManager = null;
            popManager = null;
            frontuiManager = null;
            backendManager = null;
            atlasManager = null;
            uiFXManager = null;
            stateManager = null;
            dummyNetwork = null;
            tutorialManager = null;
            rttsManager = null;
            ballerManager = null;

            instance = null;
            Destroy(MainManagerObject);
        }
    }

    private static KOBManager Instance
    {
        get
        {
            if (instance == null)
            {
                MainManagerObject = new GameObject("KOBManager");
                instance = MainManagerObject.AddComponent<KOBManager>();
                //ManagerEvents = new List<ManagerEventHandler>();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }



    private static DatabaseManager Databasemanager;
    public static DatabaseManager Database
    {
        get
        {
            if (Databasemanager == null)
            {
                GameObject DatabaseManagerObj = new GameObject("DatabaseManager");
                Databasemanager = DatabaseManagerObj.AddComponent<DatabaseManager>();
                Databasemanager.transform.SetParent(Instance.gameObject.transform);
            }
            return Databasemanager;
        }
    }

    private static MyInfoManager Myinfomanager;
    public static MyInfoManager MyInfo
    {
        get
        {
            if (Myinfomanager == null)
            {
                GameObject MyinfoObj = new GameObject("MyInfoManager");
                Myinfomanager = MyinfoObj.AddComponent<MyInfoManager>();
                Myinfomanager.transform.SetParent(Instance.gameObject.transform);
            }
            return Myinfomanager;
        }
    }


    private static LocalizationManager Localizationmanager;

    public static LocalizationManager Localization
    {
        get
        {
            if (Localizationmanager == null)
            {
                GameObject LocalizationObj = new GameObject("LocaliztionManager");
                Localizationmanager = LocalizationObj.AddComponent<LocalizationManager>();
                Localizationmanager.transform.SetParent(Instance.gameObject.transform);
            }
            return Localizationmanager;
        }
    }


    private static ResourceManager Resourcemanager;

    public static ResourceManager Resource
    {
        get
        {
            if (Resourcemanager == null)
            {
                GameObject ResourceObj = new GameObject("ResourceManager");
                Resourcemanager = ResourceObj.AddComponent<ResourceManager>();
                Resourcemanager.transform.SetParent(Instance.gameObject.transform);
            }
            return Resourcemanager;
        }
    }


    private static UIManager uiManager;
    public static UIManager UI
    {
        get
        {
            if (uiManager == null)
            {
                GameObject uiManagerObj = new GameObject("UIManager");
                uiManager = uiManagerObj.AddComponent<UIManager>();
                uiManager.transform.SetParent(Instance.gameObject.transform);
            }
            return uiManager;
        }
    }


    private static PopupManager popManager;
    public static PopupManager Popup
    {
        get
        {
            if (popManager == null)
            {
                GameObject popupManagerObj = new GameObject("PopupManager");
                popManager = popupManagerObj.AddComponent<PopupManager>();
                popManager.transform.SetParent(Instance.gameObject.transform);
            }
            return popManager;
        }
    }

    private static FrontUIManager frontuiManager;
    public static FrontUIManager FrontUI
    {
        get
        {
            if (frontuiManager == null)
            {
                GameObject frontuiManagerObj = new GameObject("FrontUIManager");
                frontuiManager = frontuiManagerObj.AddComponent<FrontUIManager>();
                frontuiManager.transform.SetParent(Instance.gameObject.transform);
            }
            return frontuiManager;
        }
    }


    private static BackendManager backendManager;
    public static BackendManager Backend
    {
        get
        {
            if (backendManager == null)
            {
                GameObject backendManagerObj = new GameObject("BackendManager");
                backendManager = backendManagerObj.AddComponent<BackendManager>();
                backendManager.transform.SetParent(Instance.gameObject.transform);
            }
            return backendManager;
        }
    }



    private static AtlasManager atlasManager;
    public static AtlasManager Atlas
    {
        get
        {
            if (atlasManager == null)
            {
                GameObject spriteAtlasManagerObj = GameObject.Instantiate(Resources.Load("Atlas/AtlasManager"), Vector3.zero, Quaternion.identity) as GameObject;
                atlasManager = spriteAtlasManagerObj.GetComponent<AtlasManager>();
                atlasManager.transform.SetParent(Instance.gameObject.transform);
            }
            return atlasManager;
        }
    }



    private static UIFXManager uiFXManager;
    public static UIFXManager UIFX
    {
        get
        {
            if (uiFXManager == null)
            {
                GameObject uifxManagerObj = GameObject.Instantiate(Resources.Load("UI/UIFXManager"), Vector3.zero, Quaternion.identity) as GameObject;
                uiFXManager = uifxManagerObj.GetComponent<UIFXManager>();
                uiFXManager.transform.SetParent(Instance.gameObject.transform);
            }
            return uiFXManager;
        }
    }


    private static StateManager stateManager;
    public static StateManager State
    {
        get
        {
            if (stateManager == null)
            {
                GameObject stateManagerObj = new GameObject("StateManager");
                stateManager = stateManagerObj.AddComponent<StateManager>();
                stateManager.transform.SetParent(Instance.gameObject.transform);
            }
            return stateManager;
        }
    }



    private static DummyNetworkManager dummyNetwork = null;
    public static DummyNetworkManager DummyNetwork
    {
        get
        {
            if (dummyNetwork == null)
            {
                GameObject dummyManagerObj = new GameObject("DummyNetworkManager");
                dummyNetwork = dummyManagerObj.AddComponent<DummyNetworkManager>();
                dummyNetwork.transform.SetParent(Instance.gameObject.transform);
            }
            return dummyNetwork;
        }
    }






    private static TutorialManager tutorialManager = null;
    public static TutorialManager Tuto
    {
        get
        {
            if (tutorialManager == null)
            {
                GameObject tutorialManagerObj = new GameObject("TutorialManager");
                tutorialManager = tutorialManagerObj.AddComponent<TutorialManager>();
                tutorialManager.transform.SetParent(Instance.gameObject.transform);
            }
            return tutorialManager;
        }
    }



    private static RttsManager rttsManager = null;
    public static RttsManager Rtts
    {
        get
        {
            if (rttsManager == null)
            {
                GameObject rttsManagerObj = new GameObject("RttsManager");
                rttsManager = rttsManagerObj.AddComponent<RttsManager>();
                rttsManager.transform.SetParent(Instance.gameObject.transform);
            }
            return rttsManager;
        }
    }


    private static BallerManager ballerManager = null;
    public static BallerManager Baller
    {
        get
        {
            if (ballerManager == null)
            {
                GameObject ballerManagerObj = new GameObject("BallerManager");
                ballerManager = ballerManagerObj.AddComponent<BallerManager>();
                ballerManager.transform.SetParent(Instance.gameObject.transform);
            }
            return ballerManager;
        }
    }
}
