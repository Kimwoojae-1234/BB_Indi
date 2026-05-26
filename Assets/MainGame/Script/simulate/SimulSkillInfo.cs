
namespace BaseBall.BallPlay
{
    /// <summary>
    /// 스킬이 사용된 단계
    /// </summary>    
    public enum SkillUseStep
    {
        BattingView = 0,
        Pitching = 1,
        Fielding = 2,
    }

    /// <summary>
    /// vs여부
    /// </summary>
    public enum VsResult
    {
        None = 0,
        OffenseWin = 1,
        DefenseWin = 2
    }

    //[System.Serializable]
    public class SimulSkillInfo
    {
        public bool bAvailable = false;

        public VsResult vsType = VsResult.None;
        
        public SkillID offenseID = SkillID.None;
        public int offenseRank;

        public SkillID defenseID = SkillID.None;
        public int defenseRank;

        public SkillID catcherID = SkillID.None;
        public int catcherRank;

        public void setOffense(int offense, int oRank)
        {
            offenseID = (SkillID)offense;
            offenseRank = oRank;
            bAvailable = true;
        }

        public void setDefense(int defense, int dRank)
        {
            defenseID = (SkillID)defense;
            defenseRank = dRank;
            bAvailable = true;
        }

        public void setCatcher(int catcher, int cRank)
        {
            catcherID = (SkillID)catcher;
            catcherRank = cRank;
            bAvailable = true;
        }
    }
}