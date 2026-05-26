namespace WebConnector {
    [System.Obsolete("삭제됨")]
    public class LivePlayBuyTicketResult {
        /// <summary>
        /// 구매후 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 티켓 정보. [수량, 최종 충전후 지난시간(초)]
        /// </summary>
        public int[] ticketInfo { get; set; }
    }
}