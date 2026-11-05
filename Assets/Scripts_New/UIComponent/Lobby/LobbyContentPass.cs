using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyContentPass : LobbyContentButton
{
    [SerializeField] private Slider PassSlider;
    [SerializeField] private Image PassNoti;
    [SerializeField] private TextMeshProUGUI PassNotiText;
    private bool ready;
    private int lastXp = -1, lastClaims = -1;
    

    public override void InitContent(System.Type type)
    {
        base.InitContent(type);
        Debug.Log("배틀패스 버튼 초기화");
        UpdateContent();
    }

    public override void OnClickButton()
    {
        base.OnClickButton();
        KOBManager.UI.OpenWindow<UI_Pass>().LastWindow = LastWindow;
    }

    public override void UpdateContent()
    {
        base.UpdateContent();
        var config = PassCatalog.Current;
        var growth = KOBManager.MyInfo.GameData.GrowthInfo;
        ready = true;
        lastXp = growth.PassXP;
        lastClaims = growth.PassFreeClaims.Count;
        int completed = config.Completed(growth.PassXP);
        if (PassSlider != null)
        {
            PassSlider.interactable = false;
            PassSlider.transition = Selectable.Transition.None;
            PassSlider.minValue = 0; PassSlider.maxValue = config.xpPerTier;
            PassSlider.value = completed >= config.tiers ? config.xpPerTier : growth.PassXP % config.xpPerTier;
            foreach (var graphic in PassSlider.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
            var text = PassSlider.transform.Find("Frame/TextXP")?.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.GetComponent<LocalizedUIText>()?.Bind("");
                text.text = string.Format("{0}/{1}", (int)PassSlider.value, config.xpPerTier);
            }
        }
        int available = 0;
        for (int tier = 1; tier <= completed; tier++) if (!growth.PassFreeClaims.Contains(tier)) available++;
        if (PassNoti != null) PassNoti.gameObject.SetActive(available > 0);
        if (PassNotiText != null) PassNotiText.text = available.ToString();
    }

    private void LateUpdate()
    {
        if (!ready) return;
        var growth = KOBManager.MyInfo.GameData.GrowthInfo;
        if (growth.PassXP != lastXp || growth.PassFreeClaims.Count != lastClaims) UpdateContent();
    }
}
