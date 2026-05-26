using System.Collections.Generic;

namespace WebConnector {
    [System.Obsolete("삭제됨")]
    public class SeasonRecordInfo {
        /// <summary>
        /// 팀경기수(모든팀 동일)
        /// </summary>
        public int numOfGames { get; set; }
        public List<SeasonRecordPitcher> pitchers { get; set; }
        public List<SeasonRecordHitter> hitters { get; set; }
    }
}