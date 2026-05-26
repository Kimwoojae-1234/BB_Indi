using System.Collections.Generic;
using System;

namespace WebConnector {
    /// <summary>
    /// 시즌모드 포스트시즌 대진표 정보
    /// </summary>
    public class SeasonPsDraws {
        /// <summary>
        /// 포스트시즌 진출팀 번호 (순서 5위 ~ 1위)
        /// </summary>
        public int[] teamNos { get; set; }
        /// <summary>
        /// 각 라운드별 결과 정보
        /// [ WildCard 각 경기별 점수, SemiPlayOff 각 경기별 점수, PlayOff 각 경기별 점수, KoreaSeries 각 경기별 점수 ]
        /// 결과 : list of scores [ 1차(도전팀(왼) 점수, 고정팀(오른) 점수), 2차, ... ]
        /// </summary>
        //public List<Tuple<int, int>>[] results { get; set; }
    }
}