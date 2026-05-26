namespace WebConnector {
    /// <summary>
    /// 상점내 배너 정보
    /// </summary>
    public class Banner {
        /// <summary>
        /// 배너 이미지 url
        /// </summary>
        public string imgUrl { get; set; }
        /// <summary>
        /// 해당 배너에 대한 상점 ID
        /// </summary>
        public int storeId { get; set; }
        /// <summary>
        /// 해당 배너의 아이템 ID
        /// </summary>
        public int itemId { get; set; }
    }
}