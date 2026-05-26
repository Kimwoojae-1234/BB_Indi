namespace WebConnector
{
    /// <summary>
    /// 주간 랭킹 정보
    /// </summary>
    public class RacePlayWeeklyRanking
    {
        public int leagueLev { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        public int point { get; set; }
    }
}