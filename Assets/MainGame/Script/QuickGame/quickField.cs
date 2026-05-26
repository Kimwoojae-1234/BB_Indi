using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class quickField : MonoBehaviour
    {
        public Transform[] position;
        public SkeletonAnimation[] ball;


        int bCount = 0;

        public void Active(SimulBattingData battingData, UIFieldCall call)
        {
            //임시
            SimulResultState result = battingData.result;

            bool bOut = false;
            if (result == SimulResultState.HomeRun)
            {
                call.HomeRun(1);
            }
            else if (result >= SimulResultState.Grounder && result <= SimulResultState.LineOut)
            {
                bOut = true;
                call.Call("out");
            }
            else
            {                
                call.Call("hit");
            }//여기까지

            int index = battingData.fIndex;
            if (index >= CPlayer._PITCHER && index <= CPlayer._RIGHTFIELDER)
            {
                StartCoroutine(ballStart(battingData, bOut));
            }

            
        }


        private IEnumerator ballStart(SimulBattingData battingData, bool bOut)
        {
            string animName = getAnimName(battingData, bCount, bOut);
            if (animName == null)
            {
                yield break;
            }
            else
            {
                ball[bCount].gameObject.SetActive(true);
                ball[bCount].state.ClearTracks();
                ball[bCount].skeleton.SetToSetupPose();
                ball[bCount].state.SetAnimation(0, animName, false);                
                bCount++;
                if (bCount >= 4) bCount = 0;

                yield return new WaitForSeconds(4.0f);
                ball[bCount].gameObject.SetActive(false);
            }

        }

        private string getAnimName(SimulBattingData battingData, int count, bool bOut)
        {
            string anim = null;
            int fIndex = battingData.fIndex;
            SimulHitType hitType = battingData.hitType;
            SimulResultState result = battingData.result;

            bool bInvert = false;
            if (fIndex == CPlayer._CENTERFIELDER)
            {
                bInvert = MyMath.Half(); 
            }
            else if (fIndex == CPlayer._FIRSTBASEMAN || fIndex == CPlayer._SECONDBASEMAN || fIndex == CPlayer._RIGHTFIELDER)
            {
                bInvert = true;
            }

            if (hitType == SimulHitType.Grounder || hitType == SimulHitType.Bunt)
            {
                ball[count].transform.localScale = new Vector3((bInvert ? -95 : 95), 95, 1);
                ball[count].timeScale = 1.0f;
                if (fIndex != CPlayer._CATCHER)
                {
                    string[] num = new string[9] { "01", "01", "03", "02", "03", "02", "04", "05", "04" };
                    anim = "BALL_DOWN_" + num[fIndex];
                }
            }
            else
            {
                ball[count].timeScale = 1.2f;
                if (result == SimulResultState.HomeRun)
                {
                    ball[count].transform.localScale = new Vector3((bInvert ? -110 : 110), 110, 1);
                    if (fIndex == CPlayer._CENTERFIELDER)
                    {
                        anim = "BALL_HOMERUN_01";                        
                    }
                    else
                    {
                        anim = "BALL_HOMERUN_02";
                    }
                }
                else
                {
                    ball[count].transform.localScale = new Vector3((bInvert ? -95 : 95), 95, 1);
                    if (result == SimulResultState.Double || result == SimulResultState.Triple ||
                        result == SimulResultState.DoubleOneError || result == SimulResultState.TripleOneError)
                    {
                        anim = "BALL_07";
                    }
                    else
                    {
                        string[] num = new string[9] { "02", "01", "04", "05", "04", "05", "06", "07", "06" };
                        anim = "BALL_" + num[fIndex];
                    }
                }
            }

            if (bOut == true) return anim + "_OUT";
            else return anim;
        }





        
    }
}