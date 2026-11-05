using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardReward : MonoBehaviour
{
    [SerializeField] private Image Frame;
    [SerializeField] private Image Potrait;
    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI CardNum;
    [SerializeField] private Image ImgLevel;
    [SerializeField] private TextMeshProUGUI Level;
    [SerializeField] private Slider CardSlider;
    [SerializeField] private GameObject Arrow;
    private Tween countTween, arrowTween;
    private Vector2 arrowOrigin;
    private bool arrowPositionSaved;

    public void Init(KOBRewardInfo reward, float delay)
    {
        StopTweens();
        var data = KOBManager.Backend.Chart.CharacterData.GetData(reward.pindex);
        var info = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[reward.pindex];
        Frame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_CardFrame_" + data.rarity);
        var glow = transform.Find("Mask/Glow")?.GetComponent<Image>();
        if (glow != null)
        {
            Color color = CardBaller.GetCardBackgroundColor(data.rarity);
            color.a = glow.color.a;
            glow.color = color;
        }
        KOBManager.Resource.LoadBallerPortrait(Potrait, data.char_idx);
        UILocalization.Set(Name, data.name_id);
        ImgLevel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + data.rarity);
        Level.text = info.level.ToString();
        bool maxLevel = info.level >= KOBConstant.MAX_LEVEL;
        int needed = maxLevel ? 0 : KOBManager.Backend.Chart.UpgradeData.UpgradeCard(info.level + 1, data.rarity);
        int count = Mathf.Max(0, info.card_number - reward.amount);
        System.Action refresh = () =>
        {
            CardSlider.value = needed > 0 ? Mathf.Clamp01((float)count / needed) : 1;
            CardNum.text = needed > 0 ? string.Format("{0}/{1}", count, needed) : count.ToString("N0");
            SetArrow(!maxLevel && needed > 0 && count >= needed);
        };
        refresh();
        if (reward.amount > 0)
            countTween = DOTween.To(() => count, value => count = value, info.card_number, Mathf.Min(1.5f, .5f + reward.amount * .1f))
                .SetDelay(delay).OnUpdate(() => refresh()).OnComplete(() => refresh());
    }

    private void SetArrow(bool available)
    {
        var rect = (RectTransform)Arrow.transform;
        if (!arrowPositionSaved) { arrowOrigin = rect.anchoredPosition; arrowPositionSaved = true; }
        Arrow.SetActive(available);
        if (!available)
        {
            arrowTween?.Kill(); arrowTween = null;
            rect.anchoredPosition = arrowOrigin;
        }
        else if (arrowTween == null || !arrowTween.IsActive())
        {
            rect.anchoredPosition = arrowOrigin;
            arrowTween = rect.DOAnchorPosY(arrowOrigin.y + 12, .5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
    }
    private void StopTweens()
    {
        countTween?.Kill(); countTween = null;
        arrowTween?.Kill(); arrowTween = null;
        if (arrowPositionSaved) ((RectTransform)Arrow.transform).anchoredPosition = arrowOrigin;
    }
    private void OnDisable() => StopTweens();
}
