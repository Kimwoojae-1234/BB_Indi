
public class OverallData 
{
    public int Overall, LastOverall;
    public int Power, LastPower;
    public int Contact, LastContact;
    public int Vision, LastVision;
    public int Fielding, LastFielding;
    public int Throwing, LastThrowing;
    public int Speed, LastSpeed;

    public void OverallUpdate(int pow, int con, int vis, int fld, int thr, int spd)
    {
        Power = pow;
        Contact = con;
        Vision = vis;
        Fielding = fld;
        Throwing = thr;
        Speed = spd;

        //오버롤 계산
        int baseValue = KOBManager.Backend.Setting.BaseStatValue + 1;
        Overall = baseValue 
                + (pow - baseValue)
                + (con - baseValue)
                + (vis - baseValue)
                + (fld - baseValue) / 2
                + (thr - baseValue) / 2
                + (spd - baseValue) / 2;
    }
}
