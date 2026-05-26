using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderComponent : MonoBehaviour
{
    [SerializeField] private Image bg;
    [SerializeField] private TextMeshProUGUI RankTxt;
    [SerializeField] private Image logo;
    [SerializeField] private TextMeshProUGUI PlayerTxt;
    [SerializeField] private TextMeshProUGUI KategorieTxt;
    [SerializeField] private TextMeshProUGUI ValueTxt;


    public bool InitComp(int idx, int value, int rank, int kategorie)
    {
        int teamIdx = idx / KOBConstant.PLAYER_RECORD_UNIT;
        int playerIdx = idx % KOBConstant.PLAYER_RECORD_UNIT;

        CharacterData cardData = KOBManager.Backend.Chart.CharacterData.GetData(playerIdx); //고정정보 - 선수고유정보

        RankTxt.text = (rank + 1).ToString();
        RttsTeam teamInfo = KOBManager.Rtts.GetTeam(teamIdx);
        bool isMySelectBaller = KOBManager.Rtts.isSelectBaller(teamIdx, playerIdx);
        bool isMyBaller = (teamIdx == 0 && cardData.char_type == CharacterType.Ballers);

        bg.sprite = KOBManager.Atlas.GetLeaderBgSprite(isMySelectBaller || isMyBaller);
        bg.color = Color.white;


        if (isMySelectBaller == true) //0번은 플레이어 본인
        {
            //내선수 - 나            
            PlayerTxt.text = KOBTextUtil.GetMyPlayerName(playerIdx); //"MY PLAYER";    //임시
        }
        else
        {            
            PlayerTxt.text = cardData.name_id;
        }

        if (teamIdx == 0 && cardData.char_type == CharacterType.Ballers)
        {
            //내팀 로고 -> 플레이어의 초상화를 
            KOBManager.Resource.LoadBallerPortrait(logo,playerIdx);
            logo.transform.localScale = new Vector3(0.5f, 0.5f);
            if (isMySelectBaller == false) bg.color = new Color(0.67f, 0.67f, 0.67f);
        }
        else
        {
            if(teamIdx == 0) KOBManager.Resource.LoadMyTeamLogo(logo); //내팀
            else KOBManager.Resource.LoadTeamLogo(logo, teamInfo.Logo);
            logo.transform.localScale = new Vector3(0.2f, 0.2f);
        }


        //bool isQPACase = false; // 규정타석 케이스
        KategorieTxt.text = KOBTextUtil._RecordTypeName[kategorie]; //임시로                
        if (kategorie == 0) //HR
        {
            ValueTxt.text = value.ToString();
        }        
        else if (kategorie == 2) //RBI
        {
            ValueTxt.text = value.ToString();
        }
        else if (kategorie == 3) //HIT
        {
            ValueTxt.text = value.ToString();
        }
        else if (kategorie == 1 || kategorie == 4) //AVG, OPS
        {
            if (value < KOBConstant.QPA_CONSTANT)
            {
                //규정타석이 아닌 경우
                RankTxt.text = "N/A";
            }
            else
            {
                //규정타석 처리
                value -= KOBConstant.QPA_CONSTANT;
            }
            ValueTxt.text = KOBTextUtil.SetAvgText(value / 100);
        }
        return isMySelectBaller;
    }



}
