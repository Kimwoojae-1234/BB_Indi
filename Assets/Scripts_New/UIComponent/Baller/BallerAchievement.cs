using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallerAchievement : MonoBehaviour
{
    [SerializeField] private GameObject Clone;
    [SerializeField] private Transform content;

    private Dictionary<int, BallerAchieveComponent> list = new Dictionary<int, BallerAchieveComponent>();
    private int SelectIdx = -1;

    public void InitUI(int _idx)
    {
        if(SelectIdx != _idx)
        {
            IReadOnlyDictionary<int, AchievementData> chart = KOBManager.Backend.Chart.AchievementData.Dictionary;
            SelectIdx = _idx;
            if(list.Count == 0) //하나도 없는 경우
            {
                foreach (KeyValuePair<int, AchievementData> item in chart)
                {
                    int key = item.Key;
                    GameObject achiveObj = GameObject.Instantiate(Clone.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
                    achiveObj.transform.parent = content;
                    achiveObj.transform.localScale = Vector3.one;
                    achiveObj.gameObject.SetActive(true);
                    BallerAchieveComponent achive = achiveObj.GetComponent<BallerAchieveComponent>();
                    list.Add(key, achive);
                    achive.Init(item.Value, SelectIdx);
                }
            }
            else //이미 있는 경우
            {
                foreach (KeyValuePair<int, AchievementData> item in chart)
                {
                    int key = item.Key;
                    list[key].Init(item.Value, SelectIdx);
                }
            }
        }
    }
}
