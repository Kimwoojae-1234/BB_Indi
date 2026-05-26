namespace WebConnector {
    public class FriendSendPointResult {
        /// <summary>
        /// 우정포인트 보유량 제한으로 우편함으로 보내진게 있다면 true
        /// </summary>
        public bool sentToPostbox { get; set; }
        /// <summary>
        /// 보내기후 우정포인트 잔액
        /// </summary>
        public int balanceFriendPoint { get; set; }
    }
}