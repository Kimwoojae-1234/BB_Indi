using UnityEngine;
using System.Collections;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class FieldCrowd : MonoBehaviour
    {
        public enum CrowdType
        {
            FieldViewOut,
            FieldViewIn,
            FieldViewOutSide,
            FieldViewSignle
        }

        public FieldCrowd.CrowdType type;
        
        SkeletonAnimation anim;
        private int index;
        void Awake()
        {
            anim = GetComponent<SkeletonAnimation>();

            if (MyMath.Percent() < 70)
            {
                index = (Random.Range(0, 100) % 4) + 1;
                setFieldNormal();
                anim.skeleton.SetColor(new Color(1, 1, 1));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }


        void OnBecameVisible()
        {
            anim.enabled = true;
        }

        void OnBecameInvisible()
        {
            anim.enabled = false;
        }


        public void setFieldNormal()
        {
                string strAnim;// = "CROWD_FRONT_NORMAL_" + type;
                if (type == CrowdType.FieldViewOutSide)
                {
                    strAnim = "CROWD_SIDE_NORMAL_" + index;
                }
                else if (type == CrowdType.FieldViewIn)
                {
                    strAnim = "CROWD_BACK_NORMAL_" + index;
                }
                else if (type == CrowdType.FieldViewSignle)
                {
                    strAnim = "CROWD_FRONT_NORMAL_SINGLE_" + index;
                }
                else
                {
                    strAnim = "CROWD_FRONT_NORMAL_" + index;
                }

                anim.skeleton.SetToSetupPose();
                anim.state.SetAnimation(0, strAnim, true);
                anim.timeScale = Random.Range(0.9f, 1.1f) * 0.75f;
        }
    }
}