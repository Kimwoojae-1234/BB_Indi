using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UIFinalStanding : MonoBehaviour
    {
        public FinalResultUI finalRewardMain;
        public GameObject _active;

        public GameObject season, race;
        public UILabel teamPowerLabel;
        public UISprite teamLogoSpr;

        public Transform[] column;

        public GameObject next;

        private int myRanking;

        private bool bPressNext = false;

        public void InitSeasonStanding()
        {
            SeasonLobbyInfo info = finalRewardMain.getLobbyInfo();
            next.SetActive(false);
            season.gameObject.SetActive(true);
            teamPowerLabel.text = string.Format("{0:n0}", finalRewardMain.getTeamPower());
            teamLogoSpr.spriteName = "league_" + info.leagueLev;
            teamLogoSpr.MakePixelPerfect();
            myRanking = info.annInfo.rsReport.ranking;
            for (int i = 0; i < 10; i++)
            {
                setColomnSeason(info, column[i], i);
            }

            StartCoroutine(init());
        }

        public void InitRaceStanding()
        {
            RacePlayLobbyInfo info = finalRewardMain.getRaceInfo();
            next.SetActive(false);
            race.gameObject.SetActive(true);
            teamPowerLabel.text = string.Format("{0:n0}", finalRewardMain.getTeamPower());
            teamLogoSpr.spriteName = "league_" + info.leagueLev;
            teamLogoSpr.MakePixelPerfect();
            myRanking = info.annInfo.lgRanking;

            if (info.annInfo.finalTeamRanks != null) //나중에 지워도되
            {
                for (int i = 0; i < 10; i++)
                {
                    setColomnRace(info, column[i], i);
                }
            }

            StartCoroutine(init());
        }


        private IEnumerator init()
        {            
            yield return new WaitForSeconds(0.5f);
            finalRewardMain.fadeIn();
            yield return new WaitForSeconds(0.2f);
            _active.SetActive(true);

            yield return new WaitForSeconds(0.6f);
            next.SetActive(true);
            bPressNext = false;
        }

        /// <summary>
        /// 시즌 랭킹 설정
        /// </summary>
        /// <param name="info"></param>
        /// <param name="col"></param>
        /// <param name="index"></param>
        private void setColomnSeason(SeasonLobbyInfo info, Transform col, int index)
        {
            bool bMyColumn = (myRanking == index + 1) ? true : false;
            SeasonTeamRecordInfo teamRecordInfo = info.annInfo.rsReport.teamRanking[index];
            SimpleTeamInfo teamInfo = info.teams[teamRecordInfo.teamNo];

            col.GetComponent<UISprite>().spriteName = bMyColumn ? "season_panentraceMyTeam_bg" : "season_panentraceTeam_bg";
            if (myRanking >= 4 && bMyColumn)
            {
                col.Find("rankLabel").GetComponent<UILabel>().color = Color.white;
            }

            //팀로고
            //col.FindChild("logo").GetComponent<UISprite>().spriteName = "logo_" + (int)teamInfo.team;         //팀로고
            Util.SetSpritePixelPerfect(col.Find("logo").GetComponent<UISprite>(), "logo_" + (int)teamInfo.team);//

            UILabel[] label = new UILabel[11];
            for (int i = 0; i < 11; i++)
            {
                label[i] = col.Find("label" + i).GetComponent<UILabel>();
                if (bMyColumn == true) label[i].color = Color.white;
            }


            label[0].text = teamInfo.name;  //팀이름
            label[1].text = teamRecordInfo.win.ToString();    //승
            label[2].text = teamRecordInfo.draw.ToString();    //무
            label[3].text = teamRecordInfo.lose.ToString();    //패
            label[4].text = string.Format("{0:F3}", teamRecordInfo.wr);  //승율
            label[5].text = string.Format("{0:F1}", teamRecordInfo.wd);  //승차
            label[6].text = string.Format("{0:F3}", teamRecordInfo.ba);  //타율
            label[7].text = teamRecordInfo.hr.ToString();    //홈런
            label[8].text = teamRecordInfo.sb.ToString();    //도루
            label[9].text = string.Format("{0:F3}", teamRecordInfo.ops);  //ops
            label[10].text = string.Format("{0:F2}", teamRecordInfo.era);  //자책
        }


        private void setColomnRace(RacePlayLobbyInfo info, Transform col, int index)
        {
            bool bMyColumn = (myRanking == index + 1) ? true : false;
            RacePlayTeamRecordInfo teamRecordInfo = info.annInfo.finalTeamRanks[index];

            col.GetComponent<UISprite>().spriteName = bMyColumn ? "season_panentraceMyTeam_bg" : "season_panentraceTeam_bg";
            if (myRanking >= 4 && bMyColumn)
            {
                col.Find("rankLabel").GetComponent<UILabel>().color = Color.white;
            }

            //팀로고
            //col.FindChild("logo").GetComponent<UISprite>().spriteName = "logo_" + (int)teamRecordInfo.team;         //팀로고
            Util.SetSpritePixelPerfect(col.Find("logo").GetComponent<UISprite>(), "logo_" + (int)teamRecordInfo.team);//

            UILabel[] label = new UILabel[11];
            for (int i = 0; i < 11; i++)
            {
                label[i] = col.Find("label" + i).GetComponent<UILabel>();
                if (bMyColumn == true) label[i].color = Color.white;
            }


            label[0].text = teamRecordInfo.name;  //팀이름
            label[1].text = teamRecordInfo.win.ToString();    //승
            label[2].text = teamRecordInfo.draw.ToString();    //무
            label[3].text = teamRecordInfo.lose.ToString();    //패
            label[4].text = string.Format("{0:F3}", teamRecordInfo.wr);  //승율
            label[5].text = teamRecordInfo.wd.ToString();    //승차
            label[6].text = string.Format("{0:F3}", teamRecordInfo.ba);  //타율
            label[7].text = teamRecordInfo.hr.ToString();    //홈런
            label[8].text = teamRecordInfo.sb.ToString();    //도루
            label[9].text = string.Format("{0:F3}", teamRecordInfo.ops);  //ops
            label[10].text = string.Format("{0:F2}", teamRecordInfo.era);  //자책
        }



        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                //Debug.Log("==========================>> next");
                StartCoroutine(deActive());
                if (finalRewardMain.curType == FinalResultUI.FinalRewardType.PenentEndPostSeason ||
                    finalRewardMain.curType == FinalResultUI.FinalRewardType.PenentEndSeasonEnd)
                {
                    finalRewardMain.finalReward.initSeasonReward();
                }
                else if (finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceDayAndWeekEnd ||
                         finalRewardMain.curType == FinalResultUI.FinalRewardType.RaceOnlyDayEnd)
                {
                    finalRewardMain.finalReward.initRaceReward();
                }
            }
        }

        private IEnumerator deActive()
        {
            finalRewardMain.changeScene();
            yield return new WaitForSeconds(0.5f);
            _active.SetActive(false);
        }
    }
}