using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class ballTrace : MonoBehaviour
    {
        private tk2dSprite spr;
        private float speed;
        // Use this for initialization
        void Start()
        {
            spr = GetComponent<tk2dSprite>();
            speed = 1 / 0.12f;
            Destroy(gameObject, 0.12f);

        }


        // Update is called once per frame
        void Update()
        {
            float curAlpha = spr.color.a - (speed * Time.deltaTime);
            spr.color = new Color(1, 1, 1, curAlpha);
        }
    }
}