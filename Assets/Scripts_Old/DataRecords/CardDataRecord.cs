using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameDefine;

/// <summary>
/// 선수 데이터 레코드
/// </summary>
[Serializable]
public class CardBaseData
{

    public int Idx { get; set; } // 카드 인덱스
    public string NameId { get; set; } // 카드 이름
    public string DescId { get; set; } // 카드 설명
    public short Backnumber { get; set; } // 등번호
    public byte Rarity { get; set; } // 
    public byte Hand { get; set; } // 
    public short Stadium { get; set; } // 스타디움
    public byte StartLevel { get; set; } // 초기레벨
    public short CardType { get; set; } // 
    public short DefenseType { get; set; } // 
    public short BodyType { get; set; } // 
    public short BodyInType { get; set; } // 
    public string PortraitTag { get; set; } // 초상화 태그
    public string SpineTag { get; set; } // 스파인 태그
    public string SdTag { get; set; } // SD 태그
    public short RaceType { get; set; } // 
    public short DefenseTypeDefault { get; set; }
    public short GenderType { get; set; }


    public string GetPlayerNameID()
    {
        return NameId;
    }

    public string GetPlayerDescID()
    {
        return DescId;
    }

    public eCardType GetPlayerType()
    {
        eCardType type = eCardType.MAX;
        switch (CardType)
        {
            case 1:
                type = eCardType.HItter;
                break;
            case 2:
                type = eCardType.Pitcher;
                break;
        }
        return type;
    }

    public eCardRarity GetPlayerRarity()
    {
        eCardRarity enumrarity = eCardRarity.Common;
        switch (Rarity)
        {
            case 1:
                enumrarity = eCardRarity.Common;
                break;
            case 2:
                enumrarity = eCardRarity.Rare;
                break;
            case 3:
                enumrarity = eCardRarity.Epic;
                break;
            case 4:
                /*(if (GameConfig.Card_Rarity_Highest.Contains(Idx))
                {
                    enumrarity = eCardRarity.Highest;
                }
                else
                {
                    enumrarity = eCardRarity.Legendary;
                }*/
                enumrarity = eCardRarity.Legendary;
                break;
        }
        return enumrarity;
    }

    public DefenseTypeDefault GetPlayerDefenseTypeDefault()
    {
        DefenseTypeDefault enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.MAX;
        switch(DefenseTypeDefault)
        {
            case 1:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.C;
                break;
            case 2:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.IF;
                break;
            case 3:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.OF;
                break;
            case 4:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.IF_C;
                break;
            case 5:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.OF_C;
                break;
            case 6:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.IF_OF;
                break;
            case 7:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.ALL;
                break;
            case 8:
                enumDefenseTypeDefault = GameDefine.DefenseTypeDefault.P;
                break;
        }
        return enumDefenseTypeDefault;
    }

    public eHand GetPlayerHand()
    {
        eHand PlayerHand = eHand.Right;

        switch (Hand)
        {
            case 1:
                PlayerHand = eHand.Left;
                break;
            case 2:
                PlayerHand = eHand.Right;
                break;
        }
        return PlayerHand;
    }

    public string GetPlayerPortraitTag()
    {
        return PortraitTag;
    }

    public short GetPlayerBackNumber()
    {
        return Backnumber;
    }

}

[Serializable]
public class CardDataRecord : BaseDataRecord
{
    public CardBaseData[] cardBaseData;
}