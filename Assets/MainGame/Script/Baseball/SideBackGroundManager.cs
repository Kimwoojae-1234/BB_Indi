using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class SideBackGroundManager : MonoBehaviour
    {
        public SkeletonAnimation solarLightEffectAnim;

        public tk2dSprite sky1, sky2;
        public GameObject [] light;
   
        public tk2dSprite[] field;
        public tk2dSprite[] building;
        public SkeletonAnimation[] skelObj;

        public GameObject runnerObj;


        public Transform crowdTransform;

        private BackGroundManager.TimeState curTimeState;

        //private List<SkeletonAnimation> crowdList = new List<SkeletonAnimation>();
        private List<tk2dSprite> crowdList = new List<tk2dSprite>();
        private int totalCrowdCount;


        private BackGroundType groundType;
        private int[] solarLightDepth = new int[3] { 8, 0, 8 };
        private float solarX, solarY;

        private bool bLeftSide;
        private BallPlayManager manager = null;
        
        /*
        void Start()
        {            
            
        }*/

        void Awake()
        {
            curTimeState = BackGroundManager.TimeState.Day;
        }

        public void init(BallPlayManager _manager, bool _bLeftSide)
        {
            manager = _manager;
            bLeftSide = _bLeftSide;

            groundType = (BackGroundType)(Mode.stadiumType);
            if (groundType != BackGroundType.Dome)
            {
                solarX = solarLightEffectAnim.transform.localPosition.x;
                solarY = solarLightEffectAnim.transform.localPosition.y;
            }
            
            totalCrowdCount = 0;

            if (crowdTransform != null)
            {
                int crowdPer = Mode.crowdPer;
                foreach (Transform trans in crowdTransform)
                {
                    if (MyMath.Percent() < crowdPer)
                    {
                        float z = -0.05f - ((200 - trans.localPosition.y) * 0.001f);
                        trans.localPosition += new Vector3(0, 0, z);

                        tk2dSprite c = trans.gameObject.GetComponent<tk2dSprite>();
                        if (c != null)
                        {
                            crowdList.Add(c);
                            totalCrowdCount++;
                        }
                        else
                        {
                            Destroy(trans.gameObject);
                        }
                    }
                    else
                    {
                        Destroy(trans.gameObject);
                    }
                }
            }
        }


        void OnEnable()
        {
            if (manager != null)
            {
                if (manager.field.run.bOnBase[bLeftSide ? FieldParm.THIRDBASE_INDEX : FieldParm.FIRSTBASE_INDEX] == true)
                {
                    runnerObj.SetActive(true);
                }
                else
                {
                    runnerObj.SetActive(false);
                }
            }

            if (Mode.stadiumType == Mode.StadiumType.Dome) return;

            bool bIsUpdating = BackGroundManager.IsUpdating();

            if (curTimeState != BackGroundManager.GetTimeState() || bIsUpdating == true)
            {
                curTimeState = BackGroundManager.GetTimeState();

                if (curTimeState == BackGroundManager.TimeState.Evening)
                {
                    if (bIsUpdating == true)
                    {
                        sideEveningUpdate();
                        curTimeState = BackGroundManager.TimeState.Day; //초기화
                    }
                    /*else
                    {
                        setEvening();
                    }*/
                }
                else if (curTimeState == BackGroundManager.TimeState.Night)
                {
                    if (bIsUpdating == true)
                    {
                        sideNightUpdate();
                        curTimeState = BackGroundManager.TimeState.Day; //초기화
                    }
                    /*else
                    {
                        setNight();
                    }*/
                }
            }
        }


        private void setCrowdColor(Color color)
        {            
            if (Mode.crowdAnimMode == true)
            {
                for (int i = 0; i < totalCrowdCount; i++)
                {
                    crowdList[i].color = color;
                }
            }
        }


        public void  setDay()
        {
            curTimeState = BackGroundManager.TimeState.Day;
            //구조물과 관중
            Color curColor1 = new Color(1,1,1);
            setCrowdColor(curColor1);
            for (int i = 0; i < building.Length; i++) building[i].color = curColor1;            
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = new Color(1, 1, 1);
            for (int i = 0; i < field.Length; i++) field[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true);
            sky2.gameObject.SetActive(false);
            sky1.spriteId = sky1.GetSpriteIdByName("sky0");
            sky1.color = new Color(1, 1, 1, 1);

            
            //라이트
            for (int i = 0; i < light.Length; i++)
            {
                light[i].gameObject.SetActive(false);
            }
        }

        public void setEvening()
        {
                curTimeState = BackGroundManager.TimeState.Evening;

                //구조물과 관중
                Color curColor1 = new Color(0.792f, 0.741f, 0.667f);
                setCrowdColor(curColor1);
                for (int i = 0; i < building.Length; i++) building[i].color = curColor1;
                for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

                //필드
                Color curColor2 = new Color(1, 0.902f, 0.749f);
                for (int i = 0; i < field.Length; i++) field[i].color = curColor2;

                //하늘
                sky1.gameObject.SetActive(true);
                sky2.gameObject.SetActive(false);
                sky1.spriteId = sky1.GetSpriteIdByName("sky1");
                sky1.color = new Color(1, 1, 1, 1);

                //하늘광
                solarLightEffectAnim.skeleton.SetToSetupPose();
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_03", true);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1));

                solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, solarLightDepth[1]);

                for (int i = 0; i < light.Length; i++)
                {
                    light[i].gameObject.SetActive(true);
                    if (groundType != BackGroundType.Jamsil)
                    {
                        light[i].GetComponent<SkeletonAnimation>().skeleton.SetColor(new Color(1, 1, 1, 1));
                    }
                }
            
        }

        public void setNight()
        {
            curTimeState = BackGroundManager.TimeState.Night;
            //구조물과 관중
            Color curColor1 = new Color(0.624f, 0.690f, 1.0f);
            setCrowdColor(curColor1);
            for (int i = 0; i < building.Length; i++) building[i].color = curColor1;            
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = new Color(1, 1, 1);
            for (int i = 0; i < field.Length; i++) field[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true);
            sky2.gameObject.SetActive(false);
            sky1.spriteId = sky1.GetSpriteIdByName("sky2");
            sky1.color = new Color(1, 1, 1, 1);

            //하늘광
            solarLightEffectAnim.skeleton.SetToSetupPose();
            solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_NIGHT_01", true);
            solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1));

            solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, solarLightDepth[2]);


            if (groundType == BackGroundType.LionsPark)
            {
                building[3].color = new Color(0.23f, 0.33f, 0.82f);
            }

            //라이트
            for (int i = 0; i < light.Length; i++)
            {
                light[i].gameObject.SetActive(true);
            }
        }


        private void sideEveningUpdate()
        {
            int count = BackGroundManager.GetUpdateCount();
            float alphagab = 1.0f / 60.0f;

            //빌딩
            Color curColor1 = BackGroundManager.GetBuildingColor();
            for (int i = 0; i < building.Length; i++) building[i].color = curColor1;
            setCrowdColor(curColor1);
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = BackGroundManager.GetFieldColor();
            for (int i = 0; i < field.Length; i++) field[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true); //노을
            sky2.gameObject.SetActive(true); //낮
            sky1.spriteId = sky1.GetSpriteIdByName("sky1");
            sky2.spriteId = sky1.GetSpriteIdByName("sky0");
            sky1.color = new Color(1, 1, 1, 1);
            sky2.color = new Color(1, 1, 1, 1 - (alphagab * count));

            for (int i = 0; i < light.Length; i++)
            {
                light[i].gameObject.SetActive(true);
                if (groundType != BackGroundType.Jamsil)
                {
                    light[i].GetComponent<SkeletonAnimation>().skeleton.SetColor(new Color(1, 1, 1, (alphagab * count)));
                }
            }

            if (count < 30)
            {
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_01", true);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1 - (alphagab * count * 2)));
            }
            else if (count == 30)
            {
                solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, solarLightDepth[1]);
            }
            else if (count > 30)
            {
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_03", true);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, (alphagab * (count - 30) * 2)));
            }
        }

        public void sideNightUpdate()
        {
            int count = BackGroundManager.GetUpdateCount();
            float alphagab = 1.0f / 60.0f;

            //빌딩
            Color curColor1 = BackGroundManager.GetBuildingColor();
            for (int i = 0; i < building.Length; i++) building[i].color = curColor1;
            setCrowdColor(curColor1);
            for (int i = 0; i < skelObj.Length; i++) skelObj[i].skeleton.SetColor(curColor1);

            //필드
            Color curColor2 = BackGroundManager.GetFieldColor();
            for (int i = 0; i < field.Length; i++) field[i].color = curColor2;

            //하늘
            sky1.gameObject.SetActive(true); //밤
            sky2.gameObject.SetActive(true); //노을         
            sky1.spriteId = sky1.GetSpriteIdByName("sky2");
            sky2.spriteId = sky1.GetSpriteIdByName("sky1");
            sky1.color = new Color(1, 1, 1, 1);
            sky2.color = new Color(1, 1, 1, 1 - (alphagab * count));

            //라이트
            for (int i = 0; i < light.Length; i++)
            {
                light[i].gameObject.SetActive(true);
            }

            if (groundType == BackGroundType.LionsPark)
            {
                building[3].color = new Color(0.23f, 0.33f, 0.82f);
            }

            if (count < 30)
            {
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_LIGHT_03", true);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, 1 - (alphagab * count * 2)));
            }
            else if (count == 30)
            {
                solarLightEffectAnim.transform.localPosition = new Vector3(solarX, solarY, solarLightDepth[2]);                           
            }
            else if (count > 30)
            {
                solarLightEffectAnim.state.SetAnimation(0, "JAMSIL_NIGHT_01", true);
                solarLightEffectAnim.skeleton.SetColor(new Color(1, 1, 1, (alphagab * (count - 30) * 2)));                
            }

        }
    }
}