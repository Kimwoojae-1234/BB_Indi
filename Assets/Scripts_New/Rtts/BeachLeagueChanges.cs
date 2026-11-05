using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A pre-game snapshot survives the baseball scene. Only a completed game can consume it.
public class BeachLeagueChanges : MonoBehaviour
{
    private static int league, game, baller;
    private static int[] beforeOrder, beforeRanks;
    private static int beforeTeamRank;
    private static int[] beforeTeamRanks;
    private static readonly bool[] unread = new bool[5];
    private static readonly bool[] rising = new bool[5];
    private static int notificationLeague = -1, notificationBaller = -1;
    private GameObject overlay;
    private GameObject mainArrow;
    private readonly List<GameObject> arrows = new List<GameObject>();
    private TMP_Text template;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        beforeOrder = null;
        notificationLeague = notificationBaller = -1;
        System.Array.Clear(unread, 0, 5);
    }

    private static Dictionary<int, int>[] Leaders() => new[] {
        KOBManager.Rtts.HomerunLeader, KOBManager.Rtts.AvgLeader, KOBManager.Rtts.RbiLeader,
        KOBManager.Rtts.HitLeader, KOBManager.Rtts.OpsLeader };

    public static int Rank(Dictionary<int, int> table, int id, bool qualified = false)
    {
        return BeachLeagueRules.Rank(table, id, qualified ? KOBConstant.QPA_CONSTANT : int.MinValue);
    }

    public static void Capture()
    {
        var manager = KOBManager.Rtts;
        manager.InitStandingInfo();
        manager.InitLeagueLeaderInfo();
        var data = KOBManager.MyInfo.GameData;
        league = data.RttsInfo.League;
        game = data.RttsInfo.PlayGame;
        baller = data.ManageInfo.SelectBaller;
        beforeOrder = manager.StadingInfo.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Select(x => x.Key).ToArray();
        beforeTeamRank = Rank(manager.StadingInfo, 0);
        beforeTeamRanks = Enumerable.Range(0, 10).Select(id => Rank(manager.StadingInfo, id)).ToArray();
        beforeRanks = Leaders().Select((x, i) => Rank(x, baller, i == 1 || i == 4)).ToArray();
    }

    public void Present(UI_RTTS owner, TMP_Text fontTemplate, TMP_Text[] tabs, TMP_Text[] categories)
    {
        template = fontTemplate;
        var data = KOBManager.MyInfo.GameData;
        if (notificationLeague != data.RttsInfo.League || notificationBaller != data.ManageInfo.SelectBaller)
        {
            System.Array.Clear(unread, 0, 5);
            notificationLeague = data.RttsInfo.League;
            notificationBaller = data.ManageInfo.SelectBaller;
        }
        if (mainArrow == null)
        {
            mainArrow = MakeArrow(tabs[2].transform, "LeaderChange");
            for (int i = 0; i < 5; i++) arrows.Add(MakeArrow(categories[i].transform, "RankChange"));
            var title = Label(transform, "beach.league.name");
            title.rectTransform.anchorMin = new Vector2(.08f, .88f);
            title.rectTransform.anchorMax = new Vector2(.45f, .94f);
            title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            title.fontSizeMax = 24;
        }
        if (beforeOrder != null && league == data.RttsInfo.League && game < data.RttsInfo.PlayGame && baller == data.ManageInfo.SelectBaller)
        {
            var leaders = Leaders();
            for (int i = 0; i < 5; i++)
            {
                int after = Rank(leaders[i], baller, i == 1 || i == 4);
                if (after > 0 && (beforeRanks[i] == 0 || beforeRanks[i] != after))
                {
                    unread[i] = true;
                    rising[i] = beforeRanks[i] == 0 || after < beforeRanks[i];
                }
            }
            int afterTeamRank = Rank(KOBManager.Rtts.StadingInfo, 0);
            var oldOrder = beforeOrder;
            beforeOrder = null; // Consume once, including games with unchanged standing.
            if (afterTeamRank != beforeTeamRank)
            {
                FrontAudio.PlayRankChange();
                StartCoroutine(ShowStanding(owner, oldOrder, beforeTeamRank, afterTeamRank));
            }
        }
        else if (beforeOrder != null && (league != data.RttsInfo.League || baller != data.ManageInfo.SelectBaller)) beforeOrder = null;
        Refresh(0);
    }

    public void Visit(int tab, int category = -1)
    {
        if (tab == 2 && category >= 0 && category < 5) unread[category] = false;
        Refresh(tab);
    }

    private void Refresh(int tab)
    {
        if (mainArrow == null) return;
        mainArrow.SetActive(tab != 2 && unread.Any(x => x));
        bool anyRise = Enumerable.Range(0, 5).Any(i => unread[i] && rising[i]);
        SetArrowDirection(mainArrow, anyRise);
        for (int i = 0; i < arrows.Count; i++)
        {
            arrows[i].SetActive(unread[i]);
            SetArrowDirection(arrows[i], rising[i]);
        }
    }

    private GameObject MakeArrow(Transform parent, string label)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = ((RectTransform)parent).pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(40, 30);
        var image = go.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>("Prototype/LeagueRankUp");
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;
        return go;
    }

    private static void SetArrowDirection(GameObject arrow, bool up)
    {
        var image = arrow.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>(up ? "Prototype/LeagueRankUp" : "Prototype/LeagueRankDown");
        image.color = Color.white; // Preserve the supplied green/red fill and white outline.
        arrow.transform.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        PositionArrow(mainArrow);
        foreach (var arrow in arrows) PositionArrow(arrow);
    }

    private static void PositionArrow(GameObject arrow)
    {
        if (arrow == null || !arrow.activeInHierarchy) return;
        var label = arrow.transform.parent.GetComponent<TMP_Text>();
        if (label == null) return;
        label.ForceMeshUpdate();
        var bounds = label.textBounds;
        float textHeight = bounds.size.y > 0 ? bounds.size.y : label.fontSize * .75f;
        float height = textHeight * 1.05f * .7f;
        var image = arrow.GetComponent<Image>();
        float aspect = image.sprite != null ? image.sprite.rect.width / image.sprite.rect.height : 4f / 3f;
        float width = height * aspect;
        var rect = (RectTransform)arrow.transform;
        rect.anchorMin = rect.anchorMax = label.rectTransform.pivot;
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(bounds.max.x + textHeight * .5f + width * .5f, bounds.center.y);
    }

    private RectTransform Box(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return (RectTransform)go.transform;
    }

    private TMP_Text Label(Transform parent, string key, params object[] args)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = template.font;
        text.fontSharedMaterial = template.fontSharedMaterial;
        text.fontSize = 27;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Midline;
        text.enableAutoSizing = true;
        text.fontSizeMin = 16;
        text.fontSizeMax = 27;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(10, 0);
        text.rectTransform.offsetMax = new Vector2(-10, 0);
        UILocalization.Set(text, key, args);
        return text;
    }

    private IEnumerator ShowStanding(UI_RTTS owner, int[] oldOrder, int oldRank, int newRank)
    {
        owner.OnClickTab(1);
        Canvas.ForceUpdateCanvases();
        var source = (RectTransform)owner.GetStandingRow(0).transform;
        float rowWidth = source.rect.width;
        float rowHeight = source.rect.height;
        if (rowWidth <= 0 || rowHeight <= 0) yield break;

        const float padding = 24, headerHeight = 112, gap = 8;
        float step = rowHeight + gap;
        int visibleCount = Mathf.Min(5, oldOrder.Length);
        float viewportHeight = step * visibleCount - gap;
        var shade = Box("BeachLeagueStandingPopup", transform, new Color(0, 0, 0, .72f));
        overlay = shade.gameObject;
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.sizeDelta = Vector2.zero;
        var panel = Box("Panel", shade, new Color(.015f, .09f, .2f));
        panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
        panel.sizeDelta = new Vector2(rowWidth + padding * 2, viewportHeight + headerHeight + padding);
        Canvas.ForceUpdateCanvases();
        // Scale the entire panel, never the row layout: preserve the Standing prefab's typography and spacing.
        float scale = Mathf.Min(1, shade.rect.width * .84f / panel.sizeDelta.x, shade.rect.height * .88f / panel.sizeDelta.y);
        panel.localScale = Vector3.one * scale;
        var heading = Box("Title", panel, Color.clear);
        heading.anchorMin = heading.anchorMax = new Vector2(.5f, 1);
        heading.pivot = new Vector2(.5f, 1);
        heading.sizeDelta = new Vector2(rowWidth, 52);
        heading.anchoredPosition = new Vector2(0, -12);
        Label(heading, "beach.rank.title").fontSizeMax = 32;
        var caption = Box("Change", panel, Color.clear);
        caption.anchorMin = caption.anchorMax = new Vector2(.5f, 1);
        caption.pivot = new Vector2(.5f, 1);
        caption.sizeDelta = new Vector2(rowWidth, 36);
        caption.anchoredPosition = new Vector2(0, -64);
        Label(caption, "beach.rank.change", oldRank, newRank).color = new Color(1, .85f, .15f);

        var viewport = new GameObject("StandingViewport", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
        viewport.SetParent(panel, false);
        viewport.anchorMin = viewport.anchorMax = new Vector2(.5f, 1);
        viewport.pivot = new Vector2(.5f, 1);
        viewport.sizeDelta = new Vector2(rowWidth, viewportHeight);
        viewport.anchoredPosition = new Vector2(0, -headerHeight);
        var content = new GameObject("StandingRows", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = content.anchorMax = new Vector2(.5f, 1);
        content.pivot = new Vector2(.5f, 1);
        content.sizeDelta = new Vector2(rowWidth, step * oldOrder.Length - gap);

        var nextOrder = KOBManager.Rtts.StadingInfo.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Select(x => x.Key).ToArray();
        int oldPosition = System.Array.IndexOf(oldOrder, 0);
        int newPosition = System.Array.IndexOf(nextOrder, 0);
        int start = BeachLeagueRules.StandingTransitionStart(oldPosition, newPosition, oldOrder.Length);
        int end = BeachLeagueRules.StandingWindowStart(newPosition, nextOrder.Length);
        content.anchoredPosition = new Vector2(0, start * step);
        var rows = new Dictionary<int, StandingComponent>();
        // Offscreen rows are clipped. The viewport shows only five teams, even on a long rank jump.
        foreach (int team in oldOrder)
        {
            var row = Instantiate(owner.GetStandingRow(team), content, false);
            row.name = "StandingTeam" + team;
            row.gameObject.SetActive(true);
            row.InitComp(KOBManager.Rtts.GetTeamIndex(team));
            row.SetDisplayedRank(beforeTeamRanks[team]);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.sizeDelta = new Vector2(rowWidth, rowHeight);
            rect.anchoredPosition = new Vector2(0, -System.Array.IndexOf(oldOrder, team) * step);
            foreach (var graphic in row.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            rows.Add(team, row);
        }
        rows[0].transform.SetAsLastSibling();
        yield return new WaitForSecondsRealtime(.5f);
        for (int i = 0; i < nextOrder.Length; i++)
            ((RectTransform)rows[nextOrder[i]].transform).DOAnchorPosY(-step * i, .9f)
                .SetEase(Ease.InOutCubic).SetUpdate(true).SetLink(overlay);
        bool longJump = Mathf.Abs(oldPosition - newPosition) >= visibleCount;
        if (longJump)
            content.DOAnchorPosY(end * step, .9f).SetDelay(.12f).SetEase(Ease.InOutCubic).SetUpdate(true).SetLink(overlay);
        yield return new WaitForSecondsRealtime(1.05f);
        foreach (var row in rows)
            row.Value.SetDisplayedRank(Rank(KOBManager.Rtts.StadingInfo, row.Key));
        if (!longJump && start != end)
        {
            // After the rank movement, centre the final five-team neighbourhood.
            content.DOAnchorPosY(end * step, .35f).SetEase(Ease.InOutCubic).SetUpdate(true).SetLink(overlay);
            yield return new WaitForSecondsRealtime(.35f);
        }
        yield return new WaitForSecondsRealtime(.8f);
        var group = overlay.AddComponent<CanvasGroup>();
        group.DOFade(0, .25f).SetUpdate(true).SetLink(overlay);
        yield return new WaitForSecondsRealtime(.25f);
        Destroy(overlay); overlay = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (overlay != null) Destroy(overlay);
    }
}
