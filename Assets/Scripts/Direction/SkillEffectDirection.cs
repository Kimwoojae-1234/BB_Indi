using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectDirection : MonoBehaviour
{
    [SerializeField] float delay = 2.0f;
    [SerializeField] float duration = 1.0f;
    [SerializeField] Vector3 movement = Vector3.one;

    [SerializeField] GameObject fx;
    [SerializeField] GameObject icon;
    [SerializeField] ParticleSystemRenderer icon1;
    [SerializeField] ParticleSystemRenderer icon2;
    [SerializeField] Spine.Unity.BoneFollower follower;

    float time;
    float adjust;
    Vector3 startPosition;
    Vector3 finishPosition;
    GameObject effectObject = null;

    private bool bPassive = false;
    private bool soundOn = false;

    public void SetActive(bool active, bool isLegend = false)
    {
        gameObject.SetActive(active);
        if (active && soundOn)
        {
            if(isLegend)
                Invoke("PlayLegendSkillSound", 0.5f);
            else
                Invoke("PlayCommonSkillSound", 0.5f);
        }
    }

    public void PlayCommonSkillSound()
    {
        
    }

    public void PlayLegendSkillSound()
    {
        
    }

    public void SetIcon(Spine.Unity.SkeletonAnimation skeletonAnimation, string iconName)
    {
        bPassive = false;
        soundOn = true;
        follower.SkeletonRenderer = skeletonAnimation;
        Spine.Bone bone = skeletonAnimation.Skeleton.FindBone("ct_head");
        if (bone != null)
        {
            // batter
            follower.SetBone("ct_head");
            follower.LateUpdate();
            adjust = 250.0f;
        }
        else
        {
            bone = skeletonAnimation.Skeleton.FindBone("ct_head1");
            if (bone != null)
            {
                // pitcher
                follower.SetBone("ct_head1");
                follower.LateUpdate();
                adjust = 100.0f;
            }
            else
            {
                bone = skeletonAnimation.Skeleton.FindBone("HD");
                if (bone != null)
                {
                    // fielder
                    follower.SetBone("HD");
                    follower.LateUpdate();
                    adjust = 230.0f;
                    soundOn = false;
                }
            }
        }
        icon.transform.localPosition = new Vector3(follower.transform.localPosition.x, follower.transform.localPosition.y + adjust, icon.transform.localPosition.z);
        icon1.material = new Material(icon1.material);
        icon2.material = new Material(icon2.material);
        //icon1.material.mainTexture = ui_manager.GetInstance.GetSprite(iconName).texture;
        //icon2.material.mainTexture = ui_manager.GetInstance.GetSprite(iconName).texture;
        icon1.material.mainTexture = Resources.Load<Texture>(string.Format("MainGame/skillicon/{0}", iconName));
        icon2.material.mainTexture = Resources.Load<Texture>(string.Format("MainGame/skillicon/{0}", iconName));
    }

    public void SetBallIcon(Spine.Unity.SkeletonAnimation skeletonAnimation, Texture texture)
    {
        SetIcon(skeletonAnimation, string.Empty);
        icon1.material.mainTexture = texture;
        icon2.material.mainTexture = texture;
    }

    public void SetPassive()
    {
        bPassive = true;
    }
    public void SetIcon(Sprite sprite)
    {
        icon1.material.SetTexture(0, sprite.texture);
        icon2.material.SetTexture(0, sprite.texture);
    }
    public void SetGameObject(GameObject effectObject)
    {
        this.effectObject = effectObject;
    }
    void Awake()
    {
        startPosition = icon1.transform.localPosition;
        finishPosition = startPosition + movement;
    }
    void OnEnable()
    {
        if(fx != null)
            fx.gameObject.SetActive(!bPassive);
        if(icon != null)
            icon.gameObject.SetActive(true);
        icon1.transform.localPosition = startPosition;
        time = Time.time;
    }
    void OnDisable()
    {
        if (effectObject != null)
        {
            effectObject.SetActive(false);
        }
        if(fx != null)
            fx.gameObject.SetActive(false);
        if(icon != null)
            icon.gameObject.SetActive(false);
        follower.SkeletonRenderer = null;
    }
    void Update()
    {
        float time = Time.time - this.time;
        if (time > delay)
        {
            if (time < (delay + duration))
            {
                float t = time - delay;
                icon1.transform.localPosition = Vector3.Lerp(startPosition, finishPosition, (t / duration));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        icon.transform.localPosition = new Vector3(follower.transform.localPosition.x, follower.transform.localPosition.y + adjust, icon.transform.localPosition.z);
    }
}
