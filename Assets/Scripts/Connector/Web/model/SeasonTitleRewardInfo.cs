namespace WebConnector {
    public class SeasonTitleRewardInfo {
        public GameCardInfo cardInfo { get; set; }
        /// <summary>
        /// 팀번호 1이면 내팀
        /// </summary>
        public int teamNo { get; set; }
        /// <summary>
        /// 보상 골드
        /// </summary>
        public int rwdGold { get; set; }
        /// <summary>
        /// 노출용 기록 정보
        /// </summary>
        public string dpRec { get; set; }
    }
}