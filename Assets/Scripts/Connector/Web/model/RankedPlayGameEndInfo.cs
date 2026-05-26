using System;
using System.Collections.Generic;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayGameEndInfo {
        public Wdl wdl { get; set; }
        /// <summary>
        /// 승점 변화량
        /// </summary>
        public int chgPoint { get; set; }
        /// <summary>
        /// 최종 승점
        /// </summary>
        public int finalPoint { get; set; }
        /// <summary>
        /// 승리보상. wdl(승무패) 에 따른 획득 코인
        /// </summary>
        public int coin { get; set; }
        /// <summary>
        /// 연승카운트
        /// </summary>
        public int stCnt { get; set; }
        /// <summary>
        /// 경기전 소속리그
        /// </summary>
        public int beforeLeague { get; set; }
        /// <summary>
        /// 경기후 소속리그
        /// </summary>
        public int afterLeague { get; set; }
    }
}