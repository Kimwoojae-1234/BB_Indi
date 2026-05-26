namespace WebConnector {
    /// <summary>
    /// 팀전력
    /// </summary>
    public class TeamPower {
        /// <summary>
        /// 공격력
        /// </summary>
        public int offense { get; set; }
        /// <summary>
        /// 수비력
        /// </summary>
        public int defense { get; set; }
        /// <summary>
        /// 투수력
        /// </summary>
        public int pitcher { get; set; }
        /// <summary>
        /// 종합전력
        /// </summary>
        public int total { get; set; }

        public static TeamPower of (int off, int def, int pit, int tot)
        {
            TeamPower tp = new TeamPower();
            tp.offense = off;
            tp.defense = def;
            tp.pitcher = pit;
            tp.total = tot;

            return tp;
        }
    }
}