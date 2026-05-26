namespace WebConnector
{
    public class RacePlayBuyTicketResult
    {
        /// <summary>
        /// 구매후 재화 잔액(갱신용)
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 구매후 티켓 정보 [현재 티켓수, 남은 충전시간]
        /// </summary>
        public int[] ticketInfo { get; set; }
    }
}