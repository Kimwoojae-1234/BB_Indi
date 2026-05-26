using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class quickBall : MonoBehaviour
    {
        public float TestTime = 3;

        public float AccelG = -80;   //중력가속도
        public float Accel = -10;     //바람감속

        public GameObject ball;
        public TrailRenderer laser;


        bool bInit = false;
        float remainTime;
        


        float curTime = 0;

        float posX, posY, posZ;
        float dV;
        float dZ;
        float angle;


        private bool bUp;
        

        void Awake()
        {
            laser.sortingOrder = 2;
        }

        // Update is called once per frame
        void Update()
        {
            if (bInit == true)
            {
                move();
            }
        }

        void deActive()
        {
            gameObject.SetActive(false);
        }


        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="battingData"></param>
        /// <param name="position"></param>
        public void init(SimulBattingData battingData, Transform[] position)
        {
            setDestPosition(battingData, position);
            Invoke("deActive", 5.0f);
        }

        /// <summary>
        /// 목적지와 걸리는 시간 구하기
        /// </summary>
        /// <param name="battingData"></param>
        /// <param name="position"></param>
        private void setDestPosition(SimulBattingData battingData, Transform[] position)
        {
            SimulResultState result = battingData.result;
            SimulHitType hitType = battingData.hitType;
            int index = battingData.fIndex;

            remainTime = TestTime;// 1.5f;// 플라이
            /*if (hitType == SimulHitType.Liner)
            {
                remainTime = 1.0f;  //라이너
            }
            else if (hitType == SimulHitType.Bunt || hitType == SimulHitType.Grounder)
            {
                remainTime = (index < CPlayer._LEFTFIELDER ? 0.7f : 1.0f); //땅볼                
            }*/
            Vector3 destPos = Vector3.zero;
            if (result == SimulResultState.Double || result == SimulResultState.Triple || result == SimulResultState.DoubleOneError || result == SimulResultState.TripleOneError)
            {
                if (index == CPlayer._LEFTFIELDER) destPos = position[9].localPosition;
                else if (index == CPlayer._LEFTFIELDER) destPos = position[10].localPosition;
                else destPos = position[MyMath.Half() ? 9 : 10].localPosition;
            }
            else
            {
                destPos = position[index].localPosition;
            }

            curTime = 0;
            posX = posY = posZ = 0;
            dV = getInitDV(destPos);
            dZ = getInitDZ();
            angle = Mathf.Atan2(destPos.y, destPos.x);

            bUp = true;
            bInit = true;

        }



        private float getInitDZ()
        {
            float dz = -0.5f * AccelG * remainTime;
            return dz;
        }

        private float getInitDV(Vector3 destPos)
        {
            float distance = Mathf.Sqrt((destPos.x * destPos.x) + (destPos.y * destPos.y));
            float dv = (distance - (0.5f*Accel*remainTime*remainTime)) /(remainTime);
            return dv;

        }


        private void move()
        {
            curTime += Time.deltaTime;
            float dx = dV * Mathf.Cos(angle);
            float dy = dV * Mathf.Sin(angle);

            //움직임
            posX += dx * Time.deltaTime;
            posY += dy * Time.deltaTime;
            posZ += dZ * Time.deltaTime;
            
            //위치 업데이트
            transform.localPosition = new Vector3(posX, posY, 0);
            ball.transform.localPosition = new Vector3(0, posZ, 0);

            //가속
            dV += Accel * Time.deltaTime;
            dZ += AccelG * Time.deltaTime;

            if (bUp == true)
            {
                if (dZ <= 0)
                {
                    bUp = false;
                }
            }
            
            if (curTime >= remainTime) bInit = false;
        }


    }
}