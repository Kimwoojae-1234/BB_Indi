namespace WebConnector
{
    public class LivePlayGameInfo
    {
        /// <summary>
        /// 선발 odr
        /// </summary>
        public int starterOdr { get; set; }
        /// <summary>
        /// 친선전 여부
        /// </summary>
        public bool friendly { get; set; }
        /// <summary>
        /// 홈팀 정보
        /// </summary>
        public LivePlayTeamInfo homeTeam { get; set; }
        /// <summary>
        /// 어웨이팀 정보
        /// </summary>
        public LivePlayTeamInfo awayTeam { get; set; }
        /// <summary>
        /// 홈팀 선수 기록
        /// </summary>
        public RecordInfo homeRecInfo { get; set; }
        /// <summary>
        /// 어웨이팀 선수 기록
        /// </summary>
        public RecordInfo awayRecInfo { get; set; }
    }
}