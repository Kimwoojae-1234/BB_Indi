using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 비복원 가챠 결과 정보
    /// </summary>
    public class ScoutPickUpResult
    {
        /// <summary>
        /// 재화 정보. array of [ruby, gold, mileage, friendpoint]
        /// </summary>
        public int[] balances { get; set; }
        /// <summary>
        /// 비복원 가챠 영입만료기한까지 남은 시간
        /// </summary>
        [Obsolete("삭제예정")]
        public DateTime remainingTime { get; set; }
        public DateTime expiryDate { get; set; }
        /// <summary>
        /// 비복원 가챠 영입 테이블 정보. 이미 영입한 선수 정보만 들어 있음.
        /// </summary>
        public Dictionary<int, int> scoutMap { get; set; }
        /// <summary>
        /// 영입된 선수 게임카드 정보.
        /// </summary>
        public List<GameCardInfo> gameCardInfos;
    }
}
