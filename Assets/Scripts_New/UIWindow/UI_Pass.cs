using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Pass : UIWindow
{
    private Transform goldenRow, freeRow, counts;
    private readonly List<Transform> goldenSlots = new List<Transform>(), freeSlots = new List<Transform>();
    private readonly Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
    private Sprite freeGoldFrame, freeChestFrame, completedSprite, lockedSprite;
    private bool saving;
    private int shownXp = -1;
    private long shownGold = -1, shownGems = -1;
    private int shownEnergy = -1;
    private Slider progress, road;

    public override void Initialize()
    {
        base.Initialize();
        goldenRow = transform.Find("Bottom/ScrollRect/Content/GoldenPass");
        freeRow = transform.Find("Bottom/ScrollRect/Content/FreePass");
        foreach (Transform slot in goldenRow) if (slot.name == "Slot") goldenSlots.Add(slot);
        foreach (Transform slot in freeRow) if (slot.name == "Slot") freeSlots.Add(slot);
        icons["gold"] = goldenSlots[0].Find("Image_Item").GetComponent<Image>().sprite;
        icons["gem"] = goldenSlots[1].Find("Image_Item").GetComponent<Image>().sprite;
        icons["small"] = goldenSlots[2].Find("Image_Item").GetComponent<Image>().sprite;
        icons["large"] = goldenSlots[4].Find("Image_Item").GetComponent<Image>().sprite;
        freeGoldFrame = freeSlots[1].GetComponent<Image>().sprite;
        freeChestFrame = freeSlots[0].GetComponent<Image>().sprite;
        var checkTemplate = freeSlots[0].Find("Icon_Check").gameObject;
        for (int i = 0; i < freeSlots.Count; i++)
        {
            int tier = i + 1;
            if (freeSlots[i].Find("Icon_Check") == null)
            {
                var check = Instantiate(checkTemplate, freeSlots[i], false); check.name = "Icon_Check";
            }
            Bind(freeSlots[i], () => Claim(tier));
            Bind(goldenSlots[i], GoldenLocked);
        }
        Bind(transform.Find("Bottom/Left/GoldenPass"), GoldenLocked);
        progress = transform.Find("SubBar/Slider_IconType01").GetComponent<Slider>();
        road = transform.Find("Bottom/ScrollRect/Content/SliderBar/SliderBar").GetComponent<Slider>();
        counts = road.transform.Find("Counts");
        completedSprite = counts.GetChild(0).GetComponent<Image>().sprite;
        lockedSprite = counts.GetChild(2).GetComponent<Image>().sprite;
        foreach (var slider in GetComponentsInChildren<Slider>(true))
        {
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            foreach (var graphic in slider.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        }
        transform.Find("SubBar/Button_Activate").GetComponent<Button>().interactable = false;
        transform.Find("SubBar/Timer").gameObject.SetActive(false);
        foreach (Transform child in transform.Find("Top/Button_Home"))
            if (child.name.StartsWith("Notify_")) child.gameObject.SetActive(false);
        Bind(transform.Find("Top/Button_Home"), ClickHomeButton);
        Bind(transform.Find("Top/Button_Back/Button_Back"), ClickBackButton);
        foreach (var button in transform.Find("Top/StatusBar_Group").GetComponentsInChildren<Button>(true))
            Bind(button.transform, () => KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = typeof(UI_Pass));
        UIButtonAudio.AttachMissing(gameObject);
    }
    public override void OpenWindow() { base.OpenWindow(); Refresh(); }
    protected override void Update()
    {
        base.Update();
        if (KOBManager.MyInfo.GameData.GrowthInfo.PassXP != shownXp) Refresh();
        RefreshCurrency();
    }
    private static void Bind(Transform target, UnityEngine.Events.UnityAction action)
    {
        var button = target.GetComponent<Button>() ?? target.gameObject.AddComponent<Button>();
        button.targetGraphic = target.GetComponent<Graphic>();
        if (button.targetGraphic != null) button.targetGraphic.raycastTarget = true;
        button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(action);
    }
    private static void Text(Transform root, string path, string value)
    {
        var label = root.Find(path)?.GetComponent<TMP_Text>();
        if (label == null) return;
        label.GetComponent<LocalizedUIText>()?.Bind(""); label.text = value;
    }
    private void RefreshCurrency()
    {
        var data = KOBManager.MyInfo.GameData;
        var c = data.CurrencyInfo;
        if (shownGold == c.Gold && shownGems == c.TotalGem && shownEnergy == c.Energy) return;
        shownGold = c.Gold; shownGems = c.TotalGem; shownEnergy = c.Energy;
        Text(transform, "Top/StatusBar_Group/Stats_Gold/Text_Value", c.Gold.ToString("N0"));
        Text(transform, "Top/StatusBar_Group/Stats_Gem/Text_Value", c.TotalGem.ToString("N0"));
        Text(transform, "Top/StatusBar_Group/Stats_Energe/Text_Value", c.Energy + "/" + data.GrowthInfo.MaxEnergy);
    }
    private void Refresh()
    {
        var config = PassCatalog.Current;
        var growth = KOBManager.MyInfo.GameData.GrowthInfo;
        shownXp = growth.PassXP;
        int completed = config.Completed(shownXp);
        progress.minValue = 0; progress.maxValue = config.xpPerTier;
        progress.value = completed >= config.tiers ? config.xpPerTier : shownXp % config.xpPerTier;
        var inner = progress.transform.Find("Slider_IconType01_Frame").GetComponent<Slider>();
        inner.minValue = 0; inner.maxValue = progress.maxValue; inner.value = progress.value;
        Text(progress.transform, "Slider_IconType01_Icon2/Text", Math.Min(config.tiers, completed + 1).ToString());
        road.minValue = 0; road.maxValue = Math.Max(1, config.tiers - 1); road.value = Math.Max(0, completed - 1);
        for (int i = 0; i < freeSlots.Count; i++)
        {
            int tier = i + 1; bool active = tier <= config.tiers;
            freeSlots[i].gameObject.SetActive(active); goldenSlots[i].gameObject.SetActive(active);
            if (!active) continue;
            Fill(goldenSlots[i], config.Reward(tier, true));
            Fill(freeSlots[i], config.Reward(tier, false));
            freeSlots[i].GetComponent<Image>().sprite = config.Reward(tier, false).kind == "gold" ? freeGoldFrame : freeChestFrame;
            bool claimed = growth.PassFreeClaims.Contains(tier);
            freeSlots[i].Find("Icon_Check").gameObject.SetActive(claimed);
            freeSlots[i].GetComponent<Button>().interactable = !saving && !claimed;
            var glow = freeSlots[i].Find("Glow");
            if (glow != null) glow.gameObject.SetActive(tier <= completed && !claimed);
            var count = counts.GetChild(i);
            count.GetComponent<Image>().sprite = tier <= completed ? completedSprite : lockedSprite;
            Text(count, "Text_Count", tier.ToString());
        }
        RefreshCurrency();
    }
    private void Fill(Transform slot, PassRewardDefinition reward)
    {
        var icon = slot.Find("Image_Item").GetComponent<Image>();
        icon.sprite = icons[reward.kind]; icon.preserveAspect = true; icon.color = Color.white;
        var label = slot.Find("Text_Value") ?? slot.Find("Text_Valu");
        label.GetComponent<LocalizedUIText>()?.Bind(""); label.GetComponent<TMP_Text>().text = reward.amount.ToString("N0");
    }
    private void GoldenLocked() { if (!saving) Toast("pass.golden.locked"); }
    private static void Toast(string key) => KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get(key));
    private void Claim(int tier)
    {
        if (saving || KOBManager.Popup.CheckPopupOpen()) return;
        if (tier > PassCatalog.Current.Completed(KOBManager.MyInfo.GameData.GrowthInfo.PassXP)) { Toast("pass.locked"); return; }
        saving = true; Refresh();
        KOBManager.DummyNetwork.SendPacket(new TRequestPassClaim { Tier = tier }, (result, response) =>
        {
            saving = false; KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            Refresh();
            if (result?.IsSuccess() != true || response?.isSuccess != true) { Toast("pass.failed"); return; }
            var intent = new Intent(); intent["RewardList"] = ((TResultShopPurchase)response).Rewards; intent["isBox"] = false;
            if (PassCatalog.Current.Reward(tier, false).kind == "gold") intent["RewardIcon"] = icons["gold"];
            KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(intent);
        });
    }
}
