using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class SeasonMvpCardInfo {
        /// <summary>
        /// 선수카드 정보
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
        /// <summary>
        /// 팀번호
        /// </summary>
        public int teamNo { get; set; }
        /// <summary>
        /// 노출용 기록 정보
        /// </summary>
        public string dpRecord { get; set; }
    }
}