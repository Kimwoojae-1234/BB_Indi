using System;
using System.Text;

namespace WebConnector {
    /// <summary>
    /// 게임 결과 (투수)
    /// </summary>
    public class GameRecordPitcher {
        /// <summary>
        /// 투수 cardSeq
        /// </summary>
        public long cardSeq { get; set; }
        /// <summary>
        /// 투수 Card ID
        /// </summary>
        public int cardId { get; set; }
        /// <summary>
        /// 선수카드 종능
        /// </summary>
        public int cardPw { get; set; }
        /// <summary>
        /// 팀번호
        /// </summary>
        public int teamNo { get; set; }
        /// <summary>
        /// 경기수
        /// </summary>
        public int g { get; set; }

        /// <summary>
        /// 아웃카운트
        /// </summary>
        public int pOC { get; set; }
        /// <summary>
        /// 승
        /// </summary>
        public int pW { get; set; }
        /// <summary>
        /// 패
        /// </summary>
        public int pL { get; set; }
        /// <summary>
        /// 홀드
        /// </summary>
        public int pHLD { get; set; }
        /// <summary>
        /// 세이브
        /// </summary>
        public int pSV { get; set; }
        /// <summary>
        /// 삼진
        /// </summary>
        public int pSO { get; set; }
        /// <summary>
        /// 볼넷
        /// </summary>
        public int pBB { get; set; }
        /// <summary>
        /// 힛바이피치
        /// </summary>
        public int pHBP { get; set; }
        /// <summary>
        /// 피안타
        /// </summary>
        public int pH { get; set; }
        /// <summary>
        /// 피홈런
        /// </summary>
        public int pHR { get; set; }
        /// <summary>
        /// 완투
        /// </summary>
        public int pCG { get; set; }
        /// <summary>
        /// 완봉
        /// </summary>
        public int pSHO { get; set; }
        /// <summary>
        /// 자책
        /// </summary>
        public int pER { get; set; }
        /// <summary>
        /// 실점
        /// </summary>
        public int pRA { get; set; }
        /// <summary>
        /// 에러
        /// </summary>
        public int pE { get; set; }
        /// <summary>
        /// 와일드피치
        /// </summary>
        public int pWP { get; set; }
        /// <summary>
        /// 피2루타
        /// </summary>
        public int p2B { get; set; }
        /// <summary>
        /// 피2루타
        /// </summary>
        public int p3B { get; set; }
        /// <summary>
        /// 투구수
        /// </summary>
        public int pNP { get; set; }
        /// <summary>
        /// 피타수
        /// </summary>
        public int pTBF { get; set; }
        /// <summary>
        /// 블론세이브
        /// </summary>
        public int pBS { get; set; }
        /// <summary>
        /// 땅볼아웃
        /// </summary>
        public int pGO { get; set; }
        /// <summary>
        /// 플라이아웃(air out)
        /// </summary>
        public int pAO { get; set; }
        /// <summary>
        /// 라이너아웃
        /// </summary>
        public int pLO { get; set; }

        public string toString() {
            StringBuilder sb = new StringBuilder()
                .Append( cardSeq ).Append('-')
                .Append( cardId ).Append('-')
                .Append( cardPw ).Append('=')
                .Append( pOC ).Append('-')
                .Append( pW ).Append('-')
                .Append( pL ).Append('-')
                .Append( pHLD ).Append('-')
                .Append( pSV ).Append('-')
                .Append( pSO ).Append('-')
                .Append( pBB ).Append('-')
                .Append( pHBP ).Append('-')
                .Append( pH ).Append('-')
                .Append( pHR ).Append('-')
                .Append( pCG ).Append('-')
                .Append( pSHO ).Append('-')
                .Append( pER ).Append('-')
                .Append( pRA ).Append('-')
                .Append( pE ).Append('-')
                .Append( pWP ).Append('-')
                .Append( p2B ).Append('-')
                .Append( p3B ).Append('-')
                .Append( pNP ).Append('-')
                .Append( pTBF ).Append('-')
                .Append( pBS ).Append('-')
                .Append( pGO ).Append('-')
                .Append( pAO ).Append('-')
                .Append( pLO );

            return sb.ToString();
        }
    }
}