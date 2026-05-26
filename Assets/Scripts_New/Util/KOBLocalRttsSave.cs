using System;

[Serializable]
public class KOBLocalRttsSave
{
    public int Standing { get; private set; }
    public int HrRank { get; private set; }
    public int AvgRank { get; private set; }
    public int RbiRank { get; private set; }
    public int HitSRank { get; private set; }
    public int OpsRank { get; private set; }


    public KOBLocalRttsSave()
    {
        Standing = -1;
        HrRank = -1;
        AvgRank = -1;
        RbiRank = -1;
        HitSRank = -1;
        OpsRank = -1;
    }


    public static void Save(KOBLocalRttsSave value)
    {

    }

    public static KOBLocalRttsSave Load()
    {

        return null;
    }


    public void SaveStading(int _Standing)
    {
        Standing = _Standing;
    }

    public void SaveHRRank(int _value)
    {
        HrRank = _value;
    }

    public void SaveAvgRank(int _value)
    {
        AvgRank = _value;
    }
    public void SaveRbiRank(int _value)
    {
        RbiRank = _value;
    }
    public void SaveHitRank(int _value)
    {
        HitSRank = _value;
    }
    public void SaveOpsRank(int _value)
    {
        OpsRank = _value;
    }
}
