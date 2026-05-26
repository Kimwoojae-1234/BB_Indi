using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTextDirection : MonoBehaviour
{
    [SerializeField] Color normal;
    [SerializeField] Color legend;
    [SerializeField] float slideIn = 0.3f;
    [SerializeField] float waiting = 1.4f;
    [SerializeField] float slideOut = 0.3f;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] AnimationCurve movement;
    [SerializeField] GameObject casterObject;
    [SerializeField] Image casterPortrait;
    [SerializeField] Image playerPortrait;
    [SerializeField] Text skillText;
    //[SerializeField] UnityEngine.UI.Extensions.Gradient2 background;
    [SerializeField] CanvasGroup canvasGroup;

    enum States
    {
        SlideIn,
        Waiting,
        SlideOut,
    }
    float time;
    States state;
    Vector3 inner;
    Vector3 outer;
    bool once = true;

    public void Play(int skillIndex, int playerIndex, int casterIndex = -1)
    {
        if (once)
        {
            once = false;
            float offsetY = 0;
            
            this.inner = rectTransform.anchoredPosition + new Vector2(0,offsetY);
            this.outer = new Vector2(0.0f, rectTransform.anchoredPosition.y + offsetY);
        }
        canvasGroup.alpha = 1.0f;
        casterObject.SetActive(casterIndex >= 0);
        SetPortrait(casterPortrait, casterIndex);
        SetPortrait(playerPortrait, playerIndex);
        SetSkillText(skillIndex);
        this.state = States.SlideIn;
        this.time = Time.time;
        this.enabled = true;
        gameObject.SetActive(true);
        PlaySkillInfoSound();
    }

    private void PlaySkillInfoSound()
    {
        
    }
    public void Stop()
    {
        gameObject.SetActive(false);
        this.rectTransform.anchoredPosition = this.outer;
        this.enabled = false;
    }
    void Update()
    {
        float time = Time.time - this.time;
        if (state == States.SlideIn)
        {
            if (time < slideIn)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(outer, inner, movement.Evaluate(time / slideIn));
            }
            else
            {
                rectTransform.anchoredPosition = inner;
                this.state = States.Waiting;
                this.time = Time.time;
            }
        }
        else if (state == States.Waiting)
        {
            if (time > waiting)
            {
                this.state = States.SlideOut;
                this.time = Time.time;
            }
        }
        else if (state == States.SlideOut)
        {
            if (time < slideOut)
            {
                canvasGroup.alpha = movement.Evaluate(time / slideIn);
                rectTransform.anchoredPosition = Vector2.Lerp(inner, outer, movement.Evaluate(time / slideOut));
            }
            else
            {
                Stop();
            }
        }
    }
    void SetPortrait(Image image, int index)
    {
        /*CardBaseData data = MainManager.Database.LoadPlayerData(index);
        if (data != null)
        {
            image.sprite = MainManager.AssetBundle.ResourcesLoad<Sprite>("Sprite/PlayerPortrait", data.PortraitTag);
        }*/
    }
    void SetSkillText(int skillIndex)
    {
        /*SkillBaseData skillData = MainManager.Database.LoadSkillBaseData(skillIndex);
        if (skillData != null)
        {
            if (skillData.IsLegendSkill())
            {
                Gradient oldGradient = background.EffectGradient;
                background.EffectGradient = new Gradient()
                {
                    alphaKeys = oldGradient.alphaKeys,
                    colorKeys = new[]
                    {
                        new GradientColorKey(legend, oldGradient.colorKeys[0].time),
                        new GradientColorKey(legend, oldGradient.colorKeys[1].time)
                    }
                };
            }
            else
            {
                Gradient oldGradient = background.EffectGradient;
                background.EffectGradient = new Gradient()
                {
                    alphaKeys = oldGradient.alphaKeys,
                    colorKeys = new[]
                    {
                        new GradientColorKey(normal, oldGradient.colorKeys[0].time),
                        new GradientColorKey(normal, oldGradient.colorKeys[1].time)
                    }
                };
            }
            skillText.text = string.Format("{0}", MainManager.Localization.GetUILocalizedValue(skillData.NameId, null));
            baseballplay.Util.SetLoadFont_Ingame(skillText);
        }*/
    }
}
