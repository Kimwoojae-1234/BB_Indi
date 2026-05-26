using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class Debug_UI : MonoBehaviour
    {
        static Debug_UI Instance_;

        public UILabel fpsLabel;

        public UILabel Round, Notice;

        public GameObject Loading;
        public GameObject Network;

        public UILabel [] pitcherProp;
        public UILabel [] batterProp;

        
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
                fpsLabel.text = "FPS: " + ((int)(1.0f / deltaTime)).ToString(); // (1.0f / deltaTime).ToString() + " FPS";
                curTime = 0;
            }
        }


        public static void SetRound(bool bActive, int round = 0)
        {
            Instance_.Round.gameObject.SetActive(bActive);
            if (bActive == true)
            {
                Instance_.Round.text = "ROUND " + round;
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
            pitcherProp[0].text = "현재체력 : " + pitcher.getCurrentStamina().ToString();
            pitcherProp[1].text = "감소율 : " + pitcher.staminaReduceRate.ToString();
            pitcherProp[2].text = "체력상태 : " + pitcher.faitgueStep.ToString();
            pitcherProp[3].text = "핀치여부 : " + pitcher.pinchState.ToString();
            pitcherProp[4].text = "핀치점수 : " + pitcher.pinchScore.ToString();

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
            batterProp[0].text = "컨택 : " + batter.getContact();
            batterProp[1].text = "선구 : " + batter.getEye();
            batterProp[2].text = "파워 : " + batter.getPower();
            batterProp[3].text = "탄도 : " + batter.getTando();
            batterProp[4].text = "보너스 : " + batter.getBonusValue();
        }

    }
}