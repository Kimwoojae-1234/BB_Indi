//#define _TEST_STATE

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIScoreShow : MonoBehaviour
    {
        public GameObject _active;
        public UILabel curInning;
        public UITexture myLogo, cpuLogo;
        public UILabel myTeamName, cpuTeamName;

        public spriteNumber myScore, cpuScore;
        public spriteNumber myScoreAlpha, cpuScoreAlpha;

        public UISprite[] myTeam;
        public UISprite inningSpr;

        public UIPanel[] scorePanel;
        public UIPanel[] scoreAlphaPanel;

        public UITexture[] light;

        private BallPlayManager manager;
        
        public void SetInit(BallPlayManager _manager)
        {
            if (_manager == null)
            {
                manager = _manager;
            }
            init(_manager);
        }
        
        public void DeActive()
        {
            StopAllCoroutines();
            _active.SetActive(false);
        }


        private void init(BallPlayManager _manager)
        {            
#if _TEST_STATE
            // DISABLED_MGRS: myLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(1))));
            // DISABLED_MGRS: cpuLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(2))));

            bool bHome = false;
            myTeam[0].enabled = !bHome;
            myTeam[1].enabled = bHome;
#else

#if _Test_Local
            // DISABLED_MGRS: myLogo.mainTexture = Util.loadBigLogo(SimulPlayerManager.myTeamIndex);// Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.myTeamIndex))));
            // DISABLED_MGRS: cpuLogo.mainTexture = Util.loadBigLogo(SimulPlayerManager.cpuTeamIndex);// Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.cpuTeamIndex))));
#else
            // DISABLED_MGRS: myLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.myTeamIndex))));
            // DISABLED_MGRS: cpuLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(SimulPlayerManager.cpuTeamIndex))));
#endif
            myTeam[0].enabled = true;// !_manager.bMyHome;
            myTeam[1].enabled = false;// _manager.bMyHome;
#endif

            myScore.init("score_", 0, 24);
            cpuScore.init("score_", 0, 24);
            myScoreAlpha.init("score_", 0, 24);
            cpuScoreAlpha.init("score_", 0, 24);

#if _TEST_STATE
            myTeamName.text = "병신이글스";
            cpuTeamName.text = "세이콘한화";
#else
            myTeamName.text = SimulPlayerManager.strMyTeam;// .strAwayTeam;// .strMyTeam;
            cpuTeamName.text = SimulPlayerManager.strCPUTeam;// .strHomeTeam;// .strCPUTeam;
#endif
            _active.SetActive(false);
        }


        public void ShowBoard(BallPlayManager manager, int team, int addScore)
        {
#if _TEST_STATE
            //curInning.text = manager.nInningCount.ToString();// +"" + (manager.bTopInning ? "회초" : "회말");
            //inningSpr.spriteName = (manager.bTopInning ? "inning_first" : "inning_bottom");

            bool bHome = false;
            bool bTopInning = false;
            int awayIndex = (bHome ? 1 : 0);
            int curIndex = bTopInning ? 0 : 1;

            int [] nGameScore = new int[2]{20,3};

            for (int i = 0; i < 2; i++)
            {
                scorePanel[i].alpha = 0;
                scorePanel[i].transform.localScale = Vector3.one;
                scoreAlphaPanel[i].alpha = 0;
                scoreAlphaPanel[i].transform.localScale = Vector3.one;
                TweenAlpha a = scorePanel[i].GetComponent<TweenAlpha>();
                TweenScale b = scorePanel[i].GetComponent<TweenScale>();
                TweenAlpha c = scoreAlphaPanel[i].GetComponent<TweenAlpha>();
                TweenScale d = scoreAlphaPanel[i].GetComponent<TweenScale>();
                if (a != null) Destroy(a);
                if (b != null) Destroy(b);
                if (c != null) Destroy(c);
                if (d != null) Destroy(d);
            }
            myScore.set(nGameScore[awayIndex] - (bTopInning ? addScore:0));
            cpuScore.set(nGameScore[1 - awayIndex] - (bTopInning ? 0: addScore));

            myScoreAlpha.set(nGameScore[awayIndex]);
            cpuScoreAlpha.set(nGameScore[1 - awayIndex]);

            light[0].enabled = bTopInning;
            light[1].enabled = !bTopInning;
#else
            curInning.text = manager.nInningCount.ToString();// +"" + (manager.bTopInning ? "회초" : "회말");
            inningSpr.spriteName = (manager.bTopInning ? "scoreboard_top" : "scoreboard_bottom");


            //int awayIndex = 0;// (manager.bMyHome ? 1 : 0);
            int curIndex = manager.bMyTurn ? 0 : 1;

            for (int i = 0; i < 2; i++)
            {
                scorePanel[i].alpha = 0;
                scorePanel[i].transform.localScale = Vector3.one;
                scoreAlphaPanel[i].alpha = 0;
                scoreAlphaPanel[i].transform.localScale = Vector3.one;

                TweenAlpha a = scorePanel[i].GetComponent<TweenAlpha>();
                TweenScale b = scorePanel[i].GetComponent<TweenScale>();
                TweenAlpha c = scoreAlphaPanel[i].GetComponent<TweenAlpha>();
                TweenScale d = scoreAlphaPanel[i].GetComponent<TweenScale>();
                if (a != null) Destroy(a);
                if (b != null) Destroy(b);
                if (c != null) Destroy(c);
                if (d != null) Destroy(d);
            }

            myScore.set(manager.nGameScore[0] - (manager.bMyTurn ? addScore : 0));
            cpuScore.set(manager.nGameScore[1] - (manager.bMyTurn ? 0 : addScore));

            myScoreAlpha.set(manager.nGameScore[0]);
            cpuScoreAlpha.set(manager.nGameScore[1]);

            light[0].enabled = manager.bMyTurn;
            light[1].enabled = !manager.bMyTurn;
#endif

            _active.SetActive(true);

            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("scoreShowAnim"));


            StartCoroutine(active(curIndex));
        }

        //public float delayTime = 0.5f;
        //public float alphaTime = 0.2f;
        //public float alphaTime2 = 0.3f;

        private IEnumerator active(int curIndex)
        {
            yield return new WaitForSeconds(0.4f);
            for (int i = 0; i < 2; i++)
            {
                TweenAlpha.Begin(scorePanel[i].gameObject, 0.3f, 1);
            }

            yield return new WaitForSeconds(0.5f);

            scoreAlphaPanel[curIndex].alpha = 1;
            scoreAlphaPanel[curIndex].transform.localScale = new Vector3(2, 2);
            TweenScale.Begin(scoreAlphaPanel[curIndex].gameObject, 0.2f, Vector3.one);
            TweenAlpha.Begin(scorePanel[curIndex].gameObject, 0.2f, 0);

            yield return new WaitForSeconds(0.2f);
#if _TEST_STATE
            bool bTopInning = true;
            if (bTopInning == true) myScore.set(20);
            else cpuScore.set(20);
#else
            //myScore.set(manager.nGameScore[awayIndex]);
            //cpuScore.set(manager.nGameScore[1 - awayIndex]);
#endif
            scorePanel[curIndex].alpha = 0.7f;
            TweenAlpha.Begin(scorePanel[curIndex].gameObject, 0.3f, 0);
            TweenScale.Begin(scorePanel[curIndex].gameObject, 0.3f, new Vector3(1.5f, 1.5f, 1));
            
            

            yield return new WaitForSeconds(4.0f);
            _active.SetActive(false);
        }

    }
}
