using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class QuickPlayerInfo : MonoBehaviour
    {
        private CPlayer displayedPlayer;
        private bool displayedPitcher;
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
            displayedPlayer = player; displayedPitcher = false;
            QuickSimLabelArt.Apply(batterPos, "order_" + (player.getOrder() + 1), 58, 22);

            RefreshMatchRecords();
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
            displayedPlayer = player; displayedPitcher = true;
            QuickSimLabelArt.Apply(pitcherPos, player.getPitcherPosition() == 0 ? "starter" : player.getPitcherPosition() == 4 ? "closer" : "relief", 58, 22);

            RefreshMatchRecords();
            setSlot(player);
            //pitcherStatmina(player);
        }

        // Refresh mutable match records even when the player reference stays unchanged.
        private void LateUpdate() => RefreshMatchRecords();

        private void RefreshMatchRecords()
        {
            if (displayedPlayer == null) return;
            var p = displayedPlayer;
            if (displayedPitcher)
            {
                int earned = p.getStat(Param.ST_PER), outs = p.getStat(Param.ST_IP);
                pitcherLabel[0].text = outs > 0 ? (earned * 27f / outs).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : "0.00";
                pitcherLabel[1].text = earned.ToString();
                pitcherLabel[2].text = p.getStat(Param.ST_PH).ToString();
                pitcherLabel[3].text = p.getStat(Param.ST_PSO).ToString();
                pitcherStatmina(p);
            }
            else
            {
                int hits = p.getStat(Param.ST_H), atBats = p.getStat(Param.ST_AB);
                batterLabel[0].text = atBats > 0 ? (hits / (float)atBats).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) : "0.000";
                batterLabel[1].text = hits + "/" + atBats;
                batterLabel[2].text = p.getStat(Param.ST_HR).ToString();
                batterLabel[3].text = p.getStat(Param.ST_RBI).ToString();
                batterLabel[4].text = p.getStat(Param.ST_SBS).ToString();
            }
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
namespace BaseBall.BallPlay
{
    internal static class QuickSimLabelArt
    {
        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
        public static void Apply(UIWidget original, string key, int width, int height, float offsetX = 0)
        {
            string path = "Localization/QuickSim/en/" + key;
            if (!cache.TryGetValue(path, out var texture))
            {
                texture = Resources.Load<Texture2D>(path);
                cache[path] = texture;
            }
            if (texture == null) return;
            var child = original.transform.Find("LocalizedLabelArt");
            UITexture graphic;
            if (child == null)
            {
                var obj = new GameObject("LocalizedLabelArt");
                obj.layer = original.gameObject.layer;
                obj.transform.SetParent(original.transform, false);
                graphic = obj.AddComponent<UITexture>();
            }
            else graphic = child.GetComponent<UITexture>();
            graphic.mainTexture = texture;
            graphic.pivot = original.pivot;
            graphic.depth = original.depth;
            graphic.color = original.color;
            graphic.SetDimensions(width, height);
            graphic.transform.localPosition = new Vector3(offsetX, 0, 0);
            original.enabled = false;
        }
    }
}
