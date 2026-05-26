using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class MainLobbyInfo {
        /// <summary>
        /// 팀,레벨,재화등 상단 GNB용 유저 정보
        /// </summary>
        public UserInfo userInfo { get; set; }
        /// <summary>
        /// 팀 요약 정보
        /// </summary>
        public TeamSummary teamSummary { get; set; }
        /// <summary>
        /// 시즌모드 개요 정보
        /// </summary>
        public SeasonSummary ssSummary { get; set; }
        /// <summary>
        /// 랭킹전 개요 정보
        /// </summary>
        public RankedPlaySummary rpSummary { get; set; }
    }
}