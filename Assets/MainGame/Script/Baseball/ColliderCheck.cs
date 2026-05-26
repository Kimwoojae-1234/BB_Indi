using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class ColliderCheck : MonoBehaviour
    {
        public FBall ball;

        void OnTriggerStay(Collider col)
        {
            //if (col.gameObject.tag == "INFIELD_FENCE_TAG") //.CompareTag("
            if (col.gameObject.CompareTag("INFIELD_FENCE_TAG") == true)
            {
                if (ball.bSideFenceCol == true && ball.bSideFenceBallDraw == true)
                {
                    ball.setDraw(false);
                    ball.bSideFenceBallDraw = false;
                }
            }

        }

        void OnTriggerEnter(Collider col)
        {
            //
            //if (col.gameObject.tag == "INFIELD_FENCE_TAG")
            if (col.gameObject.CompareTag("INFIELD_FENCE_TAG") == true)
            {
                ////Debug.Log("=======================>>>담장 충돌");
                ball.setSideFenceCol();
            }

            //if (col.gameObject.tag == "POLE_TAG")
            if (col.gameObject.CompareTag("POLE_TAG") == true)
            {
                ////Debug.Log("=======================>>>폴대 충돌");
                ball.setPoleCollision();
            }
        }

        void OnTriggerExit(Collider col)
        {
            ////Debug.Log("=======================>>>담장 충돌");
            //if (col.gameObject.tag == "INFIELD_FENCE_TAG")
            if (col.gameObject.CompareTag("INFIELD_FENCE_TAG") == true)
            {
                if (ball.bSideFenceBallDraw == false)
                {
                    ball.setDraw(true);
                    ball.bSideFenceBallDraw = true;
                }
            }
        }
    }
}
