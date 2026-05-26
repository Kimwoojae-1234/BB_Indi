namespace WebConnector {
    public class SeasonTitleMvpReward {
        /// <summary>
        /// 선수카드 정보
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
        /// <summary>
        /// 팀번호
        /// </summary>
        public int teamNo { get; set; }
        public int rwdGold { get; set; }

        public string dpRec1 { get; set; } //평균자책 / 타율
        public string dpRec2 { get; set; } //승리 / 홈런
        public string dpRec3 { get; set; } //탈삼진 / 타점
        public string dpRec4 { get; set; } //세이브 / 안타
        public string dpRec5 { get; set; } //홀드 / 도루
        
        public int rec1Rank { get; set; } //해당 기록 랭킹
        public int rec2Rank { get; set; }
        public int rec3Rank { get; set; }
        public int rec4Rank { get; set; }
        public int rec5Rank { get; set; }
    }
}