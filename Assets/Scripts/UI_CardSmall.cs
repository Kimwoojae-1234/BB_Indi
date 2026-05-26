using UnityEngine;
using System.Collections;

public class UI_CardSmall : UI_CardBase
{
    
    public void SetCardInfo(CardData cardInfo)
    {
        if (cardInfo == null)
            return;
        this.cardData = cardInfo;
        base.SetPlayerCard(this.cardData);
    }

    public override void SetPlayerCard(CardData card_data)
    {
        base.SetPlayerCard(card_data);
    }

    public override void SetPlayerCard(card cardInfo)
    {
        base.SetPlayerCard(cardInfo);
    }

    public void SetInitTeamCard(card cardInfo, int reinforce_lev)
    {
        this.SetPlayerCard(cardInfo);
        this.SetReinforce(cardInfo, reinforce_lev);
    }
}
