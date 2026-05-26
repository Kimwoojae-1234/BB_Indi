using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class fireWork : MonoBehaviour
    {
        int step = -1;
        public Transform origin;

        float posY = -0.5f;
        float curTime = 0;
        int sign = 1;

        // Use this for initialization
        public void Init()
        {
            step = 0;
            sign = 1;

            foreach (Transform t in origin)
            {
                t.gameObject.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (step == 0)
            {
                //posY -= (1 * Time.deltaTime);
                //if (posY <= -3.5f) posY = -3.5f;
                posY = -4.5f;// -3.5f;
                origin.localPosition = new Vector3(0, posY, 0);

                curTime += Time.deltaTime;
                if (curTime > 1.0f)
                {
                    string[] num = new string[6] { "00", "02", "03", "01", "04", "09" };
                    int rand = Random.Range(0, 6);
                    GameObject obj = Util.Load("MainGame/prefabs/firework/Eff_FireWorks_" + num[rand] + "_oneShot", origin, new Vector3(sign * Random.Range(2.0f, 6.5f), 0, 0));
                    //obj.layer = LayerMask.NameToLayer("BATTER_LAYER");
                    Destroy(obj, 4.0f);
                    
                    curTime = 0;
                    sign = -sign;
                }
            }
        }

        public void setBattingview()
        {
            step = 1;
            origin.localPosition = Vector3.zero;
        }


        

    }
}