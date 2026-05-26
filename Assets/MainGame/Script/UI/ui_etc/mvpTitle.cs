using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;
namespace BaseBall.BallPlay
{
    public class mvpTitle : MonoBehaviour
    {
        public UI_CardSmall card;
        public UILabel teamNameLabel;
        public UISprite myTeamSpr, myTealLogoSpr;
        public UILabel goldLabel;

        public UILabel[] info1;
        public UILabel[] info2;
        public UISprite[] winnner;

        public GameObject batter, pitcher;

        public UISprite cardLight;

        public void initMvp(SeasonTitleMvpRewardInfo mvpInfo, FinalResultUI resultMain, bool bPitcher)
        {
            batter.gameObject.SetActive(!bPitcher);
            pitcher.gameObject.SetActive(bPitcher);

            //팀인포
            SimpleTeamInfo teamInfo = resultMain.getTeamInfo(mvpInfo.teamNo);
            //로고 설정
            Util.SetSpritePixelPerfect(myTealLogoSpr, "logo_" + (int)teamInfo.team);//myTealLogoSpr.spriteName = "logo_" + (int)teamInfo.team;
            //팀이름 설정
            teamNameLabel.text = teamInfo.name;

            //내팀선수 수상
            bool bMyTeam = (mvpInfo.teamNo == BHConst.myTeamNo ? true : false);

            //획득 골드 설정
            goldLabel.text = string.Format("{0:n0}", mvpInfo.rwdGold);
            goldLabel.color = (bMyTeam ? new Color(1, 0.832f, 0) : new Color(0.5f, 0.5f, 0.5f));
            
            //내팀 선수 여부
            myTeamSpr.gameObject.SetActive(bMyTeam);

            //카드 데이터
            CardData data = new CardData(mvpInfo.cardInfo);
            card.SetCardInfo(data);

            //선수 기록
            info1[0].text = mvpInfo.dpRec1;
            info1[1].text = mvpInfo.dpRec2;
            info1[2].text = mvpInfo.dpRec3;
            info1[3].text = mvpInfo.dpRec4;
            info1[4].text = mvpInfo.dpRec5;

            //기록의 리그내 순위
            info2[0].text = mvpInfo.rec1Rank + "위";
            info2[1].text = mvpInfo.rec2Rank + "위";
            info2[2].text = mvpInfo.rec3Rank + "위";
            info2[3].text = mvpInfo.rec4Rank + "위";
            info2[4].text = mvpInfo.rec5Rank + "위";

            //1등인 경우 별표 체크
            winnner[0].gameObject.SetActive(mvpInfo.rec1Rank == 1 ? true : false);
            winnner[1].gameObject.SetActive(mvpInfo.rec2Rank == 1 ? true : false);
            winnner[2].gameObject.SetActive(mvpInfo.rec3Rank == 1 ? true : false);
            winnner[3].gameObject.SetActive(mvpInfo.rec4Rank == 1 ? true : false);
            winnner[4].gameObject.SetActive(mvpInfo.rec5Rank == 1 ? true : false);

            StartCoroutine(setLight());
        }

        private IEnumerator setLight()
        {
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForEndOfFrame();
                cardLight.spriteName = "cardback_" + string.Format("{0:00000}", i);
            }

        }

    }
}