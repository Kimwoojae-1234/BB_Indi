using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public bool Updated
    {
        get;
        private set;
    }
    private Dictionary<string, BaseDataRecord> dic_GameData = null;

    private DatabaseManager()
    {
        Updated = false;
        dic_GameData = new Dictionary<string, BaseDataRecord>();
    }


    private void OnDestroy()
    {
        dic_GameData.Clear();
        dic_GameData = null;
    }


    public T LoadGameData<T>(string tableName) where T : BaseDataRecord
    {
        string path = string.Format("Json/{0}",tableName);
        TextAsset LoadDataAsset = Resources.Load<TextAsset>(path) as TextAsset;

        if (LoadDataAsset == null)
            return null;

        T loadObject = JsonHelper.DeserializeObject<T>(LoadDataAsset.ToString());
        return loadObject;
    }


    private T DatabaseLoad<T>(string tableName) where T : BaseDataRecord
    {
        BaseDataRecord loadDatabase = null;
        if (dic_GameData.TryGetValue(tableName, out loadDatabase) == false)
        {
            loadDatabase = LoadGameData<T>(tableName);
            dic_GameData.Add(tableName, loadDatabase);
        }
        return loadDatabase as T;
    }
    public T GameDataLoad<T>(string tableName) where T : BaseDataRecord
    {
        T tableData = DatabaseLoad<T>(tableName);
        return tableData;
    }




    public StadiumBaseData[] LoadStadiumBaseData()
    {
        StadiumBaseDataRecord StadiumBaseData = GameDataLoad<StadiumBaseDataRecord>("StadiumBaseData");
        if (StadiumBaseData == null || StadiumBaseData.StadiumBaseData == null)
            return null;
        return StadiumBaseData.StadiumBaseData;
    }


    public List<InningSelectData> LoadInningSelectData(int stadiumIndex)
    {
        List<InningSelectData> selectDataList = new List<InningSelectData>();
        InningSelectDataRecord inningSelectDataRecord = GameDataLoad<InningSelectDataRecord>("InningSelectData");
        if (inningSelectDataRecord == null)
            return null;

        for (int i = 0; i < inningSelectDataRecord.InningSelectData.Length; i++)
        {
            if (inningSelectDataRecord.InningSelectData[i].StadiumIdx == stadiumIndex)
            {
                selectDataList.Add(inningSelectDataRecord.InningSelectData[i]);
            }
        }
        selectDataList.Sort((lhs, rhs) => lhs.InningIdx - rhs.InningIdx);
        return selectDataList;
    }





    public CardBaseData LoadPlayerData(int playerIndex)
    {
        CardDataRecord tableData = GameDataLoad<CardDataRecord>("CardBaseData");
        if (tableData == null)
            return null;
        CardBaseData CardDBData = null;
        for (int i = 0; i < tableData.cardBaseData.Length; i++)
        {
            if (tableData.cardBaseData[i].Idx == playerIndex)
            {
                CardDBData = tableData.cardBaseData[i];
            }
        }
        return CardDBData;
    }


    public CardLevelBalanceData LoadCardLevelBalance(int idx, int level)
    {
        CardLevelBalanceDataRecord BalanceTableData = GameDataLoad<CardLevelBalanceDataRecord>("CardLevelBalanceData");
        if (BalanceTableData == null)
            return null;
        CardLevelBalanceData BalanceData = null;
        for (int i = 0; i < BalanceTableData.cardLevelBalanceData.Length; i++)
        {
            if (BalanceTableData.cardLevelBalanceData[i].Idx == idx && BalanceTableData.cardLevelBalanceData[i].Level == level)
            {
                BalanceData = BalanceTableData.cardLevelBalanceData[i];
            }
        }
        return BalanceData;
    }

    public SkillLevelData LoadSkillLevelData(int skill_index, int level)
    {
        SkillLevelDataRecord SkillTableData = GameDataLoad<SkillLevelDataRecord>("SkillLevelData");
        if (SkillTableData == null)
            return null;
        SkillLevelData levelData = null;
        for (int i = 0; i < SkillTableData.SkillLevelData.Length; i++)
        {
            if (SkillTableData.SkillLevelData[i].Idx == skill_index && SkillTableData.SkillLevelData[i].Level == level)
            {
                levelData = SkillTableData.SkillLevelData[i];
                break;
            }
        }
        return levelData;
    }
}
