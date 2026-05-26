using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class networkWait : MonoBehaviour {

    UISprite spr;
	// Use this for initialization
	void Start () {
        spr = GetComponent<UISprite>();
        curTime = 0;
        index = 1;
	}

    float curTime;
    int index;
	// Update is called once per frame
	void Update () {
        curTime += Time.deltaTime;
        if (curTime > 0.2f)
        {
            spr.spriteName = "network" + index;
            index++;
            if (index > 5) index = 1;
            curTime = 0;
        }
	}
}
