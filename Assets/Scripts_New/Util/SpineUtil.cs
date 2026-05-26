using Spine.Unity;

public static class SpineUtil
{
    public static void ReplayAnimation(SkeletonGraphic anim, int track = 0)
    {
        anim.gameObject.SetActive(true);
        // 현재 Track(예: 0번 트랙)에 설정된 애니메이션을 다시 플레이
        var currentAnim = anim.AnimationState.GetCurrent(track)?.Animation;
        if (currentAnim != null)
        {
            anim.AnimationState.SetAnimation(track, currentAnim, false);
        }
    }
}
