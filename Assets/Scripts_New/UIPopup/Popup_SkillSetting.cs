using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_SkillSetting : UIPopup
{
    [SerializeField] private SkillSlotBaller[] SkillSlot;
    [SerializeField] private SkillSlotBaller[] SkillList;
    [SerializeField] private RectTransform origin;
    [SerializeField] private TextMeshProUGUI SkillNameTxt;
    [SerializeField] private TextMeshProUGUI SkillDescTxt;
    [SerializeField] private RectTransform Position;
    [SerializeField] private GameObject LearnBtn, UpgradeBtn, LockBtn, NotAvailble;
    public delegate void OnCloseEvent();
    private OnCloseEvent CallBack;
    private int ballerIdx, curSlot, selectedSkill;
    private bool saving, ready;
    private Button equipButton;
    private TMP_Text equipLabel, upgradeLabel;
    private Button upgradeButton;
    private readonly List<SkillListType> displayed = new();
    private GameObject lunaImage, lunaVideo;
    private bool imageWasActive, videoWasActive;
    private SkillPanelConnector connector;
    private readonly Vector3[] corners = new Vector3[4];

    public override void Open()
    {
        base.Open();
        ready = false;
        saving = false;
        Position.gameObject.SetActive(false);
        equipButton = LearnBtn.GetComponent<Button>();
        equipButton.onClick.RemoveListener(OnClickEquip);
        equipButton.onClick.AddListener(OnClickEquip);
        // Reuse the authored button, replacing its unused price layout with one label.
        equipLabel = LearnBtn.transform.GetChild(1).GetComponent<TMP_Text>();
        LearnBtn.transform.GetChild(0).gameObject.SetActive(false);
        equipLabel.gameObject.SetActive(true);
        equipLabel.rectTransform.anchorMin = Vector2.zero;
        equipLabel.rectTransform.anchorMax = Vector2.one;
        equipLabel.rectTransform.offsetMin = new Vector2(20, 10);
        equipLabel.rectTransform.offsetMax = new Vector2(-20, -10);
        equipLabel.alignment = TextAlignmentOptions.Center;
        equipLabel.enableAutoSizing = true;
        equipLabel.fontSizeMax = 54;
        equipLabel.fontSizeMin = 28;
        var buttonRect = (RectTransform)LearnBtn.transform;
        buttonRect.anchoredPosition = new Vector2(-270, 80);
        buttonRect.sizeDelta = new Vector2(500, 120);
        upgradeButton = UpgradeBtn.GetComponent<Button>();
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        if (upgradeLabel == null)
        {
            foreach (Transform child in UpgradeBtn.transform) child.gameObject.SetActive(false);
            upgradeLabel = Instantiate(equipLabel, UpgradeBtn.transform);
            upgradeLabel.name = "SkillUpgradeLabel";
        }
        var upgradeRect = (RectTransform)UpgradeBtn.transform;
        upgradeRect.anchorMin = upgradeRect.anchorMax = new Vector2(.5f, 0);
        upgradeRect.pivot = new Vector2(.5f, .5f);
        upgradeRect.anchoredPosition = new Vector2(270, 80);
        upgradeRect.sizeDelta = new Vector2(500, 120);
        Position.GetComponent<Image>().enabled = false;
        connector = Position.GetComponentInChildren<SkillPanelConnector>(true);
        if (connector == null)
        {
            var tab = new GameObject("ConnectedTab", typeof(RectTransform), typeof(CanvasRenderer), typeof(SkillPanelConnector));
            tab.transform.SetParent(Position, false);
            var rect = (RectTransform)tab.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            connector = tab.GetComponent<SkillPanelConnector>();
        }
        connector.color = new Color32(0, 126, 201, 255);
        connector.raycastTarget = false;
    }

    public override void Close()
    {
        if (saving) return;
        ready = false;
        RestoreLuna();
        var callback = CallBack;
        CallBack = null;
        base.Close();
        callback?.Invoke();
    }

    public void Setting(GameObject baller, int idx, int slot, OnCloseEvent callback)
    {
        RestoreLuna();
        ballerIdx = idx;
        curSlot = slot;
        CallBack = callback;
        baller.transform.SetParent(origin, false);
        baller.transform.localScale = Vector3.one;
        baller.transform.localPosition = Vector3.zero;
        if (idx == 1001)
        {
            lunaImage = baller.transform.Find("Image")?.gameObject;
            lunaVideo = baller.transform.Find("CharacterVideoPlayer")?.gameObject;
            imageWasActive = lunaImage != null && lunaImage.activeSelf;
            videoWasActive = lunaVideo != null && lunaVideo.activeSelf;
            if (lunaImage != null) lunaImage.SetActive(true);
            if (lunaVideo != null) lunaVideo.SetActive(false);
        }
        Refresh(true);
        ready = true;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        UpdateConnector();
    }

    private void RestoreLuna()
    {
        if (lunaImage != null) lunaImage.SetActive(imageWasActive);
        if (lunaVideo != null) lunaVideo.SetActive(videoWasActive);
        lunaImage = lunaVideo = null;
    }

    private void Refresh(bool selectEquipped)
    {
        var chart = KOBManager.Backend.Chart;
        var character = chart.CharacterData.GetData(ballerIdx);
        var config = chart.HitterSkillData.GetData(ballerIdx);
        KOBManager.MyInfo.GameData.PlayerInfo.BallerList.TryGetValue(ballerIdx, out var baller);
        int count = Mathf.Min(config?.slot_list.Count ?? 0, SkillSlot.Length);
        curSlot = Mathf.Clamp(curSlot, 1, Mathf.Max(1, count));
        for (int i = 0; i < SkillSlot.Length; i++)
        {
            SkillSlot[i].gameObject.SetActive(i < count);
            if (i >= count) continue;
            int slot = i + 1, type = i % 2 == 0 ? 1 : 2;
            if (baller == null || baller.level < config.slot_list[i])
                SkillSlot[i].SetLock(slot, type, config.slot_list[i], character.rarity);
            else if (baller.SkillEquip.TryGetValue(slot, out int equipped) && chart.SkillData.GetData(equipped) != null)
                SkillSlot[i].SetSkill(equipped, slot, type, SkillLevel(baller, equipped));
            else SkillSlot[i].SetEmpty(slot, type);
        }
        displayed.Clear();
        var typeWanted = curSlot % 2 == 1 ? SlotEquipType.HittingSkill : SlotEquipType.FieldingSkill;
        if (config != null)
            foreach (int id in config.skill_list.Distinct().OrderBy(id => id))
                if (KOBUtil.GetEquipType(id) == typeWanted && chart.SkillData.GetData(id) != null)
                    displayed.Add(new SkillListType(id, baller != null && baller.SkillList.ContainsKey(id) ? SkillLevel(baller, id) : 0));
        if (selectEquipped) selectedSkill = baller != null && baller.SkillEquip.TryGetValue(curSlot, out int equippedId) ? equippedId : 0;
        if (!displayed.Take(SkillList.Length).Any(s => s.idx == selectedSkill)) selectedSkill = displayed.Count > 0 ? displayed[0].idx : 0;
        for (int i = 0; i < SkillList.Length; i++)
        {
            SkillList[i].gameObject.SetActive(i < displayed.Count);
            if (i >= displayed.Count) continue;
            SkillList[i].SetList(displayed[i].idx, i + 1, (int)typeWanted, displayed[i].level);
            SkillList[i].SetSelect(displayed[i].idx == selectedSkill ? i + 1 : -1);
        }
        ShowSkillText();
        UpgradeBtn.SetActive(false);
        LockBtn.SetActive(false);
        bool open = count > 0 && baller != null && baller.level >= config.slot_list[curSlot - 1];
        NotAvailble.SetActive(!open);
        if (!open)
        {
            var text = NotAvailble.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            if (baller == null) UILocalization.Set(text, "skill.equip.unowned");
            else if (count > 0) UILocalization.Set(text, "ui.ee88765859d7", config.slot_list[curSlot - 1]);
            else UILocalization.Set(text, "skill.equip.empty");
        }
        bool alreadyEquipped = baller != null && baller.SkillEquip.TryGetValue(curSlot, out int current) && current == selectedSkill;
        LearnBtn.SetActive(open && selectedSkill > 0);
        equipButton.interactable = !saving && !alreadyEquipped;
        UILocalization.Set(equipLabel, alreadyEquipped ? "skill.equip.equipped" : "skill.equip.action");
        bool learned = baller != null && baller.SkillList.TryGetValue(selectedSkill, out var ownedSkill) && ownedSkill != null && ownedSkill.level > 0;
        UpgradeBtn.SetActive(open && learned);
        if (open && learned)
        {
            int currentLevel = SkillLevel(baller, selectedSkill);
            int max = SkillProgression.MaxLevel(chart.SkillData.GetData(selectedSkill));
            int cost = SkillProgression.UpgradeGold(currentLevel + 1);
            bool atMax = currentLevel >= max;
            upgradeButton.interactable = !saving && !atMax && cost > 0;
            if (atMax) UILocalization.Set(upgradeLabel, "skill.upgrade.max", max);
            else UILocalization.Set(upgradeLabel, "skill.upgrade.action", cost.ToString("N0"));
        }
        ((RectTransform)LearnBtn.transform).anchoredPosition = new Vector2(open && learned ? -270 : 0, 80);
    }

    private static int SkillLevel(KOBBaller baller, int id) =>
        baller.SkillList.TryGetValue(id, out var learned) && learned != null ? Mathf.Max(1, learned.level) : 1;

    private void ShowSkillText()
    {
        var skill = KOBManager.Backend.Chart.SkillData.GetData(selectedSkill);
        if (skill == null)
        {
            UILocalization.Set(SkillNameTxt, "skill.equip.empty"); SkillDescTxt.text = ""; return;
        }
        var entry = displayed.First(s => s.idx == selectedSkill);
        int level = Mathf.Max(1, entry.level);
        UILocalization.Set(SkillNameTxt, "skill.equip.name", UILocalization.Get(skill.name_id), level);
        int[] values = new int[6];
        for (int i = 0; i < Mathf.Min(3, skill.level_value.Count); i++)
        {
            var value = skill.level_value[i];
            if (value.Val != null && value.Val.Length > 0) values[i * 2] = value.Val[Mathf.Min(level - 1, value.Val.Length - 1)];
            if (value.Per != null && value.Per.Length > 0) values[i * 2 + 1] = value.Per[Mathf.Min(level - 1, value.Per.Length - 1)];
        }
        UILocalization.Set(SkillDescTxt, skill.desc_id, values[0], values[1], values[2], values[3], values[4], values[5]);
    }

    public void OnClickSlotTouch(SkillSlotBaller slot)
    {
        if (saving || slot.Slot == curSlot) return;
        curSlot = slot.Slot; Refresh(true);
        Canvas.ForceUpdateCanvases(); UpdateConnector();
    }
    public void OnClickListTouch(SkillSlotBaller skill)
    {
        if (saving) return;
        selectedSkill = skill.SkillIndex; Refresh(false);
    }
    public void OnClickEquip()
    {
        if (saving || selectedSkill <= 0 || !equipButton.interactable) return;
        SendSkillChange(new TRequestEquipBallerSkill { BallerIdx = ballerIdx, Slot = curSlot, SkillIdx = selectedSkill });
    }
    public void OnClickUpgrade()
    {
        if (saving || selectedSkill <= 0 || !upgradeButton.interactable) return;
        SendSkillChange(new TRequestUpgradeBallerSkill { BallerIdx = ballerIdx, SkillIdx = selectedSkill });
    }
    private void SendSkillChange(TRequestBase request)
    {
        saving = true; equipButton.interactable = upgradeButton.interactable = false;
        try
        {
            KOBManager.DummyNetwork.SendPacket(request, (callback, response) =>
            {
                saving = false;
                KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
                if (callback?.IsSuccess() != true || response?.isSuccess != true)
                    KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get(response?.ErrorCode == (int)KOBErrorCode.Not_Enough_Gold ? "skill.upgrade.gold" : "skill.equip.failed"));
                else
                    KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get(request is TRequestUpgradeBallerSkill ? "skill.upgrade.success" : "skill.equip.success"));
                Refresh(false);
                KOBManager.UI.GetUIWindow<UI_Ballers>()?.UpdateInfo(ballerIdx);
            });
        }
        catch (Exception error)
        {
            saving = false;
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            Refresh(false);
            Debug.LogException(error);
            KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get("skill.equip.failed"));
        }
    }

    private void LateUpdate() { if (ready) UpdateConnector(); }
    private void UpdateConnector()
    {
        if (curSlot < 1 || curSlot > SkillSlot.Length || !SkillSlot[curSlot - 1].gameObject.activeSelf) return;
        var parent = (RectTransform)Position.parent;
        var panel = (RectTransform)LearnBtn.transform.parent;
        panel.GetWorldCorners(corners);
        float bottom = parent.InverseTransformPoint(corners[1]).y - 8;
        var slot = (RectTransform)SkillSlot[curSlot - 1].transform;
        slot.GetWorldCorners(corners);
        float top = Mathf.Max(bottom + 30, parent.InverseTransformPoint(corners[0]).y + 24);
        float x = parent.InverseTransformPoint(slot.TransformPoint(slot.rect.center)).x;
        Position.sizeDelta = new Vector2(140, top - bottom);
        Position.localPosition = new Vector3(x, (top + bottom) * .5f, 0);
        Position.gameObject.SetActive(true);
    }
}

public class SkillListType
{
    public int idx, level;
    public SkillListType(int id, int lv) { idx = id; level = lv; }
}
public enum SlotEquipType { HittingSkill = 1, FieldingSkill = 2, Gear = 11 }
