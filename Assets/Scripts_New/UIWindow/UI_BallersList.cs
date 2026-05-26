using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class UI_BallersList : UIWindow
{
    [SerializeField] private GameObject clone;
    [SerializeField] private RectTransform collection;
    [SerializeField] private RectTransform unlock;
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI sortTxt;


    public enum SortType
    {
        Ballelr = 0,
        PowerLevel = 1,
        RarityDecending = 2,
        RarityAcending = 3,
        MostTrophy = 4,
        LeastTrophy = 5
    }

    private SortType sortType = SortType.Ballelr; //추후 로컬 저장


    public override void Initialize()
    {
        base.Initialize();
    }


    public override void OpenWindow()
    {
        base.OpenWindow();
        initUI();
    }



    //Dictionary<int, SortInfo> TotalBaller = new Dictionary<int, SortInfo>();
    Dictionary<int, SortInfo> CollectionBaller = new Dictionary<int, SortInfo>();
    Dictionary<int, SortInfo> UnlockBaller = new Dictionary<int, SortInfo>();

    private void initUI()
    {
        //TotalBaller.Clear();
        CollectionBaller.Clear();
        UnlockBaller.Clear();
        foreach (Transform child in collection) Destroy(child.gameObject);
        foreach (Transform child in unlock) Destroy(child.gameObject);

        int CurrentLeague = KOBManager.MyInfo.GameData.GrowthInfo.League;
        IReadOnlyDictionary<int, CharacterData> totalChart = KOBManager.Backend.Chart.CharacterData.Dictionary;
        Dictionary<int, KOBBaller> BallerList = KOBManager.MyInfo.GameData.PlayerInfo.BallerList;


        foreach (KeyValuePair<int, CharacterData> data in totalChart)
        {
            int idx = data.Key;
            if (data.Value.char_type == CharacterType.Ballers) //볼러인경우
            {
                if (BallerList.ContainsKey(data.Key) == true)
                {
                    //콜렉션
                    CollectionBaller.Add(idx, new SortInfo(MakeGameCard(idx, CardBaller.CardBallerState.Collection), data.Value, BallerList[idx].level));
                }
                else
                {
                    if (data.Value.league > 0)
                    {
                        if (data.Value.league <= CurrentLeague)
                        {
                            //언락킹
                            UnlockBaller.Add(idx, new SortInfo(MakeGameCard(idx, CardBaller.CardBallerState.Unlocking), data.Value));
                        }
                        else
                        {
                            //락
                            UnlockBaller.Add(idx, new SortInfo(MakeGameCard(idx, CardBaller.CardBallerState.Locked), data.Value));
                        }
                    }
                }
            }
        }


        int total = UnlockBaller.Count + CollectionBaller.Count;
        int baller = CollectionBaller.Count;        
        titleTxt.text = string.Format(KOBManager.Localization.GetUILocalizedValue2("UI.BallerListTitle"), baller, total);


        sortTxt.text = KOBManager.Localization.GetUILocalizedValue2("UI.BallerSort" + (int)sortType);
    }


    private Transform MakeGameCard(int idx, CardBaller.CardBallerState State)
    {
        GameObject obj = GameObject.Instantiate(clone, Vector3.zero, Quaternion.identity) as GameObject;

        if (obj != null)
        {
            CardBaller card = obj.GetComponent<CardBaller>();
            if (State == CardBaller.CardBallerState.Collection)
            {
                card.SetCollection(idx);
            }
            else if (State == CardBaller.CardBallerState.Unlocking)
            {
                card.SetUnlocking(idx);
            }
            else
            {
                card.SetLocked(idx);
            }

            obj.transform.parent = (State == CardBaller.CardBallerState.Collection ? collection.transform : unlock.transform);
            obj.transform.localScale = Vector3.one;
            return obj.transform;
        }
        else
        {
            return null;
        }
    }



    public void OnClickSort()
    {
        int sort = (int)(sortType) + 1;
        Debug.Log("sort : " + sort);
        if (sort > 5) sort = 0;
        sortType = (SortType)sort;
        Debug.Log("sortType : " + sortType);

        sortTxt.text = KOBManager.Localization.GetUILocalizedValue2("UI.BallerSort" + (int)sortType);

        CollectionBaller = SortingItem(sortType, CollectionBaller);
        UnlockBaller = SortingItem(sortType, UnlockBaller);

        int count = 0;
        foreach (KeyValuePair<int, SortInfo> collection in CollectionBaller)
        {
            collection.Value.trans.SetSiblingIndex(count);
            count++;
        }

        count = 0;
        foreach (KeyValuePair<int, SortInfo> unlock in UnlockBaller)
        {
            unlock.Value.trans.SetSiblingIndex(count);
            count++;
        }
    }


    private Dictionary<int, SortInfo> SortingItem(SortType type, Dictionary<int, SortInfo> dict)
    {

        if (sortType == SortType.PowerLevel)
        {
            var sortVar = from item in dict
                          orderby item.Value.level descending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
        else if (sortType == SortType.RarityDecending)
        {
            var sortVar = from item in dict
                          orderby item.Value.rarityValue descending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
        else if (sortType == SortType.RarityAcending)
        {
            var sortVar = from item in dict
                          orderby item.Value.rarityValue ascending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
        else if (sortType == SortType.MostTrophy)
        {
            var sortVar = from item in dict
                          orderby item.Value.trophy descending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
        else if (sortType == SortType.LeastTrophy)
        {
            var sortVar = from item in dict
                          orderby item.Value.trophy ascending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
        else //if (sortType == SortType.Ballelr)
        {
            var sortVar = from item in dict
                          orderby item.Value.idx ascending
                          select item;

            return sortVar.ToDictionary(x => x.Key, x => x.Value);
        }
    }

}

public class SortInfo
{
    public Transform trans;
    public int idx;
    public int rarityValue;
    public int level;
    public int trophy;

    public SortInfo(Transform t, CharacterData data, int lv = 0, int tp = 0)
    {
        trans = t;
        idx = data.char_idx;
        rarityValue = (int)data.rarity * 10000 + (9999 - idx);
        level = (lv * 10000) + (9999 - idx);
        trophy = (tp * 10000) + (9999 - idx);
    }
}
