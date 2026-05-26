namespace WebConnector {
    /// <summary>
    /// 시즌모드 팀기록 정보
    /// </summary>
    public class SeasonTeamRecordInfo {
        public int teamNo { get; set; }
        /// <summary>
        /// 팀명
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 선택구단
        /// </summary>
        public TeamCode team { get; set; }
        /// <summary>
        /// 팀 전력
        /// </summary>
        public int teamPw { get; set; }
        /// <summary>
        /// 승
        /// </summary>
        public int win { get; set; }
        /// <summary>
        /// 무승부
        /// </summary>
        public int draw { get; set; }
        /// <summary>
        /// 패
        /// </summary>
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
        /// 출루율
        /// </summary>
        public float obp { get; set; }
        /// <summary>
        /// 장타율
        /// </summary>
        public float sa { get; set; }
        /// <summary>
        /// OPS
        /// </summary>
        public float ops { get; set; }
        /// <summary>
        /// 연속 승무패값 (예 "3연승")
        /// </summary>
        public string straight { get; set; }
        /// <summary>
        /// 팀 현재 랭킹
        /// </summary>
        public int ranking { get; set; }
        /// <summary>
        /// 승차
        /// </summary>
        public float wd { get; set; }
    }
}