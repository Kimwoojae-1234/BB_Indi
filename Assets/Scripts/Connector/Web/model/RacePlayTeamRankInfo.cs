namespace WebConnector
{
    [System.Obsolete("삭제됨.")]
    public class RacePlayTeamRankInfo
    {
        public long teamId { get; set; }
        public int teamNo { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public int teamPw { get; set; }
        /// <summary>
        /// 해당 팀의 승무패 [win, draw, lose]
        /// </summary>
        public int[] wdl { get; set; }
        public int ranking { get; set; }
    }
}