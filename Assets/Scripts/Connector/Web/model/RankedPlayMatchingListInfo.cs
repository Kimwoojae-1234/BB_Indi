using System;
using System.Collections.Generic;

namespace WebConnector
{
    [Obsolete("삭제됨")]
    public class RankedPlayMatchingListInfo {
        /// <summary>
        /// 매칭 리스트
        /// </summary>
        public List<RankedPlayTeam> matchList { get; set; }
        /// <summary>
        /// 매칭리스트 최근 갱신 시각        
        /// </summary>
        public DateTime latestListRefreshDate { get; set; }

        public int[] balances { get; set; }
    }
}
