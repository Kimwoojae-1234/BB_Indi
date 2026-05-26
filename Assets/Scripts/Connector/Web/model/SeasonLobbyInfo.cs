using System;
using System.Collections.Generic;

namespace WebConnector {
    public class SeasonLobbyInfo {
        [Obsolete("삭제됨. SeasonAnnounceInfo.newInfo 참조")]
        public int[] newInfo { get; set; }
        /// <summary>
        /// 리그레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 선발 투수 odr 값
        /// </summary>
        public int starterOdr { get; set; }
        /// <summary>
        /// 모든팀 팀기록 정보
        /// </summary>
        public List<SeasonTeamRecordInfo> teamRecords { get; set; }
        /// <summary>
        /// 내팀이 홈팀이면 true
        /// </summary>
        public bool home { get; set; }
        /// <summary>
        /// 현재 정규시즌이면 정규시즌 스케줄, 포스트시즌이면 포스트시즌 스케줄
        /// </summary>
        public List<SeasonSchedule> schedules { get; set; }
        /// <summary>
        /// 현재 진행 경기 회차. schedules 에서 roundNo로 현재 경기 스케줄 정보를 얻어온다.
        /// </summary>        
        public int roundNo { get; set; }

        public int seasonNo { get; set; }

        /// <summary>
        /// 각 경기별 번호. (roundNo로 대체가능한지 확인하여 삭제할지 여부 결정 필요)
        /// </summary>
        public int gameNo { get; set; }
        /// <summary>
        /// 홈팀 정보
        /// </summary>
        public TeamInfo homeInfo { get; set; }
        /// <summary>
        /// 원정팀 정보
        /// </summary>
	    public TeamInfo awayInfo { get; set; }
        /// <summary>
        /// 상대팀과의 상대전적. array of [승,무,패]
        /// </summary>
        public int[] h2hWdl { get; set; }
        /// <summary>
        /// null이 아니면 알림 정보 (새시즌 시작, 정규시즌 종료, 포스트시즌 종료)
        /// </summary>
        public SeasonAnnounceInfo annInfo { get; set; }
        public Dictionary<int, List<int[]>> otherTeamSpInfos { get; set; }        

        public Dictionary<int, SimpleTeamInfo> _teams = null;
        /// <summary>
        /// 모든 팀 정보. map of {teamNo, TeamInfo}
        /// </summary>
        [Obsolete("팀정보(팀명,구단코드,전력) 을 SeasonTeamRecordInfo 에 포함하도록 수정했습니다. teams를 참조하지 말고 TeamInfosByNo를 사용해 주세요.")]
        public Dictionary<int, SimpleTeamInfo> teams {
            get {
                if (_teams == null) {
                    _teams = new Dictionary<int, SimpleTeamInfo>();

                    foreach (SeasonTeamRecordInfo info in teamRecords) {
                        SimpleTeamInfo teamInfo = new SimpleTeamInfo();
                        teamInfo.name = info.name;
                        teamInfo.team = info.team;
                        teamInfo.teamPw = info.teamPw;
                        _teams[info.teamNo] = teamInfo;
                    }
                }
                return _teams;
            }
        }

        public Dictionary<int, SeasonTeamRecordInfo> _teamInfosByNo = null;
        /// <summary>
        /// 팀별 정보. map of {teamNo, SeasonTeamRecordInfo}
        /// </summary>
        public Dictionary<int, SeasonTeamRecordInfo> TeamInfosByNo {
            get {
                if (_teamInfosByNo == null) {
                    _teamInfosByNo = new Dictionary<int, SeasonTeamRecordInfo>();

                    foreach (SeasonTeamRecordInfo info in teamRecords) {
                        _teamInfosByNo[info.teamNo] = info;
                    }
                }
                return _teamInfosByNo;
            }
        }

        /// <summary>
        /// 요약 정보
        /// </summary>
        public SeasonSummary SspSum
        {
            get
            {
                int ranking = 0;
                foreach (SeasonTeamRecordInfo recInfo in teamRecords)
                {
                    if (recInfo.teamNo == BHConst.myTeamNo)
                    {
                        ranking = recInfo.ranking;
                        break;
                    }
                }
                return new SeasonSummary(leagueLev, roundNo, ranking);
            }
        }
    }
}