
public class PvpBattingInfo
{
    //타구 벡터
    public float ballPower;
    public float angleZ;
    public float angle;
    public float angleHookSlice;
    public bool bHookorSlice;
    public bool bTopSpin;

    //번트 속성
    public bool bBunt;
    public BaseBall.BallPlay.SimulBuntType buntType;
    public BaseBall.BallPlay.SpecificBuntType buntResult;
    public int buntFielder;

    //플라이볼 정보
    public float[] possibleDis = new float[9];
    public float[] distanceToBall = new float[9];
}
