using BaseBall.BallPlay.UGUI;
using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class Debug_UI : MonoBehaviour
    {
        static Debug_UI Instance_;

        public GameUIElement fpsLabel;

        public GameUIElement Round, Notice;

        public GameObject Loading;
        public GameObject Network;

        public GameUIElement [] pitcherProp;
        public GameUIElement [] batterProp;

        
        void Awake()
        {
            Instance_ = this;
        }

        void OnDestroy()
        {
            Instance_ = null;
        }


        float deltaTime = 0.0f;
        float curTime = 0;
        // Update is called once per frame
        void Update()
        {
            curTime += Time.deltaTime;
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            if (curTime > 1.0f)
            {
                L10n.SetText(fpsLabel, "UI.Format.FpsValue", ((int)(1.0f / deltaTime)).ToString()); // (1.0f / deltaTime).ToString() + " FPS";
                curTime = 0;
            }
        }


        public static void SetRound(bool bActive, int round = 0)
        {
            Instance_.Round.gameObject.SetActive(bActive);
            if (bActive == true)
            {
                L10n.SetText(Instance_.Round, "UI.Format.RoundValue", round);
            }
        }

        public static void SetLoading(bool bActive)
        {
            Instance_.Loading.gameObject.SetActive(bActive);
        }

        public static void SetNetwork(bool bActive)
        {
            Instance_.Network.gameObject.SetActive(bActive);
        }

        public static void SetNotice(bool bActive)
        {
            Instance_.Notice.gameObject.SetActive(bActive);
        }


        public static void SetPitcher(CPlayer pitcher)
        {
            Instance_.setPitcher(pitcher);
        }


        public static void SetBatter(CPlayer batter)
        {
            Instance_.setBatter(batter);
        }


        private void setPitcher(CPlayer pitcher)
        {
#if _Test_Local
            L10n.SetText(pitcherProp[0], "UI.Format.CurrentStaminaValue", pitcher.getCurrentStamina().ToString());
            L10n.SetText(pitcherProp[1], "UI.Format.ReductionRateValue", pitcher.staminaReduceRate.ToString());
            L10n.SetText(pitcherProp[2], "UI.Format.StaminaStateValue", pitcher.faitgueStep.ToString());
            L10n.SetText(pitcherProp[3], "UI.Format.InAPinchValue", pitcher.pinchState.ToString());
            L10n.SetText(pitcherProp[4], "UI.Format.PinchScoreValue", pitcher.pinchScore.ToString());

            //int guweeBouns = pitcher.pPitcher.getBallValue
            /*pitcherProp[0].text = "직구 : " + pitcher.getBallValue2(0).ToString();
            pitcherProp[1].text = "커브 : " + pitcher.getBallValue2(1).ToString();
            pitcherProp[2].text = "첸졉 : " + pitcher.getBallValue2(2).ToString();
            pitcherProp[3].text = "슬라 : " + pitcher.getBallValue2(3).ToString();
            pitcherProp[4].text = "포크 : " + pitcher.getBallValue2(4).ToString();
            pitcherProp[5].text = "보너스 : " + pitcher.getGuweeBonus().ToString();// + "중첩효과 " + pitcher.pileupValue;
            //pitcherProp[0].text = string.Format("직구 {0:+#;-#;+0} 커브 {1:+#;-#;+0} 제로인 경우 {2:+#;-#;0}  확률 {3:+#.#;-#.#;+0} 마이너스 {4:+#.#;-#.#;+0}", -1, 2, 0, 44.4445f, -123.532f);*/
#endif
        }
        


        private void setBatter(CPlayer batter)
        {
            L10n.SetText(batterProp[0], "UI.Format.ContactValue", batter.getContact());
            L10n.SetText(batterProp[1], "UI.Format.BattingEyeValue", batter.getEye());
            L10n.SetText(batterProp[2], "UI.Format.PowerValue", batter.getPower());
            L10n.SetText(batterProp[3], "UI.Format.LaunchAngleValue", batter.getTando());
            L10n.SetText(batterProp[4], "UI.Format.BonusValue", batter.getBonusValue());
        }

    }
}