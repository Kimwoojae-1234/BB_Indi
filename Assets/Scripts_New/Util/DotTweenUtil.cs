using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class DotTweenUtil
{
    public static void Restart(GameObject anim)
    {
        DOTweenAnimation tAnim = anim.GetComponent<DOTweenAnimation>();
        if (tAnim != null)
        {
            tAnim.tween.Rewind();
            tAnim.tween.Kill();
            if (tAnim.isValid)
            {
                tAnim.CreateTween();
                tAnim.tween.Play();
            }
        }
    }

    public static void Stop(GameObject anim)
    {
        DOTweenAnimation tAnim = anim.GetComponent<DOTweenAnimation>();
        if (tAnim != null)
        {
            tAnim.DOPause();
        }
    }
}