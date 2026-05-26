using System;

namespace WebConnector {
    /// <summary>
    /// 상점내 판매 상품
    /// </summary>
    public class Goods {
        [Obsolete("삭제됨")]
        public int storeId { get; set; }
        [Obsolete("삭제됨")]
        public int itemId { get; set; }
        /// <summary>
        /// 상품 아이디
        /// </summary>
        public int goodsId { get; set; }
        /// <summary>
        /// 상품 이름
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 구매 재화 타입
        /// </summary>
        public MoneyType moneyType { get; set; }
        /// <summary>
        /// 가격
        /// </summary>
        public int price { get; set; }
        /// <summary>
        /// 정액 상품 여부. 해당 상품이 정액 상품이면 true
        /// </summary>
        public bool commuter { get; set; }
        /// <summary>
        /// 이전에 정액 상품을 구매 했다면 해당 상품의 만료시각
        /// </summary>
        public DateTime commuterExpiry { get; set; }

        [Obsolete("삭제됨")]
        public int dcRate { get; set; }
        [Obsolete("삭제됨")]
        public StoreTag tag { get; set; }
        [Obsolete("삭제됨")]
        public int bgColor { get; set; }
        [Obsolete("삭제됨")]
        public int bonusMileage { get; set; }
        [Obsolete("삭제됨")]
        public int bonusGold { get; set; }
    }
}