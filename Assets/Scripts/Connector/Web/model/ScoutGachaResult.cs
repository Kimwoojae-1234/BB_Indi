using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 복원 가챠 결과 정보
    /// </summary>
    public class ScoutGachaResult
    {
        /// <summary>
        /// 재화 정보. array of [ruby, gold, mileage, friendpoint]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 영입된 선수 게임카드 정보.
        /// </summary>
        public List<GameCardInfo> gameCardInfos;
        /// <summary>
        /// 잭팟 정보. array of [ruby, mileage]
        /// </summary>
        public int[] jackPotInfo { get; set; }
        /// <summary>
        /// 잭팟 상금 정보. 0 이상이면 잭팟 당첨
        /// </summary>        
        public int jackPotPrize { get; set; }        
    }
}