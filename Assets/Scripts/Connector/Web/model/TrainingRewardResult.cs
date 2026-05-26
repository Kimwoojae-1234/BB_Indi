using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 훈련 완료 후 보상
    /// </summary>
    public class TrainingRewardResult
    {
        /// <summary>
        /// 경험치 보상을 받은 선수카드 목록
        /// </summary>
        public List<GameCardInfo> cardInfos { get; set; }
        /// <summary>
        /// 획득한 아이템 목록
        /// </summary>
        public Dictionary<int, int> items { get; set; }
        /// <summary>
        /// 훈련장 전체 정보
        /// </summary>
        public List<TrainingInfo> trInfos { get; set; }
    }
}