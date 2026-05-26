using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    public enum PlayMode
    {
        RTTS,
        Tournament
    }


    [SerializeField] private CanvasGroup[] bg;
    
    

    private PlayMode bgMode = PlayMode.RTTS;
    private PlayMode lastMode = PlayMode.RTTS;
    // Start is called before the first frame update
    void Awake()
    {
        KOBManager.UI.InitBackground(this);
    }


    public void ChangeBackGround(PlayMode Mode)
    {
        if (bgMode != Mode)
        {
            lastMode = bgMode;
            bgMode = Mode;

            if (changeBgProcess!= null)
            {
                StopCoroutine(changeBgProcess);
                changeBgProcess = null;
            }
            changeBgProcess = ChangeBgProcess(bg[(int)lastMode], bg[(int)bgMode]);
            StartCoroutine(changeBgProcess);
        }
    }

    private IEnumerator changeBgProcess = null;

    private IEnumerator ChangeBgProcess(CanvasGroup fadeOut, CanvasGroup fadeIn)
    {
        fadeOut.gameObject.SetActive(true);
        fadeOut.alpha = 1;
        fadeIn.gameObject.SetActive(true);
        fadeIn.alpha = 0;

        float delay = 0;
        float alpha = 0;
        while(delay < 0.3f)
        {
            yield return null;
            delay += Time.deltaTime;
            alpha = (delay / 0.3f);
            fadeIn.alpha = alpha;
            fadeOut.alpha = 1 - alpha;

            if (delay > 0.3f)
            {
                break;
            }
        }

        fadeOut.gameObject.SetActive(false);
        fadeIn.gameObject.SetActive(true);
        fadeIn.alpha = 1;
    }

}
