using System;
using System.Collections.Generic;

namespace WebConnector
{
    [Obsolete("삭제됨")]
    public class RankedPlaySeasonResult {
        /// <summary>
        /// 지난시즌 포인트
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 지난시즌 랭킹
        /// </summary>
        public int rank { get; set; }
        /// <summary>
        /// 지난시즌 리그에 따른 보상(루비)
        /// </summary>
        public int rwdLeagueRuby { get; set; }
        /// <summary>
        /// 지난시즌 리그에 따른 보상(코인)
        /// </summary>
        public int rwdLeagueCoin { get; set; }
        /// <summary>
        /// 지난시즌 순위에 따른 보상(아이템). null이면 보상없음. array of [item_id, item_cnt]
        /// </summary>
        public Dictionary<int, int> rwdItem { get; set; }
        /// <summary>
        /// 지난시즌 순위에 따른 보상(루비)
        /// </summary>
        public int rwdRankRuby { get; set; }
        /// <summary>
        /// 업데이트용 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 업데이트용 아이템
        /// </summary>
        public Dictionary<int, int> stocks { get; set; }
    }
}