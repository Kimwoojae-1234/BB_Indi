using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class gaugeSetter : MonoBehaviour 
    {
        private readonly int MAX_VALUE = 120;

        public UISprite firstGauge, secondGague;
        public UILabel overallValue;

        

        public void set(int [] value1, int max)
        {
            int first = Mathf.Clamp(value1[0] + value1[1], 5, MAX_VALUE);
            int second = 5;//추후 장비 추가시
            int overall = value1[0] + value1[1];

            int h = firstGauge.height;
            firstGauge.SetDimensions((first * max) / 120, h);
            secondGague.SetDimensions((second * max) / 120, h);

            overallValue.text = overall.ToString();

        }


    }
}