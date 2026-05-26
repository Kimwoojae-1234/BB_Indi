using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class scoreboard : MonoBehaviour
    {
        const int MAX_INNNG = 12;

        public GameObject [] teamObj;
        public GameObject cur;

        private UISprite[] logo = new UISprite[2];
        private UILabel[] teamName = new UILabel[2];
        private UILabel[,] score = new UILabel[2,MAX_INNNG];
        private UILabel[,] stat = new UILabel[2, 3];
        private GameObject[] indicator = new GameObject[2];

        
        private void init()
        {
            int count = 0;
            for (int i = 0; i < 2; i++)
            {
                logo[i] = teamObj[i].transform.Find("logo").GetComponent<UISprite>();
                teamName[i] = teamObj[i].transform.Find("teamLabel").GetComponent<UILabel>();
                count = 0;
                Transform scoreTrans = teamObj[i].transform.Find("score");
                foreach (Transform s in scoreTrans)
                {
                    UILabel label = s.GetComponent<UILabel>();
                    if (label != null)
                    {
                        score[i, count] = label;
                        count++;
                    }
                }
                count = 0;
                Transform statTrans = teamObj[i].transform.Find("stat");
                foreach (Transform s in statTrans)
                {
                    UILabel label = s.GetComponent<UILabel>();
                    if (label != null)
                    {
                        stat[i, count] = label;
                        count++;
                    }
                }

                indicator[i] = teamObj[i].transform.Find("indicator").gameObject;
            }
        }
        /// <summary>
        /// 보드를 초기화
        /// </summary>
        /// <param name="awayTeam"></param>
        /// <param name="homeTeam"></param>
        /// <param name="awayIndex"></param>
        /// <param name="homeIndex"></param>
        public void initScoreBoard(string awayTeam, string homeTeam, int awayIndex, int homeIndex)
        {
            init();
            for (int i = 0; i < 2; i++)
            {
                //스코어 초기화
                for (int j = 0; j < MAX_INNNG; j++)
                {
                    score[i, j].text = "0";
                    score[i, j].gameObject.SetActive(false);
                }
                //스탯 초기화
                for (int j = 0; j < 3; j++)
                {
                    stat[i, j].text = "0";
                }
            }

            Util.SetSpritePixelPerfect(logo[0], "logo_" + awayIndex);//logo[0].spriteName = "logo_" + awayIndex;  //
            Util.SetSpritePixelPerfect(logo[1], "logo_" + homeIndex);//logo[1].spriteName = "logo_" + homeIndex;  //

            teamName[0].text = awayTeam;
            teamName[1].text = homeTeam;

            score[0, 0].gameObject.SetActive(true);


            indicator[1].SetActive(false);

        }



        /// <summary>
        /// 플레이중인 상태를 설정
        /// </summary>
        /// <param name="curInning"></param>
        /// <param name="bTopInning"></param>
        /// <param name="awayScore"></param>
        /// <param name="homeScore"></param>
        /// <param name="awayStat"></param>
        /// <param name="homeStat"></param>
        public void setPlaying(int curInning, bool bTopInning, int[] awayScore, int[] homeScore, int[] awayStat, int[] homeStat)
        {
            int inning = Mathf.Clamp(curInning, 1, MAX_INNNG);
            for (int i = 0; i < 2; i++)
            {
                //스코어 초기화
                for (int j = 0; j < inning; j++)
                {
                    if (i == 0 || bTopInning == false)
                    {
                        string strScore = getScore((i == 0 ? awayScore[j] : homeScore[j]));
                        score[i, j].text = strScore;// (i == 0 ? awayScore[j] : homeScore[j]).ToString();
                        score[i, j].gameObject.SetActive(true);
                    }   
                }
                //스탯 초기화
                for (int j = 0; j < 3; j++)
                {
                    stat[i, j].text = (i == 0 ? awayStat[j] : homeStat[j]).ToString();
                }
            }

            cur.transform.position = score[bTopInning ? 0 : 1, (inning - 1)].transform.position;
            cur.transform.localPosition += new Vector3(-0.5f, 1, 0);

            indicator[0].SetActive(bTopInning);
            indicator[1].SetActive(!bTopInning);

        }//curInning


        public void boardActiveByCurrentInning(int curInning, bool bTopInning, int[] awayScore, int[] homeScore)
        {
            int inning = Mathf.Clamp(curInning, 1, MAX_INNNG);
            for (int i = 0; i < 2; i++)
            {
                //스코어 초기화
                for (int j = 0; j < inning; j++)
                {
                    if (j < inning - 1)
                    {
                        string strScore = getScore((i == 0 ? awayScore[j] : homeScore[j]));
                        score[i, j].text = strScore;
                        score[i, j].gameObject.SetActive(true);
                    }
                    else
                    {
                        string strScore = getScore((i == 0 ? awayScore[j] : homeScore[j]));
                        score[i, j].text = strScore;
                        if (i == 0 || bTopInning == false)
                        {
                            score[i, j].gameObject.SetActive(true);
                        }
                    }                    
                }                
            }
        }




        public void setResult(int[] awayScore, int [] homeScore, int[] awayStat, int [] homeStat, int myteamIndex)
        {
            for (int i = 0; i < 2; i++)
            {
                //스코어 
                for (int j = 0; j < MAX_INNNG; j++)
                {
                    string strScore = getScore((i == 0 ? awayScore[j] : homeScore[j]));
                    if (strScore != null)
                    {
                        score[i, j].text = strScore;
                        score[i, j].gameObject.SetActive(true);
                    }
                    else
                    {
                        score[i, j].gameObject.SetActive(false);
                    }
                }
                //스탯 
                for (int j = 0; j < 3; j++)
                {
                    stat[i, j].text = (i == 0 ? awayStat[j] : homeStat[j]).ToString();
                }
            }

            //임시
            cur.gameObject.SetActive(false);

            //인디케이터
            indicator[myteamIndex].SetActive(true);
            indicator[1 - myteamIndex].SetActive(false);
        }

        private string getScore(int score)
        {
            string strScore = "";
            if (score == SimulParm.NOPLAY_INNING) strScore = null;           //if (score == -2000) strScore = null;
            else if (score == SimulParm.GAMEEND_INNING) strScore = "X";      //else if (score == -1000) strScore = "X";
            else if (score < 0) strScore = (-score) + "X";
            else strScore = score.ToString();

            return strScore;
        }
    }
}