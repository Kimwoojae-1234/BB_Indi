using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class UIWalkOff : MonoBehaviour
    {
        public UILabel roundLabel;
        public GameObject arrow;
        public UISprite[] strikeSpr;

        float curTime;
        int step = 0;
        void Update()
        {
            curTime += Time.deltaTime;
            if (curTime > 0.3f)
            {
                step++;
                if (step > 2) step = 0;
                arrow.transform.localPosition = new Vector3((38 + step * 13), -24, 0);
                curTime = 0;
            }
        }

        public void SetRound(int round)
        {
            roundLabel.text = round.ToString();
            
        }
    }
}