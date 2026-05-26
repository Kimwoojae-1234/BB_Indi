using System.Collections.Generic;
using Utils;

namespace WebConnector
{
    public class RacePlayTeamInfo
    {
        public int teamNo { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        public int win { get; set; }
        public int draw { get; set; }
        public int lose { get; set; }
        /// <summary>
        /// 득점 (랭킹 계산용)
        /// </summary>
        public int gp { get; set; }
        /// <summary>
        /// 실점 (랭킹 계산용)
        /// </summary>
        public int lp { get; set; }
        public int ranking { get; set; }
        /// <summary>
        /// 승차
        /// </summary>
        public float wd { get; set; }

        [System.Obsolete("삭제됨")]
        public int[] wdl { get; set; }

        [System.Obsolete("삭제됨")]
        public List<GameCardInfo> lineup { get; set; }
    }

    /// <summary>
    /// 쟁탈전 팀정보에 대한 정보 갱신 및 랭킹 처리 객체
    /// </summary>
    public class RacePlayTeamInfoManager {
        private Dictionary<int, RacePlayTeamInfo> teamInfoMap = new Dictionary<int, RacePlayTeamInfo>();
        private List<RacePlayTeamInfo> teamInfos = null;

        public RacePlayTeamInfoManager(List<RacePlayTeamInfo> teamInfos) {
            this.teamInfos = teamInfos;

            foreach (RacePlayTeamInfo info in teamInfos) {
                teamInfoMap[info.teamNo] = info;
            }
            reorder();
        }

        /// <summary>
        /// 갱신 정보를 기존정보에 합친후 랭킹및 승차값을 재계산 한다.
        /// </summary>
        public void MergeTeamInfos(List<RacePlayTeamInfo> teamUdtInfos) {
            if (teamUdtInfos != null) {
                foreach (RacePlayTeamInfo info in teamUdtInfos) {
                    RacePlayTeamInfo oriInfo = null;

                    if (teamInfoMap.TryGetValue(info.teamNo, out oriInfo)) {
                        oriInfo.win += info.win;
                        oriInfo.draw += info.draw;
                        oriInfo.lose += info.lose;
                        oriInfo.gp += info.gp;
                        oriInfo.lp += info.lp;
                    }
                }

                reorder();
            }
        }

        public RacePlayTeamInfo GetTeamInfo(int teamNo) {
            return teamInfoMap[teamNo];
        }

        //랭킹 재계산
        private void reorder() {
            teamInfos.Sort((r1, r2) => {
                float r1Wr = RecordUtils.calWinRate(r1.win, r1.lose);
                float r2Wr = RecordUtils.calWinRate(r2.win, r2.lose);

                int cmp = r2Wr.CompareTo(r1Wr);

                if (cmp == 0) { //2차 승수
                    cmp = r2.win.CompareTo(r1.win);
                }

                if (cmp == 0) { //3차 득실
                    cmp = (r2.gp - r2.lp).CompareTo((r1.gp - r1.lp));
                }

                return cmp;
            });

            int nextRanking = 1;
            int curRanking = 1;
            float stdWr = 0f;

            //랭킹 세팅 및 승차계산
            int stWin = -1, stLose = -1;
            foreach (RacePlayTeamInfo info in teamInfos) {
                //랭킹세팅
                float wr = RecordUtils.calWinRate(info.win, info.lose);
                if (wr == stdWr) {
                } else {
                    curRanking = nextRanking;
                }
                info.ranking = curRanking;
                stdWr = wr;
                nextRanking++;

                //승차세팅
                if (stWin == -1 && stLose == -1) {
                    stWin = info.win;
                    stLose = info.lose;
                    info.wd = 0f;
                } else {
                    info.wd = RecordUtils.calDifferenceOfWin(stWin, stLose, info.win, info.lose);
                }
            }
        }

        /// <summary>
        /// 현재 정보를 랭킹순으로 가져온다.
        /// </summary>
        /// <returns></returns>
        public List<RacePlayTeamInfo> GetTeamInfosOrderByRanking() {
            return teamInfos;
        }
    }
}