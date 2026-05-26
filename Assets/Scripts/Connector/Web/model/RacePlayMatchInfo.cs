namespace WebConnector
{
    public class RacePlayMatchInfo
    {
        /// <summary>
        /// 경기 스케줄 번호
        /// </summary>
        public int scheNo { get; set; }
        public int teamNo { get; set; } //해당 팀의 팀번호
        /// <summary>
        /// 스코어 [myScore, otherScore], null 이면 진행 하지 않은 경기
        /// </summary>
        public int[] score { get; set; }
        /// <summary>
        /// 경기 시각 (HH:mm)
        /// </summary>
        public string matchTime { get; set; }
        /// <summary>
        /// true 면 재경기 된 경기
        /// </summary>
        public bool rematched { get; set; }
}
}