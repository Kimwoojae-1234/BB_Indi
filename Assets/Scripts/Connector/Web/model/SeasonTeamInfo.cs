using System.Collections.Generic;

namespace WebConnector {
    public class SeasonTeamInfo {
        public TeamCode team { get; set; }
        public string name { get; set; }
        public List<GameCardInfo> lineup { get; set; }
        /// <summary>
        /// 시즌모드 팀랭킹
        /// </summary>
        public int ranking { get; set; }

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