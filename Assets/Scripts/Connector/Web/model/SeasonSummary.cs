using System;
using System.Collections.Generic;

namespace WebConnector {

    public class SeasonSummary {
        public SeasonSummary() { }
        public SeasonSummary(int leagueLev, int roundNo, int ranking)
        {
            this.leagueLev = leagueLev;
            this.roundNo = roundNo;
            this.ranking = ranking;
        }
        /// <summary>
        /// 리그레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 라운드 번호
        /// </summary>
        public int roundNo { get; set; }
        /// <summary>
        /// 시즌모드 순위. (시즌경기진행중이 아니면 0)
        /// </summary>
        public int ranking { get; set; }
    }
}