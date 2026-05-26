public class PvpPitchInfo
{
    //볼스피드
    public int curBallSpeed;
    //선택구종
    public int selectBallType; 
    //미스 여부
    public bool bMissControl;
    //유저 컨트롤 밸류
    public int userControlValue; // UserControlValue
    //위치
    public float courseX;
    public float courseY;
    //힛바이피치
    public int hitByPitchStep;
}


public class PvpPitchInfo2
{
    //카운트 동기화
    public int ballCount;
    public int strikeCount;
    public int outCount;

    //에러정보
    public bool[] bCatchError = new bool[9];
    public bool[] bThrowError = new bool[9];

}
