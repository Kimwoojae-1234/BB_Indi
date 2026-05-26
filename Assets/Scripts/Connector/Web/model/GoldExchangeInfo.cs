using System;

namespace WebConnector {
    public class GoldExchangeInfo {
        /// <summary>
        /// null이 아니면 골드 교환 후 재화 잔액
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 필요 골드
        /// </summary>
        public int gold { get; set; }
        /// <summary>
        /// 획득하게 되는 루비
        /// </summary>
        public int ruby { get; set; }
        /// <summary>
        /// 다음 교환 가능 시각
        /// </summary>
        public DateTime releaseDate { get; set; }
    }
}