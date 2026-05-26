using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UILeagueFinalTitle : MonoBehaviour
    {
        public FinalResultUI finalRewardMain;
        public GameObject _active;
        // Use this for initialization

        public UISprite leagueLogo;

        public GameObject pitcher, batter;

        public UISprite[] pLabel;
        public UISprite[] bLabel;

        public UISprite cursor;
        public GameObject mvp, title;
        public GameObject next;

        public GameObject light;


        public GameObject cardLight;


        private int step;
        private bool bPitcherTitle;

        private bool bPressNext;

        public void InitSeasonTitle()
        {
            leagueLogo.spriteName = "league_" + finalRewardMain.getLobbyInfo().annInfo.newInfo[0];

            bPitcherTitle = false;
            step = 0;
            

            StartCoroutine(init());
        }


        private IEnumerator init()
        {
            next.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            finalRewardMain.fadeIn();
            _active.SetActive(true);
            yield return new WaitForSeconds(0.2f);


            yield return new WaitForSeconds(0.5f);
            setStep();
            yield return new WaitForSeconds(0.4f);
            next.SetActive(true);
            bPressNext = false;
        }

        

        private void setStep()
        {
            if (bPitcherTitle == false)
            {
                if (step > 5)
                {
                    pitcher.SetActive(true);
                    batter.SetActive(false);
                    bPitcherTitle = true;
                }
            }
            else
            {
                if (step < 6)
                {
                    pitcher.SetActive(false);
                    batter.SetActive(true);
                    bPitcherTitle = false;
                }
            }



            if (bPitcherTitle == false)
            {
                for (int i = 0; i < 6; i++)
                {
                    bLabel[i].color = (i == step) ? new Color(0.094f, 0.859f, 0.545f) : new Color(0.455f, 0.529f, 0.776f);
                }
                int posX = -409 + bLabel[step].width + 10;
                cursor.transform.localPosition = new Vector3(posX, 8 - (step % 6) * 30, 0);
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    pLabel[i].color = (i == (step % 6)) ? new Color(0.094f, 0.859f, 0.545f) : new Color(0.455f, 0.529f, 0.776f);
                }
                int posX = -409 + pLabel[step % 6].width + 10;
                cursor.transform.localPosition = new Vector3(posX, 8 - (step % 6) * 30, 0);
            }

            SeasonAnnounceInfo annInfo = finalRewardMain.getLobbyInfo().annInfo;

            SeasonTitleMvpRewardInfo batterMvp = annInfo.titleHitterMvp;
            SeasonTitleMvpRewardInfo pitcherMvp = annInfo.titlePitcherMvp;
            Dictionary<MvpType, List<SeasonTitleRewardInfo>> titleInfo = annInfo.titleInfo;

            MvpType[] stepType = new MvpType[12]{MvpType.H_MVP, MvpType.H_BA, MvpType.H_HOMERUN, MvpType.H_RBI, MvpType.H_HIT, MvpType.H_SB,
                                                      MvpType.P_MVP, MvpType.P_WIN, MvpType.P_ERA, MvpType.P_SO, MvpType.P_SAVE, MvpType.P_HOLD};

            if (step != 0)
            {
                UITweener tween = light.GetComponent<UITweener>();
                tween.ResetToBeginning();
                tween.PlayForward();
            }

            if (step == 0 || step == 6)
            {
                mvp.GetComponent<UIPanel>().alpha = 0;
                mvp.transform.localPosition = new Vector3(70, 0, 0);
                mvp.SetActive(true);
                title.SetActive(false);
                mvp.GetComponent<mvpTitle>().initMvp((step == 0 ? batterMvp : pitcherMvp), finalRewardMain, (step == 0 ? false : true));                                
                TweenAlpha.Begin(mvp, 0.2f, 1.0f);
                TweenPosition.Begin(mvp, 0.2f, Vector3.zero);
                bPressNext = false;
            }
            else
            {
                MvpType curType = stepType[step];
                List<SeasonTitleRewardInfo> rewardTitleList = titleInfo[curType];
            

                mvp.SetActive(false);
                title.SetActive(true);

                int count = 0;
                foreach (Transform child in title.transform)
                {
                    individualTitle curTitle = child.GetComponent<individualTitle>();                    
                    if (curTitle != null)
                    {
                        curTitle.GetComponent<UISprite>().alpha = 0;
                        curTitle.transform.localPosition = new Vector3((-230 + 50 + count * 218), 200, 0);
                        SeasonTitleRewardInfo rewardTitle = rewardTitleList[count];
                        curTitle.setIndividualTitle(rewardTitle, finalRewardMain);

                        StartCoroutine(setPosition(curTitle.gameObject, 0.2f * count, count));

                        count++;
                    }
                }

                StartCoroutine(setCardLight());
            }
        }


        private IEnumerator setPosition(GameObject obj, float delay, int count)
        {
            yield return new WaitForSeconds(delay);

            TweenAlpha.Begin(obj, 0.2f, 1.0f);
            TweenPosition.Begin(obj, 0.2f, new Vector3((-230 + count * 218), 200, 0));
        }

        private IEnumerator setCardLight()
        {            
            int count = 0;
            UISprite[] card = new UISprite[3];
            foreach (Transform child in cardLight.transform)
            {
                card[count] = child.GetComponent<UISprite>();
                count++;
            }

            yield return new WaitForSeconds(0.6f);

            cardLight.gameObject.SetActive(true);

            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForEndOfFrame();
                for (int j = 0; j < count; j++)
                {
                    card[j].spriteName = "cardback_" + string.Format("{0:00000}", i);
                }
            }

            yield return new WaitForSeconds(0.1f);
            cardLight.gameObject.SetActive(false);
            bPressNext = false;
        }


        public void pressNext()
        {
            if (bPressNext == false)
            {
                bPressNext = true;
                ////Debug.Log("==========================>> next");
                step++;
                if (step >= 12)
                {
                    StartCoroutine(deActive());
                    SeasonAnnounceInfo annInfo = finalRewardMain.getLobbyInfo().annInfo;
                    if (annInfo.titleRwdItems != null)
                    {
                        if (annInfo.titleRwdItems.Count > 0)
                        {
                            //종합 보상창
                            finalRewardMain.changeScene();
                            finalRewardMain.totalReward.InitSeasonTitleReward();
                            return;
                        }
                    }
                    //리그 승강
                    finalRewardMain.leagueUpDown.InitSeasonLeagueUpDown();
                }
                else
                {
                    setStep();
                }
            }
        }

        public void pressBack()
        {
            step--;
            if (step < 0)
            {
                step = 0;
            }
            else
            {
                setStep();
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