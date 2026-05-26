using System.Collections.Generic;

namespace WebConnector {
    public class TradeResult {
        /// <summary>
        /// 골드 잔액
        /// </summary>
        public int balanceGold { get; set; }
        /// <summary>
        /// 찬스가 발동된 경우 2개의 GameCardInfo가 있다. 이때 index 0 요소카드는 유저에게 이미 지급되어 있고, 1번째 요소는 임시 저장되어 있는 상태로 Pick API를 통해 교체할 수 있다.
        /// </summary>
        public List<GameCardInfo> cardInfos { get; set; }
    }
}