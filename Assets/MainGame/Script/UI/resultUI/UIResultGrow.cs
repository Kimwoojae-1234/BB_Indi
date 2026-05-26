using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UIResultGrow : MonoBehaviour
    {
        const int initPosX1 = 16;
        const int initPosX2 = -16;
        const int initPosY = -6;
        const int gab = -42;

        public GameObject _active;
        public Transform batterGrow, pitcherGrow;

        public GameObject Light;

        private readonly int MAX_LEVEL = 20;

        

        public void init()
        {
         
            // DISABLED_MGRS: SeasonGameInfo info = Mgrs.userData.Ingame_seasonGameInfo;// .userSeasonGameInfo.seasonGameInfo;
            SeasonGameEndInfo resultInfo = ResultUI.GetSeasonEndInfo();

            //타자 성장 세팅
            //List<CPlayer> batterList = SimulPlayerManager.GetBatterChangeList(0);
            //int max = batterList.Count;

            int count = 0;            
            int MaxList = resultInfo.cardInfos.Count;

            float delay = 0.5f;

            for(int i =0; i< SimulPlayer.NUM_FIELDER ; i++)// for (int i = 0; i < max; i++)
            {
                CPlayer player = SimulPlayerManager.GetFielder(0, i);// batterList[i];
                if (player.getCard() != null)
                {
                    if (player.getCard().level == MAX_LEVEL)
                    {
                        resultGrowBar grow = Util.Load("MainGame/prefabs/resultUI/resultPlayerGrowPrefab", batterGrow.transform, new Vector3(initPosX1, (initPosY + count * gab), 0)).GetComponent<resultGrowBar>();
                        grow.setPlayer(player.getCard(), player, false, delay, true);
                        //delay += 0.2f;
                        count++;
                    }
                    else
                    {
                        for (int j = 0; j < MaxList; j++)
                        {
                            GameCardInfo growCard = resultInfo.cardInfos[j];
                            if (growCard.cardSeq == player.getCard().cardSeq)
                            {
                                resultGrowBar grow = Util.Load("MainGame/prefabs/resultUI/resultPlayerGrowPrefab", batterGrow.transform, new Vector3(initPosX1, (initPosY + count * gab), 0)).GetComponent<resultGrowBar>();
                                grow.setPlayer(growCard, player, false, delay);
                                delay += 0.2f;
                                count++;
                                //Mgrs.userData.UpdateUserCardData(growCard);
                            }
                        }
                    }
                }
            }

            if (count < 8)
            {
                batterGrow.GetComponent<UIScrollView>().enabled = false;
            }



            //투수 성장 세팅
            //List<int> pitcherList = SimulPlayerManager.GetPitcherChangeList(0);
            //max = pitcherList.Count;
            count = 0;
            delay = 0.5f;

            for(int i = 0; i< SimulPlayer.NUM_PITCHER ;i++) //for (int i = 0; i < max; i++)
            {
                //int pIndex = pitcherList[i];
                //CPlayer player = SimulPlayerManager.GetPitcher(0, pIndex);
                CPlayer player = SimulPlayerManager.GetPitcher(0, i);
                if (player.getCard() != null)
                {
                    if (player.getCard().level == MAX_LEVEL)
                    {
                        resultGrowBar grow = Util.Load("MainGame/prefabs/resultUI/resultPlayerGrowPrefab", pitcherGrow.transform, new Vector3(initPosX2, (initPosY + count * gab), 0)).GetComponent<resultGrowBar>();
                        grow.setPlayer(player.getCard(), player, true, delay, true);
                        //delay += 0.2f;
                        count++;
                    }
                    else
                    {
                        for (int j = 0; j < MaxList; j++)
                        {
                            GameCardInfo growCard = resultInfo.cardInfos[j];
                            if (growCard.cardSeq == player.getCard().cardSeq)
                            {
                                resultGrowBar grow = Util.Load("MainGame/prefabs/resultUI/resultPlayerGrowPrefab", pitcherGrow.transform, new Vector3(initPosX2, (initPosY + count * gab), 0)).GetComponent<resultGrowBar>();
                                grow.setPlayer(growCard, player, true, delay);
                                delay += 0.2f;
                                count++;
                                //Mgrs.userData.UpdateUserCardData(growCard);
                            }
                        }
                    }
                }
            }

            //로컬에서 선수 업데이트
            myPlayerUpdate(resultInfo);

            if (count < 8)
            {
                pitcherGrow.GetComponent<UIScrollView>().enabled = false;
            }

            gameObject.GetComponent<UIPanel>().alpha = 1;
            _active.SetActive(true);
            //TweenAlpha.Begin(gameObject, 0.5f, 1);
            Light.SetActive(true);

        }

        /// <summary>
        /// 투수 업데이트
        /// </summary>
        /// <param name="resultInfo"></param>
        private void myPlayerUpdate(SeasonGameEndInfo resultInfo)
        {
            int count = resultInfo.cardInfos.Count;

            for (int i = 0; i < count; i++)
            {
                GameCardInfo growCard = resultInfo.cardInfos[i];
                // DISABLED_MGRS: Mgrs.userData.UpdateUserCardData(growCard);
            }

        }


        public void deActive()
        {
            TweenAlpha.Begin(gameObject, 0.5f, 0);
            Invoke("deactive", 0.6f);
        }

        private void deactive()
        {
            _active.SetActive(false);
        }

    }
}
