using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 스킬변경 결과
    /// </summary>
    public class SkillChangeResult
    {
        /// <summary>
        /// 스킬변경후 스킬정보, 스킬고정권 및 재료로 사용된 트로피 정보
        /// 스킬고정권을 사용하면 트로피는 재료로 소모되지 않음
        /// </summary>
        public Dictionary<int,int> items { get; set; }
        public List<CardSkill> skills { get; set; }
        /// <summary>
        /// 성공인경우 변경후 사용된 아이템 정보 (items) 및 스킬목록을 반환
        /// 실패는 없음
        /// </summary>
    }
}