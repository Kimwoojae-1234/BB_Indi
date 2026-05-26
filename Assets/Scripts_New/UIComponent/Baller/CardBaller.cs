using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardBaller : MonoBehaviour
{
    [SerializeField] private Image CardImg;
    [SerializeField] private Image BallerImg;
    [SerializeField] private TextMeshProUGUI BallerName;
    [SerializeField] private GameObject[] CurrentState;
    [SerializeField] private GameObject[] Glow;

    [Header("[컬렉션]")]
    [SerializeField] private BallerTierSliderComp TierSlider;
    [SerializeField] private Slider UpgradeSlider;
    [SerializeField] private TextMeshProUGUI UpgradeTxt;
    [SerializeField] private GameObject UpgradeArrow;
    [SerializeField] private Image LevelFrame;
    [SerializeField] private TextMeshProUGUI LevelTxt;

    [Header("[언락킹]")]
    [SerializeField] private TextMeshProUGUI UnlockingGem;

    [Header("[언락]")]
    [SerializeField] private TextMeshProUGUI UnlockGem;
    [SerializeField] private TextMeshProUGUI UnlockTxt;



    public enum CardBallerState
    {
        Collection = 0, //수집함
        Unlocking = 1, //이 리그에서 해금 가능
        Locked = 2     //현재 해금 불가
    }


    private int SelectIdx;
    private CardBallerState State;


    public void OnClickCardButton()
    {
        Debug.Log("OnclickCardButton");
        UI_Ballers ballers = KOBManager.UI.OpenWindow<UI_Ballers>();
        ballers.LastWindow = typeof(UI_BallersList);
        ballers.LoadBaller(SelectIdx, State);
    }



    public void SetCollection(int idx)
    {        
        SelectIdx = idx;
        State = CardBallerState.Collection;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx); //선수기본정보
        KOBBaller info = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[idx]; //선수 성장정보
        UpgradeChart UpgradeData = KOBManager.Backend.Chart.UpgradeData;            //업글 정보

        CurrentState[0].gameObject.SetActive(true);
        for (int i = 0; i < Glow.Length; i++) Glow[i].gameObject.SetActive(true);
        baseInfo(data);

        //티어
        TierSlider.Set(info, false);

        //업그레이드
        int CardNeed = UpgradeData.UpgradeCard(info.level + 1, data.rarity);
        int curCard = info.card_number;
        //UpgradeArrow.gameObject.SetActive(curCard >= CardNeed ? true : false);
        UpgradeTxt.text = string.Format("{0}/{1}", curCard, CardNeed);
        UpgradeSlider.value = (float)curCard / (float)CardNeed;

        //레벨
        LevelFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + data.rarity.ToString());
        LevelTxt.text = info.level.ToString();
    }

    public void SetUnlocking(int idx)
    {
        SelectIdx = idx;
        State = CardBallerState.Unlocking;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
        
        CurrentState[1].gameObject.SetActive(true);
        for (int i = 0; i < Glow.Length; i++) Glow[i].gameObject.SetActive(true);
        baseInfo(data);

        BallerName.color = KOBUtil.ConvertColor(0x919191);

        //가격 - 1레벨
        UnlockingGem.text = KOBManager.Backend.Chart.UpgradeData.UnlockCost(1)[data.rarity].ToString(); 
    }

    public void SetLocked(int idx)
    {
        SelectIdx = idx;
        State = CardBallerState.Locked;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);

        CurrentState[2].gameObject.SetActive(true);
        for (int i = 0; i < Glow.Length; i++) Glow[i].gameObject.SetActive(false);
        baseInfo(data);

        Color lockColor = KOBUtil.ConvertColor(0x919191);
        CardImg.color = lockColor;
        BallerImg.color = lockColor;
        BallerName.color = lockColor;

        //가격 - 2레벨
        UnlockGem.text = KOBManager.Backend.Chart.UpgradeData.UnlockCost(2)[data.rarity].ToString();

        //언락 리그
        int league = data.league;
        UnlockTxt.text = string.Format("Available on the\nRTTS{0} League", league);
    }

    private void baseInfo(CharacterData data)
    {
        CardImg.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_CardFrame_" + data.rarity.ToString());
        KOBManager.Resource.LoadBallerPortrait(BallerImg, data.char_idx);
        BallerName.text = KOBManager.Localization.GetUILocalizedValue2(data.name_id);

        CardImg.color = Color.white;
        BallerImg.color = Color.white;
        BallerName.color = Color.white;
    }

}
