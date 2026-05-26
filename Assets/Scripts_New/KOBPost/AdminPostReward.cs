using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminPostReward
{
    public int idx { get; private set; }
    public KOBReward reward { get; private set; }
    public int pindex { get; private set; }
    public string info { get; private set; }
    public int amount { get; private set; }

    // Start is called before the first frame update
    public AdminPostReward(JsonData json,int _amount)
    {
        idx = int.Parse(json["idx"].ToString());
        reward = (KOBReward)Enum.Parse(typeof(KOBReward), json["reward"].ToString());
        pindex = int.Parse(json["pindex"].ToString());
        info = json["info"].ToString();
        amount = _amount;
    }
}
