using BaseBall.BallPlay.UGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class UIQuit : MonoBehaviour
    {
        public GameUIElement Label;

        private BallPlayManager manager;

        public void init(BallPlayManager _manager)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            manager = _manager;

            if (Mode.gameMode == Mode.GamePlayMode.Season)
            {
                L10n.SetText(Label, "UI.Message.LeaveTheGameYourPlayBallWillNotBeRefundedAndThis");
            }
            else
            {
                L10n.SetText(Label, "UI.Message.LeaveTheGameYourChallengeTicketWillNotBeRefundedAndThis");
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
