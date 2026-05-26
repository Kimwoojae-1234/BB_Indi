using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 우편함 받기후 지급 결과
    /// </summary>
    public class PostRecvResult {
        /// <summary>
        /// 재화우편을 받았다면 총보유 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 플레이볼우편을 받았다면 플레이볼정보 [수량, 마지막 충전으로부터 지난초]
        /// </summary>
        public int[] playballInfo { get; set; }
        /// <summary>
        /// 아이템우편을 받았다면 해당 아이템의 총보유개수. map of {item_id : count}
        /// </summary>
        public Dictionary<int, int> items { get; set; }
    }
}