using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class Ui_RewardDirectionItem : UIItem
{
    [SerializeField]
    private Image RewardIconBg = null;
    [SerializeField]
    private Image RewardIcon = null;
    [Range(1f, 100f)]
    [SerializeField]
    private float HideDirectionSpeed = 1;
    [SerializeField]
    private AnimationCurve twinkleCurve;
    [SerializeField]
    private UIBezierCurve bezierCurve;

    public UIBezierCurve BezierCurve { get { return bezierCurve; } }
    private Vector3 FinalPosition = Vector3.zero;
    private float HideDirectionTime = 0.0f;
    private AnimationCurve alphaCurve;
    private float alphaTime;
    private bool startHideDirection = false;
    private Action EndCallbackFunc;
    //private GameDefine.eRewardDirection directionType = GameDefine.eRewardDirection.MAX;
   

    public void SetRewardItem(KOBReward type, Vector3 StartWorldPosition, Vector3 FinalPosition, AnimationCurve moveCurve, AnimationCurve alphaCurve, Action endCallback = null)
    {
        float range = Mathf.Abs(StartWorldPosition.y - FinalPosition.y);
        float width = 0.3f; // 1.0f is Screen Size.
        float height = range * 0.5f;

        this.alphaTime = Time.time;
        this.alphaCurve = alphaCurve;
        this.FinalPosition = FinalPosition;
        EndCallbackFunc = endCallback;
        
        RewardIconBg.enabled = true;
        //RewardIconBg.SetNativeSize();

        Sprite RewardImage = null;
        if (type == KOBReward.Gold) RewardImage = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "StatusBarIcon_Gold");
        else if (type == KOBReward.Gem || type == KOBReward.Gem_Free) RewardImage = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "StatusBarIcon_Gem");
        else if (type == KOBReward.Energy) RewardImage = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "StatusBarIcon_Energy");

        if (RewardImage != null)
        {
            RewardIcon.sprite = RewardImage;
            RewardIcon.SetNativeSize();
        }
        SetRandomPosition(StartWorldPosition, width, height, moveCurve, alphaCurve);
    }



    private void SetRandomPosition(Vector3 StartPosition, float width, float height, AnimationCurve moveCurve, AnimationCurve alphaCurve)
    {
        this.transform.rotation = Quaternion.identity;
        this.transform.position = StartPosition;
        bezierCurve.SetBezierPosition_0(StartPosition);
        bezierCurve.SetBezierPosition_1(GetPosition1(StartPosition, width, height));
        bezierCurve.SetBezierPosition_2(GetPosition2(FinalPosition, width * 0.5f, height));
        bezierCurve.SetBezierPosition_3(FinalPosition);
        bezierCurve.SetEndCallBackFunc(BezierEndCallback, 0.9f);
        bezierCurve.Play(moveCurve);
    }

    private void BezierEndCallback()
    {
        startHideDirection = true;
        
    }

    protected override void Update()
    {
        if (alphaCurve != null)
        {
            RewardIcon.color = new Color(1.0f, 1.0f, 1.0f, alphaCurve.Evaluate(Time.time - alphaTime));
            RewardIconBg.color = new Color(1.0f, 1.0f, 1.0f, alphaCurve.Evaluate(Time.time - alphaTime) * twinkleCurve.Evaluate(Time.time - alphaTime));
        }
        if (RewardIconBg.sprite == null)
        {
            this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, (Time.time - alphaTime) * 360.0f);
        }
        if(startHideDirection)
        {
            HideDirectionTime += Time.deltaTime * HideDirectionSpeed;
            if(HideDirectionTime>=1)
            {
                startHideDirection = false;
                HideDirectionTime = 0;
                //FlashDirection(directionType);
                if (EndCallbackFunc != null)
                {
                    EndCallbackFunc();                  
                }
                    
            }
        }
    }


    public void ResetDirectionItem()
    {
        startHideDirection = false;
        HideDirectionTime = 0;
    }

    public override void SetParentWidnow(Transform parent_window)
    {
        base.SetParentWidnow(parent_window);
    }
    private Vector3 GetPosition1(Vector3 start, float width, float height)
    {
        float range = UnityEngine.Random.Range(-width, width);
        return new Vector3(start.x + range, start.y + UnityEngine.Random.Range(-height, height), start.z);
    }
    private Vector3 GetPosition2(Vector3 finish, float width, float height)
    {
        float range = UnityEngine.Random.Range(-width, width);
        return new Vector3(finish.x + range, finish.y - UnityEngine.Random.Range(0, height), finish.z);
    }
    private Vector3 GetPosition1FieldBonus(Vector3 start, float width, float height)
    {
        return new Vector3(start.x + width, start.y + height, start.z);
    }
}
