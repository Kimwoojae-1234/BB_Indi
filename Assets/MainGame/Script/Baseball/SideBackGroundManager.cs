using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class SideBackGroundManager : MonoBehaviour
    {
        
        public tk2dSprite[] field;
        

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
            
        }


        void OnEnable()
        {
            

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
           
        }

        public void setEvening()
        {
          
        }

        public void setNight()
        {
           
        }


        private void sideEveningUpdate()
        {
            
        }

        public void sideNightUpdate()
        {
           

        }
    }
}