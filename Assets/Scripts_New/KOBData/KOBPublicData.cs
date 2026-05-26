using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using Unity.VisualScripting;
using static UnityEditor.ShaderData;
using static UnityEngine.EventSystems.EventTrigger;

[Serializable]
public class KOBPublicData : BackendData.Base.GameData
{
    [JsonProperty] public int Trophy { get; private set; }
    [JsonProperty] public int League { get; private set; }
    [JsonProperty] public int SelectBaller { get; private set; } //선택된 선수
    [JsonProperty] public Dictionary<int, KOBLineupInfo> LineupList { get; private set; } = new Dictionary<int, KOBLineupInfo>();//라인업 (타순 / 라인업정보)

    //이거부터 임시여
    [JsonProperty] public MyRttsInfo rttsInfo { get; private set; } = new MyRttsInfo();


    private bool isTrophyAdd = false;
    private bool isLeagueAdd = false;
    private bool isSelectBallerAdd = false;
    private bool isLineupListAdd = false;
    private bool isrttsInfoAdd = false;



    protected override void InitializeData()
    {
        Trophy = 0;
        League = 1;
        SelectBaller = 1;
        LineupList.Clear();
        rttsInfo.InitLeague();
        InitAddChecker(true);
    }

    private void InitAddChecker(bool isActive)
    {
        isTrophyAdd = isActive;
        isLeagueAdd = isActive;
        isSelectBallerAdd = isActive;
        isLineupListAdd = isActive;
        isrttsInfoAdd = isActive;
    }

    // Backend.GameData.GetMyData 호출 이후 리턴된 값을 파싱하여 캐싱하는 함수
    // 서버에서 데이터를 불러오늖 함수는 BackendData.Base.GameData의 BackendGameDataLoad() 함수를 참고해주세요
    protected override void SetServerDataToLocal(JsonData gameDataJson)
    {
        if (gameDataJson.ContainsKey("Trophy"))
        {
            Trophy = int.Parse(gameDataJson["Trophy"].ToString());
        }
        if (gameDataJson.ContainsKey("League"))
        {
            League = int.Parse(gameDataJson["League"].ToString());
        }

        if(gameDataJson.ContainsKey("LineupList"))
        {
            LineupList = KOBTableUtil.DeserializeDictionary<int, KOBLineupInfo>(gameDataJson["LineupList"],
                                                            keyStr => int.Parse(keyStr),
                                                            json => JsonHelper.DeserializeObject<KOBLineupInfo>(json.ToJson()));
        }

        if (gameDataJson.ContainsKey("rttsInfo"))
        {
            rttsInfo = new MyRttsInfo(gameDataJson["rttsInfo"]); 
        }
    }

    // 테이블 이름 설정 함수
    public override string GetTableName()
    {
        return "KOBPublicData";
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
        //param.Add(GetColumnName(), this);
        if (isTrophyAdd == true)
        {
            param.Add("Trophy", Trophy);
        }
        if (isLeagueAdd == true)
        {
            param.Add("League", League);
        }
        if (isSelectBallerAdd == true)
        {
            param.Add("SelectBaller", SelectBaller);
        }
        if (isLineupListAdd == true)
        {
            param.Add("LineupList", LineupList);
        }
        if (isrttsInfoAdd == true)
        {
            param.Add("rttsInfo", rttsInfo);
        }

        return param;
    }


    public override void LocalDataUpdate()
    {
        InitAddChecker(false);
    }

    public override void RevertData()
    {
        InitAddChecker(false);
    }



    public void UpdateTrophy()
    {
        Trophy+=10;
        isTrophyAdd = true;

        IsChangedData = true;
    }

}
