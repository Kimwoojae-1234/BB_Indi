using System.Collections;
using System.Collections.Generic;
using BackendData.Rank;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Ranking : UIWindow
{
    private ScrollRect scroll;
    private Transform content;
    private Transform mine;
    private readonly List<GameObject> templates = new();
    private readonly List<GameObject> rows = new();
    private TextMeshProUGUI status;
    private int requestVersion;
    private int offset;
    private int total;
    private bool loading;
    private bool initialized;
    private Coroutine timeout;

    public override void OpenWindow()
    {
        base.OpenWindow();
        BindUI();
        RefreshRanking();
    }

    private void BindUI()
    {
        if (initialized) return;
        initialized = true;
        scroll = transform.Find("ScrollRect").GetComponent<ScrollRect>();
        content = transform.Find("ScrollRect/Mask/Content");
        mine = transform.Find("ScrollRect/List_Me");
        foreach (Transform child in content)
        {
            templates.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        BindTab("Text_Golbal", "ui.5f1184f7df96", true);
        BindTab("Text_Country", "ui.d523ebbd1014", false);
        BindTab("Text_Friends", "ui.c11d5e1d35fb", false);
        // An all-time board must not display the prefab's sample reset countdown.
        UILocalization.Set(transform.Find("Icon_Time/Text_Reset").GetComponent<TextMeshProUGUI>(), "ui.4745c5dce5e8");
        UILocalization.Set(transform.Find("Icon_Time/Text_Time").GetComponent<TextMeshProUGUI>(), "ui.3a1ef60b8e85");
        status = Instantiate(mine.Find("Text_NickName").GetComponent<TextMeshProUGUI>(), content);
        status.name = "RankingStatus";
        status.richText = false;
        status.fontSize = 26;
        status.alignment = TextAlignmentOptions.Center;
        status.rectTransform.sizeDelta = new Vector2(0, 100);
        status.raycastTarget = false;
        scroll.onValueChanged.AddListener(_ => {
            if (!loading && offset > 0 && offset < total && scroll.verticalNormalizedPosition < 0.1f)
                LoadPage(requestVersion);
        });
    }

    private void BindTab(string name, string label, bool available)
    {
        var text = transform.Find("Tab_Menu/" + name).GetComponent<TextMeshProUGUI>();
        UILocalization.Set(text, label);
        text.raycastTarget = true;
        if (!available) text.color = new Color(0.45f, 0.62f, 0.72f);
        var button = text.GetComponent<Button>() ?? text.gameObject.AddComponent<Button>();
        button.targetGraphic = text;
        button.onClick.AddListener(() => {
            if (available) { if (!loading) RefreshRanking(); }
            else KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(UILocalization.Get("ui.e89874f8b91f"));
        });
    }

    public void RefreshRanking()
    {
        int version = ++requestVersion;
        foreach (var row in rows) { row.SetActive(false); Destroy(row); }
        rows.Clear();
        offset = total = 0;
        loading = true;
        status.gameObject.SetActive(true);
        UILocalization.Set(status, "ui.bbba821f1324");
        ShowMine(null);
        scroll.verticalNormalizedPosition = 1;
        ArmTimeout(version);
        KOBManager.Backend.TrophyRanking.Sync(error => {
            if (!IsCurrent(version)) return;
            if (error != null) { ShowError(error); return; }
            LoadPage(version);
            KOBManager.Backend.TrophyRanking.GetMine((mineError, item) => {
                if (!IsCurrent(version)) return;
                ShowMine(item);
                if (mineError != null) KOBManager.FrontUI.OpenPopup<FrontUI_ToastPopup>().Init(mineError);
            });
        });
    }

    private void LoadPage(int version)
    {
        loading = true;
        ArmTimeout(version);
        KOBManager.Backend.TrophyRanking.GetPage(offset, (error, items, count) => {
            if (!IsCurrent(version)) return;
            loading = false;
            if (timeout != null) { StopCoroutine(timeout); timeout = null; }
            if (error != null) { ShowError(error); return; }
            total = count;
            foreach (var item in items)
            {
                int.TryParse(item.rank, out int rank);
                int template = rank >= 1 && rank <= 3 ? rank - 1 : 3;
                var row = Instantiate(templates[template], content);
                row.name = "Rank_" + item.rank;
                row.SetActive(true);
                UserIdentity.Decode(item.extraData, out int profile, out string country);
                FillRow(row.transform, item.rank, item.nickname, item.score, profile, country);
                rows.Add(row);
            }
            offset += items.Count;
            if (items.Count == 0) total = offset;
            status.transform.SetAsLastSibling();
            status.text = offset == 0 ? UILocalization.Get("ui.5ae717e4e803") : "";
            status.gameObject.SetActive(offset == 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content);
        });
    }

    private void ShowMine(RankUserItem item)
    {
        var identity = KOBManager.Backend.GameData.KOBPublicData;
        FillRow(mine, item?.rank ?? "-", KOBManager.MyInfo.BackkendUserInfo?.nickname ?? "Rookie",
            KOBManager.MyInfo.GameData.GrowthInfo.Trophy.ToString(), identity.ProfileId, identity.CountryCode);
    }

    private static void FillRow(Transform row, string rank, string nickname, string score, int profile, string country)
    {
        foreach (var text in row.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.richText = false;
            if (text.name == "Text_NickName")
            {
                text.text = nickname;
                text.enableAutoSizing = true;
                text.fontSizeMin = 18;
                text.overflowMode = TextOverflowModes.Ellipsis;
            }
            else if (text.name == "Text_Score" || text.name == "Text_3,524")
                text.text = long.TryParse(score, out long value) ? value.ToString("N0") : score;
            else if (text.name == "Text_4" || text.name == "Text_999") text.text = rank;
        }
        foreach (var image in row.GetComponentsInChildren<Image>(true))
        {
            if (image.name == "Image_Character")
            {
                image.sprite = UserIdentity.Portrait(profile);
                image.preserveAspect = true;
            }
            else if (image.name == "Icon_Country")
            {
                image.sprite = UserIdentity.Flag(country);
                image.enabled = image.sprite != null;
                var countryLabel = image.transform.Find("CountryCode");
                if (countryLabel != null) countryLabel.gameObject.SetActive(image.sprite == null && !string.IsNullOrEmpty(country));
                // Countries without a bundled flag still display their saved ISO code.
                if (image.sprite == null && !string.IsNullOrEmpty(country))
                {
                    var label = countryLabel != null ? countryLabel.GetComponent<TextMeshProUGUI>() :
                        new GameObject("CountryCode", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                    label.transform.SetParent(image.transform, false);
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
                    label.font = row.Find("Text_NickName").GetComponent<TextMeshProUGUI>().font;
                    label.fontSize = 18;
                    label.alignment = TextAlignmentOptions.Center;
                    label.text = country;
                    label.raycastTarget = false;
                }
            }
        }
    }

    private bool IsCurrent(int version) => this != null && isActiveAndEnabled && version == requestVersion;
    private void ShowError(string error)
    {
        loading = false;
        if (timeout != null) { StopCoroutine(timeout); timeout = null; }
        total = offset;
        status.transform.SetAsLastSibling();
        status.gameObject.SetActive(true);
        status.text = error;
    }

    private IEnumerator RequestTimeout(int version)
    {
        yield return new WaitForSecondsRealtime(25);
        if (IsCurrent(version) && loading)
        {
            timeout = null;
            ++requestVersion;
            ShowError(UILocalization.Get("ui.bc39a3e6d582"));
        }
    }

    private void ArmTimeout(int version)
    {
        if (timeout != null) StopCoroutine(timeout);
        timeout = StartCoroutine(RequestTimeout(version));
    }

    private void OnDisable()
    {
        ++requestVersion;
        loading = false;
        StopAllCoroutines();
        timeout = null;
    }
}
