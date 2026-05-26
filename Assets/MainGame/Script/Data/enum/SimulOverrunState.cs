namespace BaseBall.BallPlay
{
    //
    public enum SimulOverrunState
    {
        NONE,
        OUT,
        SAFE,
        LaserOut,   //레이저 송구에 의해 아웃
        VsOut,      //레이저 vs 주루센스 -> 레이저 승
        VsSafe      //레이저 vs 주루센스 -> 주루센스 승
    }

}
