using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FrontUI_ToastPopup : UIFrontUI
{
    [SerializeField] private TextMeshProUGUI PopupText = null;
    [SerializeField] private RectTransform ToastTransform = null;


    GameConfig.Callback CallBack = null;

    public void Init(string Text, GameConfig.Callback _callBack = null)
    {
        CallBack = _callBack;
        gameObject.GetComponent<CanvasGroup>().alpha = 1;
        ToastTransform.anchoredPosition = new Vector2(0, 94);
        if (_process != null)
        {
            StopCoroutine(_process);
            _process = null;
        }
        _process = PopupProcess();
        PopupText.text = Text;
        Open();        
        StartCoroutine(_process);
    }


    IEnumerator _process = null;

    IEnumerator PopupProcess()
    {
        ToastTransform.DOAnchorPos(new Vector2(0, -91), 0.3f);//  .DOLocalMoveY(-91, 0.3f);
        yield return new WaitForSeconds(2.0f);
        if(CallBack != null)
        {
            CallBack();
            CallBack = null;
        }
        gameObject.GetComponent<CanvasGroup>().DOFade(0, 0.3f);
        yield return new WaitForSeconds(0.31f);
        Close();
    }
    
}
