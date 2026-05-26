using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Utils
{
    /// <summary>
    /// 기록 계산기
    /// </summary>
    public class RecordUtils {
        /// <summary>
        /// 승률
        /// </summary>
        /// <param name="w">승수</param>
        /// <param name="l">패수</param>
        public static float calWinRate(int w, int l) {
            if ((w + l) > 0) {
                float ret = (float)w / (float)(w + l);
                return (float)Math.Round(ret, 3);
            }
            return 0f;
        }
        /// <summary>
        /// 승차
        /// </summary>
        /// <param name="stWin">기준팀 승수</param>
        /// <param name="stLose">기준팀 패수</param>
        /// <param name="win">팀 승수</param>
        /// <param name="lose">팀 패수</param>
        public static float calDifferenceOfWin(int stWin, int stLose, int win, int lose) {
            return ((stWin - win) + (lose - stLose)) / 2f;
        }
        /// <summary>
        /// 타율
        /// </summary>
        /// <param name="hH">총안타수</param>
        /// <param name="hAb">타수</param>
        public static float calBattingAverage(int hH, int hAb) {
            if (hAb > 0) {
                float ret = (float)hH / (float)hAb;
                return (float)Math.Round(ret, 3);
            }
            return 0f;
        }
        /// <summary>
        /// 출루율
        /// </summary>
        /// <param name="hH">총안타수</param>
        /// <param name="hAb">타수</param>
        /// <param name="hBb">볼넷</param>
        /// <param name="hHbp">사구</param>
        /// <param name="hSh">희생타</param>
        public static float calOnBasePercent(int hH, int hAb, int hBb, int hHbp, int hSh) {
            int bbBp = hBb + hHbp;

            if ((hAb + bbBp) > 0) {
                float ret = (float)(hH + bbBp) / (float)(hAb + bbBp + hSh);
                return (float)Math.Round(ret, 3);
            }
            return 0f;
        }

        /// <summary>
        /// 장타율
        /// </summary>
        /// <param name="hH">총안타수</param>
        /// <param name="h2b">2루타수</param>
        /// <param name="h3b">3루타수</param>
        /// <param name="hHr">홈런수</param>
        /// <param name="hAb">타수</param>
        public static float calSluggingAverage(int hH, int h2b, int h3b, int hHr, int hAb) {
            if (hAb > 0) {
                int b1 = hH - h2b - h3b - hHr;
                float ret = (float)(b1 + (h2b * 2) + (h3b * 3) + (hHr * 4)) / (float)hAb;
                return (float)Math.Round(ret, 3);
            }
            return 0f;
        }
        /// <summary>
        /// OPS
        /// </summary>
        /// <param name="hObp">출루율</param>
        /// <param name="hSa">장타율</param>
        public static float calOPS(float hObp, float hSa) {
            return hObp + hSa;
        }

        /// <summary>
        /// 득점권 타율
        /// </summary>
        /// <param name="hRispAB">득점권 타수</param>
        /// <param name="hRispH">득점권 안타</param>
        public static float calRISP(int hRispAB, int hRispH) {
            if (hRispAB > 0) {
                float ret = (float)hRispH / (float)hRispAB;
                return (float)Math.Round(ret, 3);
            }
            return 0f;
        }

        /// <summary>
        /// 평균자책(방어율)
        /// </summary>
        /// <param name="pEr">자책</param>
        /// <param name="pIp">총이닝수</param>
        public static float calEarnedRunAverage(int pEr, int pIp) {
            if (pIp > 0) {
                float ret = (float)(pEr * 9) / pIp;
                return (ret < 100) ? (float)Math.Round(ret, 2) : 99.99f;
            }
            return 0f;
        }

        /// <summary>
        /// WHIP (안타 + 볼넷) / 이닝
        /// </summary>
        /// <param name="pH">피안타</param>
        /// <param name="pBb">볼넷</param>
        /// <param name="pIp">이닝</param>
        public static float calWHIP(int pH, int pBb, int pIp) {
            if (pIp > 0) {
                float ret = (float)(pH + pBb) / pIp;
                return (float)Math.Round(ret, 2);
            }
            return 0f;
        }

        /// <summary>
        /// 9이닝당 삼진
        /// </summary>
        /// <param name="pIp">이닝</param>
        /// <param name="pSo">삼진</param>
        public static int calK9(int pIp, int pSo) {
            if (pSo > 0) {
                if (pIp == 0)
                    return 0;
                float tmp = (pSo / pIp) * 9;
                return (int)tmp;
            }
            return 0;
        }

        /// <summary>
        /// 피안타율
        /// </summary>
        /// <param name="pH">피안타</param>
        /// <param name="pTbf">피타수</param>
        public static float calOpponentsBattingAverage(int pH, int pTbf) {
            if (pTbf > 0) {
                float ret = (float)pH / (float)pTbf;
                return (float)Math.Round(ret, 2);
            }
            return 0f;
        }

        /// <summary>
        /// 투수 이닝수
        /// </summary>
        /// <param name="pOc">아웃카운트</param>
        public static float calIp(int pOc) {
            int quotient = (int)(pOc / 3);
            int fraction = pOc % 3;
            return quotient + (fraction / 10);
        }
    }
}
