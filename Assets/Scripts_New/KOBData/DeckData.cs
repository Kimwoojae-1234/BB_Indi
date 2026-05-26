using System.Collections;
using System.Collections.Generic;

public class DeckData
{
    //타자 레벨 정보
    public Dictionary<int, int> hitterLevel;
    //타자 덱 정보 (타순, 선수인덱스)
    public Dictionary<int, int> hitter;    
    //벤치 정보
    public List<int> bench;
    //타자 수비 정보 (선수인덱스, 수비번호)
    public Dictionary<int, PositionInfo> PosInfo;

    //투수 레벨 정보
    public Dictionary<int, int> pitcherLevel;
    //투수 덱 정보 (로테이션, 선수인덱스)
    public Dictionary<int, int> pitcher;
    //불펜 정보
    public List<int> bullpen;
    //피처 로테이션
    public int PitcherRotation;
    //투수 덱 정보 (선수인덱스, 로테이션 정보)
    public Dictionary<int, RotationInfo> RotaionInfo;

    //토너먼트 투수 덱 정보 (로테이션, 선수인덱스)
    public Dictionary<int, int> tournament_pitcher;
    //토너먼트 불펜 정보
    public List<int> tournament_bullpen;
    //토너먼트 피처 로테이션
    public int TournamentPitcherRotation;
    //토너먼트 투수 덱 정보 (선수인덱스, 로테이션 정보)
    public Dictionary<int, RotationInfo> TournamentRotaionInfo;
    
    //덱에서 내선수 타순
    public int MyPlayerOrder;
    //로테이션에서 내선수 로테이션 - TwoWay에서 사용
    public int MyPlayerRotation;

    public DeckData()
    {
        hitterLevel = new Dictionary<int, int>();
        hitter = new Dictionary<int, int>();
        bench = new List<int>();
        PosInfo = new Dictionary<int, PositionInfo>();

        pitcherLevel = new Dictionary<int, int>();
        pitcher = new Dictionary<int, int>();        
        bullpen = new List<int>();
        PitcherRotation = 0;
        RotaionInfo = new Dictionary<int, RotationInfo>();

        tournament_pitcher = new Dictionary<int, int>();
        tournament_bullpen = new List<int>();
        TournamentPitcherRotation = 0;
        TournamentRotaionInfo = new Dictionary<int, RotationInfo>();

        MyPlayerOrder = 0;
        MyPlayerRotation = -1;
    }
}

public enum RotationInfo
{
    Availble = 0,
    Out_1Day = 1,
    Out_2Day = 2,
    Out_3Day = 3,
}

public enum PositionInfo
{
    SP = 1,         //StartingPitcher
    C,              //Catcher
    B1,             //FirstBaseMan
    B2,             //SecondBaseMan
    B3,             //ThirdBaseMan
    Ss,             //ShotStop
    Lf,             //LeftFielder
    Cf,             //CenterFielder
    Rf,             
}