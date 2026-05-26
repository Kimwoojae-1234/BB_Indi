using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LobbyPropertyComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextGold = null;
    [SerializeField] private TextMeshProUGUI TextGem = null;
    [SerializeField] private TextMeshProUGUI TextEnergy = null;
    [SerializeField] private Transform TargetGold = null;
    [SerializeField] private Transform TargetGem = null;
    [SerializeField] private Transform TargetStamina = null;


    private long lastGold, lastGem;
    private int lastEnergy, lastMaxEnergy;
    const int PROCESS_TOTAL_FRAME = 20;   //낮을수록 빨리 끝남

    System.Type LastWindow;

    public void InitProperty(System.Type type)
    {
        LastWindow = type;
        CurrencyInfo currencyInfo = KOBManager.MyInfo.GameData.CurrencyInfo;
        lastGold = currencyInfo.Gold;
        lastGem = currencyInfo.TotalGem;
        lastEnergy = currencyInfo.Energy;
        lastMaxEnergy = KOBManager.MyInfo.GameData.GrowthInfo.MaxEnergy;

        TextGold.text = lastGold.ToString("N0");
        TextGem.text = lastGem.ToString("N0");
        TextEnergy.text = string.Format("{0}/{1}", lastEnergy.ToString("N0"), lastMaxEnergy.ToString("N0"));
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        StopAllCoroutines();
    }

    public void UpdateProperty()
    {
        CurrencyInfo currencyInfo = KOBManager.MyInfo.GameData.CurrencyInfo;
        if (lastGold != currencyInfo.Gold ||
            lastGem != currencyInfo.TotalGem ||
            lastEnergy != currencyInfo.Energy)
        {
            Debug.Log("재화 업데이트");
            StartCoroutine(propertyUpdate());
        }
    }


    private IEnumerator propertyUpdate()
    {
        CurrencyInfo currencyInfo = KOBManager.MyInfo.GameData.CurrencyInfo;
        if (lastGem != currencyInfo.TotalGem)
        {
            Vector2 screenPosition1 = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector3 screenPosition2 = Camera.main.WorldToScreenPoint(TargetGem.position);
            KOBManager.FrontUI.OpenPopup<Ui_RewardAcquisitionDirection>().StartDirection(KOBReward.Gem, screenPosition1, screenPosition2);

            long gem = lastGem;
            lastGem = currencyInfo.TotalGem;
            DOTween.To(() => gem, x => gem = x, lastGem, 0.5f)
                   .SetDelay(1.0f)
                   .OnUpdate(() => TextGem.text = gem.ToString("N0"))
                   .OnComplete(() => { });

            yield return new WaitForSeconds(2.0f);
        }
        if (lastGold != currencyInfo.Gold)
        {
            Vector2 screenPosition1 = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector3 screenPosition2 = Camera.main.WorldToScreenPoint(TargetGold.position);
            KOBManager.FrontUI.OpenPopup<Ui_RewardAcquisitionDirection>().StartDirection(KOBReward.Gold, screenPosition1, screenPosition2);

            long gold = lastGold;
            lastGold = currencyInfo.Gold;
            DOTween.To(() => gold, x => gold = x, lastGold, 0.5f)
                   .SetDelay(1.0f)
                   .OnUpdate(() => TextGold.text = gold.ToString("N0"))
                   .OnComplete(() => { });

            yield return new WaitForSeconds(2.0f);
        }
        if (lastEnergy != currencyInfo.Energy)
        {
            Vector2 screenPosition1 = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector3 screenPosition2 = Camera.main.WorldToScreenPoint(TargetStamina.position);
            KOBManager.FrontUI.OpenPopup<Ui_RewardAcquisitionDirection>().StartDirection(KOBReward.Energy, screenPosition1, screenPosition2);

            int energy = lastEnergy;
            lastEnergy = currencyInfo.Energy;
            DOTween.To(() => energy, x => energy = x, lastEnergy, 0.5f)
                   .SetDelay(1.0f)
                   .OnUpdate(() => TextEnergy.text = string.Format("{0}/{1}", energy.ToString("N0"), lastMaxEnergy.ToString("N0")))
                   .OnComplete(() => { });

            yield return new WaitForSeconds(2.0f);
        }
    }




    public void OnClickAddStamina()
    {
        Debug.Log("OnClickAddStamina");
        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = LastWindow;
    }


    public void OnClickAddGold()
    {
        Debug.Log("OnClickAddGold");
        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = LastWindow;
    }


    public void OnClickAddGem()
    {
        Debug.Log("OnClickAddGem");
        KOBManager.UI.OpenWindow<UI_Shop>().LastWindow = LastWindow;
    }


    public void StopProcess()
    {
        StopAllCoroutines();
    }
}
