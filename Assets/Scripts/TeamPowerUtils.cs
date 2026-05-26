using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebConnector;

namespace Utils
{
    public class TeamPowerUtils
    {
        private const float pitcherPwConst = 2.272727f; //팀투수력 맞춤 상수
        private const float offensePwConst = 1.785714f; //팀공격력 맞춤 상수
        private const float defensePwConst = 1.785714f; //팀수비력 맞춤 상수

        /**
	 * 선수카드 종합능력 계산
	 */
        public static int calCardPower(GameCardInfo cardInfo)
        {
            return (cardInfo != null) ? calCardPower(cardInfo.abilities) : 0;
        }
        /**
         * 선수카드 종합능력 계산
         */
        public static int calCardPower(Dictionary<CardAbCode, int[]> abs)
        {
            if (abs != null) {
                int totalAbSum = 0;
                int abSize = 0;

                foreach (KeyValuePair<CardAbCode, int[]> kvp in abs) {
                    if (kvp.Key != CardAbCode.TJ && kvp.Key != CardAbCode.VC) {
                        totalAbSum += kvp.Value.Sum();
                        if (kvp.Key != CardAbCode.SM && kvp.Key != CardAbCode.PW) {
                            abSize++;
                        }
                    }
                }

                if (abSize >= 5) {
                    return (int)Math.Round(totalAbSum / 6f);
                } else if (abSize >= 4) {
                    return (int)Math.Round(totalAbSum / 5.2f);
                } else {
                    return (int)Math.Round(totalAbSum / 4.4f);
                }
            }
            return 0;
        }

        /**
         * 팀전력 계산
         * @return array of { 팀공격력, 팀수비력, 팀투수력, 팀전력 }
         */
        public static TeamPower calTeamPower(List<GameCardInfo> majors)
        {
            int teamOffense = 0, teamDefense = 0, teamPitcher = 0, totPower = 0;

            if (majors != null)
            {                
                foreach (GameCardInfo cardInfo in majors)
                {
                    int cardPw = calCardPower(cardInfo);

                    if (CardUtils.isPitcher(cardInfo.cardId))
                    {
                        teamPitcher += cardPw;
                    }
                    else
                    {
                        int[] hitterPower = calHitterPower(cardInfo);

                        teamOffense += hitterPower[0];
                        teamDefense += hitterPower[1];
                    }

                    totPower += cardPw;
                }
            }

            teamPitcher = (int)Math.Round(teamPitcher * pitcherPwConst);
            teamOffense = (int)Math.Round(teamOffense * offensePwConst);
            teamDefense = (int)Math.Round(teamDefense * defensePwConst);

            return TeamPower.of(teamOffense, teamDefense, teamPitcher, totPower);
        }

        /**
         * 타자 공격력, 수비력 계산
         * @return array of { 공격력, 수비력 }
         */
        private static int[] calHitterPower(GameCardInfo cardInfo)
        {
            int offense = 0;
            int defense = 0;
            Dictionary<CardAbCode, int[]> abilities = cardInfo.abilities;

            int power = getTotalAb(abilities, CardAbCode.PW);
            int contact = getTotalAb(abilities, CardAbCode.CT);
            int battingeye = getTotalAb(abilities, CardAbCode.BE);
            int running = getTotalAb(abilities, CardAbCode.RN);

            int throwing = getTotalAb(abilities, CardAbCode.TW);
            int fielding = getTotalAb(abilities, CardAbCode.FD);

            offense = (int)Math.Round((power + contact + battingeye + running) / 4f);
            defense = (int)Math.Round((throwing + fielding) / 2f);
            
            return new int[] { offense, defense };
        }

        private static int getTotalAb(Dictionary<CardAbCode, int[]> abs, CardAbCode abCode)
        {
            int[] vals = abs[abCode];

            if (vals != null)
            {
                return vals.Sum();
            }

            return 0;
        }
    }
}
