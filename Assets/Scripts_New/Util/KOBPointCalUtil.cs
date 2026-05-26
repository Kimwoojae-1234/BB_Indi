/// <summary>
/// 인게임에서 발생되는 포인트 계산기
/// </summary>
public static class KOBPointCalUtil
{


    public static int CaculateAddTrophy(int League, int Result)
    {
        int addTrophy = 0;
        RttsInfo info = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);
        if (Result == 0) //승리시
        {

            addTrophy = info.Win;
        }
        else if (Result == 1) //무승부시
        {
            addTrophy = info.Draw;
        }
        else //if (req.Result == 2) //패배시
        {
            addTrophy = info.Lose;
        }


        return addTrophy;
    }


    public static int CaculateAddXP(TRequestBattleEnd req)
    {
        //미션과 동시에 세팅해야 할듯

        return 0;
    }


    public static int CaculateAddFame(int League, int[] myRecord)
    {
        RttsInfo info = KOBManager.Backend.Chart.RttsInfo.GetRttsInfo(League);

        return KOBPointCalUtil.Cal_BallerFame(myRecord, info.Fame);
    }


    public static int Cal_BallerFame(int [] record, int value)
    {
        int fame = 0;

        return (fame * value) / 10000;
    }

    public static int Cal_ResultValue(int myScore, int oppScore)
    {
        int value = myScore - oppScore;
        if (value > 0) return 0;
        else if (value < 0) return 2;
        else return 1;
    }
}
