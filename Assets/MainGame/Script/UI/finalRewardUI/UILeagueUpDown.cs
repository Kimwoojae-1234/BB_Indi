using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UILeagueUpDown : MonoBehaviour
    {
        public FinalResultUI finalRewardMain; 
        public ResultUI resultUIMain;

        public GameObject _active;


        public GameObject leagueUP, leagueSame, leagueDown;
        public UISprite leagueLogo, leagueLogoUp, leagueLogoDown;
        public UILabel leagueRank;

        public Spine.Unity.SkeletonAnimation animUP, animDown, animSame;

        public GameObject next;

        //라이브매치에서의 승강여부
        private bool bLiveMatchUpdown;
        //현재 애님
        private Spine.Unity.SkeletonAnimation curAnim;
        //현재 랭크
        private int curRank;

        private int levelGab;


        private bool bPressNext;

        /// <summary>
        /// 시즌 승강 초기화
        /// </summary>
        public void InitSeasonLeagueUpDown()
        {
            next.SetActive(false);
            finalRewardMain.front.GetComponent<UIPanel>().alpha = 0;

            bLiveMatchUpdown = false;

            SeasonAnnounceInfo info = finalRewardMain.getLobbyInfo().annInfo;

            int lastLevel = info.newInfo[0];
            int curLevel = info.newInfo[1];

            if (info.rsReport != null)
            {
                curRank = info.rsReport.ranking;                  
            }
            else if (info.psReport != null)
            {
                curRank = info.psReport.ranking;
            }
            leagueRank.text = "0";

            StartCoroutine(init(setLeagueLevel(curLevel, lastLevel, true)));

        }

        /// <summary>
        /// 쟁탈 승강 초기화
        /// </summary>
        public void InitRaceLeagueUpDown()
        {
            next.SetActive(false);
            finalRewardMain.front.GetComponent<UIPanel>().alpha = 0;

            bLiveMatchUpdown = false;

            RacePlayLobbyInfo info = finalRewardMain.getRaceInfo();

            int lastLevel = info.newInfo[0];
            int curLevel = info.newInfo[1];

            curRank = info.curRank;
            leagueRank.text = "0";
            //leagueRank.text = string.Format("{0:n0}", info.curRank);

            StartCoroutine(init(setLeagueLevel(curLevel, lastLevel, true)));
        }


        /// <summary>
        /// 라이브 매치 승강 초기화
        /// </summary>
        public void InitLivematchUpDown(LivePlayGameEndInfo info)
        {
            next.SetActive(false);
            bLiveMatchUpdown = true;

            int lastLevel = info.chgLeagueLev[0];
            int curLevel = info.chgLeagueLev[1];

            curRank = info.ranking;
            leagueRank.text = "0";

            //leagueRank.text = string.Format("{0:n0}", info.curRank);

            StartCoroutine(init(setLeagueLevel(curLevel, lastLevel, false)));

            
        }

        /// <summary>
        /// 테스트용
        /// </summary>
        public void InitTest()
        {
            next.SetActive(false);
            finalRewardMain.front.GetComponent<UIPanel>().alpha = 0;

            bLiveMatchUpdown = false;

            
            int lastLevel = Random.Range(1,4);
            int curLevel = Random.Range(1, 4);

            curRank = 75454;

            leagueRank.text = "0";// string.Format("{0:n0}", curRank);

            StartCoroutine(init(setLeagueLevel(curLevel, lastLevel, true)));
        }


        private GameObject setLeagueLevel(int curLevel, int lastLevel, bool bLeagueLogo)
        { 
            if (bLeagueLogo == true)
            {
                //리그로고
                leagueLogo.spriteName = "league_" + lastLevel;
                
                if (curLevel > lastLevel)
                {
                    //승급레벨
                    leagueLogoUp.spriteName = "league_" + curLevel;
                }
                else if (curLevel < lastLevel)
                {
                    //강등레벨
                    leagueLogoDown.spriteName = "league_" + curLevel;
                }
            }
            else
            {
                //랭크마크
                leagueLogo.spriteName = "rankmark_" + curLevel;

            }

            levelGab = curLevel - lastLevel;

            if (curLevel > lastLevel)
            {
                //승격    
                animUP.gameObject.SetActive(true);
                curAnim = animUP;
                return leagueUP;// //leagueUP.SetActive(true);
            }
            else if (curLevel < lastLevel)
            {
                //강등
                animDown.gameObject.SetActive(true);
                curAnim = animDown;
                return leagueDown;//leagueDown.SetActive(true);
            }
            else
            {
                //유지
                animSame.gameObject.SetActive(true);
                curAnim = animSame;
                return leagueSame;// leagueSame.SetActive(true);
            }            

        }

        private IEnumerator init(GameObject leagueState)
        {
            gameObject.GetComponent<UIPanel>().alpha = 0;
            _active.SetActive(true);
            TweenAlpha.Begin(gameObject, 0.15f, 1);
            yield return new WaitForSeconds(0.1f);
            leagueState.SetActive(true);

            //yield return new WaitForSeconds(1.30f);
            yield return new WaitForSeconds(0.8f);
            StartCoroutine(countRank(curRank));
            yield return new WaitForSeconds(0.5f);
            leagueState.transform.Find("etc").gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);

            if (levelGab > 0)
            {
                //승격
                TweenPosition.Begin(leagueLogo.gameObject, 0.5f, new Vector3(0,200,0));
                UITweener tween = leagueLogoUp.gameObject.GetComponent<UITweener>();
                tween.enabled = true;
            }
            else if (levelGab < 0)
            {
                //강등
                TweenPosition.Begin(leagueLogo.gameObject, 0.5f, new Vector3(0, -200, 0));
                UITweener tween = leagueLogoDown.gameObject.GetComponent<UITweener>();
                tween.enabled = true;
            }
            
            yield return new WaitForSeconds(0.6f);
            curAnim.timeScale = 1;
            yield return new WaitForSeconds(0.2f);
            next.SetActive(true);
            bPressNext = false;
        }


        private IEnumerator countRank(int curRank)
        {
            int rank = 0;
            float gab = Mathf.Clamp(curRank / 5.0f, 1,  20);
            while (true)
            {
                leagueRank.text = string.Format("{0:N0}", (int)(rank));
                yield return new WaitForEndOfFrame();
                rank += (int)gab;
                gab *= 1.1f;
                if (rank > curRank)
                {
                    rank = curRank;
                    break;
                }
            }
            leagueRank.text = string.Format("{0:N0}", (int)(curRank));
        }


        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                ////Debug.Log("==========================>> next");
                StartCoroutine(deActive());

                if (bLiveMatchUpdown == true)
                {
                    resultUIMain.gotoOutGame();
                }
                else
                {
                    finalRewardMain.deActive();
                }
            }
        }

        private IEnumerator deActive()
        {
            //curAnim.GetComponent<MeshRenderer>().enabled = false;
            curAnim.gameObject.SetActive(false);
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            _active.SetActive(false);
        }
    }
}