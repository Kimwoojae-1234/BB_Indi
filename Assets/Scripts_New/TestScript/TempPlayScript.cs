using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class TempPlayScript : MonoBehaviour
{
    // Update is called once per frame
    private bool bPress = false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            if (bPress == false)
            {
                Debug.Log("KeyDown A");
                TempGameEnd();
                bPress = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("KeyDown B");
        }
    }



    /// <summary>
    /// 추후 진짜 게임엔드에서 이거 고대로 가져갈것!!!
    /// </summary>
    private void TempGameEnd()
    {
        
        
        KOBManager.State.BackToLobby();
    }

    private void BackEndCallback(BackendReturnObject callback)
    {
        if (callback == null)
        {
            Debug.Log("업데이트 할 내용이 없음");
        }
        else
        {
            if (callback.IsSuccess() == true)
            {
                Debug.Log("서버 사이드 업데이트 성공");
                KOBManager.State.BackToLobby();
            }
            else
            {
                Debug.Log("서버 통신에 실패");
                //이전 값으로 복원할 것
            }
        }
    }
}
