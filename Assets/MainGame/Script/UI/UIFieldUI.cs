using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class UIFieldUI : MonoBehaviour
    {
        public GameObject _active;

        public UISprite count;
        public GameObject fielderInfo;
        public GameObject [] outCount;
        public SpriteRenderer changeView;


        //필더 정보
        public UISprite logo;
        public UISprite position;
        public UILabel playerName;
        public UILabel overall;
        public SkillSlot[] slot;
        public UISprite infoBG1, infoBG2, infoBG3;

        


        //미니맵
        public GameObject minimap;
        //필드 라인 이펙트
        public tk2dSpriteAnimator fieldLine;


        /// <summary>
        /// 해당 UI액티브 여부
        /// </summary>
        /// <param name="bActive"></param>
        public void SetActive(bool bActive)
        {
            _active.SetActive(bActive);
            if (bActive == true)
            {
                fielderInfo.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 필드에 표시되는 아웃카운트 업데이트
        /// </summary>
        /// <param name="countNum"></param>
        public void SetCountUpdate(int countNum)
        {
            outCount[0].SetActive(countNum >= 1 ? true : false);
            outCount[1].SetActive(countNum >= 2 ? true : false);
        }


        /// <summary>
        /// 이름
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="player"></param>
        public void SetName(bool bActive, CPlayer player, int teamIndex, bool bMyTurn)
        {
            if (bActive == true)
            {             
                fielderInfo.transform.localPosition = new Vector3(bMyTurn == true ? 472:-472, 299, 0);

                int pos = player.getCurPos();
                
                //이름
                playerName.text = player.getName();
#if _Test_Local
                //로고
                logo.spriteName = "logo_" + teamIndex;

                if (pos == CPlayer._PITCHER)
                {
                    //배경
                    infoBG1.spriteName = "info_pitcher_bg";
                    infoBG2.spriteName = "info_pitcher_line";
                    infoBG3.spriteName = "position_bg_p";

                    //오버롤
                    overall.text = Random.Range(50,90).ToString();
                    //포지션
                    position.spriteName = Util.getPitcherposSprite(player);
                }
                else
                {
                    //배경
                    infoBG1.spriteName = "info_batter_bg";
                    infoBG2.spriteName = "info_batter_line";
                    infoBG3.spriteName = "position_bg_b";

                    //오버롤
                    overall.text = Random.Range(50, 90).ToString();
                    //포지션
                    position.spriteName = "position_" + (pos + 1).ToString();
                }
                position.MakePixelPerfect();
                //스킬                
                setSkillSlot(player, 0, 0);
#else
                //로고
                Util.SetSpritePixelPerfect(logo, "logo_" + (int)player.getPlayerData().eTeam);//logo.spriteName = "logo_" + (int)player.getPlayerData().eTeam;

                if (pos == CPlayer._PITCHER)
                {
                    //배경
                    infoBG1.spriteName = "info_pitcher_bg";
                    infoBG2.spriteName = "info_pitcher_line";
                    infoBG3.spriteName = "position_bg_p";

                    //오버롤
                    overall.text = Utils.TeamPowerUtils.calCardPower(player.getCard()).ToString();
                    //포지션
                    position.spriteName = Util.getPitcherposSprite(player); 
                }
                else
                {
                    //배경
                    infoBG1.spriteName = "info_batter_bg";
                    infoBG2.spriteName = "info_batter_line";
                    infoBG3.spriteName = "position_bg_b";

                    //오버롤
                    overall.text = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities).ToString();
                    //포지션
                    position.spriteName = "position_" + (pos + 1).ToString();
                }
                position.MakePixelPerfect();

                //스킬
                int maxSkillCount = player.getPlayerData().max_skill_cnt;
                int skillCount = 0;
                if (player.getCard().skills != null)
                {
                    skillCount = player.getCard().skills.Count;
                }
                setSkillSlot(player, maxSkillCount, skillCount);
#endif                
                fielderInfo.gameObject.SetActive(true);
            }
            else
            {
                fielderInfo.gameObject.SetActive(false);
            }            
        }

        private void setSkillSlot(CPlayer player, int maxCount, int skillCount)
        {
#if _Test_Local
            for (int i = 0; i < 5; i++)
            {
                //slot[i].SetSkillEmpty(SkillSlot.IconSIze.Small);
            }
#else
            for (int i = 0; i < 5; i++)
            {
                slot[i].transform.localScale = new Vector3(1.15f, 1.15f, 1);
                if (i < maxCount)
                {
                    if (i < skillCount)
                    {
                        SkillData curSkillData = new SkillData(player.getCard().skills[i]);
                        slot[i].SetSkillSlot(curSkillData, SkillSlot.IconSIze.Small);
                    }
                    else
                    {
                        slot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                    }
                }
                else
                {
                    //slot[i].SetSkillEmpty(SkillSlot.IconSIze.Small);
                    slot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                }
            }
#endif
        }

       
        //미니맵 주자 오브젝트
        private GameObject hitterRunnerObj;

        /// <summary>
        /// 미니맵 주자 생성
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="runner"></param>
        /// <param name="team"></param>
        public void MakeMinimapRunner(BallPlayManager manager, Runner runner, int team)
        {
            hitterRunnerObj = Util.Load("MainGame/prefabs/ControlUI/miniRunner2", minimap.transform, Vector3.zero);
            hitterRunnerObj.GetComponent<miniRunner>().set(manager, runner, team);

        }

        //파괴
        public void DestroyHitterRunner()
        {
            Destroy(hitterRunnerObj);
        }

        //모두 파괴
        public void DestroyAllMinimapRunner()
        {
            minimap.transform.DestroyChildren();
        }



        /// <summary>
        /// 시점변환
        /// </summary>
        public void SetChangeView(int cameraState)
        {
            changeView.gameObject.SetActive(true);
            Camera curCamera = null;
            if (cameraState == BallPlayManager._FIELDVIEW)
            {
                curCamera = CameraManager.GetFieldCamera();
            }
            else if (cameraState == BallPlayManager._BATTINGVIEW)
            {
                curCamera = CameraManager.GetInstance()._camera.GetComponent<Camera>();
            }
            else if (cameraState == BallPlayManager._BATTERCAMERA)
            {
                curCamera = CameraManager.GetInstance().batterCamera.GetComponent<Camera>();
            }
            changeView.color = new Color(1, 1, 1, 1);
            changeView.sprite = Util.MakeCaptureSprite(curCamera);
            StartCoroutine(changeViewDelay());
        }

        private IEnumerator changeViewDelay()
        {
            TweenAlpha.Begin(changeView.gameObject, 0.4f, 0);
            yield return new WaitForSeconds(0.5f);
            changeView.sprite = null;
            changeView.gameObject.SetActive(false);
        }


        /// <summary>
        /// 필드 라인 이펙트 설정
        /// </summary>
        /// <param name="bActive"></param>
        public void SetLineEffect(bool bActive)
        {
            fieldLine.gameObject.SetActive(bActive);            
        }
    }
}