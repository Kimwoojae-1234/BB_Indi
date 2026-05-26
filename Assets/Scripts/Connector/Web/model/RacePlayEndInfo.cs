using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class RacePlayEndInfo
    {
        /// <summary>
        /// 리워드 지급후 총 보유 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 리워드 지급 후 지급된 아이템의 총 보유량
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 팀 랭킹 요소(승무패,득실점) 갱신 정보
        /// </summary>
        public List<RacePlayTeamInfo> teamUdtInfos { get; set; }

        [Obsolete("삭제됨")]
        public int[] rankings { get; set; }
    }
}