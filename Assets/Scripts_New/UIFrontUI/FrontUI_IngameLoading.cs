using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrontUI_IngameLoading : UIFrontUI
{
    [SerializeField] private TextMeshProUGUI loadingTxt;
    [SerializeField] private Slider slider;

    public void GotoLobby()
    {
        UI_LobbyRe.lastPlay = UI_LobbyRe.LastPlay.Rtts;
        slider.value = 0f;
        slider.DOValue(1f, 1f)
              .SetEase(Ease.Linear)
              .OnComplete(Finish);
    }

    private void Finish()
    {
        Close();
    }
}
