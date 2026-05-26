namespace WebConnector {
    public class RemoveCardsResult {
        /// <summary>
        /// 재화 잔액
        /// MoneyUtils.GetMoneyFromBalances() 으로 각 재화별 잔액 확인됨
        /// </summary>
        public int[] balances { get; set; }
    }
}