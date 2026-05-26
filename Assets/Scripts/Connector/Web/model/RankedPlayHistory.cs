using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayHistory {
        public long hisSeq { get; set; }
        public long teamId { get; set; }
        public TeamCode team { get; set; }
        /// <summary>
        /// 팀명
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 리그등급
        /// </summary>
        public int league { get; set; }
        /// <summary>
        /// 스코어. null이면 기권패 경기. array of [내점수, 상대점수] (공격전 방어전 상관없음)
        /// </summary>
        public int[] score { get; set; }
        public int chgPoint { get; set; }
        /// <summary>
        /// 리벤지 상태
        /// </summary>
        public RPRevengeStatus rvgStatus { get; set; }
    }
}