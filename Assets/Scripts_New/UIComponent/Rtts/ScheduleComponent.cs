using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using DG.Tweening;

public class ScheduleComponent : MonoBehaviour
{
    [SerializeField] private ScheduleTeam MyTeam;
    [SerializeField] private ScheduleTeam OppTeam;
    [SerializeField] private SkeletonGraphic VsAnim;
    [SerializeField] private TextMeshProUGUI DateTxt;
    [SerializeField] private TextMeshProUGUI Result1Txt;
    [SerializeField] private TextMeshProUGUI Result2Txt;

    [SerializeField] private GameObject[] btnObj; 


    int gab = 0;// 이값이 0이면 오늘을 나타냄
    bool isToday = false;

    public void InitComp()
    {
        gab = 0;
        isToday = false;
        setSchedule(gab);
    }



    private void setSchedule(int _gab)
    {
        RttsSchedule schedule = KOBManager.Rtts.GetMySchedule(_gab); //스케쥴 정보를 얻어와서 0~9 인덱스 얻어온후 
        
        MyTeam.Init(0, _gab);
        OppTeam.Init(schedule.opponent, _gab);

        if (_gab == 0) //오늘
        {
            if(isToday == false)
            {
                MyTeam.rectTrans.anchoredPosition = new Vector2(-50, 0);
                MyTeam.rectTrans.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutQuad);
                OppTeam.rectTrans.anchoredPosition = new Vector2(50, 0);
                OppTeam.rectTrans.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutQuad);
                isToday = true;
            }

            SpineUtil.ReplayAnimation(VsAnim);
            DateTxt.text = "Today Match";
            DateTxt.color = Color.white;

            Result1Txt.gameObject.SetActive(false);
            Result2Txt.gameObject.SetActive(false);
        }
        else
        {
            if (isToday == true)
            {
                MyTeam.rectTrans.anchoredPosition = new Vector2(0, 0);
                MyTeam.rectTrans.DOAnchorPos(new Vector2(-50,0), 0.3f).SetEase(Ease.OutQuad);
                OppTeam.rectTrans.anchoredPosition = new Vector2(0, 0);
                OppTeam.rectTrans.DOAnchorPos(new Vector2(50, 0), 0.3f).SetEase(Ease.OutQuad);
                isToday = false;
            }

            VsAnim.gameObject.SetActive(false);
            Result1Txt.gameObject.SetActive(true);
            Result2Txt.gameObject.SetActive(true);
            int round = (KOBManager.Rtts.PlayGame) + gab; //zero base
            if (_gab < 0) //이전 경기
            {                
                DateTxt.text = string.Format("{0} Round Result", KOBTextUtil.ToOrdinal(round + 1));  //zerobase이므로 1 더해줄것
                DateTxt.color = new Color32(149, 165, 166, 255); // #95A5A6 회색
                
                int[] Score = null;
                if(KOBManager.MyInfo.GameData.RttsInfo.LeagueResult.ContainsKey(round) == true) //데이터 있음
                {
                    Score = KOBManager.MyInfo.GameData.RttsInfo.LeagueResult[round].score;
                }                
                if (Score != null)
                {
                    Result1Txt.text = KOBTextUtil.SetResultState(Score[0], Score[1]);
                    Result1Txt.color = KOBTextUtil.SetResultColor(Score[0], Score[1]);
                    Result2Txt.text = string.Format("{0}-{1}", Score[0], Score[1]);
                }
            }
            else
            {
                DateTxt.text = string.Format("{0} Round Match", KOBTextUtil.ToOrdinal(round));
                DateTxt.color = Color.white;
                Result1Txt.text = "Upcoming";
                Result1Txt.color = new Color32(26, 188, 156, 255); 
                Result2Txt.text = "VS";
            }
        }

        //화살표 세팅 해줄것
        int PlayGame = KOBManager.Rtts.PlayGame;
        btnObj[0].gameObject.SetActive(PlayGame + _gab > 0);
        btnObj[1].gameObject.SetActive(PlayGame + _gab < KOBManager.Rtts.TotalGame);
    }

    public void OnClickLeft()
    {
        int PlayGame = KOBManager.Rtts.PlayGame;
        if(PlayGame + gab > 0)
        {
            gab--;
            setSchedule(gab);
        }
    }

    public void OnClickRight()
    {
        int PlayGame = KOBManager.Rtts.PlayGame;
        if (PlayGame + gab < KOBManager.Rtts.TotalGame)
        {
            gab++;
            setSchedule(gab);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnClickLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnClickRight();
        }
    }
#endif
}
