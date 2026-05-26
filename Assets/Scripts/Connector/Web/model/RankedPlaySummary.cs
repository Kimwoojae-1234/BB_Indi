using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlaySummary {
        public RankedPlaySummary() { }
        public RankedPlaySummary(DateTime closeDate, int leagueLev, int ranking) {
            this.closeDate = closeDate;
            this.leagueLev = leagueLev;
            this.ranking = ranking;
        }
        /// <summary>
        /// 시즌종료 시각. 과거이면 시즌 종료됨. 미래이면 진행중
        /// </summary>
        public DateTime closeDate { get; set; }
        /// <summary>
        /// 리그
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 랭킹. 0이면 참여 안함.
        /// </summary>
        public int ranking { get; set; }

        [Obsolete("삭제됨")]
        public DateTime finishDate { get; set; }
        [Obsolete("삭제됨")]
        public int league { get; set; }
        [Obsolete("삭제됨")]
        public int lastChkRanking { get; set; }
        [Obsolete("삭제됨")]
        public int curRanking { get; set; }
        [Obsolete("삭제됨")]
        public int point { get; set; }
        [Obsolete("삭제됨")]
        public int[] totalWdl { get; set; }
        [Obsolete("삭제됨")]
        public int bestRec { get; set; }
    }
}