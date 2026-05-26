using System;
using System.Collections.Generic;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayHistoryInfo {
        /// <summary>
        /// 공격 전적
        /// </summary>
        public List<RankedPlayHistory> hisOffense { get; set; }
        /// <summary>
        /// 방어 전적
        /// </summary>
        public List<RankedPlayHistory> hisDefence { get; set; }
    }
}