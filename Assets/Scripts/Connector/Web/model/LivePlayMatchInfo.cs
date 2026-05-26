namespace WebConnector {
    public class LivePlayMatchInfo {
        /// <summary>
        /// 경기시퀀스 번호. 리마인드 매치에 사용
        /// </summary>
        public long hisSeq { get; set; }
        /// <summary>
        /// 리그레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 상대방 유저아이디
        /// </summary>
        public long teamId { get; set; }
        /// <summary>
        /// 상대방 팀명
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 상대방 구단코드
        /// </summary>
        public TeamCode team { get; set; }
        /// <summary>
        /// 상대방 팀전력
        /// </summary>
        public int teamPw { get; set; }
        /// <summary>
        /// 스코어 [myScore, otherScore]
        /// </summary>
        public int[] score { get; set; }
    }
}