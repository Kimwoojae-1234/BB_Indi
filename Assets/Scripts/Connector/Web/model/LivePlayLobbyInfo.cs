using System;
using System.Collections.Generic;

namespace WebConnector {
    public class LivePlayLobbyInfo {
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
        /// 마일리지 보상 단계 정보
        /// </summary>
        public int mileageOpenStep { get; set; }
        /// <summary>
        /// 현재 핫타임 마일리지
        /// </summary>
        public int hotTimeMileage { get; set; }
        /// <summary>
        /// 핫타임 마일리지 보상 단계 정보
        /// </summary>
        public int hotTimeMileageOpenStep { get; set; }
        /// <summary>
        /// 보유티켓 수. [현재 티켓수, 남은 충전시간]
        /// </summary>
        [Obsolete("삭제됨")]        
        public int[] ticketInfo {
            get { return new int[] { 3, 0 }; }
        }
        /// <summary>
        /// 최근 경기 정보
        /// </summary>
        public List<LivePlayMatchInfo> recentMatches { get; set; }
        /// <summary>
        /// null이 아니면 보상 정보
        /// </summary>
        public LivePlayAnnounceInfo annInfo { get; set; }


        /// <summary>
        /// 요약 정보
        /// </summary>
        public LivePlaySummary RkpSum {
            get {
                return new LivePlaySummary(closeDate, leagueLev, curRank);
            }
        }
    }
}