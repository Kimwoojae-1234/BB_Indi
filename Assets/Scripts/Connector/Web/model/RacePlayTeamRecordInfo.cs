namespace WebConnector {
    public class RacePlayTeamRecordInfo {
        public int teamNo { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        public int win { get; set; }
        public int draw { get; set; }
        public int lose { get; set; }
        /// <summary>
        /// 득점
        /// </summary>
        public int gp { get; set; }
        /// <summary>
        /// 실점
        /// </summary>
        public int lp { get; set; }
        /// <summary>
        /// 홈런
        /// </summary>
        public int hr { get; set; }
        /// <summary>
        /// 도루
        /// </summary>
        public int sb { get; set; }
        /// <summary>
        /// 승률
        /// </summary>
        public float wr { get; set; }
        /// <summary>
        /// 타율
        /// </summary>
        public float ba { get; set; }
        /// <summary>
        /// 평균자책
        /// </summary>
        public float era { get; set; }
        /// <summary>
        /// OPS
        /// </summary>
        public float ops { get; set; }
        /// <summary>
        /// 랭킹
        /// </summary>
        public int ranking { get; set; }
        /// <summary>
        /// 승차
        /// </summary>
        public float wd { get; set; }
    }
}