using UnityEngine;

public class Popup_GameResult : UIPopup
{
    [SerializeField] private GameObject[] Obj;

    private RttsGameResult _resultInfo;

    public override void Set(Intent it = null)
    {
        base.Set(it);
        _resultInfo = it != null ? it["RttsGameResult"] as RttsGameResult : null;
        if (_resultInfo == null)
        {
            Debug.LogError("Popup_GameResult에 RTTS 경기 결과가 전달되지 않았습니다.");
            return;
        }

        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(i == 0);
        ResultTeam resultTeam = Obj.Length > 0 ? Obj[0].GetComponent<ResultTeam>() : null;
        resultTeam?.SetResultTeam(_resultInfo);

        if (Obj.Length > 2)
        {
            ResultStat resultStat = Obj[2].GetComponent<ResultStat>();
            if (resultStat != null) resultStat.bInit = false;
        }
    }

    public void OnClickStat()
    {
        if (_resultInfo == null || Obj.Length <= 2) return;
        Obj[2].SetActive(true);
        Obj[2].GetComponent<ResultStat>()?.SetResultStat(_resultInfo);
    }

    public void OnClickNext()
    {
        if (_resultInfo == null || Obj.Length <= 1) return;
        for (int i = 0; i < Obj.Length; i++) Obj[i].SetActive(i == 1);
        Obj[1].GetComponent<ResultPlayer>()?.Set(_resultInfo);
    }

    public void OnClickExit()
    {
        KOBManager.Rtts.BackToLobby();
        Close();
    }
}
