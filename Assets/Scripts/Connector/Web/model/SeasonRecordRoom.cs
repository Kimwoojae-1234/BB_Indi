using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 정규시즌 기록실
    /// </summary>
    public class SeasonRecordRoom {
        /// <summary>
        /// 팀경기수
        /// </summary>
        public int numOfGames { get; set; }
        public List<GameRecordPitcher> pitchers { get; set; }
        public List<GameRecordHitter> hitters { get; set; }
    }
}