using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 선수영입 로비 정보
    /// </summary>
    public class ScoutLobbyInfo
    {
        /// <summary>
        /// 잭팟 정보. array of [ruby, mileage]
        /// </summary>
        public int[] jackPotInfo { get; set; }
        /// <summary>
        /// 비복원 가챠 4성 이상 선수 명단.
        /// </summary>        
        public List<int> mainPlayer { get; set; }
        /// <summary>
        /// 비복원 가챠 영입 테이블 정보. 이미 영입한 선수 정보만 들어 있음.
        /// </summary>
        public Dictionary<int, int> scoutMap { get; set; }
        /// <summary>
        /// 비복원 가챠 영입만료기한까지 남은 시간
        /// </summary>
        [Obsolete("삭제예정")]
        public DateTime remainingTime { get; set; }
        public DateTime expiryDate { get; set; }
        /// <summary>
        /// 일반 가챠 영입비용 (골드)
        /// map of {poolType : [1회비용, 11회비용]}
        /// </summary>
        public Dictionary<GachaPoolType, int[]> normalGachaGold { get; set; }
        /// <summary>
        /// 프리미엄자챠 영입비용(루비)
        /// map of {poolType : [1회비용, 11회비용]}
        /// </summary>
        public Dictionary<GachaPoolType, int[]> premiumGachaRuby { get; set; }
        /// <summary>
        /// 지정선수 영입 비용
        /// list of [1회비용, 10회 비용, 리스트갱신 비용]
        /// </summary>
        public int[] pickupRuby { get; set; }
    }
}
