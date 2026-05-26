using System.Collections;
using System.Collections.Generic;
using tk2dRuntime.TileMap;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AtlasManager : MonoBehaviour
{
    public enum MyAtlas
    {
        UICompo = 0,
        UIIcon,
        UISkill,
        UITier,
        UIRanks,
        RandomBox,         
    }


    public SpriteAtlas[] Atlas = null;



    public SpriteAtlas GetAtlas(MyAtlas atlasName)
    {
        return Atlas[(int)atlasName];
    }

    public Sprite GetSprite(MyAtlas atlasName, string spriteName)
    {
        SpriteAtlas atlas = GetAtlas(atlasName);
        if (atlas != null)
        {
            Sprite sprite = atlas.GetSprite(spriteName);
            return sprite;
        }
        return null;
    }





    /// <summary>
    /// RTTS순위 UI의 배경 스프라이트를 idx와 rank에 알맞게 세팅
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="rank"></param>
    /// <returns></returns>
    public Sprite GetStandingRankBgSprite(int rank, bool isMyTeam)
    {
        if (isMyTeam == true)
        {
            //내팀인 경우
            return GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame09_n");
        }
        else
        {
            if (rank % 2 == 1)
            {
                //홀수열
                return GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame05");
            }
            else
            {
                //짝수열
                return GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame04");
            }
        }
    }

    public Sprite GetLeaderBgSprite(bool isSelectBaller)
    {
        if (isSelectBaller == true)
        {
            //내팀인 경우
            return GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame06_00_s");
        }
        else
        {
            return GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_ListFrame06_00_n");
        }
    }


    public void SetTierSprite(Image img, int tier)
    {
        img.gameObject.SetActive(true); 
        img.sprite = GetSprite(AtlasManager.MyAtlas.UITier, string.Format("TrophyTier{0}", tier));
        img.SetNativeSize();
    }

    public void SetBallerTierSprite(Image img, int tier)
    {
        img.gameObject.SetActive(true);
        img.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UITier, string.Format("ballertier{0}", tier));
        img.SetNativeSize();
    }


    public void SetRewardSprite(Image img, KOBReward reward)
    {
        img.gameObject.SetActive(true);
        img.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UIIcon, GetRewardSpriteName(reward));
        img.SetNativeSize();
    }



    private string GetRewardSpriteName(KOBReward reward)
    {
        if (reward == KOBReward.Gold) return "Icon_ShopIcon_Gold1";
        else if (reward == KOBReward.Gem || reward == KOBReward.Gem_Free) return "Icon_ShopIcon_Gem0";
        else return "Icon_ChestIcon_Blue02_l"; //우선 박스로 퉁쳐
    }


    public Sprite GetRewarBox(KOBReward rarity)
    {
        switch (rarity)
        {
            case KOBReward.Box_Black:
                return GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox5");
            case KOBReward.Box_Legend:
                return GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox4");
            case KOBReward.Box_Epic:
                return GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox3");
            case KOBReward.Box_Rare:
                return GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox2");
            default:
                return GetSprite(AtlasManager.MyAtlas.RandomBox, "itembox1");
        }
    }



    public Sprite GetRewardIcon(KOBReward reward)
    {
        switch (reward)
        {
            case KOBReward.Gold:
                return GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold0");
            case KOBReward.Gem:
            case KOBReward.Gem_Free:
                return GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gem0");
            case KOBReward.Energy:
                return GetSprite(AtlasManager.MyAtlas.UIIcon, "Icon_ShopIcon_Gold0");
        }
        return null;
    }

}
