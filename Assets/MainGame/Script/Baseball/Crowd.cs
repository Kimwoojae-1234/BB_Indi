#define _NO_CROWD
//#define _BATINGVIEW_OLD_CROWD


using UnityEngine;
using System.Collections;
using System.Text;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class Crowd : MonoBehaviour
    {
        public enum CrowdType{
            BattingviewOut,
            BattingviewIn,
            FieldViewOut,            
            FieldViewIn,
            BattingviewSinge,
            FieldViewOutSide,
            FieldViewSignle
        }

        public CrowdType crowdType = CrowdType.BattingviewOut;  

        public SkeletonAnimation anim;
        //int lastTrack = 0;
        private bool bFieldCrowd = false;
        private int type;
        private bool bActive;
        private bool bRendering;
        private bool bCameraCheck;

        // Use this for initialization
        void Start()
        {
#if _NO_CROWD
            bActive = false;
            bRendering = false;

#else
            bCameraCheck = false;
            if (crowdType == CrowdType.FieldViewIn || crowdType == CrowdType.FieldViewOut || crowdType == CrowdType.FieldViewOutSide)
            {
                bFieldCrowd = true;
                type = (Random.Range(0, 100) % 4) + 1;
            }
            else
            {
                bFieldCrowd = false;
                type = (Random.Range(0, 100) % 4) + 1;
                float localZ = -0.03f + (transform.localPosition.y * 0.0001f);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, localZ);
                anim = GetComponent<SkeletonAnimation>();
            }

            if (MyMath.Percent() < 70)
            {
                bActive = true;
                if (bFieldCrowd == false)
                {
                    if (crowdType == CrowdType.BattingviewOut || crowdType == CrowdType.BattingviewSinge)
                    {
                        setNormal();
                    }
                    else
                    {
                        setCheerUp();
                    }                    
                }
                else
                {
                    setFieldNormal();                    
                }
                setColor(new Color(1, 1, 1));
            }
            else
            {
                bActive = false;
            }

            //anim.GetComponent<Renderer>().enabled = bActive;
            anim.gameObject.SetActive(bActive);
            bRendering = bActive;
                    
#endif
        }


        void OnBecameVisible()
        {
            //anim.GetComponent<SkeletonAnimation>().enabled = true;
        }

        void OnBecameInvisible()
        {
            //anim.GetComponent<SkeletonAnimation>().enabled = false;
        }



        private void setAnim(string strAnim, float timeScale = 1.0f)//, int track)
        {
            //if (track != lastTrack) anim.state.ClearTrack(lastTrack);
            anim.skeleton.SetToSetupPose();
            anim.state.SetAnimation(0, strAnim, true);
            anim.timeScale = Random.Range(0.9f, 1.1f) * timeScale;
            //lastTrack = track;
        }

#if _BATINGVIEW_OLD_CROWD
        public void setNormal()
        {
            if (bActive == true)
            {
                //CROWD_01_HOMRUN
                //string strAnim = "CROWD_0" + type + "_NORMAL";
                StringBuilder strAnim = new StringBuilder("CROWD_0");
                strAnim.Append(type);
                strAnim.Append("_NORMAL");

                setAnim(strAnim.ToString());//, 0);//StartCoroutine(setAnim(strAnim, 0));
            }
        }

        public void setCheerUp()
        {
            if (bActive == true)
            {
                //string strAnim = "CROWD_0" + type + "_HOMERUN";                
                StringBuilder strAnim = new StringBuilder("CROWD_0");
                strAnim.Append(type);
                strAnim.Append(type == 1 ? "_HOMRUN" : "_HOMERUN");

                setAnim(strAnim.ToString());//, 1);//StartCoroutine(setAnim(strAnim, 1));
            }
        }
#else
        public void setNormal()
        {
            if (bActive == true)
            {
                string strAnim;
                if (crowdType == CrowdType.BattingviewSinge) strAnim = "CROWD_FRONT_NORMAL_SINGLE_" + type;
                else strAnim = "CROWD_FRONT_NORMAL_" + type;
                setAnim(strAnim, 0.5f);//, 0);//StartCoroutine(setAnim(strAnim, 0));
            }
        }

        public void setCheerUp()
        {
            if (bActive == true)
            {
                string strAnim;
                if (crowdType == CrowdType.BattingviewSinge) strAnim = "CROWD_FRONT_HOMERUN_SINGLE_" + type;
                else strAnim = "CROWD_FRONT_HOMERUN_" + type;
                setAnim(strAnim);//, 1);//StartCoroutine(setAnim(strAnim, 1));
            }
        }
#endif



        public void setFieldNormal()
        {
            if (bActive == true)
            {
                string strAnim;// = "CROWD_FRONT_NORMAL_" + type;
                if (crowdType == CrowdType.FieldViewOutSide)
                {
                    strAnim = "CROWD_SIDE_NORMAL_" + type;
                }
                else if (crowdType == CrowdType.FieldViewIn)
                {
                    strAnim = "CROWD_BACK_NORMAL_" + type;
                }
                else if (crowdType == CrowdType.FieldViewSignle)
                {
                    strAnim = "CROWD_FRONT_NORMAL_SINGLE_" + type;
                }
                else
                {
                    strAnim = "CROWD_FRONT_NORMAL_" + type;
                }

                setAnim(strAnim, 0.75f);//, 0);//StartCoroutine(setAnim(strAnim, 0));*/
            }
        }


        public void setColor(Color color)
        {
            if (bActive == true)
            {
                anim.skeleton.SetColor(color);
            }
        }

    }
}
