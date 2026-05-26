using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 비복원 가챠 결과 정보
    /// </summary>
    public class ScoutPickUpRefreshTableResult
    {
        /// <summary>
        /// 재화 정보. array of [ruby, gold, mileage, friendpoint]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 비복원 가챠 4성 이상 선수 명단.
        /// </summary>        
        public List<int> mainPlayer { get; set; }
        /// <summary>
        /// 비복원 가챠 영입만료기한까지 남은 시간
        /// </summary>
        [Obsolete("삭제예정")]
        public DateTime remainingTime { get; set; }
        public DateTime expiryDate { get; set; }        
    }
}
