using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

//[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : UIBase
{
    [SerializeField] private UITransition TransitionType = UITransition.Default;
    public System.Type LastWindow = null;
    private CanvasGroup canvasGroup = null;
    public override void Initialize()
    {
        base.Initialize();
        SafeArea();
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }

    public virtual void OpenWindow()
    {
        this.gameObject.SetActive(true);
        SetTransition(TransitionType);
    }


    protected virtual void SetTransition(UITransition type)
    {
        if (type != UITransition.None)
        {
            FrontUI_Transition transition = KOBManager.FrontUI.OpenPopup<FrontUI_Transition>();
            transition?.SetTransitionType(type);
        }
    }


    public RectTransform GetRectTransform()
    {
        return (RectTransform)transform;
    }

    public virtual void CloseWindow()
    {
        this.gameObject.SetActive(false);
    }

    public bool isActiveInGame()
    {
        return canvasGroup.alpha == 1 ? true : false;
    }

    protected override void Update()
    {
        base.Update();
#if UNITY_EDITOR        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (KOBManager.Popup.CheckPopupOpen() == false)
            {
                ClickBackButton();
            }
        }
#else
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (KOBManager.Popup.CheckPopupOpen() == false)
                {
                    ClickBackButton();
                }
            }
        }
#endif
    }

    public override void OpenUI()
    {
        base.OpenUI();
        OpenWindow();
    }

    public override void CloseUI()
    {
        //MainManager.UI.CloseLastOpenWindow();
        CloseWindow();
    }



    public virtual void ClickBackButton()
    {
        KOBManager.UI.BackToPreviousWindow(LastWindow);
        LastWindow = null;
    }


    public virtual void ClickHomeButton()
    {
        KOBManager.UI.OpenWindow<UI_LobbyRe>();
    }

    private void SafeArea()
    {
        var safeArea = Screen.safeArea;
        Vector2 CustomAnchor = safeArea.position;
        CustomAnchor.y = CustomAnchor.y / 2;
        var anchorMin = CustomAnchor;
        var anchorMax = safeArea.position + safeArea.size;

        float gabLeft = anchorMin.x / Screen.width;
        float gabRight = 1 - (anchorMax.x / Screen.width);
        float gab = (gabLeft > gabRight ? gabLeft : gabRight);
        if (gab > 0.045f) gab = 0.045f;

        anchorMin.x = gab;
        anchorMax.x = 1 - gab;
        anchorMin.y = 0;
        anchorMax.y = 1;

        RectTransform trans = gameObject.GetComponent<RectTransform>();
        trans.anchorMin = anchorMin;
        trans.anchorMax = anchorMax;
    }

    public virtual void PopupClose()
    {
        Debug.Log("PopupClose // Type : " + System.Type.GetType(gameObject.name));
    }
}
