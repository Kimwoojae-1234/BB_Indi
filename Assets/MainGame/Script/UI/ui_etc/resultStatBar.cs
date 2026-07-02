using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class resultStatBar : MonoBehaviour
    {
        public bool bPitcher;
        public GameObject bgSpr;
        public UISprite logo;
        public UISprite pos;
        public UILabel[] label;

        public void init(CPlayer player, int teamIndex, int count)
        {
            transform.localScale = Vector3.one;
            bgSpr.gameObject.SetActive(count % 2 == 0 ? true : false);
#if _Test_Local
            logo.spriteName = "logo_" + teamIndex;
#else
            Util.SetSpritePixelPerfect(logo, "logo_" + (int)player.getPlayerData().eTeam);//logo.spriteName = "logo_" + (int)player.getPlayerData().eTeam;
#endif

            //name
            label[1].text = player.getName();

            if (bPitcher == false)
            {
                int position = player.getCurPos();
                if (position > CPlayer._DH) position = player.getPosition();

                pos.spriteName = "position_" + (position + 1);
                //order
                int numOrder = count + 1;
                label[0].text = numOrder > 9 ? "Sub" : numOrder.ToString();
                
                //ability
#if _Test_Local
                //
#else
                label[2].text = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities).ToString();
#endif
                //ab
                label[3].text = player.getStat(Param.ST_AB).ToString();
                //hit
                label[4].text = player.getStat(Param.ST_H).ToString();
                //hr
                label[5].text = player.getStat(Param.ST_HR).ToString();
                //rbi
                label[6].text = player.getStat(Param.ST_RBI).ToString();
                //steal
                label[7].text = player.getStat(Param.ST_SBS).ToString();
                //bb
                label[8].text = player.getStat(Param.ST_BB).ToString();
                //run
                label[9].text = player.getStat(Param.ST_R).ToString();
            }
            else
            {
                pos.spriteName = Util.getPitcherposSprite(player);

                //성적
                label[0].text = Util.pitcherAchieve(player);

                //ability
#if _Test_Local
                //
#else
                label[2].text = Utils.TeamPowerUtils.calCardPower(player.getCard()).ToString();
#endif

                //inning
                int inning = player.getStat(Param.ST_IP);
                label[3].text = (inning / 3) + "." + (inning % 3);

                //run
                label[4].text = player.getStat(Param.ST_PR).ToString();   //자책

                //err
                label[5].text = player.getStat(Param.ST_PER).ToString();   //자책

                //k
                label[6].text = player.getStat(Param.ST_PSO).ToString();

                //h
                label[7].text = player.getStat(Param.ST_PH).ToString();

                //bb
                label[8].text = player.getStat(Param.ST_PBB).ToString();

                //hr
                label[9].text = player.getStat(Param.ST_PHR).ToString();

            }
        }


    }
}