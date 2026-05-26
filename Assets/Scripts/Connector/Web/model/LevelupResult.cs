using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 선수카드 레벨업 후 결과
    /// </summary>
    public class LevelupResult
    {
        /// <summary>
        /// 재화 잔액
        /// </summary>
        public int[] balances { get; set; } //재화 잔액. array of [Ruby, Gold]
        /// <summary>
        /// 레벨업 후 cardInfo
        /// </summary>
        public GameCardInfo cardInfo { get; set; }
        /// <summary>
        /// 잭팟 타입
        /// </summary>
        public int jackpotType { get; set; }
        /// <summary>
        /// 사용후 남은 아이템 [아이템ID, 남은수량]
        /// 사용한 아이템이 없으면 null
        /// </summary>
        public Dictionary<int, int> items { get; set; }
    }
}