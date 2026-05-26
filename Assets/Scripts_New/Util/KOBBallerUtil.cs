
public static class KOBBallerUtil
{
    public static int GetBallerBattingPower(int idx, int level)
    {
        HitterLevelData levelData = KOBManager.Backend.Chart.HitterLevelData.GetData(idx, level);
        int value = 0;
        if(levelData != null)
        {
            value += levelData.power * 150 / 100; //파워는 1.5배
            value += levelData.contact;
            value += levelData.vision;
        }
        value *= 2;

        return value;
    }

    public static int GetBallerOverallPower(int idx, int level)
    {
        HitterLevelData levelData = KOBManager.Backend.Chart.HitterLevelData.GetData(idx, level);
        int value = 0;
        if (levelData != null)
        {
            value += levelData.power * 150 / 100; //파워는 1.5배
            value += levelData.contact;
            value += levelData.vision;
            value += levelData.fielding;
            value += levelData.throwing;
            value += levelData.speed;
        }

        return value;
    }

    public static int GetBallerFieldingPower(int idx, int level)
    {
        HitterLevelData levelData = KOBManager.Backend.Chart.HitterLevelData.GetData(idx, level);
        int value = 0;
        if (levelData != null)
        {
            value += levelData.fielding;
            value += levelData.throwing;
            value += levelData.speed;
        }
        value *= 2;

        return value;
    }


}
