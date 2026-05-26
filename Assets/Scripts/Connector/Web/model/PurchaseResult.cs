using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 구매결과 정보
    /// </summary>
    public class PurchaseResult
    {
        public int[] balances { get; set; }
        /// <summary>
        /// null이 아니면 플레이볼 [수량, 마지막 충전으로부터 지난초]
        /// </summary>
        public int[] playballInfo { get; set; }
        /// <summary>
        /// null이 아니면 지급된 아이템의 총량
        /// </summary>
        public Dictionary<int, int> items { get; set; }

        //결과 Object
        private string _result;
        public string result { set { _result = value; } }

        /// <summary>
        /// 구입한 상품이 정액 상품이라면 해당 상품의 만료시각.
        /// 정액이 아니면 DateTime.MinValue
        /// </summary>
        public DateTime GetExpiryIfCommuterGoods {
            get {
                if (_result != null && _result.Trim() != "") {
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return dt.AddMilliseconds(Convert.ToDouble(_result.Trim())).ToLocalTime();
                }
                return DateTime.MinValue;
            }
        }
    }
}