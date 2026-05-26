using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSkillData
{

    public BattleSkillData()
    {
        
    }

    public short skillLevel;
    public int skillBaseDataId;
    public int activateChance;
    public int successChance;
    public Dictionary<short, int> skillEffects;
}

public class BattleUserDto
{
    public BattleUserDto()
    {
        
    }

    public long userId;
    public string nickname;
    public string photoUrl;
    public int PortraitType;
    public int UniformType;
    public int TeamColorAType;
    public int TeamColorBType;
    public int TeamLogoType;
    public string NationalFlag;
    public int trophy;
    //public ObscuredBool isHomeTeam;
    public BattleClanDto clanBadge;
    public BattleDeckDto battleDeck;
    public BattleBatItemDto[] batItems;
    public BattleBalItemDto[] ballItems;
    public BattleDeckScore BattleDeckScore;    
    public UserEmojiSlotDto EmojiSlot;
    public int startingBattingOrder;
    public byte PitcherRotation;
    public int finalOpenStadium;
    public int tier;
    public int honorTrophy;
    public int honor;
}

public class BattleClanDto
{
    public BattleClanDto()
    {
        
    }
    public string clanName;
    public int emblemIconType;
    public int emblemColorType;
}

public class BattleDeckDto
{
    public BattleDeckDto()
    {
        
    }
    public BattleCardItemDto[] cards;
    
    public int DeckPower;
}


public class BattleCardItemDto
{
    public BattleCardItemDto()
    {

    }
    public int position;
    public int battingOrder;
    public int cardDataIndex;
    public int level;    
    public int abilContact;
    public int abilPower;
    public int abilVision;
    public int abilFielding;
    public int abilThrowing;
    public int abilSpeed;
    public int abilControl;
    public int abilFastball;
    public int abilCurve;
    public int abilSlider;
    public int abilSinker;
    public int abilChangeup;
    public int abilPickoff;    
    public List<BattleSkillData> battleSkill;


#if BALANCE_TEST
    public void SetTest(BALANCE_TEST test)
    {
        abilContact = test.Contact;
        abilPower = test.Power;
        abilVision = test.Vision;
        abilFielding = test.Fielding;
        abilThrowing = test.Throwing;
        abilSpeed = test.Speed;
        abilControl = test.Control;
        abilFastball = test.Guwee;
        abilCurve = test.Guwee;
        abilSlider = test.Guwee;
        abilSinker = test.Guwee;
        abilChangeup = test.Guwee;
    }

#endif
}

public class BattleBatItemDto
{
    public BattleBatItemDto()
    {
        
    }
    public long conumableItemId;
    public int batBaseDataId;
    public int quantity;
    public int abilityTypeA;
    public int abilityTypeALevel;
    public int abilityTypeB;
    public int abilityTypeBLevel;
    public int abilityTypeC;
    public int abilityTypeCLevel;
}
public class BattleBalItemDto
{
    public BattleBalItemDto()
    {
        
    }
    public long ballItemId;
    public int baseBaseDataId;
    public int quantity;
    public int abilityTypeA;
    public BattleBallAbility abilityTypeALevel;
    public int abilityTypeB;
    public BattleBallAbility abilityTypeBLevel;
    public int abilityTypeC;
    public BattleBallAbility abilityTypeCLevel;
}

public class BattleBallAbility
{
    public BattleBallAbility()
    {
        
    }
    public int level;
    public long effect1Id;
    public int effect1Value;
    public long effect2Id;
    public int effect2Value;
    public long effect3Id;
    public int effect3Value;
}


public class BattleDeckScore
{
    public BattleDeckScore()
    {
        
    }
    public int BattingScore;
    public int DefenceScore;
    public int RunningScore;
    public int FastBallScore;
    public int BreakingBallScore;
    public int ControlBallScore;
}

public class UserEmojiSlotDto
{
    public int slot1_emoji_item;
    public int slot2_emoji_item;
    public int slot3_emoji_item;
    public int slot4_emoji_item;
    public int slot5_emoji_item;
    public int slot6_emoji_item;
}





public class PlayAspectSettings
{
    public int Idx;
    public int AspectIdx;
    public int KeywordValue;
    public int KeywordValue2;
    public bool WinSupport;
    public bool DramaticGoodBye;
    public string playerList;

    public List<int> PlayerList
    {
        get
        {
            List<int> list = new List<int>();
            if (!string.IsNullOrEmpty(playerList) && playerList.Length > 1)
            {
                string[] array = playerList.Split(new char[] { ',', '{', '}' });
                if (array != null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        int value;
                        if (int.TryParse(array[i], out value))
                        {
                            list.Add(value);
                        }
                    }
                }
            }
            return list;
        }
    }
}


public class PvEBotHabbit
{
    public int Idx;
    public int BotLevel;
    public int BotPitch;
    public int BotPitchSpeed;
    public int BallThrow;
    public int BotBattingType;
    public int BotBattingAproch;
    public int BotRunner;
    public int BotOneMoreBase;
    public int BotBunt;
    public int BotBatUse;
    public int BotPickoff;
    public int BotGiveUp;
    public int BotEmoji;
    public int EmojiSlotRefid1;
    public int EmojiSlotRefid2;
    public int EmojiSlotRefid3;
    public int EmojiSlotRefid4;
    public int EmojiSlotRefid5;
    public int EmojiSlotRefid6;
}


public class UserEquipmentDto
{
    public int ballItemId;
    public int batItemId;
}


public class DeckItemDto
{
    public long deckItemId;
    public int positionCode;
    public long cardItemId;
}


public class CardItemDto
{
    public long cardItemId;
    public int CardDataIndex;
    public int level;
    public int exp;
    public int updateAtTimestamp;
}