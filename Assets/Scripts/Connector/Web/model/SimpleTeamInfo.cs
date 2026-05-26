using System;
namespace WebConnector {
    /// <summary>
    /// 간단 팀정보
    /// </summary>
    public class SimpleTeamInfo {
        /// <summary>
        /// 팀명
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 선택구단
        /// </summary>
        public TeamCode team { get; set; }
        /// <summary>
        /// 팀 전력
        /// </summary>
        public int teamPw { get; set; }
    }
}