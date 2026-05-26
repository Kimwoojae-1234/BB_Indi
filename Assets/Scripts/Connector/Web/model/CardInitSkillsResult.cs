using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebConnector {
    /// <summary>
    /// 스킬 초기화후 결과
    /// </summary>
    public class CardInitSkillsResult {
        /// <summary>
        /// 루비 잔액
        /// </summary>
        public int balanceRuby { get; set; }
        /// <summary>
        /// 초기화 후 스킬 포인트
        /// </summary>
        public int skillPoint { get; set; }
    }
}
