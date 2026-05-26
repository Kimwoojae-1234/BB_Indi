namespace WebConnector
{
    /// <summary>
    /// 9회말2아웃 난이도별 경기장 정보
    /// </summary>
    public class WalkoffPlayArena
    {
        /// <summary>
        /// 경기장 번호 (난이도에 따라 1~3)
        /// </summary>
        public int arenaNo { get; set; }
        /// <summary>
        /// 경기장 오픈 여부
        /// </summary>
        public bool open { get; set; }
        /// <summary>
        /// 상대팀 구단코드
        /// </summary>
        public TeamCode otherTeam { get; set; }
        /// <summary>
        /// 경기장 상대투수 cardId
        /// </summary>
        public int pitcherCardId { get; set; }
        /// <summary>
        /// 경기장 상대투수 전력 (노출용)
        /// </summary>
        public int pitcherPw { get; set; }
        /// <summary>
        /// 최고 라운드
        /// </summary>
        public int bestRound { get; set; }
        /// <summary>
        /// 최고 포인트
        /// </summary>
        public int bestPoint { get; set; }
    }
}