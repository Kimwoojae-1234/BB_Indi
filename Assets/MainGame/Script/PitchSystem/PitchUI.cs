using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class PitchUI : MonoBehaviour
    {
        public float xPosition = 0;
        public float yPosition = -192;// - 171.5f;  //배팅뷰 로우        클래식: -144

        public GameObject _active, _zoneActive;
        public GameObject ballCursor;
        public GameObject ballObj;
        public Transform traceObj;

        public GameObject pCursor, blink;

        bool bRotate = false;
        float rotationX, rotationY, rotationZ;
        float rDX, rDY, rDZ;

        public pitchTimer timerObj;

        public float size = 0;

        void Start()
        {
            timerObj.gameObject.SetActive(false);
            _active.SetActive(false);
            bRotate = false;
            transform.localPosition = new Vector3(0, yPosition, 0);
        }

        
        void Update()
        {
            rotate();
        }

        public void SetActive(bool bActive)
        {
            //if (bActive == true) timerObj.init();
            if (bActive == true) ControlPitchingUI.InitTimer();
            _active.SetActive(bActive);
            bRotate = false;
        }

        public void SetTrace(bool bActive)
        {
            traceObj.gameObject.SetActive(bActive);
        }

        
        public void SetMove(float x, float y)
        {            
            float curX = x;// *1138.0f / 1280.0f;
            float curY = y;// *640.0f / 720.0f;
            ballCursor.transform.localPosition = new Vector3(curX, curY, 0);

            size = Mathf.Sqrt(x * x + y * y);
            //Debug.Log("===================>> size = " + size);
        }



        public void InitRotate(BallMoveType type, PitchingArsenal ballType, int sign)
        {
            ballObj.transform.localEulerAngles = new Vector3(0, 0, 0);

            if (ballType == PitchingArsenal.KNUCKLE)
            {
                //너클
                rotationX = rotationZ = 0;
                rotationY = 90;
                rDX = rDY = rDZ = 0;
            }
            else
            {
                //기타구종은 볼타입으로 퉁쳐
                if (type == BallMoveType.Straight)
                {
                    rotationX = rotationY = rotationZ = 0;
                    rDX = 700;
                    rDY = rDZ = 0;
                }
                else if (type == BallMoveType.Curve)
                {
                    rotationX = rotationY = 0;//
                    rotationZ = -sign * 45;
                    rDX = -400;
                    rDZ = rDY = 0;
                }
                else if (type == BallMoveType.Slide)
                {
                    rotationX = rotationY = 0;
                    rotationZ = -sign * 80;
                    rDY = sign * 400;
                    rDX = rDZ = 0;
                }
                else //if (type == BallMoveType.Straight)
                {
                    rotationX = rotationY = rotationZ = 0;
                    rDX = 200;
                    rDY = rDZ = 0;
                }

                if (Mode.cameraView == CameraView.PitcherCenter)
                {
                    rDX = -rDX;
                }
            }

            
            bRotate = true;
        }

        private void rotate()
        {
            if (bRotate == true)
            {
                rotationX += (rDX * Time.deltaTime);
                rotationY += (rDY * Time.deltaTime);
                rotationZ += (rDZ * Time.deltaTime);
                ballObj.transform.localEulerAngles = new Vector3(rotationX, rotationY, rotationZ);

            }
        }


        /// <summary>
        /// 투수 커서 활성화 / 비활성화
        /// </summary>
        /// <param name="bActive"></param>
        public void SetPitchCursor(bool bActive, float x, float y)
        {
            _zoneActive.SetActive(bActive);
            if (bActive == true)
            {
                Util.ChangeChildObjColor(pCursor,new Color(1,1,1,1));
                blink.SetActive(true);
                float curX = x;// *1138.0f / 1280.0f;
                float curY = y;// *640.0f / 720.0f;
                pCursor.transform.localPosition = new Vector3(curX, curY, 0);
            }
        }

        /// <summary>
        /// 탄착점 세팅
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetArrivePos(BallPlayManager manager, float x, float y)
        {
            StartCoroutine(setArriveSituation(manager, x, y));
        }

        private IEnumerator setArriveSituation(BallPlayManager manager, float x, float y)
        {
            Util.ChangeChildObjColor(pCursor, new Color(1, 1, 1, 0.75f));
            blink.SetActive(false);
            float curX = x;// *1138.0f / 1280.0f;
            float curY = y;// *640.0f / 720.0f;
            GameObject trace = Util.Load("MainGame/prefabs/gameUI/tracePrefab_" + (manager.bMyTurn ? "h" : "p"), traceObj, new Vector3(curX, curY, -0.001f));
            trace.transform.localScale = Vector3.one;
            trace.GetComponent<zoneTrace>().init(manager);

            if (manager.bMyTurn == false)
            {
                if (manager.pitcher.userControlValue == UserControlValue.Perfect || manager.pitcher.userControlValue == UserControlValue.Good)
                {
                    GameObject zoneHit = Util.Load("MainGame/prefabs/gameUI/traceZoneHitPrefab", traceObj, pCursor.transform.localPosition);
                    zoneHit.transform.localScale = Vector3.one;
                    Destroy(zoneHit, 0.5f);
                }
            }

            if (manager.nStrikeCount >= 2 && manager.bStrikeCheck == true)
            {
                yield return new WaitForSeconds(0.5f);
                GameObject traceK = Util.Load("MainGame/prefabs/gameUI/traceKPrefab", traceObj, new Vector3(curX, curY, -0.001f));
                traceK.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.9f);
                Destroy(traceK);
            }
            else
            {
                yield return new WaitForSeconds(1.4f);                
            }
            SetPitchCursor(false, 0, 0);
            Destroy(trace);

        }


        /// <summary>
        /// 초기 포지션 세팅
        /// </summary>
        /// <param name="bvState"></param>
        /// <param name="bLeftPitcher"></param>
        public void SetPitchUIInitPos(bool bvState, bool bLeftPitcher)
        {
            if (bvState == true)
            {
                xPosition = (bLeftPitcher?-9:37); //우투수
                yPosition = 17;
            }
            else
            {
                xPosition = 0;
                yPosition = -192;
            }
            transform.localPosition = new Vector3(xPosition, yPosition, 0);

        }


    }
}
