using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class LivePlayCoinStoreGoodsInfo
    {
        
        /// <summary>
        /// 상품 가격
        /// </summary>
        public int price { get; set; }
        /// <summary>
        /// 상품 판매 여부
        /// </summary>
        public bool isSoldOut { get; set; }
    }
}