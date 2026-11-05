using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UI_Shop : UIWindow
{
    [SerializeField] private HorizontalLayoutGroup group;
    [SerializeField] private LobbyPropertyComponent UserProperty;
    private ScrollRect scroll;
    private readonly List<Button> productButtons = new List<Button>();
    private bool saving;
    private Tween scrollTween;

    public override void Initialize()
    {
        base.Initialize();
        scroll = transform.Find("ScrollRect").GetComponent<ScrollRect>();
        foreach (var product in ShopCatalog.Current.products)
        {
            var root = Resolve(scroll.content, product.path);
            if (root == null) { Debug.LogError("Missing shop product: " + product.path); continue; }
            productButtons.Add(BindButton(root, () => Buy(product.id)));
            SetValue(Resolve(root, product.costPath), product.currency == "cash" ? product.cashPrice : product.cost.ToString("N0"));
            if (!string.IsNullOrEmpty(product.amountPath))
                SetValue(Resolve(root, product.amountPath), (product.id == "daily_subscription" ? "+" : "") + (product.gold > 0 ? product.gold : product.gems).ToString("N0"));
        }
        string[] tabs = { "Text_Daily", "Text_Chest", "Text_Gld Pack", "Text_Gem Pack" };
        int[] sections = { 0, 2, 3, 4 };
        foreach (var graphic in transform.Find("Tap").GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        for (int i = 0; i < tabs.Length; i++)
        {
            int section = sections[i];
            var tab = transform.Find("Tap/" + tabs[i]);
            BindButton(tab, () => ScrollTo(section, tab));
        }
        BindButton(scroll.content.Find("Item_1/Button_Info"), () => Toast(UILocalization.Format("shop.chest.info", ShopCatalog.Current.Chest("small").gold, ShopCatalog.Current.Chest("large").gold)));
        // No daily purchase limit/reset in the prototype.
        scroll.content.Find("Item_1/ResetTime")?.gameObject.SetActive(false);
        UIButtonAudio.AttachMissing(gameObject);
    }

    public override void OpenWindow()
    {
        base.OpenWindow();
        group.gameObject.SetActive(true);
        PropertySetting();
        Canvas.ForceUpdateCanvases();
        ScrollTo(0, transform.Find("Tap/Text_Daily"));
    }
    public void PropertySetting() => UserProperty.InitProperty(typeof(UI_Shop));

    private static Button BindButton(Transform target, UnityEngine.Events.UnityAction action)
    {
        var button = target.GetComponent<Button>() ?? target.gameObject.AddComponent<Button>();
        button.targetGraphic = target.GetComponent<Graphic>();
        if (button.targetGraphic != null) button.targetGraphic.raycastTarget = true;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(action);
        return button;
    }
    public static Transform Resolve(Transform root, string path)
    {
        foreach (string part in path.Split('/'))
        {
            string name = part; int ordinal = 0;
            int bracket = part.IndexOf('[');
            if (bracket >= 0) { name = part.Substring(0, bracket); ordinal = int.Parse(part.Substring(bracket + 1).TrimEnd(']')); }
            Transform next = null;
            foreach (Transform child in root)
                if (child.name == name && ordinal-- == 0) { next = child; break; }
            if (next == null) return null;
            root = next;
        }
        return root;
    }
    private static void SetValue(Transform target, string value)
    {
        if (target == null) return;
        target.GetComponent<LocalizedUIText>()?.Bind("");
        var label = target.GetComponent<TMP_Text>();
        if (label != null) label.text = value;
    }
    private void ScrollTo(int section, Transform tab)
    {
        Canvas.ForceUpdateCanvases();
        var target = (RectTransform)scroll.content.Find("Item_" + section);
        var viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
        // Use the section itself; decorative glows may extend beyond its layout bounds.
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        float offset = scroll.content.InverseTransformPoint(corners[0]).x - scroll.content.rect.xMin;
        float width = Mathf.Max(0, scroll.content.rect.width - viewport.rect.width);
        float position = width > 0 ? Mathf.Clamp01(offset / width) : 0;
        scroll.StopMovement(); scrollTween?.Kill();
        scrollTween = DOTween.To(() => scroll.horizontalNormalizedPosition, v => scroll.horizontalNormalizedPosition = v, position, .3f).SetUpdate(true);
        var focus = transform.Find("Tap/Focus");
        if (focus != null) { var p = focus.position; p.x = tab.position.x; focus.position = p; }
    }
    private void Buy(string id)
    {
        if (saving || KOBManager.Popup.CheckPopupOpen()) return;
        var product = ShopCatalog.Current.Find(id);
        if (product.currency == "cash") { Toast(UILocalization.Get("shop.unavailable")); return; }
        if (KOBManager.MyInfo.GameData.CurrencyInfo.TotalGem < product.cost) { Toast(UILocalization.Get("shop.gems.insufficient")); return; }
        saving = true;
        foreach (var button in productButtons) button.interactable = false;
        try
        {
            KOBManager.DummyNetwork.SendPacket(new TRequestShopPurchase { ProductId = id }, (result, response) =>
            {
                saving = false;
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
                foreach (var button in productButtons) if (button != null) button.interactable = true;
                if (result?.IsSuccess() != true || response?.isSuccess != true)
                {
                    Toast(UILocalization.Get(response?.ErrorCode == (int)KOBErrorCode.Not_Enough_Gem ? "shop.gems.insufficient" : "shop.failed"));
                    PropertySetting();
                    return;
                }
                var intent = new Intent();
                intent["RewardList"] = ((TResultShopPurchase)response).Rewards;
                intent["isBox"] = false;
                if (product.gold > 0)
                {
                    var productRoot = Resolve(scroll.content, product.path);
                    var icon = productRoot.Find("Image_Gold") ?? productRoot.Find("Chest/Image_SpecialChest");
                    if (icon != null) intent["RewardIcon"] = icon.GetComponent<Image>().sprite;
                }
                KOBManager.Popup.OpenPopup<Popup_RewardGet>().Set(intent);
                PropertySetting();
            });
        }
        catch (Exception error)
        {
            saving = false;
            foreach (var button in productButtons) button.interactable = true;
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            Debug.LogException(error);
            Toast(UILocalization.Get("shop.failed"));
        }
    }
    private static void Toast(string text) => KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(text);
    public override void CloseWindow() { scrollTween?.Kill(); base.CloseWindow(); }
}
