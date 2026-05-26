using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class RacePlayGameInfo
    {
        /// <summary>
        /// null이 아니면 경기스킵권 아이템을 사용한 경우 해당 아이템 사용후 정보
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 현재 리그 레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 내팀이 홈팀이면 true
        /// </summary>
        public bool home { get; set; }

        public int homeTeamNo { get; set; }
        public int awayTeamNo { get; set; }
        public List<GameCardInfo> homeLineup { get; set; }
        public List<GameCardInfo> awayLineup { get; set; }
        
        /// <summary>
        /// 홈팀 선수 기록
        /// </summary>
        public RecordInfo homeRecInfo { get; set; }
        /// <summary>
        /// 어웨이팀 선수 기록
        /// </summary>
        public RecordInfo awayRecInfo { get; set; }
    }
}