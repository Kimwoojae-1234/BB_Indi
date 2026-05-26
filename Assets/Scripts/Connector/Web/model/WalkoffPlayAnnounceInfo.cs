namespace WebConnector
{
    public class WalkoffPlayAnnounceInfo
    {
        /// <summary>
        /// 보상 후 재화 잔액 array of [Ruby, Gold]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 랭킹
        /// </summary>
        public int weekRanking { get; set; }
        /// <summary>
        /// 주간랭킹 보상 루비
        /// </summary>
        public int weekRankingRuby { get; set; }
    }
}