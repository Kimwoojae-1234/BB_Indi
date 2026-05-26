using System;
using System.Collections.Generic;

namespace WebConnector
{
    /// <summary>
    /// 9회말 2아웃 게임 정보
    /// </summary>
    public class WalkoffPlayGameInfo
    {
        /// <summary>
        /// null이 아니면 루비를 소모한 경우이며 이때 소비후 총 보유 재화
        /// </summary>
        [Obsolete("삭제됨. 이제 루비등의 재화로 구입할 수 없음.")]
        public int[] balances { get; set; }
        /// <summary>
        /// 내팀 코드
        /// </summary>
        public TeamCode myTeam { get; set; }
        /// <summary>
        /// 시즌 랭킹
        /// </summary>
        public int curRank { get; set; }
        /// <summary>
        /// 내팀 이름
        /// </summary>
        public string myName { get; set; }
        /// <summary>
        /// 선택한 타자
        /// </summary>
        public GameCardInfo myHitter { get; set; }
        /// <summary>
        /// 상대팀 팀코드
        /// </summary>
        public TeamCode otherTeam { get; set; }
        /// <summary>
        /// 상대팀 수비정보. 투수 1명, 포수,내야수,외야수 총 9명
        /// </summary>
        public List<GameCardInfo> otherLineup { get; set; }
        /// <summary>
        /// 라운드별 볼카운트값 [1라운드, ... 10라운드]
        /// </summary>
        public List<int> outCounts { get; set; }
        /// <summary>
        /// 내팀 주자 주력값
        /// </summary>
        public int myRn { get; set; }
    }
}