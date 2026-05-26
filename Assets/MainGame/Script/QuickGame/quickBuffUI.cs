using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class quickBuffUI : MonoBehaviour
    {
        public GameObject _active;
        public UILabel label;
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

            if (type == SkillBuffType.BatterDown) label.text = "타격 능력 하락";
            else if (type == SkillBuffType.BatterUP) label.text = "타격 능력 상승";
            else if (type == SkillBuffType.BatterSpecial)
            {
                label.text = "타자 특수 능력";
                label.transform.localPosition = Vector3.zero;
            }
            else if (type == SkillBuffType.DoctorK) label.text = "닥터 K";
            else if (type == SkillBuffType.PitcherDown) label.text = "투수 구질 하락";
            else if (type == SkillBuffType.PitcherUP) label.text = "투수 구질 상승";
            else if (type == SkillBuffType.PitcherSpecial)
            {
                label.text = "투수 특수 능력";
                label.transform.localPosition = Vector3.zero;
            }
            else if (type == SkillBuffType.SkillInvalidity)
            {
                label.text = "스킬 무효화";
                label.transform.localPosition = Vector3.zero;
            }
            _active.GetComponent<UISprite>().spriteName = bMyUI ? "buff_team1" : "buff_team2";

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
            TweenPosition.Begin(gameObject, 0.3f, new Vector3(0, (bDown?-30:30), 0));
            TweenAlpha.Begin(gameObject, 0.3f, 0);
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
            TweenAlpha.Begin(gameObject, 0.3f, 0);
            yield return new WaitForSeconds(0.4f);
            Destroy(gameObject);
        }

    }
}