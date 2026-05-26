using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class infoCard : MonoBehaviour
    {
        public GameObject origin;
        public UISprite bg;
        public GameObject basicInfo;
        public GameObject skillInfo;
        public GameObject todayInfo;
        public UI_CardSmall playerCard;


        public GameObject batterObj, pitcherObj;
        public GameObject pTodayObj, bTodayObj;

        private GameObject todayStat;

        public UISprite[] bgSpr;
        public GameObject[] light;
        public UISprite[] stamina;

        public SkillSlot[] slot;
        

        private void init(CPlayer player, int team)
        {
            todayInfo.GetComponent<UIWidget>().alpha = 1;
            bg.SetDimensions(302, 328);
                        
            basicInfo.transform.Find("name").gameObject.GetComponent<UILabel>().text = player.getName();
#if _Test_Local
            basicInfo.transform.Find("logo").gameObject.GetComponent<UISprite>().spriteName = "logo_" + team;
#else
            //basicInfo.transform.FindChild("logo").gameObject.GetComponent<UISprite>().spriteName = "logo_" + (int)player.getPlayerData().eTeam;
            Util.SetSpritePixelPerfect(basicInfo.transform.FindChild("logo").gameObject.GetComponent<UISprite>(), "logo_" + (int)player.getPlayerData().eTeam);
            playerCard.SetCardInfo(new CardData(player.getCard()));
#endif
        }

        private int curSkillCount;

        private void initSkill(CPlayer player)
        {
#if _Test_Local
            //
            for (int i = 0; i < 5; i++)
            {
                //SkillSlot slot = skillInfo.transform.Find("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                //slot.SetSkillEmpty(SkillSlot.IconSIze.Small);
            }
#else
            //위치 초기화
            skillInfo.transform.localPosition = new Vector3(0, -244, 0);

            int maxSkillCount = player.getPlayerData().max_skill_cnt;
            int skillCount = 0;
            if (player.getCard().skills != null)
            {
                skillCount = player.getCard().skills.Count;
            }

            curSkillCount = skillCount;

            for (int i = 0; i < 5; i++)
            {
                //SkillSlot slot = skillInfo.transform.FindChild("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                slot[i].transform.localScale = new Vector3(1.15f, 1.15f, 1);
                slot[i].transform.FindChild("effect").gameObject.SetActive(false);
                if (i < maxSkillCount)
                {
                    if (i < skillCount)
                    {
                        WebConnector.CardSkill skill = player.getCard().skills[i];
                        SkillData curSkillData = new SkillData(skill);
                        slot[i].SetSkillSlot(curSkillData, SkillSlot.IconSIze.Small);
                    }
                    else
                    {
                        slot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                    }
                }
                else
                {
                    slot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                    //slot[i].SetSkillEmpty(SkillSlot.IconSIze.Small);
                }
            }
#endif
        }

        private void setStatPitcher(CPlayer player)
        {
#if _Test_Local
            //
#else

#endif
        }

        private void setStatBatter(CPlayer player)
        {
#if _Test_Local
            //
#else

#endif
        }
        
        

        public void initPitcher(CPlayer player, int team, Vector3 startPos)
        {
            bgSpr[0].spriteName = "info_pitcher_bg";
            bgSpr[1].spriteName = "info_pitcher_line";
            bgSpr[2].spriteName = "position_bg_p";

            light[0].SetActive(true);
            light[1].SetActive(false);

            batterObj.SetActive(false);
            bTodayObj.SetActive(false);
            pitcherObj.SetActive(true);
            pTodayObj.SetActive(true);
            todayStat = pTodayObj;
            todayStat.transform.localPosition = new Vector3(0, -303, 0);
            //todayStat.transform.FindChild("Label1").GetComponent<UILabel>().text = "[75ACEAFF]자책 [ffffff]" + player.getStat(Param.ST_PER) + "[75ACEAFF]   피안 [ffffff]" + player.getStat(Param.ST_PH) + "[75ACEAFF]   삼진 [ffffff]" + player.getStat(Param.ST_PSO) + "[-]";
            //setStamina(player.getCurrentStamina());
            setPitcherRecord(player);

            origin.SetActive(false);
            init(player, team);
            initSkill(player);
            setStatPitcher(player);
            UISprite pos = basicInfo.transform.Find("pos").gameObject.GetComponent<UISprite>();
            pos.spriteName = Util.getPitcherposSprite(player);
            pos.MakePixelPerfect();
            
            UILabel overallLabel = basicInfo.transform.Find("overrall").gameObject.GetComponent<UILabel>();
            int overallNum;
#if _Test_Local
            overallNum = Random.Range(50, 150);            
#else
            overallNum = Utils.TeamPowerUtils.calCardPower(player.getCard());            
#endif
            overallLabel.bitmapFont = Util.GetOverallFont(overallNum);
            overallLabel.text = overallNum.ToString();

            transform.localPosition = startPos;
        }

        /// <summary>
        /// 투수 기록
        /// </summary>
        /// <param name="player"></param>
        private void setPitcherRecord(CPlayer player)
        {
            todayStat.transform.Find("Label1").GetComponent<UILabel>().text = "[75ACEAFF]자책 [ffffff]" + player.getStat(Param.ST_PER) + "[75ACEAFF]   피안 [ffffff]" + player.getStat(Param.ST_PH) + "[75ACEAFF]   삼진 [ffffff]" + player.getStat(Param.ST_PSO) + "[-]";
            setStamina(player.getCurrentStamina());
        }

        public void initBatter(CPlayer player, int team, int count, Vector3 startPos)
        {
            bgSpr[0].spriteName = "info_batter_bg";
            bgSpr[1].spriteName = "info_batter_line";
            bgSpr[2].spriteName = "position_bg_b";

            light[1].SetActive(true);
            light[0].SetActive(false);

            batterObj.SetActive(true);
            bTodayObj.SetActive(true);
            pitcherObj.SetActive(false);
            pTodayObj.SetActive(false);
            todayStat = bTodayObj;
            todayStat.transform.localPosition = new Vector3(0, -303, 0);
            todayStat.transform.Find("Label1").GetComponent<UILabel>().text = "[75ACEAFF]안타 [FFFFFF]" + player.getStat(Param.ST_H) + "/" + (player.getStat(Param.ST_AB) - 1) + "   [75ACEAFF]홈런 [FFFFFF]" + player.getStat(Param.ST_HR) + "   [75ACEAFF]타점 [FFFFFF]" + player.getStat(Param.ST_RBI) + "   [75ACEAFF]도루 [FFFFFF]" + player.getStat(Param.ST_SBS) + "[-]";

            origin.SetActive(false);
            init(player, team);
            initSkill(player);
            setStatBatter(player);
            UISprite pos = basicInfo.transform.Find("pos").gameObject.GetComponent<UISprite>();
            pos.spriteName = "info_" + count;
            pos.MakePixelPerfect();

            UILabel overallLabel = basicInfo.transform.Find("overrall").gameObject.GetComponent<UILabel>();
            int overallNum;
#if _Test_Local
            overallNum = Random.Range(50, 150);            
#else
            overallNum = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities);
#endif
            overallLabel.bitmapFont = Util.GetOverallFont(overallNum);
            overallLabel.text = overallNum.ToString();

            transform.localPosition = startPos;
        }





        public void start()//Vector3 startPos)
        {
            StartCoroutine(startAnim());
        }

        public void preSet(CPlayer player)
        {
            StartCoroutine(presetAnim(player));
        }


        private IEnumerator startAnim()
        {
            yield return new WaitForSeconds(0.1f);
            origin.SetActive(true);            
            UITweener tween1 = GetComponent<TweenPosition>();
            tween1.ResetToBeginning();            
            tween1.PlayForward();
            

            yield return new WaitForSeconds(1.0f);
            TweenAlpha.Begin(todayInfo.gameObject, 0.2f, 0);
            TweenPosition.Begin(skillInfo.gameObject, 0.2f, new Vector3(0,-37,0));

            float len = 328;
            float statPosY = -303;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                len -= 20;
                statPosY += 20;
                if (len < 113)
                {
                    len = 113;
                    statPosY = -89;
                    bg.SetDimensions(302, 113);
                    todayStat.transform.localPosition = new Vector3(0, statPosY, 0);
                    break;
                }
                if (statPosY > -89) statPosY = -89;
                todayStat.transform.localPosition = new Vector3(0, statPosY, 0);
                bg.SetDimensions(302, (int)len);                
            }
        }

        private IEnumerator presetAnim(CPlayer player)
        {
            setPitcherRecord(player);
            yield return new WaitForSeconds(0.1f);
            todayInfo.GetComponent<UIWidget>().alpha = 0;
            skillInfo.transform.localPosition = new Vector3(0, -37, 0);
            bg.SetDimensions(302, 113);
            origin.SetActive(true);            
            UITweener tween1 = GetComponent<TweenPosition>();
            tween1.ResetToBeginning();            
            tween1.PlayForward();
            
        }


        private void setStamina(float curStamina)
        {
            Color curColor1;
            Color curColor2 = new Color(0.353f, 0.353f, 0.353f);
            int max = (int)Mathf.Clamp(curStamina / 100.0f * 8,1,7);

            if (max > 5)
            {
                curColor1 = new Color(0.34f, 0.94f, 0.11f);
            }
            else if (max > 3)
            {
                curColor1 = new Color(0.11f, 0.63f, 0.80f);
            }
            else if (max > 1)
            {
                curColor1 = new Color(0.80f, 0.60f, 0.11f);
            }
            else
            {
                curColor1 = new Color(1, 0, 0);
            }


            for (int i = 0; i < 7; i++)
            {
                if (i < max)
                {
                    stamina[i].color = curColor1;
                }
                else
                {
                    stamina[i].color = curColor2;
                }
                
            }

        }


        /// <summary>
        /// 스킬 버프 UI처리
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bMyUI"></param>
        public void setBuffUI(SkillID id, bool bMyUI)
        {
            SkillBuffType type = SkillParm.GetBuffType(id);
            if (type != SkillBuffType.None)
            {
                if (gameObject.activeSelf)
                {
                    quickBuffUI obj = Util.Load("MainGame/prefabs/QuickUI/actionBuffPrefab", todayStat.transform, Vector3.zero).GetComponent<quickBuffUI>();
                    obj.InitAction(type, bMyUI);
                }
            }
        }

        /// <summary>
        /// 스킬 무효처리
        /// </summary>
        /// <param name="bMyUI"></param>
        public void setSkillInvalidityUI(bool bMyUI)
        {
            quickBuffUI obj = Util.Load("MainGame/prefabs/QuickUI/actionBuffPrefab", todayStat.transform, Vector3.zero).GetComponent<quickBuffUI>();
            obj.InitAction(SkillBuffType.SkillInvalidity, bMyUI);
        }


        public void activateSkill(CPlayer player, int id)
        {
#if _Test_Local
            //
#else
            if (player.getCard().skills != null)
            {
                curSkillCount = player.getCard().skills.Count;
                for (int i = 0; i < curSkillCount; i++)
                {
                    //SkillSlot slot = skillInfo.transform.FindChild("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                    WebConnector.CardSkill skill = player.getCard().skills[i];
                    if (id == skill.skillId)
                    {
                        slot[i].transform.FindChild("effect").gameObject.SetActive(true);
                    }
                }
            }
#endif
        }


        public void updatePitcher(CPlayer player)
        {
#if _Test_Local
            //
#else
            if (player.getCard().skills != null)
            {
                curSkillCount = player.getCard().skills.Count;
                for (int i = 0; i < curSkillCount; i++)
                {
                    //SkillSlot slot = skillInfo.transform.FindChild("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                    WebConnector.CardSkill skill = player.getCard().skills[i];

                    //필승의지와 닥터K효과 여기서 체크해줌 (아직 안됨)

                    GameObject effect = slot[i].transform.FindChild("effect").gameObject;
                    if (player.checkSkillInvoke(skill.skillId) == true)
                    {
                        effect.SetActive(true);
                    }
                    else if(player.checkPiledEffect(SkillIndex.WinSpirit, skill.skillId) == true)
                    {
                        //필승의지 지속 여부
                        effect.SetActive(true);
                    }
                    else if (player.checkPiledEffect(SkillIndex.DoctorK, skill.skillId) == true)
                    {
                        //닥터 K 지속여부
                        effect.SetActive(true);
                    }
                    else if (player.skillAvailable_id(SkillIndex.IronArm, skill.skillId) == true)
                    {
                        //강철어깨 소유여부
                        effect.SetActive(true);
                    }
                    else
                    {
                        effect.SetActive(false);
                    }
                }
            }
#endif
        }

    }
}
