namespace WebConnector {
    /// <summary>
    /// 쿠폰 정보
    /// </summary>
    public class CouponInfo {
        /// <summary>
        /// 쿠폰 아이디 (템플릿 coupon 테이블)
        /// </summary>
        public int cpnId { get; set; }
        /// <summary>
        /// 보유 개수
        /// </summary>
        public int cnt { get; set; }
    }
}