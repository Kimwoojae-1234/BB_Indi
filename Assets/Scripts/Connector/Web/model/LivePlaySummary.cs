using System;

namespace WebConnector {
    public class LivePlaySummary {
        public LivePlaySummary() { }
        public LivePlaySummary(DateTime closeDate, int leagueLev, int ranking) {
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
        /// <summary>
        /// 주간 보상 존재 여부
        /// </summary>
        public bool rwdWeekly { get; set; }

    }
}