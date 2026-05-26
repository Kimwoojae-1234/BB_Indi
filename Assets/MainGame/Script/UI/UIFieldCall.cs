using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class UIFieldCall : MonoBehaviour
    {
        public GameObject _active;
        public UISprite callSpr;
        public UISprite [] doublePlay;
        public Spine.Unity.SkeletonAnimation homeRunAnim;



        public void SetActive(bool bActive)
        {
            _active.SetActive(bActive);
        }


        public void HomeRun(int num)
        {
            StartCoroutine(homerun(num));
        }

        private IEnumerator homerun(int num)
        {
            homeRunAnim.gameObject.SetActive(true);
            homeRunAnim.skeleton.SetToSetupPose();
            homeRunAnim.state.SetAnimation(0, (num >= 4 ? "homerun_ui_02" : "homerun_ui_01"), false);
            if (Mode.bSimulationQuickPlay == true)
            {
                homeRunAnim.timeScale = 1.5f;
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                homeRunAnim.timeScale = 1;
                yield return new WaitForSeconds(3.0f);
            }
            

            homeRunAnim.gameObject.SetActive(false);
        }

        public void Call(string callStr, int doublePlayCount = 0)
        {
            if (doublePlayCount > 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    doublePlay[i].enabled = true;
                    if (doublePlayCount >= 100)
                    {
                        doublePlay[i].spriteName = "callsign_fineplay";
                    }
                    else
                    {
                        doublePlay[i].spriteName = (doublePlayCount == 2 ? "callsign_doubleplay" : "callsign_tripleplay");
                    }
                    doublePlay[i].MakePixelPerfect();
                }
            }
            else
            {
                for (int i = 0; i < 2; i++) doublePlay[i].enabled = false;
            }

            StartCoroutine(call(callStr));
        }

        private IEnumerator call(string callStr)
        {
            callSpr.spriteName = "callsign_" + callStr;
            callSpr.MakePixelPerfect();
            _active.SetActive(true);
            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("battingCallAnim"));
            if (Mode.bSimulationQuickPlay == true) anim.speed = 1.5f;
            else anim.speed = 1;
            yield return new WaitForSeconds(2.0f);
            _active.SetActive(false);
        }


    }
}