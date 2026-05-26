using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class FrontUI_Tutorial : UIFrontUI
{
    [SerializeField] protected GameObject[] BtnObj = null;
    [SerializeField] protected GameObject[] GuideAnim = null;
    [SerializeField] protected GameObject[] DialogObl = null;
    [SerializeField] protected TextMeshProUGUI[] DialogTxt = null;
    [SerializeField] protected RectTransform Position = null;

    //Step이 MaxStep보다 작으면 대화 구간
    protected int Phase;
    protected int MaxPhase;
    protected int Step;
    protected int MaxStep;

    protected bool bDialogStep = false;

    protected bool bGuildCharacter = false;
    protected bool bGuildLeft = false;


    public virtual void Init()
    {
        SafeArea();
        Step = 0;
        Phase = 0;
        if (BtnObj != null)
        {
            for(int i =0;i < BtnObj.Length;i++) BtnObj[i].gameObject.SetActive(false);
        }
        if (GuideAnim != null)
        {
            for (int i = 0; i < GuideAnim.Length; i++) GuideAnim[i].gameObject.SetActive(false);
        }
        if (DialogObl != null)
        {
            for (int i = 0; i < DialogObl.Length; i++) DialogObl[i].gameObject.SetActive(false);
        }
    }


    protected override void Update()
    {
        base.Update();
        if (Input.GetMouseButtonUp(0) == true)
        {
            if (bDialogStep == true)
            {
                if (Step < MaxStep)
                {
                    NextGuide();
                }
            }
        }
    }


    protected virtual void NextGuide()
    {
        Step++;
        Debug.Log("Step =============>> " + Step);
        if (Step >= MaxStep)
        {
            bDialogStep = false;
            DialogueEnd();
        }
        else
        {
            DialogueSetting();
        }
    }

    protected virtual void DialogueSetting()
    {

    }


    protected virtual void DialogueEnd()
    {
        
    }

    public virtual void OnClickButton()
    {

    }



    protected void SafeArea()
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
}
