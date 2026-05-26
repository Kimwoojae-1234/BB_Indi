namespace WebConnector {
    public class SeasonSchedule {
        /// <summary>
        /// 경기타입
        /// </summary>
        public SeasonGameType gameType { get; set; }
        /// <summary>
        /// 경기 회차 번호
        /// </summary>
        public int roundNo { get; set; }
        /// <summary>
        /// (사용되지 않는다면 삭제 필요)
        /// </summary>        
        public int gameNo { get; set; }
        public int homeTeamNo { get; set; }
        public int awayTeamNo { get; set; }
        public int homeScore { get; set; }
        public int awayScore { get; set; }
        public int homeSpCardId { get; set; }
        public int awaySpCardId { get; set; }
        public int mySpPw { get; set; }
        /// <summary>
        /// 경기진행 상태
        /// </summary>
        public SeasonScheduleState scheState { get; set; }
    }
}