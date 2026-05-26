
namespace BaseBall.BallPlay
{
    public class TeamStat
    {
        public int[] score = new int[2];
        public int[] hitCount = new int[2];
        public int[] hrCount = new int[2];
        public int[] stealCount = new int[2];
        public int[] kCount = new int[2];
        public int[] dpCount = new int[2];
        public int[] errorCount = new int[2];
        public int[] bbCount = new int[2];

        public int[,] inningScore = new int[2, 12];
    }
}
