namespace WebConnector
{
    /// <summary>
    /// 쟁탈전 요약 정보
    /// </summary>
    public class RacePlaySummary
    {
        public RacePlaySummary(){}
        public RacePlaySummary(int leagueLev, int ranking) {
            this.leagueLev = leagueLev;
            this.ranking = ranking;
        }
        /// <summary>
        /// 리그등급
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 랭킹. 0이면 튜토리얼 상태
        /// </summary>
        public int ranking { get; set; }
        /// <summary>
        /// 일일 보상 존재 여부
        /// </summary>
        public bool rwdDaily { get; set; }
        /// <summary>
        /// 주간 보상 존재 여부
        /// </summary>
        public bool rwdWeekly { get; set; }
    }
}