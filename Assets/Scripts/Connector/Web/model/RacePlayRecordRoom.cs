using System.Collections.Generic;

namespace WebConnector {
    public class RacePlayRecordRoom {
        /// <summary>
        /// 팀기록
        /// </summary>
        public List<RacePlayTeamRecordInfo> teamRecords { get; set; }
        /// <summary>
        /// 투수기록
        /// </summary>
        public List<GameRecordPitcher> pitchers { get; set; }
        /// <summary>
        /// 타자기록
        /// </summary>
        public List<GameRecordHitter> hitters { get; set; }
    }
}