namespace WebConnector {
    public class LivePlayWeeklyRanking {
        public int leagueLev { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        /// <summary>
        /// 랭킹 점수
        /// </summary>
        public int point { get; set; }
    }
}