using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using TMPro;
using JetBrains.Annotations;

public class BallerInfoComponent : MonoBehaviour
{
    [SerializeField] private GameObject origin;
    [SerializeField] private BallerTierSliderComp TierSlider;
    [SerializeField] private Image levelFrame;
    [SerializeField] private TextMeshProUGUI levelTxt;
    

    private SkeletonGraphic anim;
    private int idx = 0;


    public delegate void TouchCallBack();
    TouchCallBack touchCallBack = null;


    public void LoadBaller(int _idx, TouchCallBack _callBack = null)
    {
        idx = _idx;

        //캐릭터 로딩 -> 로드 볼러에서 이전로딩된거 자동으로 지워줌
        GameObject baller = KOBManager.Resource.LoadBaller(idx, origin.transform);

        touchCallBack = _callBack;
        if (baller != null && touchCallBack != null)
        {
            // Modern Ballers use Image/RawImage, not the old anim/Spine button.
            foreach (var oldButton in baller.GetComponentsInChildren<Button>(true)) oldButton.enabled = false;
            var button = baller.GetComponent<Button>() ?? baller.AddComponent<Button>();
            button.enabled = true;
            button.transition = Selectable.Transition.None;
            foreach (var graphic in baller.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = graphic.name != "shadow";
                if (graphic.raycastTarget && button.targetGraphic == null) button.targetGraphic = graphic;
            }
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(TouchBaller);
            UIButtonAudio.AttachMissing(baller);
        }

        //정보 업데이트
        UpdateInfo(idx);
    }


    public void UpdateInfo(int _idx)
    {
        KOBBaller ballerInfo = KOBManager.MyInfo.GameData.PlayerInfo.BallerList[_idx]; //변동정보 - 유저가 성장
        CharacterData ballerData = KOBManager.Backend.Chart.CharacterData.GetData(idx); //고정정보 - 선수고유정보
        //레벨(파워)
        levelTxt.text = ballerInfo.level.ToString();
        //희귀도 프레임
        levelFrame.sprite = KOBManager.Atlas.GetSprite(AtlasManager.MyAtlas.UICompo, "LevelFrame_" + ballerData.rarity.ToString());

        //트로피(선수트로피 - 팀트로피와 다름)
        TierSlider.Set(ballerInfo);
    }


    private void OnEnable()
    {
        if(idx != 0)
        {
            //정보 업데이트
            UpdateInfo(idx);
        }
    }


    public void TouchBaller()
    {
        //Debug.Log("TouchBaller");
        if (touchCallBack != null)
        {
            touchCallBack();
        }
    }
}
