using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class LivePlayCoinStorePurchaseResult
    {
        /// <summary>
        /// 시즌종료 시간
        /// </summary>
        public DateTime closeDate { get; set; }
        /// <summary>
        /// null이 아니면 핫타임 진행 시간. [from date, to date]
        /// </summary>
        public DateTime[] hotTime { get; set; }
        /// <summary>
        /// 리그레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// array of [win, draw, lose]
        /// </summary>
        public int[] wdl { get; set; }
        /// <summary>
        /// 연승수
        /// </summary>
        public int winInarow { get; set; }
        /// <summary>
        /// 현재 랭킹
        /// </summary>
        public int curRank { get; set; }
        /// <summary>
        /// 주간 랭킹 전체 사이즈 (상위 비율 계산용)
        /// </summary>
        public int curRankSize { get; set; }
        /// <summary>
        /// 최고랭킹
        /// </summary>
        public int bestRank { get; set; }
        /// <summary>
        /// 최고랭킹 달성시점 사이즈 (상위 비율 계산용)
        /// </summary>
        public int bestRankSize { get; set; }
        /// <summary>
        /// 시즌 점수
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 현재 마일리지
        /// </summary>
        public int mileage { get; set; }
        /// <summary>
        /// 보유티켓 수. [현재 티켓수, 남은 충전시간]
        /// </summary>
        public int[] ticketInfo { get; set; }
        /// <summary>
        /// 최근 경기 정보
        /// </summary>
        public List<LivePlayMatchInfo> recentMatches { get; set; }
        /// <summary>
        /// null이 아니면 보상 정보
        /// </summary>
        public LivePlayAnnounceInfo annInfo { get; set; }
    }
}