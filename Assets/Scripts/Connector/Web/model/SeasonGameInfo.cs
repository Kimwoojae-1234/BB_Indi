using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 시즌모드 인게임 정보
    /// </summary>
    public class SeasonGameInfo {
        /// <summary>
        /// null이 아니면 경기스킵권 아이템을 사용한 경우 해당 아이템 사용후 정보
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 플레이볼정보 [보유수, 마지막 충전으로부터 지난 초(보유수가 max일때는 0)]
        /// </summary>
        public int[] playballInfo { get; set; }

        public int leagueLev { get; set; }
        public bool day { get; set; }
        /// <summary>
        /// 유저팀 레벨
        /// </summary>
        public int teamLevel { get; set; }
        /// <summary>
        /// 유저팀 경험치
        /// </summary>
        public int teamExp { get; set; }
        /// <summary>
        /// 내팀이 홈팀이면 true
        /// </summary>
        public bool home { get; set; }

        public int starterOdr { get; set; }
        public SeasonGameType gameType { get; set; }
        /// <summary>
        /// scheNo별 팀번호 map of { scheNo : array of [homeTeamNo,awayTeamNo] }
        /// </summary>
        public Dictionary<int, int[]> schedule { get; set; }
        /// <summary>
        /// 팀번호별 모든 팀정보. map of {teamNo, teamInfo}
        /// </summary>
        public Dictionary<int, SeasonTeamInfo> teamInfos { get; set; }
        /// <summary>
        /// schedule 중 내팀경기의 스케줄 Seq
        /// </summary>
        public int myScheNo { get; set; }
        /// <summary>
        /// 홈팀 승무패. array of [승,무,패]
        /// </summary>
        public int[] homeWdl { get; set; }
        /// <summary>
        /// 어웨이팀 승무패. array of [승,무,패]
        /// </summary>
        public int[] awayWdl { get; set; }
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