using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;
namespace BaseBall.BallPlay
{
    public class individualTitle : MonoBehaviour
    {
        public UI_CardSmall card;
        public UILabel teamNameLabel;
        public UISprite myTeamSpr, myTealLogoSpr;
        public UILabel goldLabel;

        public UILabel info;


        public void setIndividualTitle(SeasonTitleRewardInfo rewardTitle, FinalResultUI resultMain)
        {
            //팀인포
            SimpleTeamInfo teamInfo = resultMain.getTeamInfo(rewardTitle.teamNo);
            //팀로고 설정
            Util.SetSpritePixelPerfect(myTealLogoSpr, "logo_" + (int)teamInfo.team);//myTealLogoSpr.spriteName = "logo_" + (int)teamInfo.team;
            //팀이름 설정
            teamNameLabel.text = teamInfo.name;

            //내팀선수 수상
            bool bMyTeam = (rewardTitle.teamNo == BHConst.myTeamNo ? true : false);

            //획득 골드 설정
            goldLabel.text = string.Format("{0:n0}", rewardTitle.rwdGold);
            goldLabel.color = (bMyTeam ? new Color(1, 0.832f, 0) : new Color(0.5f, 0.5f, 0.5f));
            //내팀 선수 여부 설정            
            myTeamSpr.gameObject.SetActive(bMyTeam);
            //카드 세팅
            CardData data = new CardData(rewardTitle.cardInfo);
            card.SetCardInfo(data);
            //선수 기록
            info.text = rewardTitle.dpRec;

        }

    }
}