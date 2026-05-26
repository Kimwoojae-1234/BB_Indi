using System;
using System.Collections.Generic;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class SeasonRewardInfo {
        /// <summary>
        /// 획득한 팀경험치
        /// </summary>
        public int exp { get; set; }
        /// <summary>
        /// 기본보상. array of [ vip level, reward gold ]
        /// </summary>
        public int[] gold1 { get; set; }
        /// <summary>
        /// 보너스(리그레벨) 보상. array of [ league level, reward gold ]
        /// </summary>
        public int[] gold2 { get; set; }
        /// <summary>
        /// 보너스(경기결과) 보상. array of [ win type(1:승,2:무,3:패), reward gold ]
        /// </summary>
        public int[] gold3 { get; set; }
        /// <summary>
        /// 보너스(안타) 보상. array of [ hit cnt, reward gold ]
        /// </summary>
        public int[] gold4 { get; set; }
        /// <summary>
        /// 보너스(삼진) 보상. array of [ so cnt, reward gold ]
        /// </summary>
        public int[] gold5 { get; set; }
        /// <summary>
        /// 우편함으로 지급된 보상아이템에 대한 우편함 시퀀스 번호
        /// </summary>
        public long postSeq { get; set; }
        /// <summary>
        /// 보상 아이템
        /// </summary>
        public int itemId { get; set; }
        /// <summary>
        /// 선수별 획득한 경험치
        /// </summary>
        public Dictionary<long, int> cardExps { get; set; }
    }
}