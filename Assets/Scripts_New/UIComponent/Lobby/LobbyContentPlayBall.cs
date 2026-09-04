using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyContentPlayBall : LobbyContentButton
{
    [Header("[플레이 RTTS 전용]")]
    [SerializeField] private Image ButtonImage;
    [SerializeField] private TextMeshProUGUI ButtonTxt;    
    [SerializeField] private RttsRewardComponent rttsRewardInfo = null;


    private bool isRttsActive = false;
    private bool isBtnActive = true;

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("플레이볼 버튼 초기화");
        isRttsActive = false;
        isBtnActive = true;
    }

    public void SetRttsRewardInfo()
    {
        if (rttsRewardInfo != null)
        {
            rttsRewardInfo.gameObject.SetActive(true);
            ButtonImage.gameObject.SetActive(true);
            rttsRewardInfo.InitComp();
            isRttsActive = true;
        }
    }


    public override void UpdateContent()
    {
        base.UpdateContent();
        isBtnActive = !KOBManager.Rtts.IsRoundInProgress;
        if (button != null) button.interactable = isBtnActive;

        if (isUpdate == false)
        {
            //TUTO_STEP -> 특정조건 버튼비활성화
            /*if (lobbyStep == 2) //LobbyFirstTuto 이거일듯
            {
                ButtonImage.gameObject.SetActive(false);
                RttsSlider.gameObject.SetActive(false);
            }*/
            isUpdate = true;
        }
    }

    public override void OnClickButton()
    {
        if (isBtnActive == false) return;
        isBtnActive = false;
        if (button != null) button.interactable = false;
        base.OnClickButton();

        //해당 볼러가 라인업에 있는지 확인 후 진행 한다.
        int baller_idx = KOBManager.MyInfo.GameData.ManageInfo.SelectBaller;
        KOBManager.MyInfo.SetUISelectedBaller(baller_idx); //게임 진입시 안전빵으로 한번더

        KOBManager.Baller.Check_Baller_Put_Lineup(baller_idx, true, () =>
        {
            PlayGame();
        });
    }


    private void PlayGame()
    {
        // 로비와 RTTS 화면의 플레이 버튼은 같은 리그 라운드 진입점입니다.
        // 어느 화면에서 시작했든 결과/보상 종료 후 갱신된 UI_RTTS로 돌아갑니다.
        KOBManager.Rtts.PlayRound(success =>
        {
            // 성공 시에는 결과/보상 화면이 끝날 때까지 중복 입력을 막습니다.
            // 실패한 경우에만 현재 로비에서 다시 시도할 수 있게 버튼을 되살립니다.
            if (!success)
            {
                isBtnActive = true;
                if (button != null) button.interactable = true;
            }
        }, true);
    }



    public void SimulGame()
    {
        StartCoroutine(startGame());
    }


    private IEnumerator startGame()
    {
        
        yield return null;
        
    }


}
