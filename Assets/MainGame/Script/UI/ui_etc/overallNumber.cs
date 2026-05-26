using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class overallNumber : spriteNumber
    {
        public void Set(int number)   //38
        {
            gabX = 10;
            if (number >= 121) str = "rate_r_";
            else if (number >= 91) str = "rate_y_";
            else if (number >= 61) str = "rate_g_";
            else str = "rate_w_";

            base.set(number);
        }
    }
}