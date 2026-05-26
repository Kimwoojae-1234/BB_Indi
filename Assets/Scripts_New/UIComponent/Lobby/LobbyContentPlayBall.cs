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
        /* //이놈은 전체 시뮬
        KOBManager.Rtts.SimulMyGame(() =>
        {
            KOBManager.Rtts.SimulOtherGames(); //--> 진짜 게임에서는 찬스모드 돌린 후 이놈만 호출하면
        });*/


        //SimulGame();// --> 이게 테스트용



        //그냥 게임         
        //일반게임 테스트
        //KOBManager.FrontUI.OpenPopup<FrontUI_IngameLoading>().IngameLoading(Mode.SimulMode.None); //일반게임
        //찬스모드 테스트 --> 결국에 이거고 이게 끝나면 SimulOtherGames 돌리면 됨
        //KOBManager.FrontUI.OpenPopup<FrontUI_IngameLoading>().IngameLoading(Mode.SimulMode.Chance); //찬스모드
          

        //찬스모드--> 정체불명임
        //KOBManager.Popup.OpenPopup<Popup_GameSimulator>().StartGame(null);

        isBtnActive = true;
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
