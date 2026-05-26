using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class UIPostSeasonSchedule : MonoBehaviour
    {
        public FinalResultUI finalRewardMain;
        public GameObject _active;

        public GameObject next;

        public GameObject[] trophy;

        public UILabel[] teamName;

        public UITexture[] teamLogo;

        public GameObject myTeam;

        public postScheduleInfo[] scheduleInfo;

        public UISprite leagueLogo;



        //도전자 승리여부
        private bool[] bChallengerWin = new bool[4];

        private bool bPressNext;

        /*
        //테스트용
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitPostSeason();
            }
        }*/

        public void InitPostSeason()
        {
           
        }

        private IEnumerator init()
        {
            //finalRewardMain.changeScene();  //지워지워
            yield return new WaitForSeconds(0.5f);
            finalRewardMain.fadeIn();
            yield return new WaitForSeconds(0.2f);
            _active.SetActive(true);

            yield return new WaitForSeconds(0.9f);
            next.SetActive(true);
            bPressNext = false;
        }

        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                //Debug.Log("==========================>> next");
                StartCoroutine(deActive());

                finalRewardMain.changeScene();
                if (finalRewardMain.curType == FinalResultUI.FinalRewardType.PostSeasonEnd)
                {
                    //포스트 시즌 시작
                    finalRewardMain.leagueFinalTitle.InitSeasonTitle();
                }
                else
                {
                    finalRewardMain.deActive();
                }
            }
        }

        private IEnumerator deActive()
        {
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
            _active.SetActive(false);
        }
    }
}
