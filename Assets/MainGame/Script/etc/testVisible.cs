using UnityEngine;
using System.Collections;
using Spine.Unity;

public class testVisible : MonoBehaviour {

	// Use this for initialization

    //private tk2dSpriteAnimator anim;

    SkeletonAnimation anim;
    void Awake()
    {
        anim = GetComponent<SkeletonAnimation>();
    }

	
    void OnBecameVisible()
    {
        anim.enabled = true;
    }

    void OnBecameInvisible()
    {
        anim.enabled = false;
    }
}
