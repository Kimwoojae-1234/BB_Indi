using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_GearSetting : UIPopup
{
    [SerializeField] private GearSlotBaller[] GearSlot, GearList;
    [SerializeField] private RectTransform origin, Position;
    [SerializeField] private TextMeshProUGUI GearNameTxt, GearDescTxt;
    [SerializeField] private GameObject BuyBtn, UpgradeBtn, LockBtn, NotAvailble;
    public delegate void OnCloseEvent();
    private OnCloseEvent callback;
    private int ballerIdx, currentSlot, selectedBase;
    private bool saving, ready;
    private Button equipButton, upgradeButton;
    private TMP_Text equipLabel, upgradeLabel;
    private GameObject lunaImage, lunaVideo;
    private bool imageWasActive, videoWasActive;
    private readonly Vector3[] corners = new Vector3[4];

    public override void Open()
    {
        base.Open(); ready = false; saving = false;
        Position.gameObject.SetActive(false);
        PrepareButton(BuyBtn, ref equipLabel, OnClickEquip);
        PrepareButton(UpgradeBtn, ref upgradeLabel, OnClickUpgrade);
        equipButton = BuyBtn.GetComponent<Button>(); upgradeButton = UpgradeBtn.GetComponent<Button>();
        Position.GetComponent<Image>().enabled = false;
        var connector = Position.GetComponentInChildren<SkillPanelConnector>(true);
        if (connector == null)
        {
            var go = new GameObject("ConnectedTab", typeof(RectTransform), typeof(CanvasRenderer), typeof(SkillPanelConnector));
            go.transform.SetParent(Position, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            connector = go.GetComponent<SkillPanelConnector>();
        }
        connector.color = new Color32(0, 126, 201, 255); connector.raycastTarget = false;
    }
    private void PrepareButton(GameObject go, ref TMP_Text label, UnityEngine.Events.UnityAction action)
    {
        var button = go.GetComponent<Button>();
        button.onClick.RemoveListener(action); button.onClick.AddListener(action);
        if (label == null)
        {
            foreach (Transform child in go.transform) child.gameObject.SetActive(false);
            label = Instantiate(GearNameTxt, go.transform);
            label.name = "ActionLabel"; label.gameObject.SetActive(true);
            label.color = Color.white; label.raycastTarget = false;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true; label.fontSizeMax = 54; label.fontSizeMin = 26;
            label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(20, 10); label.rectTransform.offsetMax = new Vector2(-20, -10);
        }
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(500, 120);
    }
    public void Setting(GameObject baller, int idx, int slot, OnCloseEvent onClose)
    {
        RestoreLuna(); ballerIdx = idx; currentSlot = Mathf.Clamp(slot, 1, 4); callback = onClose;
        baller.transform.SetParent(origin, false); baller.transform.localScale = Vector3.one; baller.transform.localPosition = Vector3.zero;
        if (idx == 1001)
        {
            lunaImage = baller.transform.Find("Image")?.gameObject; lunaVideo = baller.transform.Find("CharacterVideoPlayer")?.gameObject;
            imageWasActive = lunaImage != null && lunaImage.activeSelf; videoWasActive = lunaVideo != null && lunaVideo.activeSelf;
            if (lunaImage != null) lunaImage.SetActive(true); if (lunaVideo != null) lunaVideo.SetActive(false);
        }
        Refresh(true); ready = true;
        Canvas.ForceUpdateCanvases(); LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform); UpdateConnector();
    }
    private void RestoreLuna()
    {
        if (lunaImage != null) lunaImage.SetActive(imageWasActive); if (lunaVideo != null) lunaVideo.SetActive(videoWasActive);
        lunaImage = lunaVideo = null;
    }
    public override void Close()
    {
        if (saving) return;
        ready = false; RestoreLuna(); var action = callback; callback = null; base.Close(); action?.Invoke();
    }
    private void Refresh(bool selectEquipped)
    {
        var chart = KOBManager.Backend.Chart;
        KOBManager.MyInfo.GameData.PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller);
        for (int i = 0; i < GearSlot.Length; i++)
        {
            GearSlot[i].gameObject.SetActive(i < 4);
            if (i >= 4) continue;
            if (baller != null && baller.GearEquip.TryGetValue(i + 1, out int id)) GearSlot[i].SetGear(id, i + 1);
            else GearSlot[i].SetEmpty(i + 1);
            GearSlot[i].SetSelect(currentSlot == i + 1);
        }
        var candidates = BallerGearCatalog.Families(BallerGearCatalog.SlotType(currentSlot)).ToArray();
        if (selectEquipped)
            selectedBase = baller != null && baller.GearEquip.TryGetValue(currentSlot, out int id)
                ? BallerGearCatalog.Base(chart.GearData.GetData(id))?.idx ?? 0 : 0;
        if (!candidates.Any(g => g.idx == selectedBase)) selectedBase = candidates.FirstOrDefault()?.idx ?? 0;
        for (int i = 0; i < GearList.Length; i++)
        {
            GearList[i].gameObject.SetActive(i < candidates.Length);
            if (i >= candidates.Length) continue;
            var actual = BallerGearCatalog.AtLevel(candidates[i], Mathf.Max(1, BallerGearCatalog.OwnedLevel(baller, candidates[i]))) ?? candidates[i];
            GearList[i].SetGear(actual.idx, i + 1); GearList[i].SetSelect(candidates[i].idx == selectedBase);
        }
        var basis = chart.GearData.GetData(selectedBase);
        int level = BallerGearCatalog.OwnedLevel(baller, basis);
        var gear = BallerGearCatalog.AtLevel(basis, Mathf.Max(1, level));
        if (gear != null)
        {
            UILocalization.Set(GearNameTxt, "skill.equip.name", UILocalization.Get("gear.family." + selectedBase), gear.att_lv);
            GearDescTxt.text = BallerGearCatalog.Description(gear);
        }
        else { GearNameTxt.text = ""; GearDescTxt.text = ""; }
        bool equipped = baller != null && baller.GearEquip.TryGetValue(currentSlot, out int current) && current == gear?.idx;
        BuyBtn.SetActive(baller != null && gear != null);
        UpgradeBtn.SetActive(baller != null && gear != null && level > 0);
        LockBtn.SetActive(false); NotAvailble.SetActive(baller == null);
        if (baller == null) UILocalization.Set(NotAvailble.GetComponentInChildren<TMP_Text>(true), "gear.unowned");
        UILocalization.Set(equipLabel, equipped ? "skill.equip.equipped" : "skill.equip.action");
        equipButton.interactable = !saving && !equipped;
        var next = BallerGearCatalog.AtLevel(basis, level + 1);
        bool max = level > 0 && next == null;
        int cost = SkillProgression.UpgradeGold(level + 1);
        upgradeButton.interactable = !saving && !max && cost > 0;
        if (max) UILocalization.Set(upgradeLabel, "skill.upgrade.max", level);
        else UILocalization.Set(upgradeLabel, "skill.upgrade.action", cost.ToString("N0"));
        ((RectTransform)BuyBtn.transform).anchoredPosition = new Vector2(level > 0 ? -270 : 0, 80);
        ((RectTransform)UpgradeBtn.transform).anchoredPosition = new Vector2(270, 80);
    }
    public void OnClickSlotTouch(GearSlotBaller slot) { if (saving) return; currentSlot = slot.Slot; Refresh(true); Canvas.ForceUpdateCanvases(); UpdateConnector(); }
    public void OnClickListTouch(GearSlotBaller item) { if (saving) return; selectedBase = BallerGearCatalog.Base(KOBManager.Backend.Chart.GearData.GetData(item.GearIndex))?.idx ?? 0; Refresh(false); }
    public void OnClickEquip() { if (equipButton.interactable) Send(false); }
    public void OnClickUpgrade() { if (upgradeButton.interactable) Send(true); }
    private void Send(bool upgrade)
    {
        if (saving || selectedBase <= 0) return;
        saving = true; equipButton.interactable = upgradeButton.interactable = false;
        try
        {
            KOBManager.DummyNetwork.SendPacket(new TRequestBallerGear { BallerIdx = ballerIdx, Slot = currentSlot, GearIdx = selectedBase, Upgrade = upgrade }, (result, response) =>
            {
                saving = false; KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
                bool success = result?.IsSuccess() == true && response?.isSuccess == true;
                string key = success ? (upgrade ? "gear.upgrade.success" : "gear.equip.success")
                    : response?.ErrorCode == (int)KOBErrorCode.Not_Enough_Gold ? "skill.upgrade.gold" : "gear.failed";
                KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get(key));
                Refresh(false); KOBManager.UI.GetUIWindow<UI_Ballers>()?.UpdateInfo(ballerIdx);
            });
        }
        catch (Exception error)
        {
            saving = false; KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close(); Refresh(false);
            Debug.LogException(error); KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get("gear.failed"));
        }
    }
    private void LateUpdate() { if (ready) UpdateConnector(); }
    private void UpdateConnector()
    {
        var parent = (RectTransform)Position.parent; var panel = (RectTransform)BuyBtn.transform.parent;
        panel.GetWorldCorners(corners); float bottom = parent.InverseTransformPoint(corners[1]).y - 8;
        var slot = (RectTransform)GearSlot[currentSlot - 1].transform;
        slot.GetWorldCorners(corners); float top = Mathf.Max(bottom + 30, parent.InverseTransformPoint(corners[0]).y + 24);
        float x = parent.InverseTransformPoint(slot.TransformPoint(slot.rect.center)).x;
        Position.sizeDelta = new Vector2(140, top - bottom); Position.localPosition = new Vector3(x, (top + bottom) * .5f, 0);
        Position.gameObject.SetActive(true);
    }
}
