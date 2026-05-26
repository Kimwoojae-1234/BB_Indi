using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class PitchCollider : MonoBehaviour
    {

        public GameObject zoneCollider;
        public GameObject endCollider;


        const float INIT_ZONEX = 0;
        const float INIT_ZONEY = 5.2f;
        const float INIT_ZONEZ = -255;

        // Use this for initialization
        void Start()
        {
            zoneCollider.transform.localPosition = new Vector3(INIT_ZONEX, INIT_ZONEY, INIT_ZONEZ);
        }

    }
}