using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class LivePlayCoinStoreInfo
    {
        /// <summary>
        /// 상점 상품 갱신 시간
        /// </summary>
        public DateTime expiryDate { get; set; }
        /// <summary>
        /// 재화 정보. array of [Ruby, Gold, FriendPoint, LvpCoin, DogamPoint], 변경사항 없으면 null
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 전시된 상품 목록 정보
        /// </summary>
        public List<LivePlayCoinStoreGoodsInfo> goodsList { get; set; }
    }
}