using BaseBall.BallPlay.UGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay
{
    public class UIWalkOffResult : MonoBehaviour
    {
        public GameObject _active;

        //보드
        public GameUIElement logo;
        public GameUIElement teamLabel;
        public Transform score;
        public GameUIElement totalScore;

        //
        public GameObject gauge;
        public GameUIElement gold;
        public GameUIElement rank;
        public GameUIElement percent;
        public GameUIElement finalScore;
        public GameUIElement finalRound;
        public GameUIElement goldPerRun;

        public GameObject effectObj;


        private int getGold;

        public void Init(BallPlayManager manager)
        {
            gauge.SetActive(false);

            Util.SetSpritePixelPerfect(logo, "logo_" + SimulPlayerManager.myTeamIndex);// //logo.spriteName = "logo_" + SimulPlayerManager.myTeamIndex;
            teamLabel.text = SimulPlayerManager.strMyTeam;
            totalScore.text = manager.nineTwoScore.ToString();
            int count = 0;
            foreach(Transform child in score)
            {
                GameUIElement roundScore = child.GetComponent<GameUIElement>();
                if (roundScore != null)
                {
                    if (manager.nineTwoRoundScore[count] >= 0)
                    {
                        roundScore.text = manager.nineTwoRoundScore[count].ToString();
                    }
                    count++;
                }
            }

#if _Test_Local

#else
            //WebConnector.WalkoffPlayGameInfo gameinfo = Mgrs.userData.walkoffInfo;
            WebConnector.WalkoffPlayEndInfo info = ResultUI.GetWalkoffEndInfo();

            //골드
            ////Debug.Log("=============>>이전 골드 : " + Mgrs.userData.GetUserHaveGold());
            ////Debug.Log("=============>>총 골드 : " + info.balances[(int)DefineEnum.ECurrency.Gold]);
            //획득골드
            getGold = (int)(info.rwdGold[1]);
            gold.text = string.Format("{0:N0}", 0);//info.rwdGold[1]);
            //랭크
            rank.text = string.Format("{0:N0}", info.curRank);
            //퍼센트
            float topRankPer = (float)(info.curRank * 100) / (float)(info.curRankSize);
            L10n.SetText(percent, "UI.Format.TopValue", string.Format("{0:F2}", topRankPer));
            //최종스코어
            L10n.SetText(finalScore, "UI.Format.ValuePoints.Lowercase", manager.nineTwoFinalScore);
            //최종라운드
            finalRound.text = manager.nineTwoFinalRound.ToString();

            //점수당 골드            
            goldPerRun.text = string.Format("{0:N0}", (int)(info.rwdGold[0]));
            //StartCoroutine(setGold((int)(info.rwdGold[0])));

            //재화 업데이트
            // DISABLED_MGRS: Mgrs.userData.SetUserBalances(info.balances);

#endif

            _active.SetActive(true);

        }


        public void deActive()
        {
            GameUITweenAlpha.Begin(gameObject, 0.5f, 0);
            Invoke("deactive", 0.6f);
        }

        private void deactive()
        {
            _active.SetActive(false);
        }



        public void setEffectStart(GameObject titleObj, GameObject leftObj, GameObject rightObj)
        {
            StartCoroutine(effectStart(titleObj, leftObj, rightObj));
        }


        private IEnumerator effectStart(GameObject titleObj, GameObject leftObj, GameObject rightObj)
        {
            yield return new WaitForSeconds(0.5f);
            effectObj.SetActive(true);
            if (leftObj.activeSelf) GameUITweenPosition.Begin(leftObj, 0.15f, new Vector3(-437, 0, 0));
            if (rightObj.activeSelf) GameUITweenPosition.Begin(rightObj, 0.15f, new Vector3(437, 0, 0));
            yield return new WaitForSeconds(0.15f);
            GameUITweenAlpha.Begin(effectObj.transform.Find("light1").gameObject, 0.2f, 0);
            GameUITweenAlpha.Begin(effectObj.transform.Find("light2").gameObject, 0.2f, 0);
            yield return new WaitForSeconds(0.15f);
            titleObj.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            StartCoroutine(setGold(getGold));
        }

        private IEnumerator setGold(int getgold)
        {
            int curGold = 0;
            float gab = 20;
            while (true)
            {
                gold.text = string.Format("{0:N0}", (int)(curGold));
                yield return new WaitForEndOfFrame();
                curGold += (int)gab;
                gab *= 1.1f;
                if (curGold > getgold)
                {
                    curGold = getgold;
                    break;
                }
            }
            gold.text = string.Format("{0:N0}", (int)(getgold));
        }

    }
}
