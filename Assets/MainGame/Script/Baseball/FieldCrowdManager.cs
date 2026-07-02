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


        //public tk2dSprite[] building;
        //public MeshRenderer hinge;
        public tk2dSprite field;
        
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
            
        }



        private void setFieldDay()
        {        
            Color fieldColor = new Color(1, 1, 1);            
            field.color = fieldColor;
        }

        private void setFieldEvening()
        {
           
        }

        private void setFieldNight()
        {
            
        }



        private void setfieldBackPosition(float angleX, float hookSlice)
        {
            
        }
    }
}
