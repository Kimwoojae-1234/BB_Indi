using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 상점 로비 정보
    /// </summary>
    public class StoreLobbyInfo {
        [Obsolete("삭제됨")]
        public List<Banner> banners { get; set; }
        [Obsolete("삭제됨")]
        public Dictionary<int, List<Goods>> goods { get; set; }
        [Obsolete("삭제됨")]
        public List<CouponInfo> coupons { get; set; }

        /// <summary>
        /// 탭별 상품 정보
        /// </summary>
        public List<StoreShelf> storeSelfs { get; set; }
        /// <summary>
        /// 골드 교환 정보
        /// </summary>
        public GoldExchangeInfo exchangeInfo { get; set; }
    }

    /// <summary>
    /// 상점 탭 종류
    /// </summary>
    public enum StoreType
    {
        /// <summary>
        /// 패키지
        /// </summary>
        Package,
        /// <summary>
        /// 한정 상품
        /// </summary>
        Limited,
        /// <summary>
        /// 루비탭
        /// </summary>
        Ruby,
        /// <summary>
        /// 골드탭
        /// </summary>
        Gold,
        /// <summary>
        /// 소모품탭
        /// </summary>
        Consumable,
        /// <summary>
        /// 입장권
        /// </summary>
        Ticket
    }
}