using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebConnector
{
    public class TrainingInfo
    {        
        /// <summary>
        /// 캠프장 인덱스
        /// </summary>
        public int campNo { get; set; }
        /// <summary>
        /// 훈련타입 (1, 3, 10시간)
        /// </summary>
        public TrainingType trType { get; set; }
        /// <summary>
        /// 훈련중인 선수 목록
        /// </summary>
        public List<long> cardSeqs { get; set; }
        /// <summary>
        /// 훈련 시작 시간
        /// </summary>
        public DateTime startDate { get; set; }
    }
}