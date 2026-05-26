using BaseBall.BallPlay;

[System.Serializable]
public class SendBattingInfo
{
    public int Timing;

    public float ballPower;
    public float ballAngle;
    public float ballAngleZ;
    public float hookSlice;
    public bool bHookSlice;
    public bool bTopSpin;

    public bool bBunt;
    public int buntType;    //SimulBuntType
    public int buntResult;  //SpecificBuntType
    public int buntFielder;

    //타자주자의 터보 동기화 여부
    public bool bTurboOn;



    public void Set(BallPlayManager manager)
    {
        Timing = (int)manager.batter.timing;

        //배팅
        ballPower = manager.field.ballPower;
        ballAngle = manager.field.ball.firstAngle;
        ballAngleZ = manager.field.ball.firstAngleZ;
        hookSlice = manager.field.ball.angleHookSlice;
        bHookSlice = manager.field.ball.bHookorSlice;
        bTopSpin = manager.field.ball.bTopSpin;

        //번트
        bBunt = manager.batter.bBuntHit;
        buntType = (int)manager.batter.buntType;
        buntResult = (int)manager.batter.buntResult;
        buntFielder = manager.batter.buntFielder;

        //터보
        Runner hitterRunner = manager.field.run.getHitterRunner();
        if (hitterRunner != null)
        {
            bTurboOn = hitterRunner.bTurboSkillOn;
        }
        else
        {
            bTurboOn = false;
        }

        //파울팁여부
        //bTip = (manager.batter.contact == BattingContact.TIP ? true : false);


    }

}


public enum NoHitType
{
    NoSwing = 0,
    HutSwing = 1,
    BallCountSync = 2
}

[System.Serializable]
public class SendNoHitInfo
{
    public int type;
    public bool bWildPitch;
    public bool bStrikeCheck;
    public bool bSwing;
    public bool bHutSwing;
    public bool bCheckSwing;
    public bool bBunt;
    public int ball;
    public int strike;
    public int outCount;

    public void Set(NoHitType _type, BallPlayManager manager)
    {
        type = (int)_type;
        bWildPitch = manager.pitcher.bWildPitch;
        if (_type == NoHitType.NoSwing)
        {
            bStrikeCheck = manager.bStrikeCheck;
            bSwing = manager.batter.bSwing;
        }
        else if (_type == NoHitType.HutSwing)
        {
            bHutSwing = true;
            bCheckSwing = manager.batter.bCheckSwing;
            bBunt = manager.batter.bBunt;
        }
        else if (_type == NoHitType.BallCountSync)
        {
            ball = manager.nBallCount;
            strike = manager.nStrikeCount;
            outCount = manager.nOutCount;
        }
    }
}

/*
[System.Serializable]
public class SendNoSwingInfo
{
    public bool bWildPitch;
    public bool bStrikeCheck;
    public bool bSwing;     
   

    public void Set(BallPlayManager manager)
    {
        bWildPitch = manager.pitcher.bWildPitch;
        bStrikeCheck = manager.bStrikeCheck;
        bSwing = manager.batter.bSwing;
            
    }
}

[System.Serializable]
public class SendHutSwingInfo
{
    public bool bHutSwing;
    public bool bCheckSwing;
    public bool bBunt;

    public void Set(BallPlayManager manager)
    {
        bHutSwing = true;            
        bCheckSwing = manager.batter.bCheckSwing;
        bBunt = manager.batter.bBunt;
    }
}*/

