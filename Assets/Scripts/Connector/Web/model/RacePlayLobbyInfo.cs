using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 쟁탈전 로비 정보
    /// </summary>
    public class RacePlayLobbyInfo
    {
        /// <summary>
        /// 주간 시즌 닫힘 시간
        /// </summary>
        public DateTime closeDate { get; set; }
        /// <summary>
        /// null이 아니면 새리그. [이전리그등급, 현재리그등급]
        /// </summary>
        public int[] newInfo { get; set; }
        /// <summary>
        /// 시즌랭킹
        /// </summary>
        public int curRank { get; set; }
        public int curRankSize { get; set; }
        /// <summary>
        /// 역대 최고 랭킹
        /// </summary>
        public int bestRank { get; set; }
        public int bestRankSize { get; set; }
        /// <summary>
        /// 시즌 점수
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 보유티켓 수. [현재 티켓수, 남은 충전시간]
        /// </summary>
        public int[] ticketInfo { get; set; }
        /// <summary>
        /// 현재 리그 레벨
        /// </summary>
        public int leagueLev { get; set; }
        /// <summary>
        /// 나의 팀 번호
        /// </summary>
        public int myTeamNo { get; set; }
        /// <summary>
        /// 나의 토너먼트 번호 (RacePlay_CheckMatchResult() 의 인자로 사용됨)
        /// </summary>
        public int myTmNo { get; set; }
        /// <summary>
        /// 결과 확인한 마지막 라운드 번호
        /// </summary>
        public int chkRoundNo { get; set; }

        [Obsolete("삭제됨. teamInfos 대체")]
        public List<RacePlayTeamRankInfo> teamRanks { get; set; }
        /// <summary>
        /// 팀별 승무패 정보
        /// </summary>
        public List<RacePlayTeamInfo> teamInfos { get; set; }
        /// <summary>
        /// 경기 스케줄 정보
        /// </summary>
        public List<RacePlayMatchInfo> matches { get; set; }
        /// <summary>
        /// null이 아니면 보상 정보
        /// </summary>
        public RacePlayAnnounceInfo annInfo { get; set; }

        [Obsolete("삭제됨. teamInfos 에 대해 랭킹을 메기고 해당 정보로 메인 로비 정보 업데이트 필요. 결과 확인 할 때마다 필요")]
        public RacePlaySummary RcpSum { get { return new RacePlaySummary(leagueLev, 0); } }

        /// <summary>
        /// 결과 확인시 리턴되는 갱신 정보 또는 인게임에 넘겨주어 경기 결과의 갱신정보를 업데이트 후 랭킹을 계산한다.
        /// </summary>
        public RacePlayTeamInfoManager CreateTeamInfoManager {
            get {
                return new RacePlayTeamInfoManager(teamInfos);
            }
        }
    }
}