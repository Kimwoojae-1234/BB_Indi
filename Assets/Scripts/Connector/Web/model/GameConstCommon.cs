using System;
using System.Collections.Generic;

namespace WebConnector {
    /// <summary>
    /// 인게임 상수 전달 데이터 모델
    /// </summary>
    public class GameConstCommon {
        public List<Skill> skills { get; set; }
        /// <summary>
        /// 인게임 밸런스 상수
        /// </summary>
        public Dictionary<string, double> balanceConsts { get; set; }

        /// <summary>
        /// 스킬 효과 정보. key : "$SkillId_$SkillLevel"
        /// </summary>
        private Dictionary<int, Skill> _skillsMap;

        public Dictionary<int, Skill> SkillsMap
        {
            get
            {
                if (_skillsMap == null)
                {
                    _skillsMap = new Dictionary<int, Skill>();

                    foreach (Skill s in skills)
                    {
                        _skillsMap.Add(s.id, s);
                    }
                }
                return _skillsMap;
            }
        }
    }
}