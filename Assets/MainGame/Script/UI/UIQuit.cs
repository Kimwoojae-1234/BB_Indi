using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class UIQuit : MonoBehaviour
    {
        public UILabel Label;

        private BallPlayManager manager;

        public void init(BallPlayManager _manager)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            manager = _manager;

            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                Label.text = "게임을 나가시겠습니까?\n\n[ffff00]경기를 나가실 경우\n플레이볼은 복구가 안 되며,\n해당 경기는 취소됩니다.";     
            }
            else
            {
                Label.text = "게임을 나가시겠습니까?\n\n[ffff00]경기를 나가실 경우\n도전권은 복구가 안 되며,\n해당 경기는 취소됩니다.";     
            }

        }



        public void quit()
        {
            // DISABLED_MGRS: Mgrs.ManagerSupervise(false);
            SkillEffectDisplayManager.Destroy();
            Destroy(GameObject.FindWithTag("SIMUL_TAG").gameObject);
            // DISABLED_MGRS: Mgrs.userData.UserLobbyReason = UserData.EReason.OutGame_Lobby;
            // DISABLED_MGRS: Mgrs.SceneLoad.LoadScene(SceneID.Lobby);
        }


        public void resume()
        {
            if (Mode.bSimulationQuickPlay == true)
            {
                //시뮬모드
                manager.simulator.resumeGame();
            }
            else
            {
                //플레이
                Mode.bPauseGame = false;
                manager.pitcher.setResume();
            }

            Destroy(gameObject);
        }
    }
}
