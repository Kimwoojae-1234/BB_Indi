using System.Collections;
using UnityEngine;
using Spine.Unity;

public class FrontUI_Transition : UIFrontUI
{
    [SerializeField] SkeletonGraphic anim;

    public override void Open()
    {
        base.Open();
        Invoke("Close", 0.5f);
    }


    public void SetTransitionType(UITransition type)
    {
        string anim_name = "2_loop";

        if (type == UITransition.Type1)
        {
            anim_name = "2_loop_l";
        }
        else if (type == UITransition.Type2)
        {
            anim_name = "2_loop_p";
        }
        else if (type == UITransition.Type3)
        {
            anim_name = "2_loop_pl";
        }

        anim.AnimationState.ClearTrack(0);
        anim.AnimationState.SetAnimation(0, anim_name, true);
    }


}
