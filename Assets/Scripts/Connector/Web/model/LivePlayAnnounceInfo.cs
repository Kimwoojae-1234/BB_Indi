using System.Collections.Generic;

namespace WebConnector {
    public class LivePlayAnnounceInfo {
        /// <summary>
        /// 총 재화 (클라 갱신용)
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 지급된 아이템의 총 보유량. (클라 갱신용)
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 지난시즌 포인트
        /// </summary>
        public int finalPoint { get; set; }
        /// <summary>
        /// 지난시즌 리그랭킹
        /// </summary>
        public int finalRank { get; set; }
        /// <summary>
        /// 최종 리그에 따른 루비 보상
        /// </summary>
        public int weekLeagueRuby { get; set; }
        /// <summary>
        /// 최종 리그에 따른 코인 보상
        /// </summary>
        public int weekLeagueCoin { get; set; }
        /// <summary>
        /// 최종 리그에  따른 아이템 보상. map of <item_id, item_cnt>
        /// </summary>
        public Dictionary<int, int> weekLeagueItem { get; set; }
        /// <summary>
        /// 최종 순위에 따른 루비 보상
        /// </summary>
        public int rwdRankRuby { get; set; }
    }
}