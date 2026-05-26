using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebConnector {
    /// <summary>
    /// 선수카드 능력치 초기화 결과
    /// </summary>
    public class CardInitAbsResult {
        /// <summary>
        /// 루비 잔액
        /// </summary>
        public int balanceRuby { get; set; }
        /// <summary>
        /// 초기화후 능력치 포인트
        /// </summary>
        public int abPoint { get; set; }
    }
}
