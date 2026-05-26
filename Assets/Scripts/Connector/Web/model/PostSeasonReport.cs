using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 포스트시즌 종료 결과 정산 (화면 노출용)
    /// </summary>
    public class PostSeasonReport {
        /// <summary>
        /// 최종순위
        /// </summary>
        public int ranking { get; set; }
        /// <summary>
        /// 보상 골드
        /// </summary>
        public int gold { get; set; }

        [Obsolete("삭제됨 rwdItems로 대체")]
        public List<int> itemIds { get; set; }
        /// <summary>
        /// 지급된 아이템 {itemId, cnt}
        /// </summary>
        public Dictionary<int, int> rwdItems { get; set; }
        /// <summary>
        /// 포스트시즌 종료 후 최종 대진표를 보여주기 위한 스케줄 정보
        /// </summary>
        public List<SeasonSchedule> sches { get; set; }
    }
}