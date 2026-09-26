using UnityEngine;

namespace BaseBall.BallPlay
{
    // The legacy result prefab still renders with NGUI. Keep its bindings separate
    // from the UGUI scoreboard used by QuickSimulator and inning transitions.
    public sealed class LegacyResultScoreboard : MonoBehaviour
    {
        private const int MaxInnings = 12;
        public GameObject[] teamObj;
        public GameObject cur;

        private readonly UILabel[,] scores = new UILabel[2, MaxInnings];
        private readonly UILabel[,] stats = new UILabel[2, 3];
        private readonly GameObject[] indicators = new GameObject[2];

        public void initScoreBoard(string awayTeam, string homeTeam, int awayIndex, int homeIndex)
        {
            for (int team = 0; team < 2; team++)
            {
                Transform row = teamObj[team].transform;
                int index = 0;
                foreach (Transform child in row.Find("score"))
                {
                    var label = child.GetComponent<UILabel>();
                    if (label == null) continue;
                    scores[team, index++] = label;
                    label.text = "0";
                    label.gameObject.SetActive(false);
                }
                index = 0;
                foreach (Transform child in row.Find("stat"))
                {
                    var label = child.GetComponent<UILabel>();
                    if (label == null) continue;
                    stats[team, index++] = label;
                    label.text = "0";
                }
                var logo = row.Find("logo").GetComponent<UISprite>();
                Util.SetSpritePixelPerfect(logo, "logo_" + (team == 0 ? awayIndex : homeIndex));
                logo.transform.localScale = new Vector2(.75f, .75f);
                row.Find("teamLabel").GetComponent<UILabel>().text = team == 0 ? awayTeam : homeTeam;
                indicators[team] = row.Find("indicator").gameObject;
            }
            scores[0, 0].gameObject.SetActive(true);
            indicators[1].SetActive(false);
        }

        public void setResult(int[] awayScore, int[] homeScore, int[] awayStat, int[] homeStat, int myteamIndex)
        {
            for (int team = 0; team < 2; team++)
            {
                for (int inning = 0; inning < MaxInnings; inning++)
                {
                    string text = ScoreText((team == 0 ? awayScore : homeScore)[inning]);
                    scores[team, inning].gameObject.SetActive(text != null);
                    if (text != null) scores[team, inning].text = text;
                }
                for (int stat = 0; stat < 3; stat++)
                    stats[team, stat].text = (team == 0 ? awayStat : homeStat)[stat].ToString();
            }
            cur.SetActive(false);
            indicators[myteamIndex].SetActive(true);
            indicators[1 - myteamIndex].SetActive(false);
        }

        private static string ScoreText(int score)
        {
            if (score == SimulParm.NOPLAY_INNING) return null;
            if (score == SimulParm.GAMEEND_INNING) return "X";
            return score < 0 ? (-score) + "X" : score.ToString();
        }
    }
}
