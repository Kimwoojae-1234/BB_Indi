using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class perfectTimer : MonoBehaviour
    {
        public GameObject scale;

        public void init(float remainTime)
        {
            //scale.duration = remainTime;
            //scale.enabled = true;
            scale.transform.localScale = new Vector3(2, 2, 1);
            TweenScale.Begin(scale, remainTime, new Vector3(0.1f,  0.1f, 1));
        }
    }
}
