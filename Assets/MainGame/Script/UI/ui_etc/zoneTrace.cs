using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class zoneTrace : MonoBehaviour
    {

        public UISprite grade, gradeWhite;
        public UISprite circle;
        public UISprite call;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void init(BallPlayManager manager)
        {
            UserControlValue value = manager.pitcher.userControlValue;
            bool bStrike = manager.bStrikeCheck;

            if (manager.bMyTurn == true)
            {
                setSprite(circle, bStrike ? "zone_strike_h" : "zone_ball_h");
            }
            else
            {
                if (value == UserControlValue.Perfect)
                {
                    //setSprite(grade, "zone_perfect");
                    //setSprite(gradeWhite, "zone_perfect");
                    //setSprite(circle, "zone_perpect_circle");
                }
                else if (value == UserControlValue.Good || value == UserControlValue.Normal)
                {
                    setSprite(grade, "zone_good");
                    setSprite(gradeWhite, "zone_good");
                    setSprite(circle, "zone_good_circle");
                }
                else if (value == UserControlValue.Bad)
                {
                    setSprite(grade, "zone_bad");
                    setSprite(gradeWhite, "zone_bad");
                    setSprite(circle, "zone_bad_circle");
                }
                else //miss
                {
                    setSprite(grade, "zone_miss");
                    setSprite(gradeWhite, "zone_miss");
                    setSprite(circle, "zone_miss_circle");
                }
                setSprite(call, bStrike ? "zone_strike" : "zone_ball");
            }
        }

        private void setSprite(UISprite spr, string name)
        {
            spr.spriteName = name;
            spr.MakePixelPerfect();
        }


    }
}
