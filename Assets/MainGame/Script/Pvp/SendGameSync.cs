using BaseBall.BallPlay;
[System.Serializable]
public class SendGameSync
{
    public bool bMyTurn;
    public bool[] bOnBase = new bool[3];
    public int[] runnerIndex = new int[3];
    public int[] batterLineupCount = new int[2];

    public int inning;
    public int ballCount;
    public int strikeCount;
    public int outCount;


    public int[] scoreNum = new int[2];
    public int[] hitNum = new int[2];
    public int[] errorNum = new int[2];



    public void Set(BallPlayManager manager)
    {
        //내턴 여부
        bMyTurn = !manager.bMyTurn;

        //주루 동기화
        for (int i = 0; i < 3; i++)
        {
            bOnBase[i] = manager.field.run.bOnBase[i];
            runnerIndex[i] = -1;
        }
        Runner first = manager.field.run.getRunner(FieldParm.FIRSTBASE_INDEX);
        Runner second = manager.field.run.getRunner(FieldParm.SECONDBASE_INDEX);
        Runner third = manager.field.run.getRunner(FieldParm.THIRDBASE_INDEX);

        if (first != null)
        {
            runnerIndex[0] = first.lineupCount;
        }

        if (second != null)
        {
            runnerIndex[1] = second.lineupCount;
        }

        if (third != null)
        {
            runnerIndex[2] = third.lineupCount;
        }

        //타순
        batterLineupCount[0] = SimulPlayerManager.GetLineupCount(1);
        batterLineupCount[1] = SimulPlayerManager.GetLineupCount(0);

        //이닝 및 카운트 정보
        inning = manager.nInningCount;
        ballCount = manager.nBallCount;
        strikeCount = manager.nStrikeCount;
        outCount = manager.nOutCount;

        //RHE 정보
        scoreNum[0] = manager.nGameScore[1];
        scoreNum[1] = manager.nGameScore[0];

        hitNum[0] = manager.nHitCount[1];
        hitNum[1] = manager.nHitCount[0];

        errorNum[0] = manager.nErrorCount[1];
        errorNum[1] = manager.nErrorCount[0];

    }
}


[System.Serializable]
public class SendAskSync
{
    public int type;

    public void Set()
    {
        type = 0;
    }
}