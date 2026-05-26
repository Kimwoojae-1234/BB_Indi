using System.Collections.Generic;
namespace WebConnector
{
    public class LivePlayTeamInfo
    {
        public long teamId { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        public List<GameCardInfo> lineup { get; set; }
        /// <summary>
        /// 시즌점수
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 시즌동안 승무패 정보. array of [win, draw, lose]
        /// </summary>
        public int[] wdl { get; set; }
    }
}