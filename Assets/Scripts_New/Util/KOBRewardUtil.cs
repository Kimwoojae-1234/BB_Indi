using System.Collections.Generic;
using System.Linq;

public static class KOBRewardUtil
{
    public static int GetRandomPindex(KOBRewardInfo info)//int random, KOBRarity rarity = KOBRarity.COMMON)
    {
        int pIndex = info.pindex;

        if (pIndex < 0)
        {
            KOBRarity rarity = KOBRewardUtil.GetCardRarity(info.reward);
            if (pIndex == KOBConstant.RANDOM_YOUHAVE) //가지고 있는 것중에 랜덤
            {
                return GetRandomYouHave(rarity);
            }
            else if (pIndex == KOBConstant.RANDOM_NOTHAVE) //없는 것중에 랜덤
            {
                return GetRandomNoHave(rarity);
            }
            else if (pIndex == KOBConstant.RANDOM_NOCONDITION) //소유 여부 관계없이 랜덤
            {
                return GetRandomNocondition(rarity);
            }
            else if (pIndex == KOBConstant.BALLPER_YOUPLAY) //내선택중인
            {
                return GetBallerYouPlay();
            }
            else
            {
                return KOBConstant.FIRSTBALLER; //
            }
        }
        else
        {
            return pIndex;
        }
    }

    public static int GetRandomYouHave(KOBRarity rarity)
    {
        List<int> list = KOBManager.Backend.Chart.CharacterData.GetBallersByRarityList(rarity);
        Dictionary<int, KOBBaller> PlayerList = KOBManager.MyInfo.GameData.PlayerInfo.BallerList;
        List<int> youHave = list.Where(idx => PlayerList.ContainsKey(idx)).ToList();

        if (youHave.Count > 0)
        { 
            //내가 가진게 있는 경우
            int random = UnityEngine.Random.Range(0, youHave.Count);
            return youHave[random];
        }
        else
        {
            //내가 가진게 없는 경우
            if(list.Count > 0)
            {
                //리스트에 제일 처음 것
                return list[0];
            }
            else
            {
                return KOBConstant.FIRSTBALLER;
            }
        }
    }

    public static int GetRandomNoHave(KOBRarity rarity)
    {
        List<int> list = KOBManager.Backend.Chart.CharacterData.GetBallersByRarityList(rarity);
        Dictionary<int, KOBBaller> PlayerList = KOBManager.MyInfo.GameData.PlayerInfo.BallerList;
        List<int> youDontHave = list.Where(idx => !PlayerList.ContainsKey(idx)).ToList();

        if (youDontHave.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, youDontHave.Count);
            return youDontHave[random];
        }
        else
        {
            //다 소유한 경우
            //리스트 중에 랜덤
            int random = UnityEngine.Random.Range(0, list.Count);
            return list[0];
        }
    }

    public static int GetRandomNocondition(KOBRarity rarity)
    {
        List<int> list = KOBManager.Backend.Chart.CharacterData.GetBallersByRarityList(rarity);
        if (list.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, list.Count);
            return list[random];
        }
        else
        {
            return KOBConstant.FIRSTBALLER;
        }
    }

    public static int GetBallerYouPlay()
    {
        return KOBManager.MyInfo.UISelectedBaller; // UI_Baller관련 됨 (명성트로피로드 보상이기때문)
    }

    public static KOBRarity GetCardRarity(KOBReward reward)
    {
        if(reward == KOBReward.Card_Black) return KOBRarity.BLACK;
        else if (reward == KOBReward.Card_Legend) return KOBRarity.LEGENDARY;
        else if (reward == KOBReward.Card_Epic) return KOBRarity.EPIC;
        else if (reward == KOBReward.Card_Rare) return KOBRarity.RARE;
        else return KOBRarity.COMMON;
    }


    public static bool CheckCardType(KOBReward type)
    {
        if (type == KOBReward.Card_Common ||
           type == KOBReward.Card_Rare ||
           type == KOBReward.Card_Epic ||
           type == KOBReward.Card_Legend ||
           type == KOBReward.Card_Black)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 아이템 개수에 따른 박스 타입 설정
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static KOBReward SetBoxType(List<RewardData> list)
    {
        int count = list.Count;

        //추후 상자 타입에 따라 결정해줘
        // -> 카드에 레전드가 있는 경우
        // -> 카드에 에픽, 레어가 있는 경우
        // -> 젬이 일정 이상인경우
        // -> 기타 등등
        
        //우선은 선물개수에 따라..
        if(count >= 7)
        {
            return KOBReward.Box_Legend;
        }
        else if (count >= 5)
        {
            return KOBReward.Box_Epic;
        }
        else if (count >= 4)
        {
            return KOBReward.Box_Rare;
        }
        else
        {
            return KOBReward.Box_Common;
        }
    }



}
