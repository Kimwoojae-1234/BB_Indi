using LitJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CurrencyInfo
{
    [JsonProperty] public long Gold { get; private set; }
    [JsonProperty] public long Gem { get; private set; }
    [JsonProperty] public long Gem_Free { get; private set; }
    [JsonProperty] public int Energy { get; private set; }

    public long TotalGem 
    { 
        get
        {
            return Gem + Gem_Free;
        } 
    }

    public CurrencyInfo()
    {
        Init();
    }

    public CurrencyInfo(JsonData json)
    {
        Gold = long.Parse(json["Gold"].ToString());
        Gem = long.Parse(json["Gem"].ToString());
        Gem_Free = long.Parse(json["Gem_Free"].ToString());
        Energy = int.Parse(json["Energy"].ToString());
    }

    public void Init()
    {        
        Gold = KOBManager.Backend.Setting.InitGold;
        Gem = KOBManager.Backend.Setting.InitGem;
        Gem_Free = 0;
        Energy = KOBManager.Backend.Setting.InitStamina;        
    }


    public bool UpdateCurrency(KOBRewardInfo RewardInfo)
    {
        bool IsChangedData = false;
        switch (RewardInfo.reward)
        {
            case KOBReward.Gold:
                Gold += RewardInfo.amount;
                IsChangedData = true;
                break;
            case KOBReward.Gem:
                Gem += RewardInfo.amount;
                IsChangedData = true;
                break;
            case KOBReward.Gem_Free:
                Gem_Free += RewardInfo.amount;
                IsChangedData = true;
                break;
            case KOBReward.Energy:
                Energy += RewardInfo.amount;
                IsChangedData = true;
                break;
        }

        return IsChangedData;
    }
    public bool UpdateCurrency(long gold, long gem, long gem_free, int energy)
    {
        Gold += gold;
        Gem += gem;
        Gem_Free += gem_free;
        Energy += energy;
        return true;
    }
}

[System.Serializable]
public class GrowthInfo
{
    public const int MAX_RECORD = 20;
    [JsonProperty] public int Trophy { get; private set; }
    [JsonProperty] public int MaxTrophy { get; private set; }
    [JsonProperty] public int MaxEnergy { get; private set; }
    [JsonProperty] public int League { get; private set; }
    [JsonProperty] public int PassXP { get; private set; }
    [JsonProperty] public int MyTier { get; private set; } = 0;

    //트로피 획득 정보
    [JsonProperty] public List<int> TropyGetList { get; private set; } = new List<int>();
    [JsonProperty] public Dictionary<int, List<int>> BallerTropyGetList { get; private set; } = new  Dictionary<int, List<int>>();
    [JsonProperty] public Dictionary<int, Dictionary<int, int[]>> BallerStat { get; private set; } = new Dictionary<int, Dictionary<int, int[]>>(); //pa(0),ab(1),h(2),2b(3),3b(4),hr(5),rbi(6),sb(7),bb(8),so(9),r(10),결승타(11),역전타(12),타격왕(13),홈런왕(14),타점왕(15),MVP(16), 우승(17), 호수비-보살포함(18)
    [JsonProperty] public Dictionary<int, Dictionary<int, int>> AchievementList { get; private set; } = new Dictionary<int, Dictionary<int, int>>();

    public void Init()
    {
        Trophy = 0;
        MaxTrophy = 0;
        League = 1;
        MaxEnergy = KOBManager.Backend.Setting.InitMaxStamina;
        PassXP = 0;
        MyTier = 0;
        TropyGetList.Clear();
        BallerTropyGetList.Clear();
        AchievementList.Clear();
        BallerStat.Clear();

        SetBallerAdditionalInfo(KOBConstant.FIRSTBALLER);
    }

    public GrowthInfo()
    {
        Init();
    }

    public GrowthInfo(JsonData json)
    {
        if(json.ContainsKey("Trophy") == true) Trophy = int.Parse(json["Trophy"].ToString());
        if(json.ContainsKey("MaxTrophy") == true) MaxTrophy = int.Parse(json["MaxTrophy"].ToString());
        if (json.ContainsKey("League") == true) League = int.Parse(json["League"].ToString());
        if (json.ContainsKey("MaxEnergy") == true) MaxEnergy = int.Parse(json["MaxEnergy"].ToString()); ;
        if (json.ContainsKey("PassXP") == true) PassXP = int.Parse(json["PassXP"].ToString());
        if (json.ContainsKey("MyTier") == true) MyTier = int.Parse(json["MyTier"].ToString());
        if (json.ContainsKey("TropyGetList") == true) TropyGetList = KOBTableUtil.DeserializeList<int>(json["TropyGetList"], json => JsonHelper.DeserializeObject<int>(json.ToJson()));
        if (json.ContainsKey("BallerTropyGetList") == true)
        {
            BallerTropyGetList = KOBTableUtil.DeserializeDictionary<int, List<int>>(json["BallerTropyGetList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<List<int>>(json.ToJson()));
        }
        if (json.ContainsKey("BallerStat") == true)
        {
            BallerStat = KOBTableUtil.DeserializeDictionary<int, Dictionary<int, int[]>>(json["BallerStat"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<Dictionary<int, int[]>>(json.ToJson()));
        }
        if (json.ContainsKey("AchievementList") == true)
        {
            AchievementList = KOBTableUtil.DeserializeDictionary<int, Dictionary<int, int>>(json["AchievementList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<Dictionary<int, int>>(json.ToJson()));
        }
    }


    public void AddTrophyReward(int idx)
    {
        if(TropyGetList.Contains(idx) == false)
        {
            TropyGetList.Add(idx);  
        }
    }
    
    /// <summary>
    /// 볼러 추가시 해줌
    /// </summary>
    /// <param name="idx"></param>
    public void SetBallerAdditionalInfo(int idx)
    {
        if(BallerTropyGetList.ContainsKey(idx) == false)
        {
            BallerTropyGetList.Add(idx, new List<int>());
        }
        if(BallerStat.ContainsKey(idx) == false)
        {
            Dictionary<int, int[]> record = new Dictionary<int, int[]>();// new int[MAX_RECORD];
            BallerStat.Add(idx , record);
        }
        if (AchievementList.ContainsKey(idx) == false)
        {
            AchievementList.Add(idx, new Dictionary<int, int>());
        }
    }

    public bool AddTrophy(int trophy)
    {
        if (trophy != 0)
        {
            Trophy += trophy;

            //트로피는 0보다 작을 수 없다
            if (Trophy < 0)
            {
                Trophy = 0;
            }

            //현재 설정된 Max값을 넘을 수 없다.
            if(Trophy > KOBConstant.MAX_TROPHY)
            {
                Trophy = KOBConstant.MAX_TROPHY;
            }

            //MaxTrophy는 내가 획득한 최대 트로피 값으로 티어를 계산할때는 이걸로 계산함
            if (Trophy > MaxTrophy)
            {
                MaxTrophy = Trophy;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AddXP(int xp)
    {
        if (xp != 0)
        {
            PassXP += xp;
            if (PassXP < 0)
            {
                PassXP = 0;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 레코드를 int[]에 저장
    /// 용량 관계로 20개 저장
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="record"></param>
    /// <returns></returns>
    public bool AddRecord(int idx, int league, int[] record)
    {
        if (record == null) return false;

        if(BallerStat.ContainsKey(idx) == true)
        {
            Dictionary<int, int[]> value = BallerStat[idx];
            if(value.ContainsKey(league) == true)
            {
                for (int i = 0; i < MAX_RECORD; i++)
                {                    
                    value[league][i] += record[i];
                }
            }
            else
            {
                value.Add(league, record);
            }
        }
        else
        {
            //없는 경우 새로 
            Dictionary<int, int[]> recordItem = new Dictionary<int, int[]>();// new int[MAX_RECORD];
            int[] addRecord = new int[MAX_RECORD];
            for (int i = 0; i < MAX_RECORD; i++) addRecord[i] = record[i];
            recordItem.Add(league, addRecord);
            BallerStat.Add(idx, recordItem);
        }

        return true;
    }


    /// <summary>
    /// 볼러트로피획득 (명성보상) 시 해당 정보 업데이트 -> 2번 못받도록
    /// Request Baller Trophy 호출시 이거 호출한다음 세팅된 보상을 내려준다
    /// </summary>
    /// <param name="baller_idx"></param>
    /// <param name="trophy_idx"></param>
    /// <returns></returns>
    public bool BallerTrophyGet(int baller_idx, int trophy_idx)
    {
        if (BallerTropyGetList.ContainsKey(baller_idx) == true)
        {
            BallerTropyGetList[baller_idx].Add(trophy_idx);
        }
        else
        {
            List<int> addList = new List<int>();
            addList.Add(trophy_idx);
            BallerTropyGetList.Add(baller_idx, addList);
        }
        return true;
    }


    public bool SetNewTier(int newTier)
    {
        if (MyTier < newTier)
        {
            MyTier = newTier;
            return true;
        }
        else
        {
            return false;
        }
    }

}



[System.Serializable]
public class MyPlayerInfo
{
    [JsonProperty] public Dictionary<int, KOBBaller> BallerList { get; private set; } //보유선수 (HMan도 포함)    
    [JsonProperty] public Dictionary<int, KOBBaller> PitcherList { get; private set; } //투수     

    public void Init()
    {
        //선수 초기화 - 8명할것 -> 초반기본맨+(1Star) 설정
        BallerList = new Dictionary<int, KOBBaller>();
        BallerList.Clear();
        for (int i = 0; i < 8; i++)
        {
            int idx = KOBConstant.FIRSTBALLER + i; //이거 추후에 테이블로
            KOBBaller player = new KOBBaller();
            player.InitBaller(idx);
            BallerList.Add(idx, player);            
        }

        //투수 초기화 - 4명 할것 -> 초반기본투수설정
        PitcherList = new Dictionary<int, KOBBaller>();
        PitcherList.Clear();
        for (int i = 0; i < 4; i++)
        {
            int p_idx = KOBConstant.FIRSTPITCHER + i; //이것도 테이블로
            KOBBaller pitcher = new KOBBaller();
            pitcher.InitBaller(p_idx);
            PitcherList.Add(p_idx, pitcher);
        }
    }

    public MyPlayerInfo()
    {
        Init();
    }

    public MyPlayerInfo(JsonData json)
    {
        
        BallerList = KOBTableUtil.DeserializeDictionary<int, KOBBaller>(json["BallerList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<KOBBaller>(json.ToJson()));
        PitcherList = KOBTableUtil.DeserializeDictionary<int, KOBBaller>(json["PitcherList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<KOBBaller>(json.ToJson()));

    }

    public bool UpgradeCard(int idx, int needCard)
    {
        bool isChangeData = false;
        if (BallerList.ContainsKey(idx))
        {
            if (BallerList[idx].level < KOBConstant.MAX_LEVEL)
            {
                BallerList[idx].UpgradeCard(needCard);
                isChangeData = true;
            }
            else
            {
                UnityEngine.Debug.LogError("더이상 업그레이드가 불가능합니다.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("해당 인덱스의 카드가 없습니다.");
        }

        return isChangeData;
    }

    public bool AddFame(int idx, int value)
    {
        bool isChangeData = false;
        if (BallerList.ContainsKey(idx))
        {
            BallerList[idx].AddTrophy(value);
            isChangeData = true;
        }
        else
        {
            UnityEngine.Debug.LogError("해당 인덱스의 카드가 없습니다.");
        }

        return isChangeData;
    }

}


[System.Serializable]
public class MyManageInfo
{
    //이부분은 자주 바뀌는 거라 따로 세팅
    [JsonProperty] public int SelectBaller { get; private set; } //선택된 선수
    [JsonProperty] public int Rotation { get; private set; } //현재로테이션 - 경기할때마다 하나씩 증가


    public MyManageInfo()
    {
        SelectBaller = KOBConstant.FIRSTBALLER;
        Rotation = 1;
    }

    public MyManageInfo(JsonData json)
    {
        SelectBaller = int.Parse(json["SelectBaller"].ToString());
        Rotation = int.Parse(json["Rotation"].ToString());
    }


    public void SetBaller(int idx)
    {
        SelectBaller = idx;
    }
}

[System.Serializable]
public class MyDeckInfo
{
    //팀관련
    [JsonProperty] public Dictionary<int, KOBLineupInfo> LineupList { get; private set; } //라인업 (타순 / 라인업정보)
    [JsonProperty] public Dictionary<int, int> RotationList { get; private set; } //로테이션 (로테이션 / 투수idx)

    public MyDeckInfo()
    {
        Init();
    }

    public MyDeckInfo(JsonData json)
    {
        LineupList = KOBTableUtil.DeserializeDictionary<int, KOBLineupInfo>(json["LineupList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<KOBLineupInfo>(json.ToJson()));
        RotationList = KOBTableUtil.DeserializeDictionary<int, int>(json["RotationList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<int>(json.ToJson()));
    }

    public void Init()
    {
        //팀관련 초기화
        LineupList = new Dictionary<int, KOBLineupInfo>();
        LineupList.Clear();
        //라인업에 있는 선수 쫘르륵
        int order = 1;
        Dictionary<int, KOBBaller> BallerList = KOBManager.Backend.GameData.KOBGameData.PlayerInfo.BallerList;  
        foreach (KeyValuePair<int, KOBBaller> baller in BallerList)
        {
            KOBLineupInfo info = new KOBLineupInfo();
            info.idx = baller.Key;
            info.position = 1 + order;
            LineupList.Add(order, info);
            order++;
        }

        RotationList = new Dictionary<int, int>();
        RotationList.Clear();
        //로테이션에 있는 선수 쫘르륵
        int rotation = 1;
        Dictionary<int, KOBBaller> PitcherList = KOBManager.Backend.GameData.KOBGameData.PlayerInfo.PitcherList;
        foreach (KeyValuePair<int, KOBBaller> pitchers in PitcherList)
        {
            int _idx = pitchers.Key;
            RotationList.Add(rotation, _idx);
            rotation++;
        }
    }


    public bool ChangeDeck(Dictionary<int, KOBLineupInfo> NewDeck)
    {
        if(NewDeck.Count == 8)
        {
            LineupList = NewDeck;
            return true;
        }
        else
        {
            return false;
        }
    }

}

[System.Serializable]
public class MyItemInfo
{
    //소비성 아이템
    [JsonProperty] public Dictionary<int, KOBBat> BatList { get; private set; } //소유한 배트리스트         

    //여기에 장비 티켓등이 포함될 예정


    public MyItemInfo()
    {
        Init();
    }

    public MyItemInfo(JsonData json)
    {
        BatList = KOBTableUtil.DeserializeDictionary<int, KOBBat>(json["BatList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<KOBBat>(json.ToJson()));
    }

    public void Init()
    {
        //아이템 초기화
        BatList = new Dictionary<int, KOBBat>();
        BatList.Clear();
    }

}




[Serializable]
public class KOBBaller
{
    [JsonProperty] public int idx { get; private set; }
    [JsonProperty] public int level { get; private set; } = 1; //해당 레벨에 대한 TP + coin 지급하여 업글
    [JsonProperty] public int card_number { get; private set; } = 1;
    [JsonProperty] public int baller_trophy { get; private set; } = 0;
    [JsonProperty] public int baller_rank { get; private set; } = 0;
    [JsonProperty] public Dictionary<int, int> SkillEquip { get; private set; } = new Dictionary<int, int>(); //Key:슬롯인덱스 : Value: 슬롯에 있는 스킬인덱스
    [JsonProperty] public Dictionary<int, int> GearEquip { get; private set; } = new Dictionary<int, int>(); //Key:슬롯인덱스 : Value: 슬롯에 있는 장비인덱스
    [JsonProperty] public Dictionary<int, int> SkinEquip { get; private set; } = new Dictionary<int, int>(); //Key:슬롯인덱스 : Value: 슬롯에 있는 스킨인덱스
    [JsonProperty] public Dictionary<int, KOBSkill> SkillList { get; private set; } = new Dictionary<int, KOBSkill>(); //추가 Key:스킬인덱스 : Value: 스킬현재상태 (여기서는현재가지고 있는  차트는 가질수 있는) //-->현재 스킬의 업그레이드 상태
    
    public KOBBaller() { }


    public void InitBaller(int _idx, int amount = 1) //초기화
    {
        idx = _idx;
        level = 1; //해당 레벨에 대한 TP + coin 지급하여 업글
        card_number = amount;
        baller_trophy = 0;
        baller_rank = 0;
        SkillEquip.Clear();
        GearEquip.Clear();
        SkinEquip.Clear();
        SkillList.Clear();
        //KOBManager.Backend.GameData.KOBGameData.GrowthInfo.AddBaller(idx);
    }

    public void UpgradeCard(int needCard)
    {
        card_number -= needCard;
        level++;
    }

    public void AddTrophy(int add)
    {
        baller_trophy += add;
        if(baller_trophy > KOBConstant.MAX_BALLER_FAME)
        {
            baller_trophy = KOBConstant.MAX_BALLER_FAME;
        }
    }


    public void AddCard(int add)
    {
        card_number += add;
    }

    public bool UpgradeFame()
    {
        int curFame = KOBManager.Backend.Chart.BallerTrophyRoadData.GetFameByTrophy(baller_trophy); //필요한 숫자 다시 계산
        if (curFame > baller_rank)
        {
            baller_rank = curFame;
            return true;
        }
        else
        {
            return false;
        }
    }

}
[Serializable]
public class KOBLineupInfo
{
    public int idx;
    public int position;
    public int lineup_power; //저장데이터는 아니고, 오토라인업시 실시간으로 계산하여 

    public KOBLineupInfo()
    {

    }

    //딥카피용 생성자
    public KOBLineupInfo(KOBLineupInfo other)
    {
        idx = other.idx;
        position = other.position;
        lineup_power = other.lineup_power;
    }

}

[Serializable]
public class KOBSkill
{
    public int idx;
    public int level;

    public KOBSkill() { }

    public void Init(int _idx)
    {

    }

}
[Serializable]
public class KOBGear
{
    public int idx;
    public int level;

    public KOBGear() { }
    public void Init(int _idx)
    {

    }

}
[Serializable]
public class KOBBat
{
    public int idx;
    public int number;

    public KOBBat() { }
    public KOBBat(int _idx, int _num)
    {
        idx = _idx;
        number = _num;
    }

}

[Serializable]
public class KOBSkin
{
    public int idx;
    public int level;

    public KOBSkin() { }

}
