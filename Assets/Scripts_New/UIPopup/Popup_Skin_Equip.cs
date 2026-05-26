using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

public class Popup_Skin_Equip : UIPopup
{
    [SerializeField] private RectTransform PopupBody;

    [Header("[타이틀 관련]")]
    [SerializeField] private Image TitleLabel;
    [SerializeField] private TextMeshProUGUI TitleText;


    [Header("[장착 아이콘 관련]")]
    //[SerializeField] private SkinSlotComponent skinSlot;
    [SerializeField] private TextMeshProUGUI ItemNameText;
    [SerializeField] private TextMeshProUGUI ItemDescText;

    [Header("[버튼 관련]")]
    [SerializeField] private GameObject[] ButtonObj;

    [Header("[화살표]")]
    [SerializeField] private GameObject ArrowObj;

    [Header("[디스크립션]")]
    [SerializeField] private GameObject DescObj;
    [SerializeField] private GameObject MaxDesc;

    
    [Header("[재화 관련]")]
    [SerializeField] private RectTransform PropertyObj;
    [SerializeField] private TextMeshProUGUI PropertyText;
    [SerializeField] private TextMeshProUGUI ItemText;
    [SerializeField] private Image PropertyGold;
    [SerializeField] private Image ItemIcon;

}
