using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class UIOutGameLoading : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            StartCoroutine(backToOutGame());
        }

        // Update is called once per frame
        void Update()
        {

        }


        private IEnumerator backToOutGame()
        {
            yield return new WaitForSeconds(1.0f);

            GameObject fieldObj = GameObject.FindWithTag("FIELDINGVIEW_TAG");
            if (fieldObj != null)
            {
                Destroy(fieldObj.gameObject);
            }
            /*
            if (BallPlayManager.GetInstance() != null)
            {
                Destroy(BallPlayManager.GetInstance().gameObject);
            }*/

            if (CameraManager.GetInstance() != null)
            {
                Destroy(CameraManager.GetInstance().gameObject);
            }

#if _Test_Local 
            //로컬에서 아웃게임
#else
            //아웃게임
#endif
        }
    }
}
