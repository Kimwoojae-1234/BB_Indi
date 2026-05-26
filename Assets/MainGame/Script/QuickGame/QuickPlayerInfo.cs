using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class QuickPlayerInfo : MonoBehaviour
    {
        public GameObject batter, pitcher;
        public UILabel[] batterLabel;
        public UILabel[] pitcherLabel;

        public Transform playerCard;
        //public SkillSlot[] skillSlot;

        public UISprite batterPos;
        public UISprite pitcherPos;

        public UISprite staminaGauge;

        public GameObject batterChange, pitcherChange;

        public GameObject light;
                

        public void setBatter(CPlayer player)
        {
            playerCard.transform.eulerAngles = Vector3.zero;
            batter.SetActive(true);
            pitcher.SetActive(false);

            batterPos.spriteName = "info_" + (player.getOrder() + 1);
            batterPos.MakePixelPerfect();

            int curHitNum = player.getStat(Param.ST_H);
            int curABNum = player.getStat(Param.ST_AB);            
            batterLabel[1].text = curHitNum + "/" + curABNum;  //안타
            batterLabel[2].text = player.getStat(Param.ST_HR).ToString();   //홈런
            batterLabel[3].text = player.getStat(Param.ST_RBI).ToString();   //타점
            batterLabel[4].text = player.getStat(Param.ST_SBS).ToString();   //도루

#if _Test_Local
            batterLabel[0].text = "0.333";
#else
            batterLabel[0].text = Util.GetCurAvg(player.getBatterRecord(), curHitNum, curABNum);
            playerCard.SetCardInfo(new CardData(player.getCard()));
#endif
            setSlot(player);

            if (transform.parent.gameObject.activeSelf)
            {
                StartCoroutine(setLight());
            }

        }

        public void setPitcher(CPlayer player)
        {
            if (player == null) return;
            //Debug.Log("pppppppp SetPitcher");
            playerCard.transform.eulerAngles = Vector3.zero;
            batter.SetActive(false);
            pitcher.SetActive(true);


            pitcherPos.spriteName = Util.getPitcherposSprite(player);
            pitcherPos.MakePixelPerfect();

            //pitcherLabel[0].text = Util.RandomErr();   //방어
            int per = player.getStat(Param.ST_PR);
            int poc = player.getStat(Param.ST_IP);
            pitcherLabel[1].text = per.ToString();  //자책
            pitcherLabel[2].text = player.getStat(Param.ST_PH).ToString();   //피안
            pitcherLabel[3].text = player.getStat(Param.ST_PSO).ToString();   //삼진

#if _Test_Local
            pitcherLabel[0].text = "1.57";

#else
            pitcherLabel[0].text = Util.GetCurErr(player.getPitcherRecord(), per, poc);
            playerCard.SetCardInfo(new CardData(player.getCard()));
#endif
            setSlot(player);
            //pitcherStatmina(player);
        }

        private readonly int maxGauge = 79;
        public void pitcherStatmina(CPlayer player)
        {
            int cur = (player.getCurrentStamina() * maxGauge) / 100;
            staminaGauge.SetDimensions(cur, 12);
        }



        private void setSlot(CPlayer player)
        {
#if _Test_Local
            //
            for (int i = 0; i < 5; i++)
            {
                //skillSlot[i].SetSkillEmpty(SkillSlot.IconSIze.Small);
            }
#else
            int maxSkillCount = player.getPlayerData().max_skill_cnt;
            int skillCount = 0;
            if (player.getCard().skills != null)
            {
                skillCount = player.getCard().skills.Count;
            }

            curSkillCount = skillCount;

            for (int i = 0; i < 5; i++)
            {
                //skillSlot[i].transform.localScale = new Vector3(1.15f, 1.15f, 1);
                if (i < maxSkillCount)
                {
                    if (i < skillCount)
                    {
                        SkillData curSkillData = new SkillData(player.getCard().skills[i]);
                        skillSlot[i].SetSkillSlot(curSkillData, SkillSlot.IconSIze.Small);
                    }
                    else
                    {
                        skillSlot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                    }
                }
                else
                {
                    //skillSlot[i].SetSkillEmpty(SkillSlot.IconSIze.Small);
                    skillSlot[i].SetLockSlot(SkillSlot.IconSIze.Small);
                }
            }
#endif
        }


        int curSkillCount = 0;
        public void activateSkill(CPlayer player, int id)
        {
#if _Test_Local

#else
            if (player.getCard().skills != null)
            {
                curSkillCount = player.getCard().skills.Count;
                for (int i = 0; i < curSkillCount; i++)
                {
                    //SkillSlot slot = skillInfo.transform.FindChild("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                    WebConnector.CardSkill skill = player.getCard().skills[i];
                    GameObject effect = skillSlot[i].transform.FindChild("effect").gameObject;
                    if (id == skill.skillId)
                    {
                        effect.gameObject.SetActive(true);
                    }
                }
            }
#endif
        }

        public void initSkillEffect()
        {
            for (int i = 0; i < 5; i++)
            {
                //GameObject effect = skillSlot[i].transform.Find("effect").gameObject;
                //effect.gameObject.SetActive(false);
            }
        }


        public void updatePitcher(CPlayer player)
        {
#if _Test_Local

#else
            if (player.getCard().skills != null)
            {
                curSkillCount = player.getCard().skills.Count;
                for (int i = 0; i < curSkillCount; i++)
                {
                    //SkillSlot slot = skillInfo.transform.FindChild("SkillSlot" + (i + 1)).gameObject.GetComponent<SkillSlot>();
                    WebConnector.CardSkill skill = player.getCard().skills[i];
                                        
                    GameObject effect = skillSlot[i].transform.FindChild("effect").gameObject;
                    if (player.checkSkillInvoke(skill.skillId) == true)
                    {
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


        public void setAnim(bool bLeft)
        {
            if (bLeft == true)
            {
                //165
                playerCard.transform.localPosition = new Vector3(165, -70, 0);
                TweenPosition.Begin(playerCard.gameObject, 0.15f, new Vector3(226, -70, 0));
            }
            else
            {
                playerCard.transform.localPosition = new Vector3(-165, -70, 0);
                TweenPosition.Begin(playerCard.gameObject, 0.15f, new Vector3(-226, -70, 0));
            }
        }

        public void initPos(bool bLeft)
        {
            if (bLeft == true)
            {
                //165
                playerCard.transform.localPosition = new Vector3(226, -70, 0);
            }
            else
            {
                playerCard.transform.localPosition = new Vector3(-226, -70, 0);
            }
        }

        public void SetLight(float delay)
        {
            StartCoroutine(setLight(delay));
        }

        private IEnumerator setLight(float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            Util.SetTween(light);
            yield return new WaitForSeconds(0.28f);
            light.SetActive(false);
        }


        public void ChangeEvent(bool bBatter)
        {
            StartCoroutine(changeEvent(bBatter));
        }

        private IEnumerator changeEvent(bool bBatter)
        {
            Util.SetTween(bBatter ? batterChange : pitcherChange);
            yield return new WaitForSeconds(0.25f);
            TweenRotation rot = playerCard.GetComponent<TweenRotation>();
            rot.enabled = true;
            yield return new WaitForSeconds(0.25f);
            rot.enabled = false;
            playerCard.transform.eulerAngles = Vector3.zero;
            yield return new WaitForSeconds(0.25f);
            if (bBatter) batterChange.SetActive(false);
            else pitcherChange.SetActive(false);
            SetLight(0);
        }

    }
}