using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 정규시즌 종료후 정산 정보 (노출용)
    /// </summary>
    public class RegularSeasonReport {
        /// <summary>
        /// 정규시즌 팀랭킹
        /// </summary>
        public List<SeasonTeamRecordInfo> teamRanking { get; set; }
        
        /// <summary>
        /// 최종순위
        /// </summary>
        public int ranking { get; set; }

        [Obsolete("삭제됨")]
        public int gold { get; set; }
        [Obsolete("삭제됨")]
        public Dictionary<int, int> rwdItems { get; set; }
        
    }
}