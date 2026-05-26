using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class LineupInfo
    {
        public long cardSeq { get; set; }
        public Lineup lineup { get; set; }
        /// <summary>
        /// 시너지효과로 인해 상승하는 능력치 (해당 선수카드에 적용된 모든 시너지 효과의 상승치를 더한 값)
        /// GameCardInfo.abilities value의 4번째 요소
        /// </summary>
        public int synergyUp { get; set; }
        /// <summary>
        /// null이 아니면 해당 라인업에 발동한 시너지 정보
        /// </summary>
        public List<SynergyInfo> synergyInfos { get; set; }
    }

    /// <summary>
    /// 발동한 시너지 정보
    /// </summary>
    public class SynergyInfo
    {
        public SynergyType type { get; set; }
        public int rank { get; set; }
        public string subType { get; set; }

        /// <summary>
        /// 연도 시너지라면 발생한 연도
        /// </summary>
        public int getYearIfTypeYEAR
        {
            get
            {
                if (type == SynergyType.YEAR)
                {
                    return int.Parse(subType);
                }
                return 0;
            }
        }
        /// <summary>
        /// 팀 시너지라면 발생한 팀 코드
        /// </summary>
        public TeamCode getTeamCodeIfTypeTeam
        {
            get
            {
                if (type == SynergyType.TEAM)
                {
                    return (TeamCode)int.Parse(subType);
                }
                return 0;
            }
        }
    }
}