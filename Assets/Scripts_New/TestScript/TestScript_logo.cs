using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestScript_logo : MonoBehaviour
{

    //public DOTweenAnimation anim;
    public Transform anim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            /*
            anim.enabled = true;
            anim.DORewind();
            anim.DOPlay();*/
            //anim.DOScale(1.2f, 0.5f);
            JsonTest();
            //{"reward":1,"rewardFrom":2,"pindex":0,"amount":100}
        }
    }


    private void JsonTest()
    {
        KOBRewardInfo aaa = new KOBRewardInfo();
        aaa.reward = KOBReward.Gold;
        aaa.rewardFrom = KOBRewardFrom.TrophyRoad;
        aaa.pindex = 0;
        aaa.amount = 100;
        string ggg= JsonHelper.SerializeObject(aaa);
        Debug.Log(ggg);
    }

}
