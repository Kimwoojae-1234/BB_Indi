using System;
using System.Collections.Generic;

namespace WebConnector
{
    public class WalkoffPlayLobbyInfo
    {
        /// <summary>
        /// 현재 시즌 순위
        /// </summary>
        public int curRank { get; set; }
        public int curRankSize { get; set; }
        /// <summary>
        /// 역대 최고 랭킹. 0이면 참여한적 없음.
        /// </summary>
        public int bestRank { get; set; }
        public int bestRankSize { get; set; }

        [Obsolete("삭제됨. curRank / curRankSize 로 처리")]
        public float topRankPer { get; set; }
        /// <summary>
        /// 시즌 점수
        /// </summary>
        public int point { get; set; }
        /// <summary>
        /// 도전권 개수. [사용한개수, 무료제공개수]
        /// </summary>        
        public int[] ticket { get; set; }
        /// <summary>
        /// 도전권 개수. [사용한개수, 무료제공개수, 일일최대개수]
        /// </summary>
        [Obsolete("삭제됨. ticket 필드로 대체")]
        public int[] ticketInfo {
            get { return new int[] { 2, 3, 10 }; }
        }
        /// <summary>
        /// 시즌 닫힘 시간 (다음 월요일 자정)
        /// </summary>
        public DateTime closeDate { get; set; }
        /// <summary>
        /// 내 타자 목록
        /// </summary>
        public List<GameCardInfo> hitters { get; set; }
        /// <summary>
        /// 하루동안사용한 타자 정보. {teamId : list of [cardSeq]}
        /// teamId 0 이면 내팀, 0이상이면 친구의 userId
        /// </summary>
        [Obsolete("삭제됨. 기획내용 변경으로 필요 없어짐.")]
        public Dictionary<long, List<long>> usedHitters { get; set; }
        /// <summary>
        /// 경기장 난이도별로 3개의 배열 [난이도1, 난이도2, 난이도3]. 오픈되지 않았다면 null.
        /// </summary>
        public WalkoffPlayArena[] arenas { get; set; }
        /// <summary>
        /// null이 아니면 보상 정보
        /// </summary>
        public WalkoffPlayAnnounceInfo annInfo { get; set; }
    }
}