using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonActionSmall : MonoBehaviour {

	public GameObject mainController;
    public UITweener[] tween;
    public float delay = 0.3f;

    public int width = 300;
    public int height = 100;

    public GameObject[] sizeObj;

    public UIPanel[] panels;

    private string message;
    private bool bActive = false;
    private bool bSelect = false;

    void Awake()
    {
        int count = tween.Length;
        for (int i = 0; i < count; i++)
        {
            TweenAlpha alpha = tween[i].GetComponent<TweenAlpha>();
            if (alpha != null)
            {
                tween[i].GetComponent<UISprite>().alpha = 0;
            }
            tween[i].enabled = false;
        }
        
        
    }

    void Start()
    {
        if (panels != null)
        {
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].depth = mainController.GetComponent<UIPanel>().depth + i + 1000;
            }
        }
    }

    public void Init(string sendMessage)
    {
        if (bActive == false)
        {
            message = sendMessage;
            StartCoroutine(startAnim(false));
        }
    }

    public void release()
    {
        
    }

    public void hoverOut()
    {
        if (bSelect == true)
        {
            bSelect = false;
        }
    }


    private IEnumerator startAnim(bool bDestroy)
    {
        bActive = true;
        
        int count = tween.Length;
        for (int i = 0; i < count; i++)
        {
            tween[i].enabled = true;
            tween[i].ResetToBeginning();
            tween[i].PlayForward();
        }
        yield return new WaitForSeconds(delay);

        bSelect = true;
        
    }


    private IEnumerator endAnim()
    {
        bActive = true;

        int count = tween.Length;
        for (int i = 0; i < count; i++)
        {
            TweenAlpha alpha = tween[i].GetComponent<TweenAlpha>();
            if (alpha != null)
            {
                tween[i].enabled = true;
                tween[i].PlayReverse();
            }
        }

        yield return new WaitForSeconds(delay);

        if (bSelect == true)
        {
            mainController.SendMessage(message);
        }
        bActive = false;

    }

#if UNITY_EDITOR
    public void setData()
    {
        sizeObj[0].GetComponent<UIPanel>().baseClipRegion = new Vector4(0,0,width-2, height);
        sizeObj[1].GetComponent<UISprite>().SetDimensions(width, height);
    }
#endif

}
