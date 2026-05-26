using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class otherResultSetting : MonoBehaviour
    {
        public UITexture[] logo;
        public UILabel[] teamLabel;
        public UILabel[] rankLabel;
        public UILabel[] scoreLabel;
        public UISprite[] updown;
        public GameObject[] win;
        public GameObject[] lose;
        public UILabel[] pitcherLabel;
        public UILabel[] pitcherOverall;


        public void set(int curIndex, SeasonGameInfo info, SeasonGameEndInfo resultInfo, List<SeasonGameResult> gameResult, List<SeasonPitcherResult> pitcherResult)
        {
            int curSeq = gameResult[curIndex].scheNo;
            int[] teamNo = info.schedule[curSeq];

            //팀넘버
            int awayTeamNo = teamNo[1];
            int homeTeamNo = teamNo[0];
            int [] curTeamNo = new int[2]{awayTeamNo,homeTeamNo};

            //팀정보                
            SeasonTeamInfo awayTeam = info.teamInfos[awayTeamNo];
            SeasonTeamInfo homeTeam = info.teamInfos[homeTeamNo];
            SeasonTeamInfo[] curTeamInfo = new SeasonTeamInfo[2]{awayTeam,homeTeam};

            //투수 정보
            SeasonPitcherResult pResult = pitcherResult[curIndex];

            for (int i = 0; i < 2; i++)
            {
                //로고
                int index = (int)(curTeamInfo[i].team);
                // DISABLED_MGRS: logo[i].mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeMid((UserData.ETeamCode)(index))));
                
                //팀이름
                teamLabel[i].text = curTeamInfo[i].name;
                
                //팀랭크                
                int lastRank = info.teamInfos[curTeamNo[i]].ranking;
                int curRank = resultInfo.teamRankings[curTeamNo[i]];
                if (curRank < lastRank) updown[i].spriteName = "result_rankup";
                else if (curRank > lastRank) updown[i].spriteName = "result_rankdown";
                else updown[i].spriteName = "result_rankkeep";                
                updown[i].MakePixelPerfect();

                rankLabel[i].text = curRank + "위";

                //팀점수
                scoreLabel[i].text = (i == 0 ? gameResult[curIndex].awayScore : gameResult[curIndex].homeScore).ToString();
            }

            if (gameResult[curIndex].awayScore > gameResult[curIndex].homeScore)
            {
                //어웨이 이김
                win[0].SetActive(true);
                pitcherLabel[0].text = pResult.winPitcher;
                pitcherOverall[0].text = pResult.winPitcherOverall.ToString();
                lose[1].SetActive(true);
                pitcherLabel[1].text = pResult.losePitcher;
                pitcherOverall[1].text = pResult.losePitcherOverall.ToString();
            }
            else if (gameResult[curIndex].awayScore < gameResult[curIndex].homeScore)
            {
                //홈 이김
                lose[0].SetActive(true);
                pitcherLabel[0].text = pResult.losePitcher;
                pitcherOverall[0].text = pResult.losePitcherOverall.ToString();
                win[1].SetActive(true);
                pitcherLabel[1].text = pResult.winPitcher;
                pitcherOverall[1].text = pResult.winPitcherOverall.ToString();
            }
            else
            {
                //비김
                for(int i=0;i<2;i++)
                {
                    win[i].SetActive(false);
                    lose[i].SetActive(false);
                    pitcherLabel[i].gameObject.SetActive(false);
                    pitcherOverall[i].gameObject.SetActive(false);
                }
            }
        }



        
    }
}
