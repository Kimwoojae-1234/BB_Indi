using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class spriteNumber : MonoBehaviour
    {
        public UISprite [] num;


        protected int gabX;
        protected string str;


        public void init(string _str, int number,int _gabX = -1)   //38
        {
            gabX = _gabX;
            str = _str;

            set(number);
        }

        public void set(int number)   //38
        {
            int max = 1;
            if (number < 0) number = 0;
            else if (number > 999) number = 999;

            num[0].gameObject.SetActive(true);
            num[1].gameObject.SetActive(number > 9 ? true : false);
            num[2].gameObject.SetActive(number > 99 ? true : false);

            if (number > 99)
            {
                max = 3;
                num[0].transform.localPosition = new Vector3(-gabX * 2, 0, 0);
                num[1].transform.localPosition = new Vector3(0, 0, 0);
                num[2].transform.localPosition = new Vector3(gabX * 2, 0, 0);
                num[0].spriteName = str + (number / 100);
                num[1].spriteName = str + ((number % 100) / 10);
                num[2].spriteName = str + ((number % 100) % 10);
            }
            else if (number > 9)
            {
                max = 2;
                num[0].transform.localPosition = new Vector3(-gabX, 0, 0);
                num[1].transform.localPosition = new Vector3(gabX, 0, 0);
                num[0].spriteName = str + (number / 10);
                num[1].spriteName = str + (number % 10);
            }
            else
            {
                num[0].transform.localPosition = new Vector3(0, 0, 0);
                num[0].spriteName = str + number;
            }

            for (int i = 0; i < max; i++) num[i].MakePixelPerfect();
        }

        public void deActive()   //38
        {
            num[0].gameObject.SetActive(false);
            num[1].gameObject.SetActive(false);
            num[2].gameObject.SetActive(false);
        }
    }
}