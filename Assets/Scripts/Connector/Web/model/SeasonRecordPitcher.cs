using System;

namespace WebConnector {
    [System.Obsolete("삭제됨")]
    public class SeasonRecordPitcher : GameRecordPitcher {
        /// <summary>
        /// 팀번호
        /// </summary>
        public int teamNo { get; set; }
        [Obsolete("삭제됨")]
        public int g { get; set; }
    }
}