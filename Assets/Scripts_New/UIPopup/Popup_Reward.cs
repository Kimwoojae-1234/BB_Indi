using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using DG.Tweening;

public class Popup_Reward : UIPopup
{
    [SerializeField] private GameObject RewardEvent;
    [SerializeField] private GameObject ResultEvent;


    [SerializeField] private RectTransform RewardPosition;
    [SerializeField] private TextMeshProUGUI RewardName;
    [SerializeField] private GameObject RewardRemain;
    [SerializeField] private TextMeshProUGUI RewardRemainNumber;
    [SerializeField] private LobbyPropertyComponent UserProperty;


    [SerializeField] private GridLayoutGroup ResultContent;

    

}


