using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class PitchviewManager : MonoBehaviour
    {
        public SkeletonAnimation [] backEffectAnim;
        public Transform stand = null;
        public tk2dSprite field;
        public tk2dSprite[] building;

        public int maxBackAnimNum = 7;

        private int[] backAnimNum;

        private bool bEnable = false;


        private BackGroundType groundType;
        private BackGroundManager.TimeState curTimeState = BackGroundManager.TimeState.Day;


        void Awake()
        {
            bEnable = false;
            curTimeState = BackGroundManager.TimeState.Day;
        }

        public void init()
        {
            groundType = (BackGroundType)(Mode.stadiumType);
            int count = 0;

            
            if (stand != null)
            {
                foreach (Transform child in stand)
                {
                    SkeletonAnimation anim = child.gameObject.GetComponent<SkeletonAnimation>();
                    if (anim != null)
                    {
                        setCrowdAnim(anim, count);
                        count++;
                    }
                }
            }

            int animNum = backEffectAnim.Length;
            if (animNum > 0)
            {
                bEnable = true;
                backAnimNum = new int[animNum];
                for (int i = 0; i < backEffectAnim.Length; i++)
                {
                    backEffectAnim[i].skeleton.SetColor(new Color(0.85f, 0.85f, 0.85f));
                    backAnimNum[i] = Random.Range(1, maxBackAnimNum);
                    StartCoroutine(playBackAnim(i));
                }
            }
        }

        void OnEnable()
        {
            if (bEnable == true)
            {
                for (int i = 0; i < backEffectAnim.Length; i++)
                {
                    StartCoroutine(playBackAnim(i));
                }
            }
            groundType = (BackGroundType)(Mode.stadiumType);
            if (groundType != BackGroundType.Dome)
            {
                checkTimeChange();
            }

        }

        void OnDisable()
        {
            StopAllCoroutines();
        }



        private IEnumerator playBackAnim(int index)
        {
            //backEffectAnim.state.ClearTrack(0);
            backEffectAnim[index].skeleton.SetToSetupPose();
            backEffectAnim[index].state.SetAnimation(0, "DISPLAY_0" + backAnimNum[index], false);
            backEffectAnim[index].timeScale = 0.75f;

            yield return new WaitForSeconds(15);
            backAnimNum[index]++;
            if (backAnimNum[index] > maxBackAnimNum) backAnimNum[index] = 1;

            StartCoroutine(playBackAnim(index));
        }


        private void setCrowdAnim(SkeletonAnimation crowd, int count)
        {
            crowd.transform.localScale = new Vector3(100, 100, 100);
            crowd.transform.localPosition = new Vector3(-870 + count * 122, -23, -0.001f);
            int crowdNum = (count % 6) + 1;// Random.Range(1, 7);
            crowd.state.SetAnimation(0, "PITCHERVIEW_CROWD_"+crowdNum, true);
            crowd.timeScale = Random.Range(0.9f, 1.1f);

            crowd.skeleton.SetColor(new Color(0.85f, 0.85f, 0.85f));
        }


        public void checkTimeChange()
        {
            if (curTimeState != BackGroundManager.GetTimeState())
            {
                curTimeState = BackGroundManager.GetTimeState();

                Color buildingColor = new Color(1, 1, 1);
                Color fieldColor = new Color(1, 1, 1);

                if (curTimeState == BackGroundManager.TimeState.Evening)
                {
                    buildingColor = new Color(0.792f, 0.741f, 0.667f);
                    fieldColor = new Color(1, 0.902f, 0.749f);
                }
                else if (curTimeState == BackGroundManager.TimeState.Night)
                {
                    buildingColor = new Color(0.624f, 0.690f, 1.0f);
                    fieldColor = new Color(1, 1, 1);
                }

                field.color = fieldColor;
                for (int i = 0; i < building.Length; i++) building[i].color = buildingColor;
                
                /*
                if (stand != null)
                {
                    foreach (Transform child in stand)
                    {
                        SkeletonAnimation anim = child.gameObject.GetComponent<SkeletonAnimation>();
                        if (anim != null)
                        {
                            anim.skeleton.SetColor(buildingColor);
                        }
                    }
                }*/
            }
        }
    }
}