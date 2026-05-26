using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class postScheduleInfo : MonoBehaviour
    {
        public GameObject[] box;

        public UISprite[] leftLine;
        public UISprite[] rightLine;

        public UITexture challergerLogo;

        /// <summary>
        /// 박스스코어 설정
        /// </summary>
        /// <param name="index"></param>
        /// <param name="score1"></param>
        /// <param name="score2"></param>
        public void setBoxScore(int index, int score1, int score2)
        {
            Transform curBox = box[index].transform.Find("done");
            bool bLeftWin = (score1 > score2);
            UILabel away = curBox.Find("away").GetComponent<UILabel>();
            UILabel home = curBox.Find("home").GetComponent<UILabel>();

            away.text = score1.ToString();
            home.text = score2.ToString();

            Color winColor = new Color(255/255.0f,226/255.0f,35/255.0f);
            Color loseColor = new Color(64/255.0f,85/255.0f,127/255.0f);
            away.color = bLeftWin ? winColor : loseColor;
            home.color = bLeftWin ? loseColor : winColor;
        }

        public void setNoScore(int index)
        {
            box[index].SetActive(false);
        }

        /// <summary>
        /// 도전자 로고 설정
        /// </summary>
        /// <param name="code"></param>
        public void setChallengerTeam(WebConnector.TeamCode code)
        {
            challergerLogo.gameObject.SetActive(true);
            // DISABLED_MGRS: challergerLogo.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(code))));            
        }

        /// <summary>
        /// 나의 포스트 시즌 경기 상태 설정
        /// </summary>
        /// <param name="bChallengerWin"></param>
        public void setMyGameState(bool bMyTeamChallenger)
        {
            GetComponent<UISprite>().spriteName = "postseason_title_2";
            if (bMyTeamChallenger)
            {
                //내팀이 도전자인경우
                for (int i = 0; i < 2; i++)
                    leftLine[i].spriteName = "postseason_line_green";
            }
            else
            {
                for (int i = 0; i < 2; i++)
                    rightLine[i].spriteName = "postseason_line_green";
            }
        }


    }

}
