using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class LivePlayMileageRewardInfo {
        /// <summary>
        /// 갱신된 재화정보 (여기서는 라이브코인)
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 지급받은 후 마일리지 잔액
        /// </summary>
        [Obsolete("삭제됨")]
        public int mileage { get; set; }
        /// <summary>
        /// 지급받은 후 해당 아이템의 총 보유 갯수 (갱신정보)
        /// </summary>
        [Obsolete("삭제됨")]
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 보상받은 아이템 (노출정보)
        /// </summary>
        [Obsolete("삭제됨")]
        public Dictionary<int, int> rwdItems { get; set; }
    }
}
