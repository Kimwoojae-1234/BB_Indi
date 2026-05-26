using System;
using System.Text;

namespace WebConnector {
    /// <summary>
    /// 게임 결과 (타자)
    /// </summary>
    public class GameRecordHitter {
        /// <summary>
        /// 타자 CardSeq
        /// </summary>
        public long cardSeq { get; set; }
        /// <summary>
        /// 타자 Card ID
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
        /// 타석
        /// </summary>
        public int hPA { get; set; }
        /// <summary>
        /// 타수
        /// </summary>
        public int hAB { get; set; }
        /// <summary>
        /// 총안타수
        /// </summary>
        public int hH { get; set; }
        /// <summary>
        /// 2루타
        /// </summary>
        public int h2B { get; set; }
        /// <summary>
        /// 3루타
        /// </summary>
        public int h3B { get; set; }
        /// <summary>
        /// 홈런
        /// </summary>
        public int hHR { get; set; }
        /// <summary>
        /// 타점
        /// </summary>
        public int hRBI { get; set; }
        /// <summary>
        /// 도루성공
        /// </summary>
        public int hSB { get; set; }
        /// <summary>
        /// 볼넷
        /// </summary>
        public int hBB { get; set; }
        /// <summary>
        /// 사구
        /// </summary>
        public int hHBP { get; set; }
        /// <summary>
        /// 득점
        /// </summary>
        public int hR { get; set; }
        /// <summary>
        /// 희생타
        /// </summary>
        public int hSH { get; set; }
        /// <summary>
        /// 삼진
        /// </summary>
        public int hSO { get; set; }
        /// <summary>
        /// 병살타
        /// </summary>
        public int hGDP { get; set; }
        /// <summary>
        /// 도루실패
        /// </summary>
        public int hCS { get; set; }
        /// <summary>
        /// 땅볼아웃
        /// </summary>
        public int hGO { get; set; }
        /// <summary>
        /// 플라이아웃 (air out)
        /// </summary>
        public int hAO { get; set; }
        /// <summary>
        /// 라이너아웃
        /// </summary>
        public int hLO { get; set; }
        /// <summary>
        /// 땅볼 Hit
        /// </summary>
        public int hGB { get; set; }
        /// <summary>
        /// 플라이 Hit
        /// </summary>
        public int hFB { get; set; }
        /// <summary>
        /// 라이터 Hit
        /// </summary>
        public int hLB { get; set; }
        /// <summary>
        /// 득점권타수
        /// </summary>
        public int hRISPAB { get; set; }
        /// <summary>
        /// 득점권안타
        /// </summary>
        public int hRISPH { get; set; }
        /// <summary>
        /// 자살 (수비)
        /// </summary>
        public int fPO { get; set; }
        /// <summary>
        /// 보살 (수비)
        /// </summary>
        public int fA { get; set; }
        /// <summary>
        /// 에러 (수비)
        /// </summary>
        public int fE { get; set; }
        /// <summary>
        /// 도루허용 (수비)
        /// </summary>
        public int fSBA { get; set; }
        /// <summary>
        /// 도루저지 (수비)
        /// </summary>
        public int fCS { get; set; }

        public string toString() {
            StringBuilder sb = new StringBuilder(90)
                .Append( cardSeq ).Append('-')
                .Append( cardId ).Append('-')
                .Append( cardPw ).Append('=')
                .Append( hPA ).Append('-')
                .Append( hAB ).Append('-')
                .Append( hH ).Append('-')
                .Append( h2B ).Append('-')
                .Append( h3B ).Append('-')
                .Append( hHR).Append('-')
                .Append( hRBI).Append('-')
                .Append( hSB).Append('-')
                .Append( hBB ).Append('-')
                .Append( hHBP ).Append('-')
                .Append( hR ).Append('-')
                .Append( hSH ).Append('-')
                .Append( hSO ).Append('-')
                .Append( hGDP ).Append('-')
                .Append( hCS ).Append('-')
                .Append( hGO ).Append('-')
                .Append( hAO ).Append('-')
                .Append( hLO ).Append('-')
                .Append( hGB ).Append('-')
                .Append( hFB ).Append('-')
                .Append( hLB ).Append('-')
                .Append( hRISPAB ).Append('-')
                .Append( hRISPH ).Append('-')
                .Append( fPO ).Append('-')
                .Append( fA ).Append('-')
                .Append( fE ).Append('-')
                .Append( fSBA ).Append('-')
                .Append( fCS );

            return sb.ToString();
        }
    }
}