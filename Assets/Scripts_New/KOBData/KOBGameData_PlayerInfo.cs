using System;
using System.Collections.Generic;
/// <summary>
/// 플레이어 정보 관련
/// </summary>

public partial class KOBGameData : BackendData.Base.GameData
{ 
    public KOBBaller GetSelectedBaller()
    {
        int select = ManageInfo.SelectBaller;
        return PlayerInfo.BallerList[select];
    }

    public KOBBaller GetBaller(int idx)
    {
        if (PlayerInfo.BallerList.ContainsKey(idx))
        {
            return PlayerInfo.BallerList[idx];
        }
        else
        {
            return null;    
        }
    }

    public KOBBaller GetPitcher(int idx)
    {
        if (PlayerInfo.PitcherList.ContainsKey(idx))
        {
            return PlayerInfo.PitcherList[idx];
        }
        else
        {
            return null;
        }
    }


    public bool AddBaller(KOBRewardInfo info)// int idx, int amount, KOBReward reward)
    {
        int playerIdx = info.pindex;
        int amount = info.amount;

        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(playerIdx);
        if (data == null)
        {
            //해당 데이터의 선수가 존재하지 않음
            UnityEngine.Debug.LogError("No Exist Player idx : " + playerIdx);
            return false;
        }


        bool isUnlock = false;
        if (PlayerInfo.BallerList.ContainsKey(playerIdx) == false)
        {
            KOBBaller newBaller = new KOBBaller();//
            newBaller.InitBaller(playerIdx, amount);
            PlayerInfo.BallerList.Add(playerIdx, newBaller);
            GrowthInfo.SetBallerAdditionalInfo(playerIdx);
            isGrowthChange = true;
            isUnlock = true;
        }
        else
        {
            PlayerInfo.BallerList[playerIdx].AddCard(amount);
        }
        isPlayerChange = true;
        IsChangedData = true;
        return isUnlock;
    }


    public bool AddPitcher(int idx, int amount)
    {
        bool isUnlock = false;

        if (PlayerInfo.PitcherList.ContainsKey(idx) == false)
        {
            KOBBaller newBaller = new KOBBaller();//
            newBaller.InitBaller(idx, amount);
            PlayerInfo.PitcherList.Add(idx, newBaller);
            isUnlock = true;
        }
        else
        {
            PlayerInfo.PitcherList[idx].AddCard(amount);
        }
        isPlayerChange = true;
        IsChangedData = true;
        return isUnlock;
    }


    public int UpgradeCard(int idx)
    {
        int errorCode = (int)KOBErrorCode.None;
        if (PlayerInfo.BallerList.ContainsKey(idx))
        {
            KOBBaller baller = PlayerInfo.BallerList[idx];
            CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
            UpgradeChart UpgradeData = KOBManager.Backend.Chart.UpgradeData;            //업글 정보

            int needGold = UpgradeData.UpgradeGold(baller.level + 1, data.rarity);
            int needCard = UpgradeData.UpgradeCard(baller.level + 1, data.rarity);

            UnityEngine.Debug.Log("needGold : " + needGold + "  //  needCard : " + needCard);

            if (baller.level < KOBConstant.MAX_LEVEL) //레벨체크
            {
                if (KOBManager.Backend.GameData.KOBGameData.CurrencyInfo.Gold >= needGold) //골드체크
                {
                    if (baller.card_number >= needCard) //카드체크
                    {
                        isCurrencyChange = CurrencyInfo.UpdateCurrency(-needGold, 0, 0, 0);
                        isPlayerChange = PlayerInfo.UpgradeCard(idx, needCard);
                        IsChangedData = true;
                    }
                    else
                    {
                        //업데이트를 하기위한 카드 부족
                        errorCode = (int)KOBErrorCode.Not_Enough_Card;
                    }
                }
                else
                {
                    //업데이트를 하기위한 골드 부족
                    errorCode = (int)KOBErrorCode.Not_Enough_Gold;
                }
            }
            else
            {
                //볼러는 맥스레벨에 도달함
                errorCode = (int)KOBErrorCode.Baller_Max_Level;
            }
        }
        else
        {
            //해당 볼러 없음
            errorCode = (int)KOBErrorCode.Not_Have_Baller;
        }

        return errorCode;
    }



    public int SetBaller(int idx)
    {
        int errorCode = 0;
        if (PlayerInfo.BallerList.ContainsKey(idx))
        {
            ManageInfo.SetBaller(idx);
            isManagerChange = true;
            IsChangedData = true;
        }
        else
        {
            errorCode = (int)KOBErrorCode.Not_Have_Baller;
        }
        return errorCode;
    }



    public int ChangeDeck(TRequestChangeDeck req)
    {
        int errorCode = 0;
        bool possible = DeckInfo.ChangeDeck(req.NewDeck);
        if(possible == true)
        {
            if(req.SelectIdx != -1)
            {
                errorCode = SetBaller(req.SelectIdx);
                if (errorCode != 0) return errorCode;
            }
            isDeckChange = true;
            IsChangedData = true;
        }
        else
        {
            errorCode = (int)KOBErrorCode.Deck_Change_Error;
        }
        return errorCode;
    }



    public int BallerFameUpgrade(int idx)
    {
        //int errorCode = PlayerInfo.UpgradeFame(idx);
        int errorCode = 0;
        if (PlayerInfo.BallerList.ContainsKey(idx))
        {
            isPlayerChange = PlayerInfo.BallerList[idx].UpgradeFame();
            if(isPlayerChange == true)
            {
                IsChangedData = true;
            }
            else
            {
                errorCode = (int)KOBErrorCode.Incorrect_Reputation_Setting_Upgrade;
            }
        }
        else
        {
            errorCode = (int)KOBErrorCode.Not_Have_Baller;
        }
        return errorCode;
    }

}
