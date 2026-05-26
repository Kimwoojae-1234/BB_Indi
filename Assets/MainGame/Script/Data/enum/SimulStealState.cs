
namespace BaseBall.BallPlay
{
    //
    public enum SimulStealState
    {
        NONE = 0,
        Success =1,
        Success_Skill = 2,        
        Fail = 3,
        Fail_Skill = 4,
        DoubleSteal = 5,
        VsSkill = 6,
        VsSkill_RunnerWin = 7,
        VsSkill_CatcherWin = 8,
        Error = 10,
        PickOffOut = 11,
        PickOffLaserOut = 12,
        PickOffVsOut = 13,
        PickOffVsSafe = 14,
    }
}
