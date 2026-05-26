using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class UIResultPlayerRecord : MonoBehaviour
    {

        const int initPosY = 150;
        const int gab = -42;

        public GameObject _active;

        public GameObject batterObj, pitcherObj;
        public GameObject [] batterView, pitcherView;


        public UISprite batterTab, pitcherTab;
        public UISprite myTeamTab, cpuTeamTab;


        private resultStatBar[] batterBar;
        private resultStatBar[] pitcherBar;
        

        private bool bBatterState;
        private bool bMyTeamState;

        

        public void Init(bool bInit = false)
        {
            if (bInit == false)
            {
                bBatterState = true;
                bMyTeamState = true;
                batterBar = new resultStatBar[SimulPlayer.NUM_FIELDER];
                pitcherBar = new resultStatBar[SimulPlayer.NUM_PITCHER];


                initBatter();
                initPitcher();

                batterView[1].SetActive(false);
                pitcherView[1].SetActive(false);

                batterObj.SetActive(true);
                pitcherObj.SetActive(false);
            }
            _active.SetActive(true);
            
        }


        private void initBatter()
        {
            for (int team = 0; team < 2; team++)
            {
                List<CPlayer> batterList = SimulPlayerManager.GetBatterChangeList(team);
                int max = batterList.Count;

                int count = 0;
                CPlayer lastPlayer = null;
                for (int i = 0; i < max; i++)
                {
                    CPlayer player = batterList[i];
                    if (player != lastPlayer)
                    {
                        if (count < SimulPlayer.NUM_FIELDER)
                        {
                            batterBar[count] = Util.Load("MainGame/prefabs/resultUI/resultBatterRecordBarPrefab", batterView[team].transform, new Vector3(0, (initPosY + count * gab), 0)).GetComponent<resultStatBar>();
                            batterBar[count].init(player, team == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex, count);// count % 2 == 0 ? true : false);
                        }
                        lastPlayer = player;
                        count++;
                    }
                }

                if (count < 10)
                {
                    batterView[team].GetComponent<UIScrollView>().enabled = false;
                }
            }
        }

        private void initPitcher()
        {
            for (int team = 0; team < 2; team++)
            {
                List<int> pitcherList = SimulPlayerManager.GetPitcherChangeList(team);
                int max = pitcherList.Count;
                int count = 0;
                CPlayer lastPlayer = null;
                for (int i = 0; i < max; i++)
                {
                    int pIndex = pitcherList[i];
                    CPlayer player = SimulPlayerManager.GetPitcher(team, pIndex);
                    if (player != lastPlayer)
                    {
                        pitcherBar[count] = Util.Load("MainGame/prefabs/resultUI/resultPitcherRecordBarPrefab", pitcherView[team].transform, new Vector3(0, (initPosY + count * gab), 0)).GetComponent<resultStatBar>();//.init(i % 2 == 0 ? true : false);
                        pitcherBar[count].init(player, team == 0 ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex, count);// count % 2 == 0 ? true : false);
                        lastPlayer = player;
                        count++;
                    }
                }

                if (count < 10)
                {
                    pitcherView[team].GetComponent<UIScrollView>().enabled = false;
                }

            }

        }



        public void pressBatterTab()
        {
            if (bBatterState == false)
            {                
                batterTab.spriteName = "stat_tab_on";
                batterTab.transform.Find("spr").GetComponent<UISprite>().spriteName = "stat_batter_on";
                pitcherTab.spriteName = "stat_tab_off";
                pitcherTab.transform.Find("spr").GetComponent<UISprite>().spriteName = "stat_pitcher_off";

                batterObj.SetActive(true);
                pitcherObj.SetActive(false);
                batterView[bMyTeamState ? 0 : 1].SetActive(true);
                batterView[bMyTeamState ? 1 : 0].SetActive(false);
                bBatterState = true;
            }
        }

        public void pressPitcherTab()
        {
            if (bBatterState == true)
            {
                batterTab.spriteName = "stat_tab_off";
                batterTab.transform.Find("spr").GetComponent<UISprite>().spriteName = "stat_batter_off";
                pitcherTab.spriteName = "stat_tab_on";
                pitcherTab.transform.Find("spr").GetComponent<UISprite>().spriteName = "stat_pitcher_on";

                batterObj.SetActive(false);
                pitcherObj.SetActive(true);
                pitcherView[bMyTeamState ? 0 : 1].SetActive(true);
                pitcherView[bMyTeamState ? 1 : 0].SetActive(false);
                bBatterState = false;
            }
        }

        public void pressMyTeamTab()
        {
            if (bMyTeamState == false)
            {
                myTeamTab.spriteName = "stat_myteam1";
                myTeamTab.transform.Find("focus").gameObject.SetActive(true);
                cpuTeamTab.spriteName = "stat_otherteam2";
                cpuTeamTab.transform.Find("focus").gameObject.SetActive(false);
                if (bBatterState == true)
                {
                    batterView[0].SetActive(true);
                    batterView[1].SetActive(false);
                }
                else
                {
                    pitcherView[0].SetActive(true);
                    pitcherView[1].SetActive(false);
                }

                bMyTeamState = true;
            }
        }


        public void pressCpuTeamTab()
        {
            if (bMyTeamState == true)
            {
                myTeamTab.spriteName = "stat_myteam2";
                myTeamTab.transform.Find("focus").gameObject.SetActive(false);
                cpuTeamTab.spriteName = "stat_otherteam1";
                cpuTeamTab.transform.Find("focus").gameObject.SetActive(true);
                if (bBatterState == true)
                {
                    batterView[1].SetActive(true);
                    batterView[0].SetActive(false);
                }
                else
                {
                    pitcherView[1].SetActive(true);
                    pitcherView[0].SetActive(false);
                }

                bMyTeamState = false;
            }
        }


        public void quit()
        {
            ResultUI.BackFromPopup();
            _active.SetActive(false);
        }

    }
}