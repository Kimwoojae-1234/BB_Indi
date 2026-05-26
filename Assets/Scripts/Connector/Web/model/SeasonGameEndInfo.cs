using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 시즌모드 경기완료후 반환정보
    /// </summary>
    public class SeasonGameEndInfo {
        /// <summary>
        /// 최종 보유 재화
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 최종 팀레벨
        /// </summary>
        public int teamLevel { get; set; }
        /// <summary>
        /// 최종 팀경험치
        /// </summary>
        public int teamExp { get; set; }
        /// <summary>
        /// 경기 이후 경험치를 얻은 선수카드
        /// </summary>
        public List<GameCardInfo> cardInfos { get; set; }
        /// <summary>
        /// 지급된 아이템의 총 보유량
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 해당경기 완료후 팀랭킹. map of {TeamNo : Ranking}. 팀정보는 게임시작시 받은 SeasonGameInfo.teamInfos 를 참조해야함.
        /// </summary>
        public Dictionary<int, int> teamRankings { get; set; }
        /// <summary>
        /// 0 보다 크면 남은 자동경기 진행 수
        /// </summary>
        public int autoGames { get; set; }
    }
}