using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class CardSmall : MonoBehaviour
    {
        public UISprite spriteBackground; // 선수 백그라운드
        public UISprite spriteNameBackground; //선수 이름 백그라운드
        public UISprite spriteTeamLogo;   // 팀 로고
        public UITexture texturePlayer;     // 선수 사진
        public UILabel labelPlayerName;     // 선수 이름
        public UILabel labelPlayerLevel;    // 선수 레벨
        public UISprite spritePlayerPosition;   // 선수 포지션 스프라이트
        public UISprite spritePlayerEnhance;    // 선수 강화 스프라이트
        public UILabel labelOverRoll;

#if _Test_Local
        public void SetCard(CPlayer player)
        {
            this.labelPlayerLevel.gameObject.SetActive(true);
            //this.spriteBackground.spriteName = UI_Helper.ConvertCardSmallBackground(netCardInfo.DB_CardInfo.rank);
            //this.spriteNameBackground.spriteName = UI_Helper.ConvertCardSmallNameBackground(netCardInfo.DB_CardInfo.rank);
            //this.texturePlayer.mainTexture = Mgrs.DataLoad.LoadPlayerTexture((UserData.ETeamCode)netCardInfo.DB_CardInfo.team, string.Format("{0}_s", netCardInfo.DB_CardInfo.player_id));
            string convertName = player.getName();
            //this.spriteTeamLogo.spriteName = ((UserData.ETeamCode)netCardInfo.DB_CardInfo.team).ToString().ToLower();
            this.labelPlayerName.text = player.getName();
            //this.labelPlayerLevel.text = netCardInfo.level.ToString();

            //this.spritePlayerPosition.spriteName = UI_Helper.ConvertCardPositionSmallSprite(player.getPosition());
        }
#else

        //NetworkData_GameCardInfo 관련수정

        //지워지워
        public void SetCard(CPlayer player)
        {
            this.labelPlayerLevel.gameObject.SetActive(true);
            //this.spriteBackground.spriteName = UI_Helper.ConvertCardSmallBackground(netCardInfo.DB_CardInfo.rank);
            //this.spriteNameBackground.spriteName = UI_Helper.ConvertCardSmallNameBackground(netCardInfo.DB_CardInfo.rank);
            //this.texturePlayer.mainTexture = Mgrs.DataLoad.LoadPlayerTexture((UserData.ETeamCode)netCardInfo.DB_CardInfo.team, string.Format("{0}_s", netCardInfo.DB_CardInfo.player_id));
            string convertName = player.getName();
            //this.spriteTeamLogo.spriteName = ((UserData.ETeamCode)netCardInfo.DB_CardInfo.team).ToString().ToLower();
            this.labelPlayerName.text = player.getName();
            //this.labelPlayerLevel.text = netCardInfo.level.ToString();

            this.spritePlayerPosition.spriteName = UI_Helper.ConvertCardPositionSmallSprite(player.getPosition());
        }//이거 지워지워

        /*
        public void SetCard(UserData.NetworkData_GameCardInfo netCardInfo)
        {            
            this.labelPlayerLevel.gameObject.SetActive(true);
            this.spriteBackground.spriteName = UI_Helper.ConvertCardSmallBackground(netCardInfo.DB_CardInfo.rank);
            this.spriteNameBackground.spriteName = UI_Helper.ConvertCardSmallNameBackground(netCardInfo.DB_CardInfo.rank);
            // DISABLED_MGRS: this.texturePlayer.mainTexture = Mgrs.DataLoad.LoadPlayerTexture((UserData.ETeamCode)netCardInfo.DB_CardInfo.team, string.Format("{0}_s", netCardInfo.DB_CardInfo.player_id));
            string convertName = string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeSamll((UserData.ETeamCode)netCardInfo.DB_CardInfo.team));
            this.spriteTeamLogo.spriteName = ((UserData.ETeamCode)netCardInfo.DB_CardInfo.team).ToString().ToLower();
            this.labelPlayerName.text = netCardInfo.PlayerYearName;
            this.labelPlayerLevel.text = netCardInfo.level.ToString();

            this.spritePlayerPosition.spriteName = UI_Helper.ConvertCardPositionSmallSprite(netCardInfo.DB_CardInfo.position);

            // 타입에 따라서 나뉜다. 
            switch (netCardInfo.DB_CardInfo.PlayerType)
            {
                case WebConnector.PlayerType.Hitter:
                    {
                        int overValue = UI_HelperCalculator.Calc_HitterTotalAbility(netCardInfo.dic_NetworkCardStat);
                        if (this.labelOverRoll == null)
                            this.overRollValue.SetOverRollValue(overValue);
                        else
                        {
                            this.labelOverRoll.gradientBottom = UI_Helper.ConvertCardOverRollColorName(overValue);
                            this.labelOverRoll.text = (overValue).ToString();
                        }

                    }
                    break;
                case WebConnector.PlayerType.Pitcher:
                    {
                        int overValue = UI_HelperCalculator.Calc_PitcherTotalAbility(netCardInfo.dic_NetworkCardStat);
                        if (this.labelOverRoll == null)
                            this.overRollValue.SetOverRollValue(overValue);
                        else
                        {
                            this.labelOverRoll.gradientBottom = UI_Helper.ConvertCardOverRollColorName(overValue);
                            this.labelOverRoll.text = (UI_HelperCalculator.Calc_PitcherTotalAbility(netCardInfo.dic_NetworkCardStat)).ToString();
                        }
                    }
                    break;
            }

            if (netCardInfo.grade == 0)
                this.enhanceValue.gameObject.SetActive(false);
            else
            {
                this.enhanceValue.gameObject.SetActive(true);
                this.enhanceValue.SetEnhanceValue(DefineEnum.ECardSize.Small, netCardInfo.grade);
            }
            this.spritePlayerEnhance.spriteName = UI_Helper.ConvertEnhanceSmallBackground(netCardInfo.grade);
        }*/
#endif  
    }
}
