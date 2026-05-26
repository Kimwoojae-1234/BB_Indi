using System.Collections.Generic;

namespace WebConnector
{
    public class RacePlayAnnounceInfo
    {
        /// <summary>
        /// 재화 잔액 (갱신용)
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 지급된 아이템의 총 보유량 (갱신용)
        /// </summary>
        public Dictionary<int, int> items { get; set; }

        /// <summary>
        /// 일일 리그 최종 랭킹 (0 보다 크면 일일 리그 보상을 받음)
        /// </summary>
        public int lgRanking { get; set; }
        /// <summary>
        /// 일일 리그 보상 골드
        /// </summary>
        public int lgGold { get; set; }
        /// <summary>
        /// 일일 리그 보상 아이템
        /// </summary>
        public Dictionary<int, int> lgItems { get; set; }
        /// <summary>
        /// 일일 최종 보상
        /// </summary>
        public List<RacePlayTeamRecordInfo> finalTeamRanks { get; set; }

        /// <summary>
        /// 주간 최종 랭킹 (0보다 크면 주간 보상을 받음)
        /// </summary>
        public int weekRanking { get; set; }
        /// <summary>
        /// 주간 리그 보상 루비
        /// </summary>
        public int weekLeagueRuby { get; set; }
        /// <summary>
        /// 주간 리그 보상 아이템
        /// </summary>
        public Dictionary<int, int> weekLeagueItems { get; set; }
        /// <summary>
        /// 주간 랭킹 보상 루비
        /// </summary>
        public int weekRankingRuby { get; set; }
    }
}