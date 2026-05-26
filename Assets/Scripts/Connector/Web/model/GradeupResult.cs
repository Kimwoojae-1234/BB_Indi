using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 선수 승급 후 결과
    /// </summary>
    public class GradeupResult
    {
        /// <summary>
        /// 재화 잔액. array of [Ruby, Gold]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 승급 후 cardInfo
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
        /// <summary>
        /// 승급 후 itemInfo (훈련용 선수)
        /// 사용후 남은 아이템 [아이템ID, 남은수량]
        /// 사용한 아이템이 없으면 null
        /// </summary>
        public Dictionary<int, int> items { get; set; }
    }
}