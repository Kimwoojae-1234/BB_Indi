using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 아이템 판매 결과
    /// </summary>
    public class ItemSellResult
    {
        /// <summary>
        /// 재화 잔액. array of [Ruby, Gold]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 판매한 아이템의 판매 후 총량
        /// </summary>
        public Dictionary<int, int> items { get; set; }
    }
}