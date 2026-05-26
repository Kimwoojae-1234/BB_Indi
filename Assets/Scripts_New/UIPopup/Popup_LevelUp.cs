using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Popup_LevelUp : UIPopup
{
    [SerializeField] private GameObject RewardObj = null;
    [SerializeField] private GameObject ResultObj = null;


    [SerializeField] private TextMeshProUGUI LevelText = null;
    [SerializeField] private RectTransform rewardItems = null;


    [SerializeField] private RectTransform RewardPosition;
    [SerializeField] private TextMeshProUGUI RewardName;
    [SerializeField] private GameObject RewardRemain;
    [SerializeField] private TextMeshProUGUI RewardRemainNumber;
    [SerializeField] private LobbyPropertyComponent UserProperty;

    private GameConfig.Callback backCallBack = null;

    //RewardComponent CurrentItem = null;
    private List<RewardSetting> RewardList = new List<RewardSetting>();

    private delegate void RewardPresentationStep();
    private readonly Queue<RewardPresentationStep> PresentationQue = new Queue<RewardPresentationStep>();


    private int State = 0;

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        if (backCallBack != null)
        {
            backCallBack();
        }
        backCallBack = null;
    }



}
