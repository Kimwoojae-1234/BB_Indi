using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            makeJson();
        }
    }



    void makeJson()
    {
        /*Dictionary<int, SkillElement> level_value1 = new Dictionary<int, SkillElement>();


        SkillElement level1 = new SkillElement()
        {
            Key = KOBSkillKey.Power,
            Condition = KOBSkillCondition.Cumulative,
            Activation = 100,
            Value = 5
        };

        SkillElement level2 = new SkillElement()
        {
            Key = KOBSkillKey.Power,
            Condition = KOBSkillCondition.Cumulative,
            Activation = 100,
            Value = 6
        };

        SkillElement level3 = new SkillElement()
        {
            Key = KOBSkillKey.Power,
            Condition = KOBSkillCondition.Cumulative,
            Activation = 100,
            Value = 7
        };


        SkillElement level4 = new SkillElement()
        {
            Key = KOBSkillKey.Power,
            Condition = KOBSkillCondition.Cumulative,
            Activation = 100,
            Value = 8
        };

        SkillElement level5 = new SkillElement()
        {
            Key = KOBSkillKey.Power,
            Condition = KOBSkillCondition.Cumulative,
            Activation = 100,
            Value = 10
        };

        level_value1.Add(1, level1);
        level_value1.Add(2, level2);
        level_value1.Add(3, level3);
        level_value1.Add(4, level4);
        level_value1.Add(5, level5);


        string json = JsonHelper.SerializeObject(level_value1);
        Debug.Log(json);*/

        //Dictionary<int, SkillElement> parsing_value = JsonHelper.DeserializeObject<Dictionary<int, SkillElement>>(json);



        //Dictionary<int, int[]> level_value = new Dictionary<int, int[]>();

        //int[] level1 = { 20, 20, 20, 20, 20, 20 };
        //int[] level2 = { 30, 30, 30, 30, 30, 30 };
        //int[] level3 = { 40, 40, 40, 40, 40, 40 };

        //level_value.Add(1, level1);
        //level_value.Add(2, level2);
        //level_value.Add(3, level3);

        Dictionary<int, int[]> level_value = new Dictionary<int, int[]>();
        level_value.Add(1, new int[2] { 1, 2 });
        level_value.Add(2, new int[2] { 2, 4 });
        level_value.Add(3, new int[2] { 5, 2 });

        /*
        List<int> level_value = new List<int>();
        level_value.Add(1);
        level_value.Add(5);
        level_value.Add(4);
        level_value.Add(2);
        level_value.Add(9);*/

        string json = JsonHelper.SerializeObject(level_value);
        Debug.Log(json);


        //int[] vvv = JsonHelper.DeserializeObject<int[]>(json);
        //List<int> vvv = JsonHelper.DeserializeObject<List<int>>(json);

    }
}
