using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayGameInfo {
        /// <summary>
        /// 시즌 종료 시각
        /// </summary>
        public DateTime finishDate { get; set; }
        /// <summary>
        /// 홈팀 정보 (myTeam)
        /// </summary>
        public RankedPlayTeamInfo homeTeam { get; set; }
        /// <summary>
        /// 어웨이팀 정보 (awayTeam)
        /// </summary>
        public RankedPlayTeamInfo awayTeam { get; set; }
        /// <summary>
        /// 홈팀 선수 기록
        /// </summary>
        public RecordInfo homeRecInfo { get; set; }
        /// <summary>
        /// 어웨이팀 선수 기록. null이면 더미팀
        /// </summary>
        public RecordInfo awayRecInfo { get; set; }
    }
}