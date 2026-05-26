using System;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayAnnounceInfo {
        /// <summary>
        /// array of [이전시즌 리그, 현 시즌 리그]. 이전리그 0이면 배치고사 완료
        /// null이라면 시즌은 종료되고 새시즌이 시작안됨
        /// </summary>
        public int[] leagues { get; set; }
        /// <summary>
        /// null이 아니면 지난시즌 결과
        /// </summary>
        public RankedPlaySeasonResult result { get; set; }
    }
}