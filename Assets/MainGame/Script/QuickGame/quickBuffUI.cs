using BaseBall.BallPlay.UGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class quickBuffUI : MonoBehaviour
    {
        public GameObject _active;
        public GameUIElement label;
        public GameObject up, down;
        public GameObject light;

        

        void OnDisable()
        {
            ////Debug.Log("===================>>이게 호출될 경우");
            StopAllCoroutines();
            Destroy(gameObject);
        }


        private void initSetting(SkillBuffType type, bool bMyUI)
        {
            transform.localScale = Vector3.one;

            if (type == SkillBuffType.BatterDown) L10n.SetText(label, "UI.Label.BattingAbilityDown");
            else if (type == SkillBuffType.BatterUP) L10n.SetText(label, "UI.Label.BattingAbilityUp");
            else if (type == SkillBuffType.BatterSpecial)
            {
                L10n.SetText(label, "UI.Label.BatterSpecialAbility");
                label.transform.localPosition = Vector3.zero;
            }
            else if (type == SkillBuffType.DoctorK) L10n.SetText(label, "UI.Label.DoctorK");
            else if (type == SkillBuffType.PitcherDown) L10n.SetText(label, "UI.Label.PitchQualityDown");
            else if (type == SkillBuffType.PitcherUP) L10n.SetText(label, "UI.Label.PitchQualityUp");
            else if (type == SkillBuffType.PitcherSpecial)
            {
                L10n.SetText(label, "UI.Label.PitcherSpecialAbility");
                label.transform.localPosition = Vector3.zero;
            }
            else if (type == SkillBuffType.SkillInvalidity)
            {
                L10n.SetText(label, "UI.Label.SkillNullified");
                label.transform.localPosition = Vector3.zero;
            }
            _active.GetComponent<GameUIElement>().spriteName = bMyUI ? "buff_team1" : "buff_team2";

        }


        /// <summary>
        /// 시뮬레이터에서 호출
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bMyUI"></param>
        /// <param name="count"></param>
        public void Init(SkillBuffType type, bool bMyUI, int count)
        {
            initSetting(type, bMyUI);
            if (gameObject.activeSelf)
            {
                StartCoroutine(setEffect(type, count));
            }
        }


        private IEnumerator setEffect(SkillBuffType type, int count)
        {
            bool bDown = false;
            yield return new WaitForSeconds(1.5f + count*0.5f);
            _active.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            light.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            if (type == SkillBuffType.BatterDown || type == SkillBuffType.PitcherDown)
            {
                down.SetActive(true);
                bDown = true;
            }
            else if (type == SkillBuffType.BatterUP || type == SkillBuffType.PitcherUP)
            {
                up.SetActive(true);
            }

            yield return new WaitForSeconds(0.85f);
            GameUITweenPosition.Begin(gameObject, 0.3f, new Vector3(0, (bDown?-30:30), 0));
            GameUITweenAlpha.Begin(gameObject, 0.3f, 0);
            yield return new WaitForSeconds(0.4f);
            Destroy(gameObject);
        }

        /// <summary>
        /// 액션에서 호출
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bMyUI"></param>
        public void InitAction(SkillBuffType type, bool bMyUI)
        {
            initSetting(type, bMyUI);
            StartCoroutine(setEffect2(type));
        }


        private IEnumerator setEffect2(SkillBuffType type)
        {
            yield return new WaitForSeconds(0.3f);
            _active.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            light.SetActive(true);
            yield return new WaitForSeconds(0.15f);
            if (type == SkillBuffType.BatterDown || type == SkillBuffType.PitcherDown)
            {
                down.SetActive(true);
            }
            else if (type == SkillBuffType.BatterUP || type == SkillBuffType.PitcherUP)
            {
                up.SetActive(true);
            }
            yield return new WaitForSeconds(1.5f);
            GameUITweenAlpha.Begin(gameObject, 0.3f, 0);
            yield return new WaitForSeconds(0.4f);
            Destroy(gameObject);
        }

    }
}