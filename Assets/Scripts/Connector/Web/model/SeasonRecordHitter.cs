using System;

namespace WebConnector {
    [System.Obsolete("삭제됨")]
    public class SeasonRecordHitter : GameRecordHitter {
        public int teamNo { get; set; }

        [Obsolete("삭제됨")]
        public int g { get; set; }
    }
}