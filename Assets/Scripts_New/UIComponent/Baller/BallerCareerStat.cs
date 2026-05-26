using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BallerCareerStat : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private CareerStatComponent clone;

    int LastIdx = -1;
    Dictionary<int, CareerStatComponent> List = new Dictionary<int, CareerStatComponent>();


    public void InitUI(int _idx)
    {
        //다시 만들것
        if (LastIdx != _idx)
        {
            LastIdx = _idx;

            if(List.Count > 0)
            {
                for(int i = 0;i<List.Count;i++)
                {
                    Destroy(List[i].gameObject);
                }
            }
            List.Clear();


            Dictionary<int, int[]> BallerStat = KOBManager.MyInfo.GameData.GrowthInfo.BallerStat[_idx];//

            

            foreach (KeyValuePair<int, int[]> stat in BallerStat)
            {
                int league = stat.Key;
                GameObject statObj = GameObject.Instantiate(clone.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
                statObj.transform.parent = content;
                statObj.transform.localScale = Vector3.one;
                statObj.gameObject.SetActive(true);
                CareerStatComponent statComp = statObj.GetComponent<CareerStatComponent>();
                List.Add(league,statComp);
                if (statComp != null)
                {                    
                    statComp.InitLeague(league, _idx, BallerStat);
                }
            }

            GameObject totalObj = GameObject.Instantiate(clone.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
            totalObj.transform.parent = content;
            totalObj.transform.localScale = Vector3.one;
            totalObj.gameObject.SetActive(true);
            CareerStatComponent totalComp = totalObj.GetComponent<CareerStatComponent>();
            List.Add(1000, totalComp);
            totalComp.InitTotal(_idx, BallerStat);


            //소팅
            var sortVar = from item in List
                          orderby item.Key descending
                          select item;

            sortVar.ToDictionary(x => x.Key, x => x.Value);

            int count = 0;
            foreach (KeyValuePair<int, CareerStatComponent> collection in sortVar)
            {
                collection.Value.transform.SetSiblingIndex(count);
                count++;
            }
        }
    }

    /* 테스트용
    private int [] tempArray()
    {
        int [] value = new int[11];

        value[0] = Random.Range(401, 500);
        value[1] = Random.Range(350, 400);
        value[2] = Random.Range(101, 200);
        for (int i = 3; i<value.Length;i++)
        {
            value[i] = Random.Range(5, 8);
        }
        return value;
    }*/
}
