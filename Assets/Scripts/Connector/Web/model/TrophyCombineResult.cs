using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 트로피 합성 결과
    /// </summary>
    public class TrophyCombineResult
    {
        /// <summary>
        /// 최종 보유한 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 트로피 사용후 최종 개수 map of {item_id : cnt}
        /// </summary>
        public Dictionary<int, int> items { get; set; }
    }
}