using System;
using System.Collections.Generic;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayTeamInfo {
        /// <summary>
        /// 해당 팀의 userId
        /// </summary>
        public long teamId { get; set; }
        public TeamCode team { get; set; }
        public string name { get; set; }
        /// <summary>
        /// 리그 등급
        /// </summary>
        public int league { get; set; }
        /// <summary>
        /// 승점
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 승무패. array of [win, draw, lose]
        /// </summary>
        public int[] wdl { get; set; }
        /// <summary>
        /// 선발투수가 선정된 21명의 출전 선수카드
        /// </summary>
        public List<GameCardInfo> lineup { get; set; }

        public List<GameCardInfo> pitchers {
            get {
                return lineup.FindAll(p => p.PlayerType == PlayerType.Pitcher);
            }
        }
        public List<GameCardInfo> hitters {
            get {
                return lineup.FindAll(p => p.PlayerType == PlayerType.Hitter);
            }
        }
    }
}