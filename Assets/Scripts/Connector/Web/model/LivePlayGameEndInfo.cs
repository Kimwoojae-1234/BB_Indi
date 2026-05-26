using System;

namespace WebConnector
{
    /// <summary>
    /// 경기 종료후 정보
    /// </summary>
    public class LivePlayGameEndInfo
    {
        /// <summary>
        /// null이 아니면 총재화
        /// </summary>
        [Obsolete("삭제됨")]
        public int[] balances { get; set; }
        /// <summary>
        /// 포인트 변화량
        /// </summary>
        public int chgPoint { get; set; }
        /// <summary>
        /// 경기 후 포인트
        /// </summary>
        public int point { get; set;}
        /// <summary>
        /// 승무패에 따른 마일리지 보상
        /// </summary>        
        public int mileage { get; set; }
        /// <summary>
        /// 승리보상코인
        /// </summary>
        [Obsolete("삭제됨")]
        public int coin { get; set; }
        /// <summary>
        /// 연승카운트
        /// </summary>
        public int stCnt { get; set; }
        /// <summary>
        /// 리그레벨 변화 [이전리그, 이후리그]
        /// </summary>
        public int[] chgLeagueLev { get; set; }
        /// <summary>
        /// 경기후 랭킹
        /// </summary>
        public int ranking { get; set; }
    }
}