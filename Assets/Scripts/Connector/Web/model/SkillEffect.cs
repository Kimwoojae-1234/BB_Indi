using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 스킬 효과 정보
    /// </summary>
    public class SkillEffect {
        public int id { get; set; }
        public int level { get; set; }
        public List<SkillEffectArg> args { get; set; }
    }
}