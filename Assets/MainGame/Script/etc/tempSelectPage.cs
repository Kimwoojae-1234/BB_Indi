using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BaseBall.BallPlay;

public class tempSelectPage : MonoBehaviour {

    public static int SKILL_TYPE = 0;
    public static int ERROR_PER = 0;
    public static int SP_PER = 0;

    public static int FIELDING_STAT = 500;
    public static int THROW_STAT = 500;
    public static int RUNNING_STAT = 500;
    public static int PITCHING_STAT = 500;
    public static int BATTING_STAT = 500;
    public static int CONTACT_STAT = 500;


    public static PitchingArsenal pitch1 = PitchingArsenal.FASTBALL;
    public static PitchingArsenal pitch2 = PitchingArsenal.CURVE;
    public static PitchingArsenal pitch3 = PitchingArsenal.FORK;
    public static PitchingArsenal pitch4 = PitchingArsenal.SLIDER;
    public static PitchingArsenal pitch5 = PitchingArsenal.CHANGEUP;



    public UISprite button1, button2;


    bool bInit;
	// Use this for initialization
	void Start () {
        bInit = false;
        Mode.bPitchingViewActive = false;
        Mode.bBattingSPMode = false;

        if (button1 != null)
        {
            button1.color = new Color(0, 0, 0);
            button1.transform.Find("Label").GetComponent<UILabel>().text = "[aaaaaa]투수 모드 OFF";
        }
        if (button2 != null)
        {
            button2.color = new Color(0, 0, 0);
            button2.transform.Find("Label").GetComponent<UILabel>().text = "[aaaaaa]특수 능력 OFF";
        }

    }


    public void pitcherMode(UILabel label)
    {
        if (Mode.bPitchingViewActive == true)
        {
            button1.color = new Color(0, 0, 0);
            button1.transform.Find("Label").GetComponent<UILabel>().text = "[aaaaaa]투수 모드 OFF";
            Mode.bPitchingViewActive = false;
        }
        else
        {
            button1.color = new Color(1, 1, 1);
            button1.transform.Find("Label").GetComponent<UILabel>().text = "[ffffff]투수 모드 ON";
            Mode.bPitchingViewActive = true;
        }
    }


    public void spMode(UILabel label)
    {
        if (Mode.bBattingSPMode == true)
        {
            button2.color = new Color(0, 0, 0);
            button2.transform.Find("Label").GetComponent<UILabel>().text = "[aaaaaa]특수 능력 OFF";
            Mode.bBattingSPMode = false;
        }
        else
        {
            button2.color = new Color(1, 1, 1);
            button2.transform.Find("Label").GetComponent<UILabel>().text = "[ffffff]특수 능력 ON";
            Mode.bBattingSPMode = true;
        }
    }


    public void dongneYagu()
    {
        SKILL_TYPE = 0;
        ERROR_PER = 22;
        SP_PER = 0;

        FIELDING_STAT = 400;
        THROW_STAT = 400;
        RUNNING_STAT = 350;
        PITCHING_STAT = 600;
        BATTING_STAT = 550;
        CONTACT_STAT = 700;
        pitch1 = PitchingArsenal.FASTBALL;
        pitch2 = PitchingArsenal.CURVE;
        pitch3 = PitchingArsenal.FORK;
        pitch4 = PitchingArsenal.SLIDER;
        pitch5 = PitchingArsenal.CHANGEUP;

        GameStart();
    }

    public void kboYagu()
    {
        SKILL_TYPE = 1;
        ERROR_PER = 4;
        SP_PER = 40;

        FIELDING_STAT = 650;
        THROW_STAT = 750;
        RUNNING_STAT = 650;
        PITCHING_STAT = 850;
        BATTING_STAT = 750;
        CONTACT_STAT = 850;

        pitch1 = PitchingArsenal.FASTBALL;
        pitch2 = PitchingArsenal.SLURVE;
        pitch3 = PitchingArsenal.TWOSEAM;
        pitch4 = PitchingArsenal.SINKER;
        pitch5 = PitchingArsenal.CIRCLE;

        GameStart();
    }

    public void mlbYagu()
    {
        SKILL_TYPE = 2;
        ERROR_PER = 0;
        SP_PER = 100;

        FIELDING_STAT = 800;
        THROW_STAT = 1000;
        RUNNING_STAT = 800;
        PITCHING_STAT = 1050;
        BATTING_STAT = 850;
        CONTACT_STAT = 980;

        pitch1 = PitchingArsenal.FASTBALL;
        pitch2 = PitchingArsenal.GIRO_CURVE;
        pitch3 = PitchingArsenal.VULCAN;
        pitch4 = PitchingArsenal.CUT_FAST;
        pitch5 = PitchingArsenal.FRISBEE;

        GameStart();
    }
       



    void GameStart()
    {
        if (bInit == false)
        {
            bInit = true;
            AsyncOperation async = SceneManager.LoadSceneAsync("MainLoading");
        }
    }
    
}
