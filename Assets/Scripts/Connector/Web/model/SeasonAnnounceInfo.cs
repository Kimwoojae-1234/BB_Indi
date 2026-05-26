using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 시즌스케줄 진행중 알림 정보
    /// </summary>
    public class SeasonAnnounceInfo {
        /// <summary>
        /// null이 아니면 총 보유 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// null이 아니면 지급받은 아이템의 총 보유량
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// null이 아니면 새시즌 시작 정보. [이전시즌 리그레벨, 이번시즌 리그레벨]
        /// </summary>
        public int[] newInfo { get; set; }
        /// <summary>
        /// null이 아니면 정규시즌 종료 결과 리포트
        /// </summary>
        public RegularSeasonReport rsReport { get; set; }
        /// <summary>
        /// null이 아니면 포스트시즌 종료 결과
        /// </summary>
        public PostSeasonReport psReport { get; set; }
        
        /// <summary>
        /// 투수 MVP 정보
        /// </summary>
        public SeasonTitleMvpRewardInfo titlePitcherMvp { get; set; }
        /// <summary>
        /// 타자 MVP 정보
        /// </summary>
        public SeasonTitleMvpRewardInfo titleHitterMvp { get; set; }
        /// <summary>
        /// MVP 제외한 나머지 타이틀. 1위~3위
        /// </summary>
        public Dictionary<MvpType, List<SeasonTitleRewardInfo>> titleInfo { get; set; }
        /// <summary>
        /// 타이틀로 획득한 아이템 목록 map of { item_id : cnt }
        /// </summary>
        public Dictionary<int, int> titleRwdItems { get; set; }
    }
}