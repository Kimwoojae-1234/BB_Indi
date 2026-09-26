using BaseBall.BallPlay.UGUI;
using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIBattingCall : MonoBehaviour
    {
        public GameObject _active;
        
        public GameUIElement callSpr;
        //public GameUIElement ballType;
        public GameObject stateBG;
        public GameUIElement ballSpeed;
        public GameUIElement swingComment;


        private bool bPview;
        private string[] ballNameKeys = new string[25]
        {
            "UI.Label.Fastball",
            "UI.Label.TwoSeam",
            "UI.Label.RisingFastball",

            "UI.Label.Curveball",
            "UI.Label.PowerCurve",
            "UI.Label.SlowCurve",
            "UI.Label.DropCurve",
            "UI.Label.KnuckleCurve",

            "UI.Label.Changeup",
            "UI.Label.CircleChange",
            "UI.Label.VulcanChange",
            "UI.Label.Palmball",
            "UI.Label.Knuckleball",

            "UI.Label.Slider",
            "UI.Label.HardSlider",
            "UI.Label.Slurve",
            "UI.Label.CutFastball",
            "UI.Label.Frisbee",

            "UI.Label.Forkball",
            "UI.Label.Sinker",
            "UI.Label.Splitter",
            "UI.Label.HardSinker",

            "UI.Label.Gyroball",
            "UI.Label.Gyroball",
            "UI.Label.Gyroball"
        };
        private string[] ballName => L10n.Texts(ballNameKeys);



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
            
            if (type == 0) L10n.SetText(ballSpeed, "UI.Format.FastballValueKmH", spd);
            else L10n.SetText(ballSpeed, "UI.Format.ValueValueKmH", ballName[type - 1], spd);

            GameUIElement bg = stateBG.GetComponent<GameUIElement>();
            bg.spriteName = (bPview ? "call_bg_p" : "call_bg");
            bg.MakePixelPerfect();

            swingComment.gameObject.SetActive(!bPview);
            if (bPview == false)
            {                
                if (batter == null)
                {
                    L10n.SetText(swingComment, "UI.Label.DidntSwing");
                }
                else
                {
                    if (batter.bSwing == false) //timing == BattingTiming.NOSWING)
                    {
                        L10n.SetText(swingComment, "UI.Label.DidntSwing");
                    }
                    else
                    {
                        if (batter.contact == BattingContact.HUT_SWING)
                        {
                            L10n.SetText(swingComment, "UI.Label.MissedTheBall");
                        }
                        else
                        {
                            if (batter.timing < BattingTiming.JUST_EARLY)
                            {
                                L10n.SetText(swingComment, "UI.Label.TooEarly");
                            }
                            else if (batter.timing > BattingTiming.JUST_LATE)
                            {
                                L10n.SetText(swingComment, "UI.Label.TooLate");
                            }
                            else
                            {
                                L10n.SetText(swingComment, "UI.Label.MissedTheBall");
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

            GameUIElement bg = stateBG.GetComponent<GameUIElement>();
           
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