using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    

    public void LoadTempGameScene()
    {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("TempPlay");
    }


    public void BackToLobby()
    {
        KOBManager.FrontUI.BackToLobby();
    }



}
