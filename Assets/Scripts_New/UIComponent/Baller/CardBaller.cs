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

        for (int state = 0; state < CurrentState.Length; state++) CurrentState[state].SetActive(state == 0);
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
        LevelFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + data.rarity)
            ?? KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_COMMON");
        LevelTxt.text = info.level.ToString();
    }

    public void SetUnlocking(int idx)
    {
        SelectIdx = idx;
        State = CardBallerState.Unlocking;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
        
        for (int state = 0; state < CurrentState.Length; state++) CurrentState[state].SetActive(state == 1);
        for (int i = 0; i < Glow.Length; i++) Glow[i].gameObject.SetActive(true);
        baseInfo(data);



        //가격 - 1레벨
        UnlockingGem.text = PrototypeBallers.UnlockCost(data, KOBManager.MyInfo.GameData.GrowthInfo.League).ToString("N0"); 
    }

    public void SetLocked(int idx)
    {
        SelectIdx = idx;
        State = CardBallerState.Locked;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);

        for (int state = 0; state < CurrentState.Length; state++) CurrentState[state].SetActive(state == 2);
        for (int i = 0; i < Glow.Length; i++) Glow[i].gameObject.SetActive(true);
        baseInfo(data);



        //가격 - 2레벨
        UnlockGem.text = PrototypeBallers.UnlockCost(data, KOBManager.MyInfo.GameData.GrowthInfo.League).ToString("N0");

        //언락 리그
        int league = data.league;
        UILocalization.Set(UnlockTxt, "ui.99fa24f147d4", league);
    }

    private void baseInfo(CharacterData data)
    {
        var frame = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_CardFrame_" + data.rarity);
        CardImg.sprite = frame != null ? frame : KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_CardFrame00_Base");
        KOBManager.Resource.LoadBallerPortrait(BallerImg, data.char_idx);
        UILocalization.Set(BallerName, data.name_id);

        CardImg.color = frame != null ? Color.white : KOBUtil.GetRarityColor(data.rarity);
        // Frame sprites have their own palette (different from the rarity text colors).
        Color glowColor = frame != null ? GetCardBackgroundColor(data.rarity) : CardImg.color;
        var bottomGlow = transform.Find("Mask/Glow")?.GetComponent<Image>();
        if (bottomGlow != null)
        {
            glowColor.a = bottomGlow.color.a;
            bottomGlow.color = glowColor;
        }
        BallerImg.color = Color.white;
        BallerName.color = Color.white;
    }

    public static Color GetCardBackgroundColor(KOBRarity rarity)
    {
        // Base colors sampled from Frame_CardFrame_* artwork.
        switch (rarity)
        {
            case KOBRarity.COMMON: return new Color32(42, 170, 253, 255);
            case KOBRarity.RARE: return new Color32(83, 216, 4, 255);
            case KOBRarity.EPIC: return new Color32(217, 85, 255, 255);
            case KOBRarity.LEGENDARY: return new Color32(255, 155, 25, 255);
            default: return KOBUtil.GetRarityColor(rarity);
        }
    }

}
