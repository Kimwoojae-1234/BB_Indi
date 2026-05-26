using BaseBall.BallPlay;
[System.Serializable]
public class SendPitchInfo
{
    public int RandomSeed;

    //스킬
    public int pitchSkill;
    public int battingSkill;
    public int catcherSkill;

    //볼스피드
    public int curBallSpeed;
    //선택구종
    public int selectBallType; //(PitchingArsenal) -> setBallAndGuwee()함수에 넣어
    //미스 여부
    public bool bMissControl;
    //유저 컨트롤 밸류
    public int userControlValue; // UserControlValue


    public float courseX;
    public float courseY;

    public int hitByPitchStep;


    public void Set(BallPlayManager manager, int seed)
    {
        //랜덤시드
        RandomSeed = seed;

        //피치 스킬 파싱
        CSkill pSkill = SimulManager.GetPitchPitcherSkill();
        CSkill bSkill = SimulManager.GetPitchBatterSkill();
        CSkill cSkill = SimulManager.GetPitchCatcherSkill();
        if (pSkill != null)
        {
            pitchSkill = (int)pSkill.effectIndex;
        }
        else
        {
            pitchSkill = -1;
        }

        if (bSkill != null)
        {
            battingSkill = (int)bSkill.effectIndex;
        }
        else
        {
            battingSkill = -1;
        }

        if (cSkill != null)
        {
            catcherSkill = (int)cSkill.effectIndex;
        }
        else
        {
            catcherSkill = -1;
        }

        //피치 정보
        curBallSpeed = (int)manager.pitcher.curBallSpeed;
        selectBallType = (int)manager.pitcher.selectedBallIndex;
        userControlValue = (int)manager.pitcher.userControlValue;

        //로케이션 정보
        float realX = manager.pitcher.courseX2 + manager.pitcher.preHenkaX;
        float realY = manager.pitcher.courseY2 + manager.pitcher.preHenkaY;
        //UnityEngine.Debug.Log("보내는 값 courseX = " + realX + "    ====  courseY = " + realY);
        courseX = -realX * Zone.STRIKE_ZONE_WIDTH / Zone.STRIKE_ZONE_WIDTH_PV;
        courseY = realY * Zone.STRIKE_ZONE_HEIGHT / Zone.STRIKE_ZONE_HEIGHT_PV;
        //UnityEngine.Debug.Log("보내는 변환된 값 courseX = " + courseX + "    ====  courseY = " + courseY);

        //폭투정보
        bMissControl = manager.pitcher.bMissControl;
        hitByPitchStep = manager.pitcher.hitByPitchStep;

    }

}


[System.Serializable]
public class SendPickOffInfo
{
    public int RandomSeed;

    public int nTargetIndex;

    public void Set(int target, int seed)
    {
        RandomSeed = seed;
        nTargetIndex = target;
    }

}

[System.Serializable]
public class SendStealInfo
{
    public int RandomSeed;

    public int nTargetIndex;

    public void Set(int target, int seed)
    {
        RandomSeed = seed;
        nTargetIndex = target;
    }

}