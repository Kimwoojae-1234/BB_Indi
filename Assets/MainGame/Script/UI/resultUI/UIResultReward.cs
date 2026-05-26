using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace BaseBall.BallPlay
{
    public class UIResultReward : MonoBehaviour
    {
        private readonly int GaugeMax = 312; 

        public GameObject _active;

        public GameObject gaugeObj;
        public UILabel gold;
        
        public Transform grid;
        public GameObject noReward;

        public GameObject Light;

        private readonly int Max_Gauge = 312;

        public void init()
        {
            
        }


        public void deActive()
        {
            noReward.gameObject.SetActive(false);
            //deactive();
            TweenAlpha.Begin(gameObject, 0.3f, 0);
            Invoke("deactive", 0.32f);
        }

        private void deactive()
        {
            _active.SetActive(false);
        }


        /// <summary>
        /// 보상 아이템 세팅
        /// </summary>
        /// <param name="value"></param>
        int itemCount;
        private void setItem(KeyValuePair<int, int> value)
        {
            
        }

        private IEnumerator setGold(UILabel label, int getgold)
        {
            yield return new WaitForSeconds(1.5f);
            int curGold = 0;
            float gab = 20;
            while (true)
            {
                label.text = string.Format("{0:N0}", (int)(curGold));
                yield return new WaitForEndOfFrame();
                curGold += (int)gab;
                gab *= 1.1f;
                if (curGold > getgold)
                {
                    curGold = getgold;
                    break;
                }
            }
            label.text = string.Format("{0:N0}", (int)(getgold));
        }
    }
}
