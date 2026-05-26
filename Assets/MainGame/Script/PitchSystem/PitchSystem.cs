//#define _TEST_SYSTEM

using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class PitchSystem : MonoBehaviour
    {

        //클래식 앵글
        /*const int cameraY = 0; //이전
        const int cameraZ = -330;
        const float cameraAngleX = 3.75f;
        const int originZ = -50;
        const int originAngleX = 355;*/

        //배터로우앵글
        

        public Camera _camera;
        public GameObject origin;
        public PitchOrigin pitchOrigin;
        public PitchOriginPv pitchOriginPv;



        public BattingSystem battingSystem;
        public BattingSystemPv battingSystemPv;



        // Use this for initialization
        void Start()
        {
            //origin.transform.localPosition = new Vector3(0, 0, originZ);
            //_camera.transform.localPosition = new Vector3(0, cameraY, cameraZ);
            //_camera.transform.localEulerAngles = new Vector3(cameraAngleX, 0, 0);
            if (pitchOrigin != null)
            {
                pitchOrigin = origin.GetComponent<PitchOrigin>();
            }
            if (pitchOriginPv != null)
            {
                pitchOriginPv = origin.GetComponent<PitchOriginPv>();
            }
            setInit(false);

#if _TEST_SYSTEM
            battingSystemPv.gameObject.SetActive(false);
            setInitPv(true,  false);

            //카메라 세팅
            int layerMask = 1 << LayerMask.NameToLayer("PITCH_LAYER");
            origin.gameObject.SetActive(true);
            pitchOriginPv.setVectorInit();
            Camera unityCam = _camera.GetComponent<Camera>();
            unityCam.cullingMask = layerMask;
            
#endif
        }

        // Update is called once per frame
        void Update()
        {
#if _TEST_SYSTEM
            if (Input.GetKeyDown(KeyCode.Space))
            {                
                pitchOriginPv.setPitcherTest(startPosX, startPosY);

                pitchOriginPv.ballType = select;
                pitchOriginPv.setMoveTypeAndGuwee(ballType, 1000);

                startPitchPv(false);
            }
            if(Input.GetKeyDown(KeyCode.Return))
            {
                battingSystemPv.init(0, 0);
            }
#endif

        }


#if _TEST_SYSTEM
        public int startPosX = 0;
        public int startPosY = 12; //오버핸드 기준
        public int cameraX = -11; //우투수 기준
        public int cameraY = 20;
        public int cameraZ = 100;
        public int originZ = 0;
        public float cameraAngleX = 5;      //우투수
        public float cameraAngleY = 177;    //우투수

        public bool bLeft = false;

        public PitchingArsenal select = PitchingArsenal.FASTBALL;
        public BallMoveType ballType = BallMoveType.Straight;

#endif



        private void setInit(bool bActive)
        {
            int cameraX, cameraY, cameraZ, originZ;
            float cameraAngleX = -10;//
            float cameraAngleY = 0;
            
            cameraX = 0;
            cameraY = -5;
            cameraZ = -330;
            cameraAngleX = -10;
            originZ = -50;

            transform.position = new Vector3(0, 50, -1000);
            origin.transform.localPosition = new Vector3(0, 0, originZ);
            _camera.transform.localPosition = new Vector3(cameraX, cameraY, cameraZ);
            _camera.transform.localEulerAngles = new Vector3(cameraAngleX, cameraAngleY, 0);
            origin.SetActive(bActive);
        }


        //슬라이더류 : 최대 무브먼트 5,  스피드레이트 1.5f : 시작높이 8
        //커브류 : 최대 무브먼트 5,  스피드레이트 1.2f : 시작높이 8
        //업슛 :  최대 무브먼트 5,  스피드레이트 1.2f : 시작높이 -2 : //프로그램적으로 스트레이트 계열
        //오프스피드 : 최대 무브먼트 5,  스피드레이트 1.2f : 시작높이 13
        //포크볼 : 최대 무브먼트 5,  스피드레이트 1.5f  시작높이 7    //프로그램적으로 오프스피드 계열
        //SFF : 최대 무브먼트 5,  스피드레이트 1.7f     시작높이 8    //프로그램적으로 오프스피드 계열
        //패스트브레이킹 :최대 무브먼트 5,  스피드레이트 1.7f : 시작높이 12
        //직구  스피드레이트 1.8f : 시작높이 12


        public void startPitch()
        {
            setInit(true);
            pitchOrigin.setVector();
        }


        public void setZone(float x, float y, float maxX, float maxY)
        {
            pitchOrigin.setZone(x, y, maxX, maxY);
        }



        private void setInitPv(bool bActive, bool bLeftPitcher)
        {
            
            //직구 오프스피드볼 계열 startPosY = 12
            //커브 / 슬라이더 계열 오버 8, 언더 2, 사이드 3


            //슬라이더, 체인지업(서클,벌칸, 노멀)은 브레이크가 더빨리 되면 좋음
            //패스트 브레이킹류 볼도 브레이크가 더 빨리 되야함
            //커터, 투심, Sff, 하드싱커(싱킹브레이크)

            int cameraX, cameraY, cameraZ, originZ;
            float cameraAngleX = -10;//
            float cameraAngleY = 0;
            //int originAngleX = 355;
            if (bLeftPitcher == false)
            {
                //우투수
                pitchOriginPv.zoneCollider.transform.localPosition = new Vector3(2.5f, 4, -255);
                cameraX = -11; //우투수 기준
                cameraY = 20;
                cameraZ = 100;
                originZ = 0;
                cameraAngleX = 5;
                cameraAngleY = 177;    //우투수
            }
            else
            {
                //좌투수
                pitchOriginPv.zoneCollider.transform.localPosition = new Vector3(-3, 3.5f, -255);
                cameraX = 15;
                cameraY = 20;
                cameraZ = 100;
                originZ = 0;
                cameraAngleX = 5;
                cameraAngleY = 183;   
            }
            transform.position = new Vector3(0, 50, -1000);
            origin.transform.localPosition = new Vector3(0, 0, originZ);
            _camera.transform.localPosition = new Vector3(cameraX, cameraY, cameraZ);
            _camera.transform.localEulerAngles = new Vector3(cameraAngleX, cameraAngleY, 0);
            origin.SetActive(bActive);
        }


        public void startPitchPv(bool bLeftPitcher)
        {
            setInitPv(true, bLeftPitcher);
            pitchOriginPv.setVector();
        }


        public void setZonePv(float x, float y, float maxX, float maxY)
        {
            pitchOriginPv.setZone(-x, y, maxX, maxY);
        }

    }
}