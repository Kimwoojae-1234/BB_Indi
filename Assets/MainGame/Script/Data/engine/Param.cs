using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class Param
    {
        public enum DetailRecord
        {
            None = 0,
            Baseonball = 1,
            HitbyPitched = 2,
            Sacrify = 3,
            Grounder = 4,
            Liner = 5,
            Fly = 6,            
            StrikeOut = 7,
            Error = 8,
            FieldersChoice = 9,
            Single = 10,
            Double = 11,
            Tripple = 12,
            Homerun = 13,
        }

        


        public const int ST_G = 3,    //게임
                         ST_PA = 4,   //타석
                         ST_AB = 5,   //타수
                         ST_H = 6,    //안타  
                        ST_2B = 7,    //2루타
                        ST_3B = 8,    //3루타
                        ST_HR = 9,    //홈런
                        ST_RBI = 10,  //타점
                        ST_R = 11,    //득점
                        ST_BB = 12,   //포볼
                        ST_HBP = 13,  //힛바이피치
                        ST_SO = 14,   //삼진
                        ST_DP = 15,   //병살타
                        ST_SBS = 16,  //도루
                        ST_SBF = 17,  //도루자
                        ST_PO = 18,   //자살
                        ST_A = 19,    //보살
                        ST_E = 20,    //에러
                        ST_SBA = 21,  //도루허용
                        ST_CS = 22,   //도루저지
                        ST_IP = 23,   //이닝
                        ST_PSO = 24,  //투수삼진
                        ST_PBB = 25,  //투수포볼
                        ST_PH = 26,   //피안타
                        ST_PR = 27,   //실점
                        ST_PER = 28,  //자책
                        ST_PE = 29,   //에러
                        ST_PWP = 30,  //와일드 피치
                        ST_PHBP = 31, //힛바이 피치 
                        ST_PHR = 32,  //피홈런
                        ST_P2B = 33,  //피2루타
                        ST_P3B = 34,  //피3루타
                        ST_PNP = 35,  //투구수 
                        ST_TBF = 36,  //피타수
                        ST_PW = 37,   //승
                        ST_PL = 38,   //패
                        ST_HLD = 39,  //홀드
                        ST_SV = 40,   //세이브
                        ST_BS = 41,   //블론 
                        ST_CG = 42,   //완투
                        ST_SHO = 43;  //완봉

        public const int ST_FLY = 0,
                         ST_GROUNDER = 1,
                         ST_LINER = 2,
                         ST_FLYHIT = 3,
                         ST_GROUNDERHIT = 4,
                         ST_LINERHIT = 5;


        public const int P_ACHIEVE_NONE = 0,    //해당업적 관계없음
                         P_ACHIEVE_TRY = 1,     //해당업적 시도중
                         P_ACHIEVE_COMPLETE = 2; //해당업적 완료

        
        public static string [] position = new string[10]
        {
            "DH","C","1B","2B","3B","SS","LF","CF","RF","DH"
        };
    }
}