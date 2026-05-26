using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class TempTutorialScript : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            TutorialEnd();
        }
    }



    private void TutorialEnd()
    {
        KOBManager.Tuto.SetTutorialComplete(TutorialManager.TutoStep.FirstTuto, (bool isSuccess) =>
        {
            if ((isSuccess))
            {
                NextScene();
            }
        });
        
    }


    private void NextScene()
    {
        if (KOBManager.Tuto.IsTuroialComplete(TutorialManager.TutoStep.NickNameSetting) == false) //이름
        {
            //ShowDataName("Entering Nickname Setting");
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("NicknameScene");
        }
        else
        {
            //ShowDataName("Entering the Main Lobby");
            KOBManager.State.BackToLobby();
        }

    }
}
