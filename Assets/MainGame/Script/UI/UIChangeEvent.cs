using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay
{
    public class UIChangeEvent : MonoBehaviour
    {
        public GameObject _active;

        public UITexture logoTexture;
        public UILabel teamLabel;
        public UI_CardSmall outPlayer, inPlayer;
        public GameObject arrow;

        public UISprite bg;

        private BallPlayManager manager;

        public void InitPlayerChangeUI(bool bMyTeam, BallPlayManager _manager, CPlayer outPlayerCard, CPlayer inPlayerCard, UIPlayerChange.PlayerChangeType changeType, int index)
        {
            manager = _manager;

            //배경세팅
            manager.setChangeEvent();

#if _Test_Local

#else
            //UI세팅
            int teamIndex = bMyTeam ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex;
            // DISABLED_MGRS: logoTexture.mainTexture = Mgrs.DataLoad.LoadTexture(string.Format("{0}/{1}", "Logo", UI_Helper.ConvertTeamCodeBig((UserData.ETeamCode)(teamIndex))));
            teamLabel.text = bMyTeam ? SimulPlayerManager.strMyTeam : SimulPlayerManager.strCPUTeam;
            outPlayer.SetCardInfo(new CardData(outPlayerCard.getCard()));
            inPlayer.SetCardInfo(new CardData(inPlayerCard.getCard()));
#endif

            changePlayer(bMyTeam, outPlayerCard, inPlayerCard, changeType, index);

            gameObject.GetComponent<UIPanel>().alpha = 1;
            bg.color = new Color(1, 1, 1, 0);
            _active.SetActive(true);            
            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("playerchange"));

            TweenAlpha.Begin(bg.gameObject, 1.0f, 1);

            StartCoroutine(endEvent(changeType));
        }


        private IEnumerator endEvent(UIPlayerChange.PlayerChangeType changeType)
        {
            float curTime = 0;
            int count = 0;
            while (curTime < 4.0f)
            {
                curTime += 0.2f;
                count = (++count) % 3;
                arrow.transform.localPosition = new Vector3(-15 + count * 15, -108, 0);
                yield return new WaitForSeconds(0.2f);
            }

            TweenAlpha.Begin(gameObject, 0.4f, 0);
            yield return new WaitForSeconds(0.4f);
            manager.returnFromChangeEvent(changeType);
            _active.SetActive(false);
        }

        /// <summary>
        /// 선수교체 실행
        /// </summary>
        /// <param name="bMyTeam"></param>
        /// <param name="outPlayerCard"></param>
        /// <param name="inPlayerCard"></param>
        /// <param name="changeType"></param>
        private void changePlayer(bool bMyTeam, CPlayer outPlayerCard, CPlayer inPlayerCard, UIPlayerChange.PlayerChangeType changeType, int index)
        {
            int team = bMyTeam ? 0 : 1;
            if (changeType == UIPlayerChange.PlayerChangeType.PitcherChange)
            {
                //투수교체
                if (bMyTeam == true || Mode.bPvpMode == true)
                {
                    //실제선수 교체 -> 직접교체시만 활성화
                    int inPitcherIndex = inPlayerCard.originLineup;
                    int outPitcherIndex = outPlayerCard.originLineup;
                    manager.pitcher.setManualPitcherChange(inPitcherIndex, outPitcherIndex);
                }
                CPlayer newPitcher = inPlayerCard;
                //manager.pitcher.initPitcher(newPitcher, team, false);
                Fielder curPitcher = manager.field.fielder[CPlayer._PITCHER];
                curPitcher.initParameter(newPitcher, CPlayer._PITCHER);
                curPitcher.loadFielder(manager.bTopInning);
                manager.batter.bNewBatter = true;
                manager.batter.bNewBatterInfo = true;
            }
            else
            {
                if (bMyTeam == true || Mode.bPvpMode == true)
                {
                    //실제선수 교체 -> 직접교체시만 활성화
                    int inPlayer = inPlayerCard.getOrder();
                    int outPlayer = outPlayerCard.getOrder();
                    SimulPlayerManager.SetFielderChange(team, inPlayer, outPlayer, 0);
                }

                if (changeType == UIPlayerChange.PlayerChangeType.BatterChange)
                {
                    //타자교체 해줌
                    CPlayer pinchHitter = inPlayerCard;
                    manager.batter.initBatter(pinchHitter, 0, true);
                    manager.batter.bNewBatterInfo = true;
                }
                else if (changeType == UIPlayerChange.PlayerChangeType.RunnerChange)
                {
                    int baseIndex = index;
                    //주자교체 해줌        
                    Runner runner = manager.field.run.getRunner(baseIndex);
                    if (runner != null)
                    {
                        //우선 이전 주자 삭제
                        int arrayIndex = runner.arrayIndex;
                        manager.field.run.runnerActive[arrayIndex] = false;
                        Destroy(runner.gameObject);
                    }
                    //새로 생성                    
                    CPlayer pinchRunner = inPlayerCard;
                    manager.field.run.makeChanceRunner(pinchRunner, baseIndex).setInitPos(baseIndex, false, true);
                }
                else if (changeType == UIPlayerChange.PlayerChangeType.FielderChange)
                {
                    //현재 포지션
                    int currentPos = index;
                    //야수교체 해줌
                    CPlayer fielder = inPlayerCard;
                    if (fielder.getPosition() != currentPos) fielder.setMissMatch(true); //미스매치 세팅
                    Fielder curFielder = manager.field.fielder[currentPos];
                    curFielder.initParameter(fielder, currentPos);
                    curFielder.loadFielder(manager.bTopInning);
                }
            }
        }
    }
}
