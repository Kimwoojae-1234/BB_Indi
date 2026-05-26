using System;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static BackendManager;
using Newtonsoft.Json;

[Serializable]
public partial class KOBGameData : BackendData.Base.GameData
{
    //재화정보
    [JsonProperty] public CurrencyInfo CurrencyInfo { get; private set; }

    //성장정보
    [JsonProperty] public GrowthInfo GrowthInfo { get; private set; }

    //튜토리얼 정보
    [JsonProperty] public Dictionary<TutorialManager.TutoStep, bool> TutoInfo { get; private set; }

    //RTTS정보
    [JsonProperty] public MyRttsInfo RttsInfo { get; private set; }

    [JsonProperty] public MyPlayerInfo PlayerInfo { get; private set; }

    [JsonProperty] public MyManageInfo ManageInfo { get; private set; }

    [JsonProperty] public MyDeckInfo DeckInfo { get; private set; }

    [JsonProperty] public MyItemInfo ItemInfo { get; private set; }


    protected override void InitializeData()
    {
        CurrencyInfo = new CurrencyInfo();
        GrowthInfo = new GrowthInfo();
        RttsInfo = new MyRttsInfo();
        PlayerInfo = new MyPlayerInfo();
        ManageInfo = new MyManageInfo();
        DeckInfo = new MyDeckInfo(); //반드시 PlayerInfo 다음에 초기화 할것
        ItemInfo = new MyItemInfo();
        TutoInfo = new Dictionary<TutorialManager.TutoStep, bool>();
        TutoInfo.Clear();

        changeFlagSetting(true);
    }

    // Backend.GameData.GetMyData 호출 이후 리턴된 값을 파싱하여 캐싱하는 함수
    // 서버에서 데이터를 불러오늖 함수는 BackendData.Base.GameData의 BackendGameDataLoad() 함수를 참고해주세요
    protected override void SetServerDataToLocal(JsonData gameDataJson)
    {
        //재화 정보
        if (gameDataJson.ContainsKey("CurrencyInfo"))
        {
            CurrencyInfo = new CurrencyInfo(gameDataJson["CurrencyInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            CurrencyInfo = new CurrencyInfo();
        }

        //성장 정보
        if (gameDataJson.ContainsKey("GrowthInfo"))
        {
            GrowthInfo = new GrowthInfo(gameDataJson["GrowthInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            GrowthInfo = new GrowthInfo();
        }

        //튜토리얼 정보
        //TutoInfo
        if (gameDataJson.ContainsKey("TutoInfo"))
        {
            TutoInfo = KOBTableUtil.DeserializeDictionary<TutorialManager.TutoStep, bool>(gameDataJson["TutoInfo"],
                                                            keyStr => (TutorialManager.TutoStep)Enum.Parse(typeof(TutorialManager.TutoStep), keyStr),
                                                            json => JsonHelper.DeserializeObject<bool>(json.ToJson()));
        }
        else
        {
            //해당항목없으면 초기화할것
            TutoInfo = new Dictionary<TutorialManager.TutoStep, bool>();
            TutoInfo.Clear();
        }

        //RTTS 리그 정보
        if (gameDataJson.ContainsKey("RttsInfo"))
        {
            RttsInfo = new MyRttsInfo(gameDataJson["RttsInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            RttsInfo = new MyRttsInfo();
        }

        //플레이어(볼러) 정보
        if (gameDataJson.ContainsKey("PlayerInfo"))
        {
            PlayerInfo = new MyPlayerInfo(gameDataJson["PlayerInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            PlayerInfo = new MyPlayerInfo();
        }

        //매니지 정보 -> 낮은 용량 잦은 바뀜
        if (gameDataJson.ContainsKey("ManageInfo"))
        {
            ManageInfo = new MyManageInfo(gameDataJson["ManageInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            ManageInfo = new MyManageInfo();
        }

        //덱 정보
        if (gameDataJson.ContainsKey("DeckInfo"))
        {
            DeckInfo = new MyDeckInfo(gameDataJson["DeckInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            DeckInfo = new MyDeckInfo();
        }

        //아이템 정보
        if (gameDataJson.ContainsKey("ItemInfo"))
        {
            ItemInfo = new MyItemInfo(gameDataJson["ItemInfo"]);
        }
        else
        {
            //해당항목없으면 초기화할것
            ItemInfo = new MyItemInfo();
        }

        changeFlagSetting(false);
        IsChangedData = false;
    }

    // 테이블 이름 설정 함수
    public override string GetTableName()
    {
        return "KOBGameData";
    }

    // 컬럼 이름 설정 함수
    public override string GetColumnName()
    {
        return null;
    }

    // 데이터 저장 시 저장할 데이터를 뒤끝에 맞게 파싱하는 함수
    public override Param GetParam()
    {
        Param param = new Param();
        
        if(isCurrencyChange == true) 
            param.Add("CurrencyInfo", CurrencyInfo);
        if (isGrowthChange == true)
            param.Add("GrowthInfo", GrowthInfo);
        if (isTutoChange == true)
            param.Add("TutoInfo", TutoInfo);
        if (isRttsChange == true)
            param.Add("RttsInfo", RttsInfo);
        if (isPlayerChange == true)
            param.Add("PlayerInfo", PlayerInfo);
        if (isManagerChange == true)
            param.Add("ManageInfo", ManageInfo);
        if (isDeckChange == true)
            param.Add("DeckInfo", DeckInfo);
        if (isItemChange == true)
            param.Add("ItemInfo", ItemInfo);

        return param;
    }


    private bool isCurrencyChange = false;
    private bool isGrowthChange = false;
    private bool isTutoChange = false;
    private bool isRttsChange = false;
    private bool isPlayerChange = false;
    private bool isManagerChange = false;
    private bool isDeckChange = false;
    private bool isItemChange = false;

    private void changeFlagSetting(bool isActive)
    {
        isCurrencyChange = isActive;
        isGrowthChange = isActive;
        isTutoChange = isActive;
        isRttsChange = isActive;
        isPlayerChange = isActive;
        isManagerChange = isActive;
        isDeckChange = isActive;
        isItemChange = isActive;
    }


    public override void LocalDataUpdate()
    {
        UnityEngine.Debug.Log("KOBGameData LocalDataUpdate");
        changeFlagSetting(false);
        KOBManager.MyInfo.UserInfoUpdate();
    }

    public override void RevertData()
    {
        UnityEngine.Debug.Log("KOBGameData RevertData");
        changeFlagSetting(false);
        KOBGameData data = KOBManager.MyInfo.UserInfoRevert();
    }



    public KOBGameData DeepCopy()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            return (KOBGameData)formatter.Deserialize(stream);
        }
    }


    public void TutorailCompete(TutorialManager.TutoStep step)
    {
        if(TutoInfo.ContainsKey(step))
        {
            TutoInfo[step] = true;
        }
        else
        {
            TutoInfo.Add(step, true);   
        }
        isTutoChange = true;
        IsChangedData = true;        
    }
}
