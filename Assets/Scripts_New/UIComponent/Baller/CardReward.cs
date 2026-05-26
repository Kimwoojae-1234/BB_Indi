using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting;

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


    public void Init(KOBRewardInfo reward, float delay)
    {
        int idx = reward.pindex;
        CharacterData data = KOBManager.Backend.Chart.CharacterData.GetData(idx);
        KOBBaller info = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[idx]; //선수 성장정보
        UpgradeChart UpgradeData = KOBManager.Backend.Chart.UpgradeData;            //업글 정보


        Frame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "Frame_CardFrame_" + data.rarity.ToString());
        KOBManager.Resource.LoadBallerPortrait(Potrait, data.char_idx);
        Name.text = KOBManager.Localization.GetUILocalizedValue2(data.name_id);

        int amount = reward.amount;

        //슬라이더
        int CardNeed = UpgradeData.UpgradeCard(info.level + 1, data.rarity);
        int CurCard = info.card_number - amount;
        Arrow.gameObject.SetActive(CurCard >= CardNeed ? true : false);
        CardSlider.value = (float)CurCard / (float)CardNeed;
        CardNum.text = string.Format("{0}/{1}", CurCard, CardNeed);

        //레벨 프레임
        ImgLevel.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + data.rarity.ToString());
        Level.text = info.level.ToString();

        if(amount > 0)
        {
            int last = CurCard;
            int cur = CurCard + amount;

            float _time = 0.5f + (cur-last) * 0.1f;
            if (_time > 1.5f) _time = 1.5f;

            DOTween.To(() => last, x => last = x, cur, _time)
                   .SetDelay(delay)
                   .OnUpdate(() => {
                       CardSlider.value = (float)last / (float)CardNeed;
                       CardNum.text = string.Format("{0}/{1}", last, CardNeed);
                       Arrow.gameObject.SetActive(last >= CardNeed ? true : false);
                   })
                   .OnComplete(() => { });
        }
    }
}
