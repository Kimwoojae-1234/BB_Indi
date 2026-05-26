using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class resultGrowBar : MonoBehaviour
    {
        public const int MAX_GRADE = 7;

        public UISprite pos1, pos2;
        public UILabel name, overall;
        public GameObject gaugeObj;

        public void setPlayer(WebConnector.GameCardInfo growCard, CPlayer player, bool bPitcher,float delay, bool bMaxLevel = false)
        {
            transform.localScale = Vector3.one;
#if _Test_Local

#else
            //int curLevel = player.getCard().level;
            //int curExp = player.getCard().exp;
            //Debug.Log(player.getName() + " : 현 exp : " + player.getCard().exp + " ->  다음 exp : " + growCard.exp + "   등급 : " + growCard.grade);            

            if (bPitcher == false)
            {
                pos1.spriteName = "position_bg_b";
                pos2.spriteName = "position_" + (player.getPosition() + 1).ToString();
                overall.text = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities).ToString();
            }
            else
            {
                pos1.spriteName = "position_bg_p";
                pos2.spriteName = Util.getPitcherposSprite(player);
                overall.text = Utils.TeamPowerUtils.calCardPower(player.getCard()).ToString();
            }
            name.text = player.getName();

            //임시
            gaugeObj.GetComponent<gaugePlayerMove>().SetPlayerExp(growCard.grade,
                                        player.getCard().level, player.getCard().exp,
                                        growCard.level, growCard.exp,
                                        delay);
            gaugePlayerMove gauge = gaugeObj.GetComponent<gaugePlayerMove>();
            if (bMaxLevel == false)
            {
                gauge.SetPlayerExp(growCard.grade,
                                        player.getCard().level, player.getCard().exp,
                                        growCard.level, growCard.exp,
                                        delay);
            }
            else
            {
                //이미 맥스레벨 달성
                bool bFinalGrade = (growCard.grade == MAX_GRADE ? true : false);
                gauge.setMaxLevel(true, bFinalGrade);
            }
            
#endif
        }


        public void gotoUpgrade(gaugePlayerMove gauge)
        {
            if (gauge.bMaxLevel == true)
            {
                ResultUI.GotoUpgrade();
            }
        }


    }

}
