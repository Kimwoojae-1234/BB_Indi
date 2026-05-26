using System.Collections.Generic;

namespace WebConnector
{
    public class StoreShelf
    {
        /// <summary>
        /// 상품 진열 상점 탭 (특가할인, 루비, 골드 등)
        /// </summary>
        public StoreType type { get; set; } //
        /// <summary>
        /// 상품 진열 레이아웃 타입 (상품 순서는 goods 에 해당 순서대로 담겨 잇음)
        /// </summary>
        public int layout { get; set; }
        /// <summary>
        /// 해당 탭내 상품 정보
        /// </summary>
        public List<Goods> goods { get; set; } //상품목록, 진열 순서로 정렬됨
    }
}