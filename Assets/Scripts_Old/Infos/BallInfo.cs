using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInfo
{
    private BallBaseData ballBaseData;
    private BallAbilityBaseData AType_Ability;
    private BallAbilityBaseData BType_Ability;
    private BallAbilityBaseData CType_Ability;
    private BallAbilityLevelData AType_AbilityEffect;
    private BallAbilityLevelData BType_AbilityEffect;
    private BallAbilityLevelData CType_AbilityEffect;
    private int BallHaveCount;
    private bool isEquip;
    public BallInfo(int ballIndex)
    {
        /*ballBaseData = MainManager.Database.LoadBallBaseData(ballIndex);
        if (ballBaseData == null)
            return;
        if(ballBaseData.AbilityTypeA>0)
        {
            AType_Ability = MainManager.Database.LoadBallAbilityBaseData(ballBaseData.AbilityTypeA);
            AType_AbilityEffect = MainManager.Database.LoadBallAbilityLevelData(ballBaseData.AbilityTypeA, ballBaseData.AbilityTypeALevel);
        }
        if(ballBaseData.AbilityTypeB>0)
        {
            BType_Ability = MainManager.Database.LoadBallAbilityBaseData(ballBaseData.AbilityTypeB);
            BType_AbilityEffect = MainManager.Database.LoadBallAbilityLevelData(ballBaseData.AbilityTypeB, ballBaseData.AbilityTypeBLevel);
        }
        if(ballBaseData.AbilityTypeC>0)
        {
            CType_Ability = MainManager.Database.LoadBallAbilityBaseData(ballBaseData.AbilityTypeC);
            CType_AbilityEffect = MainManager.Database.LoadBallAbilityLevelData(ballBaseData.AbilityTypeC, ballBaseData.AbilityTypeCLevel);
        }*/
    }

    public BallInfo()
    {
        ballBaseData = new BallBaseData();
        ballBaseData.Idx = 0;
        ballBaseData.AbilityTypeA = 0;
        ballBaseData.AbilityTypeB = 0;
        ballBaseData.AbilityTypeC = 0;
    }

    public void SetHaveBallCount(int Quantity)
    {
        BallHaveCount = Quantity;
    }

    public void AddHaveBallCount(int Quantity)
    {
        BallHaveCount += Quantity;
    }

    public void SetIsEquipBall(bool isEquip)
    {
        this.isEquip = isEquip;
    }

    public int GetBallIndex()
    {
        if (ballBaseData == null)
            return 0;
        return ballBaseData.Idx;
    }

    public string GetBallNameID()
    {
        if (ballBaseData == null)
            return string.Empty;
        return ballBaseData.NameId;
    }

    public string GetBallDescID()
    {
        if (ballBaseData == null)
            return string.Empty;
        return ballBaseData.DescId;
    }

    public Sprite GetBallSprite()
    {
        if (ballBaseData == null)
            return null;

        Sprite BallIcon = null;// MainManager.UI.GetAtlas("Ball", ballBaseData.IconTag);

        return BallIcon;
    }

    public string GetBallIconTag()
    {
        if (ballBaseData == null)
            return string.Empty;
        return ballBaseData.IconTag;
    }
    public string GetBallTextureTag()
    {
        if (ballBaseData == null)
            return string.Empty;
        return ballBaseData.TextureTag;
    }

    public bool GetIsEquip()
    {
        return isEquip;
    }

    public bool isDisplay()
    {
        if (ballBaseData == null)
            return false;

        return System.Convert.ToBoolean(ballBaseData.Display);
    }

    public BallAbilityBaseData GetATypeAbilityData()
    {
        if (AType_Ability == null || ballBaseData.AbilityTypeA <= 0)
            return null;
        return AType_Ability;
    }
    public BallAbilityLevelData GetATypeAbilityEffectData()
    {
        if (AType_AbilityEffect == null || ballBaseData.AbilityTypeA <= 0)
            return null;
        return AType_AbilityEffect;
    }

    public short GetATypeAbilityLevel()
    {
        if (ballBaseData == null || ballBaseData.AbilityTypeA <= 0)
            return 0;

        return ballBaseData.AbilityTypeALevel;
    }

    public BallAbilityBaseData GetBTypeAbilityData()
    {
        if (BType_Ability == null || ballBaseData.AbilityTypeB <= 0)
            return null;
        return BType_Ability;
    }
    public BallAbilityLevelData GetBTypeAbilityEffectData()
    {
        if (BType_AbilityEffect == null || ballBaseData.AbilityTypeA <= 0)
            return null;
        return BType_AbilityEffect;
    }

    public short GetBTypeAbilityLevel()
    {
        if (ballBaseData == null || ballBaseData.AbilityTypeB <= 0)
            return 0;

        return ballBaseData.AbilityTypeBLevel;
    }

    public BallAbilityBaseData GetCTypeAbilityData()
    {
        if (CType_Ability == null || ballBaseData.AbilityTypeC <= 0)
            return null;
        return CType_Ability;
    }
    public BallAbilityLevelData GetCTypeAbilityEffectData()
    {
        if (CType_AbilityEffect == null || ballBaseData.AbilityTypeA <= 0)
            return null;
        return CType_AbilityEffect;
    }

    public short GetCTypeAbilityLevel()
    {
        if (ballBaseData == null || ballBaseData.AbilityTypeC <= 0)
            return 0;

        return ballBaseData.AbilityTypeCLevel;
    }

    public int GetHaveBallCount()
    {
        return BallHaveCount;
    }


    public string GetIngameColor()
    {
        return ballBaseData.IngameColor;
    }
}
