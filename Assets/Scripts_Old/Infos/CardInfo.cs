using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDefine;
using System.Linq;
public class DeckInfo
{
    public long DeckItemID;
    public int DeckPreset;
    public int BattingOrder;
    public int PitcheroffCount;
    public ePlayerPosition PlayerPosition;
    public eStartingPitcherOrder PitcherOrder;
}

public class CardInfo
{
    private long CardItemID;
    private int Level;
    private int Exp;
    private bool isNew;
    private int GameOffCount;
    private DeckInfo[] deckInfo;
    private CardBaseData playerData;

    public CardInfo(CardItemDto cardItemDto)
    {
        CardItemID = cardItemDto.cardItemId;
        Level = cardItemDto.level;
        Exp = cardItemDto.exp;
        GameOffCount = 0;
        /*playerData = MainManager.Database.LoadPlayerData(cardItemDto.CardDataIndex);
        deckInfo = new DeckInfo[GameConfig.MAXDECKCOUNT];
        for (int i = 0; i < deckInfo.Length; i++)
        {
            deckInfo[i] = new DeckInfo();
        }*/
    }

    public CardInfo(long Card_ID,int Card_Index, int Card_Level, int Card_EXP)
    {
        CardItemID = Card_ID;
        Level = Card_Level;
        Exp = Card_EXP;
        GameOffCount = 0;
        /*playerData = MainManager.Database.LoadPlayerData(Card_Index);
        deckInfo = new DeckInfo[GameConfig.MAXDECKCOUNT];
        for(int i = 0; i<deckInfo.Length; i++)
        {
            deckInfo[i] = new DeckInfo();
        }*/
    }

    public void SetCardDeckID(int deckPreset, long deckID, int battingOrder, ePlayerPosition position, eStartingPitcherOrder pitcherOrder)
    {
        deckInfo[deckPreset - 1].DeckItemID = deckID;
        deckInfo[deckPreset - 1].DeckPreset = deckPreset;
        deckInfo[deckPreset - 1].BattingOrder = battingOrder;
        deckInfo[deckPreset - 1].PlayerPosition = position;
        deckInfo[deckPreset - 1].PitcherOrder = pitcherOrder;
        
    }

    public void SetCardLevel(int level)
    {
        Level = level;
    }

    public void SetCardExp(int exp)
    {
        Exp = exp;
    }

    public void AddCardExp(int exp)
    {
        Exp += exp;
    }

    public void SetNewCard(bool isNew)
    {
        this.isNew = isNew;
    }

    public long GetCardItemID()
    {
        return CardItemID;
    }

    public long GetDeckItemID(int deckPreset)
    {
        return deckInfo[deckPreset - 1].DeckItemID;
    }

    public int GetCardLevel()
    {
        return Level;
    }

    public int GetCardExp()
    {
        return Exp;
    }

    public bool GetIsNewCard()
    {
        return isNew;
    }

    public CardBaseData GetPlayerData()
    {
        return playerData;
    }

    public int GetGameOffCount()
    {
        return GameOffCount;
    }

    public bool GetUpgradeable()
    {
        /*CardLevelBalanceData Balancedata = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (Balancedata == null)
            return false;
        return (Balancedata.LvupExp <= Exp && Level < GameConfig.PLAYERMAXLEVEL);*/
        return false;
    }

    public ePlayerPosition GetPlayerPosition(int deckPreset)
    {
        int DeckNo = deckPreset - 1;
        if(playerData.GetPlayerType() == eCardType.HItter)
        {
            if (deckInfo == null || deckInfo[DeckNo] == null)
                return ePlayerPosition.MAX;

            return deckInfo[DeckNo].PlayerPosition;
        }
        else
        {
            return ePlayerPosition.SP;
        }
        
    }

    public int GetDeckPreset(int deckPreset)
    {
        if (deckInfo == null || deckInfo[deckPreset - 1] == null)
            return 0;
        return deckInfo[deckPreset - 1].DeckPreset;
    }

    public int GetBattingOrder(int deckPreset)
    {
        if (deckInfo == null || deckInfo[deckPreset - 1] == null)
            return 0;
        return deckInfo[deckPreset - 1].BattingOrder;
    }

    public eStartingPitcherOrder GetPitcherOrder(int deckPreset)
    {
        if (deckInfo == null || deckInfo[deckPreset - 1] == null)
            return eStartingPitcherOrder.MAX;
        return deckInfo[deckPreset - 1].PitcherOrder;
    }

    public int GetPitcherOffGameCount(int deckPreset)
    {
        if(deckInfo == null || deckInfo[deckPreset - 1] == null)
            return 99;
        return deckInfo[deckPreset - 1].PitcheroffCount;
    }

    public int GetCardOpenStadium()
    {
        if (playerData == null)
            return 0;
        return playerData.Stadium;
    }

    public short GetPlayerBacknumber()
    {
        if (playerData == null)
            return 0;
        return playerData.GetPlayerBackNumber();
    }

    public float GetHitterOverall()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        float Overall = ((balanceData.AbilPower * 23) + (balanceData.AbilContact * 18)
            + (balanceData.AbilVision * 15) + (balanceData.AbilFielding * 18)
            + (balanceData.AbilSpeed * 14) + (balanceData.AbilThrowing * 12)) * 0.01f;
        return Overall;*/
        return 100;
    }

    public float GetPlayerOffenseRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        float offenseRating = ((balanceData.AbilContact * 30) + (balanceData.AbilPower * 40) + (balanceData.AbilVision * 30)) * 0.01f;
        //Debug.Log(offenseRating);
        return offenseRating;*/
        return 100;
    }

    public float GetPlayerDefenseRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        float defenseRating = ((balanceData.AbilFielding * 50) + (balanceData.AbilThrowing * 40) + balanceData.AbilSpeed*10) * 0.01f;
        return defenseRating;*/
        return 100;
    }

    public short GetPayerContactRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilContact;*/
        return 100;
    }

    public short GetPlayerPowerRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilPower;*/
        return 100;
    }

    public short GetPlayerVisionRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilVision;*/
        return 100;
    }

    public short GetPlayerFieldingRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilFielding;*/
        return 100;
    }

    public short GetPlayerThrowingRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilThrowing;*/
        return 100;
    }

    public int GetPlayerRunningRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilSpeed;*/
        return 100;
    }

    public float GetPitcherOverall()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        int controll_value = balanceData.AbilControl * 25;
        float ballAvg = 0;
        List<short> PitcherAbilityList = new List<short>();
        PitcherAbilityList.Add(balanceData.AbilFastball);
        PitcherAbilityList.Add(balanceData.AbilCurve);
        PitcherAbilityList.Add(balanceData.AbilSlider);
        PitcherAbilityList.Add(balanceData.AbilSinker);
        PitcherAbilityList.Add(balanceData.AbilChangeup);
        IOrderedEnumerable<short> LinqSort = null;
        LinqSort = from n in PitcherAbilityList orderby n descending select n;
        PitcherAbilityList = LinqSort.ToList<short>();

        for (int n = 0; n < 3; n++)
        {
            ballAvg += PitcherAbilityList[n];
        }
        ballAvg = (ballAvg / 3) * 75;
        return (controll_value + ballAvg) * 0.01f;*/
        return 1.0f;
    }

    public float GetPitcherFastBallRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilFastball;*/
        return 1.0f;
    }

    public float GetPitcherControlRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilControl;*/
        return 1.0f;
    }

    public float GetPitcherCurveRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilCurve;*/
        return 1.0f;
    }

    public float GetPitcherSliderRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilSlider;*/
        return 1.0f;
    }

    public float GetPitcherSinkerRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilSinker;*/
        return 1.0f;
    }

    public float GetPitcherChangeupRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        return balanceData.AbilChangeup;*/
        return 1.0f;
    }

    public float GetPitcherBreakingBallRating()
    {
        /*CardLevelBalanceData balanceData = MainManager.Database.LoadCardLevelBalance(playerData.Idx, Level);
        if (balanceData == null)
            return 0;
        float ballAvg = 0;
        List<short> PitcherAbilityList = new List<short>();
        PitcherAbilityList.Add(balanceData.AbilCurve);
        PitcherAbilityList.Add(balanceData.AbilSlider);
        PitcherAbilityList.Add(balanceData.AbilSinker);
        PitcherAbilityList.Add(balanceData.AbilChangeup);
        IOrderedEnumerable<short> LinqSort = null;
        LinqSort = from n in PitcherAbilityList orderby n descending select n;
        PitcherAbilityList = LinqSort.ToList<short>();
        for (int i = 0; i<2; i++)
        {
            ballAvg += PitcherAbilityList[i];
        }
        return ballAvg / 2;*/
        return 1.0f;

    }

    public void SetPlayerDeckData(long deckItemID, int PositionCode)
    {
        int DeckNo = Mathf.FloorToInt(PositionCode * 0.001f) - 1;
        deckInfo[DeckNo].DeckItemID = deckItemID;
        deckInfo[DeckNo].DeckPreset = Mathf.FloorToInt(PositionCode * 0.001f);
        switch (playerData.GetPlayerType())
        {
            case eCardType.HItter:
                deckInfo[DeckNo].BattingOrder = Mathf.FloorToInt((PositionCode - (deckInfo[DeckNo].DeckPreset * 1000) - 100) * 0.1f);
                SetPlayerPosition(DeckNo, PositionCode - (deckInfo[DeckNo].DeckPreset * 1000) - 100 - (10 * deckInfo[DeckNo].BattingOrder));
                break;
            case eCardType.Pitcher:
                deckInfo[DeckNo].BattingOrder = Mathf.FloorToInt((PositionCode - (deckInfo[DeckNo].DeckPreset * 1000) - 200) * 0.1f);
                SetPlayerPosition(DeckNo, PositionCode - (deckInfo[DeckNo].DeckPreset * 1000) - 200 - (10 * deckInfo[DeckNo].BattingOrder));
                break;
            case eCardType.Gear:
                deckInfo[DeckNo].BattingOrder = 0;
                break;
        }
    }

    private void SetPlayerPosition(int deckPreset, int position)
    {
        switch(playerData.GetPlayerType())
        {
            case eCardType.HItter:
                deckInfo[deckPreset].PitcherOrder = eStartingPitcherOrder.MAX;
                deckInfo[deckPreset].PlayerPosition = GetDefencePosition(position);
                break;
            case eCardType.Pitcher:
                deckInfo[deckPreset].PlayerPosition = ePlayerPosition.SP;
                deckInfo[deckPreset].PitcherOrder = GetStartingPitcherOrder(position);
                break;
        }
    }

    public void SetPitcherOffCount(int gameOffCount)
    {
        this.GameOffCount = gameOffCount;
    }

    private ePlayerPosition GetDefencePosition(int position)
    {
        ePlayerPosition defence_position = ePlayerPosition.MAX;
        switch (position)
        {
            case 2:
                defence_position = ePlayerPosition.C;
                break;
            case 3:
                defence_position = ePlayerPosition.B1;
                break;
            case 4:
                defence_position = ePlayerPosition.B2;
                break;
            case 5:
                defence_position = ePlayerPosition.B3;
                break;
            case 6:
                defence_position = ePlayerPosition.Ss;
                break;
            case 7:
                defence_position = ePlayerPosition.Lf;
                break;
            case 8:
                defence_position = ePlayerPosition.Cf;
                break;
            case 9:
                defence_position = ePlayerPosition.Rf;
                break;
        }
        return defence_position;
    }

    private eStartingPitcherOrder GetStartingPitcherOrder(int position)
    {
        eStartingPitcherOrder starting_order = eStartingPitcherOrder.MAX;
        switch(position)
        {
            case 1:
                starting_order = eStartingPitcherOrder.SP1;
                break;
            case 2:
                starting_order = eStartingPitcherOrder.SP2;
                break;
            case 3:
                starting_order = eStartingPitcherOrder.SP3;
                break;
            case 4:
                starting_order = eStartingPitcherOrder.SP4;
                break;
        }
        return starting_order;
    }

    public GameDefine.DefenseTypeDefault GetDefenseTypeDefault()
    {
        if (playerData == null)
            return DefenseTypeDefault.MAX;
        return playerData.GetPlayerDefenseTypeDefault();
    }

    public void SetPlayerData(CardBaseData player_Data)
    {
        playerData = player_Data;
    }

    public int DefenseType
    {
        get
        {
            return playerData.DefenseType;
        }
    }
    public int CompareByStat(CardInfo lhs, CardInfo rhs)
    {
        /*CardLevelBalanceData lbd = MainManager.Database.LoadCardLevelBalance(lhs.GetPlayerData().Idx, lhs.Level);
        CardLevelBalanceData rbd = MainManager.Database.LoadCardLevelBalance(rhs.GetPlayerData().Idx, rhs.Level);
        if ((lbd != null) && (rbd != null))
        {
            if (lbd.AbilPower == rbd.AbilPower)
            {
                if (lbd.AbilFielding == rbd.AbilFielding)
                {
                    if (lbd.AbilContact == rbd.AbilContact)
                    {
                        if (lbd.AbilVision == rbd.AbilVision)
                        {
                            if (lbd.AbilSpeed == rbd.AbilSpeed)
                            {
                                if (lbd.AbilThrowing == rbd.AbilThrowing)
                                {
                                    return (lbd.Idx < rbd.Idx) ? -1 : 1;
                                }
                                return (lbd.AbilThrowing < rbd.AbilThrowing) ? -1 : 1;
                            }
                            return (lbd.AbilSpeed < rbd.AbilSpeed) ? -1 : 1;
                        }
                        return (lbd.AbilVision < rbd.AbilVision) ? -1 : 1;
                    }
                    return (lbd.AbilContact < rbd.AbilContact) ? -1 : 1;
                }
                return (lbd.AbilFielding < rbd.AbilFielding) ? -1 : 1;
            }
            return (lbd.AbilPower < rbd.AbilPower) ? -1 : 1;
        }*/
        return 0;
    }
    public int CompareByOverall(CardInfo rhs)
    {
        if (this.GetHitterOverall() == rhs.GetHitterOverall())
        {
            //if (this.GetPlayerOffenseRating() == rhs.GetPlayerOffenseRating())
            {
                //if (this.GetPlayerDefenseRating() == rhs.GetPlayerDefenseRating())
                {
                    return CompareByStat(this, rhs);
                }
                //return (this.GetPlayerDefenseRating() < rhs.GetPlayerDefenseRating()) ? -1 : 1;
            }
            //return (this.GetPlayerOffenseRating() < rhs.GetPlayerOffenseRating()) ? -1 : 1;
        }
        return (this.GetHitterOverall() < rhs.GetHitterOverall()) ? -1 : 1;

    }
    public int CompareByHitting(CardInfo rhs)
    {
        if (this.GetPlayerOffenseRating() == rhs.GetPlayerOffenseRating())
        {
            if (this.GetHitterOverall() == rhs.GetHitterOverall())
            {
                return (this.GetPlayerData().Idx > rhs.GetPlayerData().Idx) ? -1 : 1;
            }
            return (this.GetHitterOverall() > rhs.GetHitterOverall()) ? -1 : 1;
        }
        return (this.GetPlayerOffenseRating() > rhs.GetPlayerOffenseRating()) ? -1 : 1;
    }
    public int CompareByFielding(CardInfo rhs)
    {
        if (this.GetPlayerDefenseRating() == rhs.GetPlayerDefenseRating())
        {
            if (this.GetHitterOverall() == rhs.GetHitterOverall())
            {
                return (this.GetPlayerData().Idx > rhs.GetPlayerData().Idx) ? -1 : 1;
            }
            return (this.GetHitterOverall() > rhs.GetHitterOverall()) ? -1 : 1;
        }
        return (this.GetPlayerDefenseRating() > rhs.GetPlayerDefenseRating()) ? -1 : 1;
    }
    public static int CompareByOverall(CardInfo lhs, CardInfo rhs)
    {
        return lhs.CompareByOverall(rhs);
    }
    public static int CompareByFielding(CardInfo lhs, CardInfo rhs)
    {
        return lhs.CompareByFielding(rhs);
    }
}
