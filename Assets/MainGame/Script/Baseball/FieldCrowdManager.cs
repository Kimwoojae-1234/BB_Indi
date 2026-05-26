//#define _NO_TIMECHANGE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace BaseBall.BallPlay
{
    public class FieldCrowdManager : MonoBehaviour
    {
        private static FieldCrowdManager Instance_ = null;
        private CrowdState lastState;


        public GameObject light1, light2;
        //public tk2dSprite[] building;
        //public MeshRenderer hinge;
        public tk2dSprite field;
        public Transform [] crowdTrans;
        public GameObject fieldBack;

        public Transform[] fenceTrans;

        //public BoxCollider[] crowdCollider;

        //private List<Crowd> crowdList = new List<Crowd>();
        //private int totalCrowdCount;

        private BackGroundType groundType;
        private int initY, initZ;  //뒷배경의 뎁스

        void Awake()
        {
            Instance_ = this;
            lastState = CrowdState.Normal;
            groundType = (BackGroundType)Mode.stadiumType;
        }

        void Start()
        {
            setFenceSize();

            groundType = (BackGroundType)Mode.stadiumType;

            if (groundType == BackGroundType.LionsPark)
            {
                initY = 2000;
                initZ = 0;
            }
            else 
            {
                initY = 1700;
                initZ = 100;
            }


            //crowdList.Clear();
            int crowdPer = Mode.crowdPer;
            int totalCrowdCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Mode.crowdAnimMode == false)
                {
                    Destroy(crowdTrans[i].gameObject);
                }
                else
                {
                    if (crowdTrans[i] != null)
                    {
                        foreach (Transform trans in crowdTrans[i])
                        {                            
                            if (MyMath.Percent() < crowdPer)
                            {
                                //crowdList.Add(c);
                                trans.gameObject.SetActive(true);
                                totalCrowdCount++;
                            }
                            else
                            {
                                Destroy(trans.gameObject);
                            }
                        }
                    }
                }
            }

            if (groundType != BackGroundType.Dome)
            {
                //돔구장이 아닌경우 라이트 세팅
                light1.gameObject.SetActive(false);
                light2.gameObject.SetActive(false);
            }

        }


        void OnDestroy()
        {
            Instance_ = null;
        }


        public static void SetActive(bool bActive)
        {
            if (Instance_ != null)
            {
                Instance_.gameObject.SetActive(bActive);
            }
        }

        public static void SetCrowdActive(bool bActive, int index)
        {
            Instance_.setCrowdActive(bActive, index);
        }

        private void setCrowdActive(bool bActive, int index)
        {
            if (crowdTrans[index].gameObject.activeSelf != bActive)
            {
                crowdTrans[index].gameObject.SetActive(bActive);
            }
        }

        public static void SetCrowdActiveAll(bool bActive)
        {
            if (Mode.crowdAnimMode == true)
            {
                if (Instance_ != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Instance_.setCrowdActive(bActive, i);
                    }
                }
            }
        }

        public static void ChangeTime(BackGroundManager.TimeState timeState)
        {
            if (Instance_ != null)
            {                
                Instance_.changeTime(timeState);
            }
        }


        public static void SetfieldBackPosition(float angleX, float hookSlice)
        {
            Instance_.setfieldBackPosition(angleX, hookSlice);
        }

        /*
        public static void CheckCrowdBound()
        {
            Instance_.checkCrowdBound();
        }


        private void checkCrowdBound()
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 topRight = CameraManager.fieldWorldToScreenPoint(crowdTrans[i].position + crowdCollider[i].bounds.extents);
                Vector3 bottomLeft = CameraManager.fieldWorldToScreenPoint(crowdTrans[i].position + crowdCollider[i].bounds.extents);

                if (topRight.x < 1280 && topRight.y < 720 && bottomLeft.x > 0 && bottomLeft.y > 0)
                {
                    setCrowdActive(true, i);
                }
                else
                {
                    setCrowdActive(false, i);
                }
            }
        }*/


        private void setFenceSize()
        {
            FieldSize.FENCE_LEFT_POLE_X = fenceTrans[0].localPosition.x;
            FieldSize.FENCE_LEFT_POLE_Y = fenceTrans[0].localPosition.y;

            FieldSize.FENCE_LEFT_POINT_X1 = fenceTrans[1].localPosition.x;
            FieldSize.FENCE_LEFT_POINT_Y1 = fenceTrans[1].localPosition.y;

            FieldSize.FENCE_LEFT_POINT_X2 = fenceTrans[2].localPosition.x;
            FieldSize.FENCE_LEFT_POINT_Y2 = fenceTrans[2].localPosition.y;

            FieldSize.FENCE_LEFT_POINT_X3 = fenceTrans[3].localPosition.x;
            FieldSize.FENCE_LEFT_POINT_Y3 = fenceTrans[3].localPosition.y;

            FieldSize.FENCE_LEFT_POINT_X5 = fenceTrans[4].localPosition.x;
            FieldSize.FENCE_LEFT_POINT_Y5 = fenceTrans[4].localPosition.y;

            FieldSize.FENCE_RIGHT_POINT_X5 = fenceTrans[5].localPosition.x;
            FieldSize.FENCE_RIGHT_POINT_Y5 = fenceTrans[5].localPosition.y;

            FieldSize.FENCE_RIGHT_POINT_X3 = fenceTrans[6].localPosition.x;
            FieldSize.FENCE_RIGHT_POINT_Y3 = fenceTrans[6].localPosition.y;

            FieldSize.FENCE_RIGHT_POINT_X2 = fenceTrans[7].localPosition.x;
            FieldSize.FENCE_RIGHT_POINT_Y2 = fenceTrans[7].localPosition.y;

            FieldSize.FENCE_RIGHT_POINT_X1 = fenceTrans[8].localPosition.x;
            FieldSize.FENCE_RIGHT_POINT_Y1 = fenceTrans[8].localPosition.y;

            FieldSize.FENCE_RIGHT_POLE_X = fenceTrans[9].localPosition.x;
            FieldSize.FENCE_RIGHT_POLE_Y = fenceTrans[9].localPosition.y;

            Destroy(fenceTrans[0].parent.gameObject);
            fenceTrans = null;
    }



        private void changeTime(BackGroundManager.TimeState timeState)
        {
            groundType = (BackGroundType)Mode.stadiumType;
            if(groundType != BackGroundType.Dome)
            {
                if (timeState == BackGroundManager.TimeState.Day)
                {
                    setFieldDay();
                }
                else if (timeState == BackGroundManager.TimeState.Evening)
                {
                    setFieldEvening();
                }
                else //if (timeState == BackGroundManager.TimeState.Night)
                {
                    setFieldNight();
                }

                if (groundType == BackGroundType.LionsPark)
                {
                    Color timeColor = new Color(1,1,1);
                    if (timeState == BackGroundManager.TimeState.Evening) timeColor = new Color(0.624f, 0.690f, 1.0f);
                    else if (timeState == BackGroundManager.TimeState.Night) timeColor = new Color(0.23f, 0.33f, 0.82f);
                    fieldBack.GetComponent<tk2dSprite>().color = timeColor;
                }
                else //if (groundType == BackGroundType.Jamsil || groundType == BackGroundType.ChamionsField ||groundType == BackGroundType.Hanhwa)
                {                    
                    int timeIndex = (int)timeState + 1;
                    fieldBack.GetComponent<SpriteRenderer>().sprite = (Sprite)Resources.Load("MainGame/Texture/stadiumBack/city_back" + timeIndex, typeof(Sprite));
                }
            }
        }



        private void setFieldDay()
        {        
            Color fieldColor = new Color(1, 1, 1);            
            field.color = fieldColor;
        }

        private void setFieldEvening()
        {
            Color fieldColor = new Color(1, 0.902f, 0.749f);
            field.color = fieldColor;

            /*
            for (int i = 0; i < totalCrowdCount; i++)
            {
                crowdList[i].setColor(buildingColor);
            }

            for (int i = 0; i < building.Length; i++)
            {
                building[i].color = buildingColor;
            }*/

            light1.gameObject.SetActive(true);
            light2.gameObject.SetActive(true);
        }

        private void setFieldNight()
        {
            
            Color fieldColor = new Color(1, 1, 1);
            field.color = fieldColor;

            /*
            //Color buildingColor = new Color(0.624f, 0.690f, 1.0f);
            for (int i = 0; i < totalCrowdCount; i++)
            {
                crowdList[i].setColor(buildingColor);
            }

            for (int i = 0; i < building.Length; i++)
            {
                building[i].color = buildingColor;
            }*/

            light1.gameObject.SetActive(true);
            light2.gameObject.SetActive(true);
        }



        private void setfieldBackPosition(float angleX, float hookSlice)
        {
            if (Mode.stadiumType != Mode.StadiumType.Dome)
            {
                //
                float gabX = (-angleX * 25) + (-hookSlice * 100);
                fieldBack.transform.localPosition = new Vector3(1887 + gabX, initY, initZ); 

            }
        }
    }
}
