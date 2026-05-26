using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 선수 라인업 정보
    /// </summary>
    public class GameLineup
    {
        public List<GameCardInfo> homeTeam { get; set; }
        public List<GameCardInfo> awayTeam { get; set; }

        public List<GameCardInfo> homePitchers
        {
            get
            {
                return homeTeam.FindAll(p => p.PlayerType == PlayerType.Pitcher);
            }
        }

        public List<GameCardInfo> homeHitters
        {
            get
            {
                return homeTeam.FindAll(p => p.PlayerType == PlayerType.Hitter);
            }
        }

        public List<GameCardInfo> awayPitchers
        {
            get
            {
                return awayTeam.FindAll(p => p.PlayerType == PlayerType.Pitcher);
            }
        }

        public List<GameCardInfo> awayHitters
        {
            get
            {
                return awayTeam.FindAll(p => p.PlayerType == PlayerType.Hitter);
            }
        }
    }
}