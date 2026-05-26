using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class UISkillShow : MonoBehaviour
    {

        private static UISkillShow Instance_;
        public GameObject _active;

        int step;

        public UISprite[] teamText;
        public UISprite[] teamBG;
        public UISprite[] logo;
        public UILabel[] playerName;
        public UILabel[] skillName;

        public UISprite[] skillIcon;

        void Awake()
        {
            Instance_ = this;
            skillCount = 0;
        }


        void OnDestroy()
        {            
            Instance_ = null;
        }


        public static void SetActive(int skillID, string _name, bool bMySkill)
        {
            Instance_.setActive(skillID, _name, bMySkill);
        }

        /*
        public static void SetDeActive()
        {
            Instance_.setDeActive();
        }*/


        int skillCount;
        private void setActive(int skillID, string _name, bool bMySkill)
        {
            _active.SetActive(true);

            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            string _skillName = SimulParm.GetSkillInfo(skillID).skillName;
            if (skillCount == 0)
            {
                setBg(0, skillID,  _skillName,  _name,  bMySkill);
                anim.Play(Animator.StringToHash("skilldirectionAnim"));
                step = 0;
                curTime = 0;
                skillCount++;
                StartCoroutine(setDeActive());
            }
            else
            {
                curTime = 0;
                if (step > 0)
                {
                    change();
                }
                setBg(1, skillID, _skillName, _name, bMySkill);

                anim.Play(Animator.StringToHash("skilldirectionAnim2"));
                step++;
            }
        }

        private void setBg(int index, int skillID, string _skillName, string _name, bool bMySkill)
        {
            int ddd = Random.Range(101, 112);
            skillIcon[index].spriteName = ddd.ToString();
            teamText[index].spriteName = bMySkill ? "skillmyteam" : "skillenemyteam";
            teamBG[index].spriteName = bMySkill ? "skilmyteambg" : "skillenemyteambg";
            Util.SetSpritePixelPerfect(logo[index], "logo_" + (bMySkill ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex));//logo[index].spriteName = "logo_" + (bMySkill ? SimulPlayerManager.myTeamIndex : SimulPlayerManager.cpuTeamIndex);
            playerName[index].text = _name;
            skillName[index].text = _skillName;
            teamText[index].MakePixelPerfect();
            logo[index].MakePixelPerfect();
            
        }

        private void change()
        {
            skillIcon[0].spriteName = skillIcon[1].spriteName;
            teamText[0].spriteName = teamText[1].spriteName;
            teamBG[0].spriteName = teamBG[1].spriteName;
            logo[0].spriteName = logo[1].spriteName;
            playerName[0].text = playerName[1].text;
            skillName[0].text = skillName[1].text;
            teamText[0].MakePixelPerfect();
            logo[0].MakePixelPerfect();
        }

        float curTime = 0;

        private IEnumerator setDeActive()
        {
            while (curTime < 2.0f)
            {
                curTime += 0.2f;
                yield return new WaitForSeconds(0.2f);
            }

            Animator anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            anim.Rebind();
            anim.Play(Animator.StringToHash("skilldirectionAnim3"));

            skillCount = 0;

            yield return new WaitForSeconds(0.5f);
            
            _active.SetActive(false);
        }


    }
}
