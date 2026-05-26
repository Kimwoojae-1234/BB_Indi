using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 시즌모드 게임 기록 전달 정보
    /// </summary>
    public class SeasonGameResult {
        public int scheNo { get; set; }
        public int homeScore { get; set; }
        public int awayScore { get; set; }

        public List<GameRecordPitcher> homePitchers { get; set; }
        public List<GameRecordHitter> homeHitters { get; set; }
        public List<GameRecordPitcher> awayPitchers { get; set; }
        public List<GameRecordHitter> awayHitters { get; set; }
        
        //${scheNo}:${homeScore}:${awayScore}:${homeRecords}:${awayRecords}
        public override string ToString() {
            string[] arrHomePitcher = Array.ConvertAll<GameRecordPitcher, string>(homePitchers.ToArray(), rec => rec.toString());
            string[] arrHomeHitters = Array.ConvertAll<GameRecordHitter, string>(homeHitters.ToArray(), rec => rec.toString());
            string[] arrAwayPitcher = Array.ConvertAll<GameRecordPitcher, string>(awayPitchers.ToArray(), rec => rec.toString());
            string[] arrAwayHitters = Array.ConvertAll<GameRecordHitter, string>(awayHitters.ToArray(), rec => rec.toString());

            string homeRecords = string.Join("_", arrHomePitcher) + "_" + string.Join("_", arrHomeHitters);
            string awayRecords = string.Join("_", arrAwayPitcher) + "_" + string.Join("_", arrAwayHitters);
            
            return string.Format("{0}:{1}:{2}:{3}:{4}", scheNo, homeScore, awayScore, homeRecords, awayRecords);
        }
    }
}