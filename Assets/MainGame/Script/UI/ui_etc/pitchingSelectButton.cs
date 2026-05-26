using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class pitchingSelectButton : MonoBehaviour
    {
        public UISprite back, text;
        //public overallNumber num;
        public UILabel num;
        public UITexture effect;
        public GameObject _light;

        private PitchingArsenal _selectedBall;
        private int _selectedSlot;

        private bool bPushAvail;
        

        public void setInit(CPlayer pitcher, PitchingArsenal index, int slot)
        {
            bPushAvail = true;
            effect.gameObject.SetActive(false);
            _light.gameObject.SetActive(false);
            _selectedBall = index;
            _selectedSlot = slot;// (int)PitchingMechanism.getBallType(index);

            text.spriteName = "pselect_" + (int)(index);

            int ballValue = (pitcher.getBallValue(index)/10);
            //num.Set(ballValue);
            num.text = ballValue.ToString();

            string str = "pselect_ball_stat1";
            if (ballValue >= 100) str = "pselect_ball_stat4";
            else if (ballValue >= 80) str = "pselect_ball_stat3";
            else if (ballValue >= 60) str = "pselect_ball_stat2";
            back.spriteName = str;

            transform.localPosition = new Vector3(400 + (20 * slot), (66 * slot), 0);
            StartCoroutine(setPosition(slot));

        }



        public void pushButton()
        {
            if (bPushAvail == true)
            {
                bPushAvail = false;                
                IngameUI.GetPitchingSelect().SetBallType(_selectedSlot, _selectedBall);
                StartCoroutine(buttonEffect());                
            }
        }


        private IEnumerator buttonEffect()
        {
            _light.gameObject.SetActive(true);            
            yield return new WaitForSeconds(0.2f);
            _light.gameObject.SetActive(false);
            effect.gameObject.SetActive(true);            
            effect.alpha = 0;
            effect.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            TweenAlpha.Begin(effect.gameObject, 0.4f, 1).ResetToBeginning();
            TweenScale.Begin(effect.gameObject, 0.4f, new Vector3(0.87f, 0.87f, 1)).ResetToBeginning();
            yield return new WaitForSeconds(0.4f);
            effect.gameObject.SetActive(false);
        }


        public void setRelease(int selected, int type)
        {
            StartCoroutine(setPosition2(selected, type));
        }

        private IEnumerator setPosition(int type)
        {
            yield return new WaitForSeconds(type * 0.1f);
            TweenPosition.Begin(gameObject, 0.2f, new Vector3((20 * type) - 50, (66 * type), 0));
            yield return new WaitForSeconds(0.2f);
            TweenPosition.Begin(gameObject, 0.1f, new Vector3((20 * type), (66 * type), 0));
        }


        private IEnumerator setPosition2(int selected, int type)
        {
            bPushAvail = false;
            if (_selectedSlot == selected)
            {
                yield return new WaitForSeconds(0.5f + 0.45f);
            }
            else
            {
                yield return new WaitForSeconds(type * 0.05f);
            }
            TweenPosition.Begin(gameObject, 0.1f, new Vector3((20 * type) - 50, (66 * type), 0));
            yield return new WaitForSeconds(0.1f);
            TweenPosition.Begin(gameObject, 0.2f, new Vector3((20 * type) + 500, (66 * type), 0));
            
        }

    }
}