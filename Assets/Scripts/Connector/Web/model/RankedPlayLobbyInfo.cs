using System;
using System.Collections.Generic;

namespace WebConnector {
    [Obsolete("삭제됨")]
    public class RankedPlayLobbyInfo {
        /// <summary>
        /// 시즌 남은 시각. 과거이면 시즌 종료됨. 미래이면 진행중
        /// </summary>
        public DateTime finishDate { get; set; }
        /// <summary>
        /// 현재 리그등급. 0이면 배치경기 진행중
        /// </summary>        
        public int league { get; set; }
        /// <summary>
        /// 팀 전력
        /// </summary>
        public int teamPower { get; set; }
        /// <summary>
        /// 승무패. array of [win, draw, lose]
        /// </summary>
        public int[] wdl { get; set; }
        /// <summary>
        /// 연승 횟수
        /// </summary>
        public int winInarow { get; set; }
        /// <summary>
        /// 현재 승점
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 나의 랭킹 (ranking)
        /// </summary>
        public int ranking { get; set; }
        /// <summary>
        /// 적립된 마일리지 
        /// </summary>
        public int mileage { get; set; }
        /// <summary>
        /// 매칭리스트 최근 갱신 시각        
        /// </summary>
        public DateTime latestListRefreshDate { get; set; }
        /// <summary>
        /// 남은 도전권[보유 랭킹전도전권수, 마지막 충전으로부터 지난 초(보유 도전권개수가 max일때는 0)]
        /// </summary>
        public int[] ticketInfo { get; set; }
        /// <summary>
        /// 매칭 리스트
        /// </summary>
        public List<RankedPlayTeam> matchList { get; set; }
        /// <summary>
        /// 랭킹전 시즌결과 - null이 아니면 시즌종료
        /// </summary>
        public RankedPlaySeasonResult result { get; set; }

        /// <summary>
        /// 요약 정보
        /// </summary>
        public RankedPlaySummary RkpSum {
            get {
                return new RankedPlaySummary(finishDate, league, ranking);
            }
        }
    }
}