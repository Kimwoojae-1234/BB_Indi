namespace WebConnector
{
    public class WalkoffPlayEndInfo
    {
        /// <summary>
        /// 보상을 받았다면 받은 후 최종 재화
        /// </summary>
        public int[] balances { get; set; }

        /// <summary>
        /// 시즌 순위
        /// </summary>
        public int curRank { get; set; }
        /// <summary>
        /// 시즌 순위 총 사이즈
        /// </summary>
        public int curRankSize { get; set; }
        /// <summary>
        /// 노출용 정보[득점당 골드, 최종 골드]
        /// </summary>
        public int[] rwdGold { get; set; }
    }
}