using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 보유팀 요약 정보
    /// </summary>
    public class TeamSummary {
        public TeamPower teamPower { get; set; }
        /// <summary>
        /// 장착한 세트덱 아이디 목록
        /// </summary>
        public List<int> setdeckIds { get; set; }
        /// <summary>
        /// 보유카드수 개요. array as [투수수, 타자수, 전설, 영웅, 희귀, 일반]
        /// </summary>
        public int[] numOfCards { get; set; }
    }
}