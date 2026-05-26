namespace WebConnector {
    /// <summary>
    /// 우정포인트는 GameInfoBundle.balances[2] 값 참조
    /// </summary>
    public class FriendStoreInfo {
        /// <summary>
        /// 1회 가격
        /// </summary>
        public int price { get; set; }
        /// <summary>
        /// 5회 가격
        /// </summary>
        public int priceOfBunch { get; set; }
    }
}