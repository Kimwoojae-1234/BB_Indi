using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIBattingCall : MonoBehaviour
    {
        public GameObject _active;
        
        public UISprite callSpr;
        //public UILabel ballType;
        public GameObject stateBG;
        public UILabel ballSpeed;
        public UILabel swingComment;


        private bool bPview;
        private string[] ballName = new string[25]
        {
            "Fastball",
            "Two-Seam",
            "Rising Fastball",

            "Curveball",
            "Power Curve",
            "Slow Curve",
            "Drop Curve",
            "Knuckle Curve",

            "Changeup",
            "Circle Change",
            "Vulcan Change",
            "Palmball",
            "Knuckleball",

            "Slider",
            "Hard Slider",
            "Slurve",
            "Cut Fastball",
            "Frisbee",

            "Forkball",
            "Sinker",
            "Splitter",
            "Hard Sinker",

            "Gyroball",
            "Gyroball",
            "Gyroball"
        };



        public void SetActive(bool bActive)
        {
            _active.SetActive(bActive);
        }


        public void Call(CALLTYPE _call, int type, int spd, Batter batter)
        {
            bPview = Mode.cameraView == CameraView.PitcherCenter ? true : false;

            if (_call == CALLTYPE.CALL_STRIKE)
            {
                callSpr.spriteName = "callsign_strike";
            }
            else if (_call == CALLTYPE.CALL_BALL)
            {
                callSpr.spriteName = "callsign_ball";
            }
            else if (_call == CALLTYPE.CALL_STRIKEOUT)
            {
                callSpr.spriteName = "callsign_strikeout";
            }
            else if (_call == CALLTYPE.CALL_FOURBALL)
            {
                callSpr.spriteName = "callsign_baseonball";
            }
            else if (_call == CALLTYPE.CALL_FOUL)
            {
                callSpr.spriteName = "callsign_foul";
            }
            else
            {
                callSpr.spriteName = "callsign_hitbypitch";
            }

            callSpr.MakePixelPerfect();
            
            if (type == 0) ballSpeed.text = "[FFEA00]Fastball[-]   " + spd + "km";
            else ballSpeed.text = "[FFEA00]" + ballName[type - 1] + "[-]   " + spd + "km";

            UISprite bg = stateBG.GetComponent<UISprite>();
            bg.spriteName = (bPview ? "call_bg_p" : "call_bg");
            bg.MakePixelPerfect();

            swingComment.gameObject.SetActive(!bPview);
            if (bPview == false)
            {                
                if (batter == null)
                {
                    swingComment.text = "Didn't Swing!";
                }
                else
                {
                    if (batter.bSwing == false) //timing == BattingTiming.NOSWING)
                    {
                        swingComment.text = "Didn't Swing!";
                    }
                    else
                    {
                        if (batter.contact == BattingContact.HUT_SWING)
                        {
                            swingComment.text = "Missed the Ball!";
                        }
                        else
                        {
                            if (batter.timing < BattingTiming.JUST_EARLY)
                            {
                                swingComment.text = "Too Early!";
                            }
                            else if (batter.timing > BattingTiming.JUST_LATE)
                            {
                                swingComment.text = "Too Late!";
                            }
                            else
                            {
                                swingComment.text = "Missed the Ball!";
                            }
                        }
                    }
                }
            }
            
            StartCoroutine(active());

        }

        private IEnumerator active()
        {
            _active.transform.localPosition = new Vector3(0, (bPview ? 22 : -160), 0);
            _active.SetActive(true);
            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("battingCallAnim"));

            yield return new WaitForSeconds(0.3f);

            UIWidget bg = stateBG.GetComponent<UIWidget>();
           
            bg.alpha = 1.0f;
            stateBG.SetActive(true);

            yield return new WaitForSeconds(1.3f);

            float alpha = 1.0f;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                alpha -= 0.1f;
                if (alpha < 0)
                {
                    stateBG.SetActive(false);
                    break;
                }
                bg.alpha = alpha;
            }

            yield return new WaitForSeconds(0.3f);
            _active.SetActive(false);
        }

    }
}