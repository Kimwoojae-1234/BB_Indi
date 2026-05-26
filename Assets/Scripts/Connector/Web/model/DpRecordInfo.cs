namespace WebConnector {
    /// <summary>
    /// 선수 상세보기 기록 정보
    /// 12개의 배열이며 투수 또는 타자에 따라 순서는 아래와 같다.
    /// 투수: array of (시합수,평균자책,승리,패배,세이브,홀드,투구이닝,탈삼진,사사구,피홈런,자책점,WHIP)
    /// 타자: array of (시합수,타율,타수,안타,홈런,타점,득점,도루,볼넷,장타율,출루율,OPS)
    /// </summary>
    public class DpRecordInfo {
        /// <summary>
        /// 현재 시즌
        /// </summary>
        public string[] curSeason { get; set; }
        /// <summary>
        /// 누적 시즌
        /// </summary>
        public string[] totSeason { get; set; }
        /// <summary>
        /// 현재 랭킹전 기록
        /// </summary>
        public string[] curRankedPlay { get; set; }
        /// <summary>
        /// 누적 랭킹전 기록
        /// </summary>
        public string[] totRankedPlay { get; set; }
    }
}