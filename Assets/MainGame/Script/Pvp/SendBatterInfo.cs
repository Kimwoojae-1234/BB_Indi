using BaseBall.BallPlay;

[System.Serializable]
public class SendBatterInfo
{

    //랜덤시드
    public int RandomSeed;

    //투타 스킬
    public int pitcherSkill;
    public int batterSkill;
    public int batterSkillFlag;
    public bool bOffenseSkillWin;
    public bool bVsType;


    //에러플래그
    public bool[] bCatchError = new bool[9];
    public bool[] bThrowError = new bool[9];


    public void Set(BallPlayManager manager, int seed)
    {
        RandomSeed = seed;

        CSkill pSkill = SimulManager.GetPitcherSkill();
        CSkill bSkill = SimulManager.GetBatterSkill();
        if (pSkill != null)
        {
            pitcherSkill = (int)pSkill.effectIndex;
        }
        else
        {
            pitcherSkill = -1;
        }

        if (bSkill != null)
        {
            batterSkill = (int)bSkill.effectIndex;
        }
        else
        {
            batterSkill = -1;
        }
        bOffenseSkillWin = SimulManager.CheckVsBatterWin();
        batterSkillFlag = (int)manager.batterSkillFlag;
        bVsType = manager.vsType;


        for (int i = 0; i < 9; i++)
        {
            bCatchError[i] = manager.field.fielder[i].bCatchErrorFlag;
            bThrowError[i] = manager.field.fielder[i].bThrowErrorFlag;
        }
    }
}

[System.Serializable]
public class SendPowerBattingInfo
{
    public bool bPowerBatting;

    public void Set(BallPlayManager manager)
    {
        bPowerBatting = manager.batter.bGangTa;
    }

}

/*
[System.Serializable]
public class SendRunnerSyncInfo
{
    public int arrayIndex;
    public float timeValue;

    public void Set(int index, float tValue)
    {
        arrayIndex = index;
        timeValue = tValue;
    }
}*/


public enum FieldSyncType
{
    Target,
    OneMoreValue,
    BaseSafe
}

[System.Serializable]
public class SendFieldSyncInfo
{
    //public int posIndex;
    //public int nTargetIndex;
    public int type;
    public int arrayIndex;
    public float value;

    public void Set(FieldSyncType _type, int index, float _value)
    {
        type = (int)_type;
        arrayIndex = index;
        if (_type == FieldSyncType.OneMoreValue)
        {
            value = _value;
        }
        else
        {
            value = (int)_value;
        }
    }
}