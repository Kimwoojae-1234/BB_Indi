using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayTeam {
        /// <summary>
        /// 랭킹전 상대팀 정보
        /// </summary>        
        public long teamId { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int league { get; set; }
        public int rank { get; set; }
        public int power { get; set; }
        public int point { get; set; }
        public bool win { get; set; }
    }
}