namespace WebConnector
{
    /// <summary>
    /// 9회말 2아웃 주간 랭킹 정보
    /// </summary>
    public class WalkoffPlayWeeklyRanking
    {
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        public int point { get; set; }
    }
}