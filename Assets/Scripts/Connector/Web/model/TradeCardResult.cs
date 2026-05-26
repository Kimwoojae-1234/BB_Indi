namespace WebConnector {
    public class TradeCardResult {
        /// <summary>
        /// 첫번째 트레이드 인 경우는 null
        /// 재시도인 경우는 루비를 소모한 이후 총 재화 잔액
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 트레이드로 획득한 선수카드
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
    }
}