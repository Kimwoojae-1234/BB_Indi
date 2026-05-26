using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class Batting : MonoBehaviour
    {
        public BallPlayManager manager;
        public Pitcher pitcher;
        public Batter batter;
        public Field field; //
        public runnerManager run;
        public Zone zoneUI;


        public Transform battingViewObj;
        public GameObject center,centerObj;
        public GameObject left, leftObj;
        public GameObject right, rightObj;
        public GameObject pitcherview, pitcherviewObj;
        public tk2dSpriteAnimator justmeet;
        public GameObject zone;


        public bvRunner _1stRunner, _2ndRunner;


        //센터
        public GameObject _stand, _field;
        public GameObject _runnerPosition;

        //레프트
        public GameObject _leftStand, _leftField;
        public GameObject _leftRunnerPosition;

        //라이트
        public GameObject _rightStand, _rightField;
        public GameObject _rightRunnerPosition;
     


        public void initInstance(BallPlayManager manager)
        {
            //this.manager = manager;
            //pitcher = manager.pitcher;
            //batter = manager.batter;
            //field = manager.field;
            //run = field.run;

            //zone = GameObject.FindWithTag("ZONEUI_TAG");
            //zone.transform.parent = transform;
            //zoneUI = zone.GetComponent<Zone>();
            //zoneUI.initInstance(manager);

            //관중사운드
            MusicManager.Get().PlayMusic(MusicManager.MusicID.Idle);

            setInitPosition();
            //BackGroundManager.SetInitTime(0.0f);
            lastView = CameraView.None;

        }

        
        public void loadCenter()
        {
            centerObj = Util.Load("MainGame/prefabs/BattingViewPrefab/bg/centerBGPrefab" + Mode.stadiumNum, center.transform, Vector3.zero);// _center;
            centerObj.GetComponent<BackGroundManager>().init(manager); ////살려살려
            _stand = centerObj.transform.Find("stand").gameObject;
            _field = centerObj.transform.Find("field").gameObject;
        }

        public void loadLeft()
        {
            leftObj = Util.Load("MainGame/prefabs/BattingViewPrefab/bg_side/leftBGPrefab" + Mode.stadiumNum, left.transform, new Vector3(0, -120, 0));
            leftObj.GetComponent<SideBackGroundManager>().init(manager, true);
            _leftStand = leftObj.transform.Find("stand").gameObject;
            _leftField = leftObj.transform.Find("field").gameObject;
            _leftRunnerPosition = leftObj.transform.Find("runnerPos").gameObject;
        }

        public void loadRight()
        {
            rightObj = Util.Load("MainGame/prefabs/BattingViewPrefab/bg_side/rightBGPrefab" + Mode.stadiumNum, right.transform, new Vector3(0, -120, 0));
            rightObj.GetComponent<SideBackGroundManager>().init(manager, false);
            _rightStand = rightObj.transform.Find("stand").gameObject;
            _rightField = rightObj.transform.Find("field").gameObject;
            _rightRunnerPosition = rightObj.transform.Find("runnerPos").gameObject;
        }

        public void loadPitcher()
        {
            pitcherviewObj = Util.Load("MainGame/prefabs/BattingViewPrefab/pitcher/pitcherBGPrefab" + Mode.stadiumNum, pitcherview.transform, new Vector3(0, 0, 0));
            pitcherviewObj.GetComponent<PitchviewManager>().init();
        }

        public void pitcherviewTimeCheck()
        {
            pitcherviewObj.GetComponent<PitchviewManager>().checkTimeChange();
        }


        public CameraView lastView = CameraView.None;
        public void settingView(CameraView view, bool bDontDestroyBatter = false)
        {
            //Debug.Log("==============================>> 카메라 세팅");
            if (view != lastView)
            {
                Mode.cameraView = view;// 

                lastView = view;

                //타자 설정
                batter.setCameraSetting(bDontDestroyBatter);

                //투수 설정
                pitcher.setCameraSetting();

                //존 설정
                zoneUI.initPosition();

                //피치 시스템 설정

            }
        }

        public void resetView()
        {
            lastView = CameraView.None;
            CameraView curView = CameraView.BatterLow;
            if (Mode.bPitchingViewActive == true)
            {
                curView = (manager.bMyTurn == true ? CameraView.BatterLow : CameraView.PitcherCenter);
            }
            settingView(curView); 
        }




        public void setInitPosition()
        {
            //int initPosY = 80;
            transform.position = new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY - 80, 8);

            if (Mode.cameraView == CameraView.PitcherCenter)  //if (manager.bMyTurn == false)// 
            {
                //initPosY = 80;
                setActiveView("PITCHER");
                battingViewObj.localPosition = new Vector3(0, 80, 0);

            }
            else //if (Mode.cameraView == CameraView.BatterLow)
            {
                //initPosY = 150;
                setActiveView("CENTER");
                battingViewObj.localPosition = new Vector3(0, 150, 0);
            }  

            setJustMeet(false);
        }

        public void setCameraOffset()
        {
            //Debug.Log("=================>>>setCameraOffset  pitcher.pitchHand = " + pitcher.pitchHand);
            if (Mode.cameraView == CameraView.PitcherCenter)  //if (manager.bMyTurn == false)//
            {
                //transform.localScale = Vector3.one;
                transform.localScale = new Vector3(0.95f, 0.95f, 1);
                if (pitcher.pitchHand == CPlayer._LEFTHAND)
                {
                    CameraManager.SetCameraInitPos(new Vector3(-120, 0, 0));
                }
                else
                {
                    CameraManager.SetCameraInitPos(new Vector3(80, 0, 0));                    
                }
            }
            else
            {
                transform.localScale = new Vector3(0.9f, 0.9f, 1);
                CameraManager.SetCameraInitPos(new Vector3(0, 0, 0));
                _runnerPosition.SetActive(true);                
            }
            batter.gameObject.SetActive(true);

            //Debug.Log("setCameraOffset");
        }



        public void setActiveView(string viewName)
        {
            pitcherview.SetActive(viewName == "PITCHER" ? true : false);
            center.SetActive(viewName == "CENTER" ? true : false);
            left.SetActive(viewName == "LEFT" ? true : false);
            right.SetActive(viewName == "RIGHT" ? true : false);

        }

        /*
        public void setBattingViewPosOffset(float x, float y)
        {
            battingViewObj.localPosition = new Vector3(x, 80 + y, 0);
        }*/

        /*
        public void setHomerunPosition()
        {
            transform.position = new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, BallPlayManager.BATTINGVIEW_CAMERA_INITY - 80 - 230, 8);
        }*/

        //배팅뷰상 러너 체크해주기
        public void setBVRunnerState(bool bSecondLead = false)
        {
            if (Mode.cameraView == CameraView.PitcherCenter)
            {
                return;
            }
            else
            {
                //int initPosY;
                float initScale;
                if (Mode.cameraView == CameraView.BatterLow)
                {
                    initScale = 0.3f;
                    //initPosY = 210;
                }
                else //디폴트
                {
                    initScale = 0.3f;
                    //initPosY = 426;
                }
                _1stRunner.set1stRunnerInit(field.run.bOnBase[FieldParm.FIRSTBASE_INDEX], initScale);
                _2ndRunner.set2ndRunnerInit(field.run.bOnBase[FieldParm.SECONDBASE_INDEX],initScale);
                field.run.setRunnerCamera(true);

                if (bSecondLead == true)
                {
                    if (field.run.bOnBase[FieldParm.SECONDBASE_INDEX] == true)
                    {
                        if (field.b2ndLeadCheck == true)
                        {
                            _2ndRunner.set2ndRunnerLead();
                        }
                    }
                }
            }
        }

        public void setBattingviewRunnerDelete()
        {
            _1stRunner.set1stRunnerInit(false,1);
            _2ndRunner.set2ndRunnerInit(false,1);
        }




        public void setReadyState(bool zoom = false)
        {
            if (Mode.cameraView == CameraView.PitcherCenter) //if (manager.bMyTurn == false)//
            {
                //transform.localScale = Vector3.one;
                transform.localScale = new Vector3(0.95f, 0.95f, 1);
            }
            else //if (Mode.cameraView == CameraView.BatterLow)
            {
                transform.localScale = new Vector3(0.9f, 0.9f, 1);
            }
            setCameraOffset();
            zone.SetActive(true);
        }


        public void setCenterView()
        {
            center.SetActive(true);
            left.SetActive(false);
            right.SetActive(false);
            //투수 활성화
            manager.pitcher.gameObject.SetActive(true);

        }

        public void setLeftView(float posY)
        {
            setLeftAngle();
            //투수 비활성화
            manager.pitcher.anim.GetComponent<Renderer>().enabled = false;//.gameObject.SetActive(false);
            CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, 915, -200));
            center.SetActive(false);
            left.SetActive(true);
            left.transform.localPosition = new Vector3(0, 0, 0);

            leftObj.transform.localPosition = new Vector3(0, posY, 0);
        }


        public void setRightView(float posY)
        {
            setRightAngle();
            //투수 비활성화
            manager.pitcher.anim.GetComponent<Renderer>().enabled = false;//manager.pitcher.gameObject.SetActive(false);
            CameraManager.SetCameraPos(new Vector3(BallPlayManager.BATTINGVIEW_CAMERA_INITX + 640, 915, -200));
            center.SetActive(false);
            right.SetActive(true);

            right.transform.localPosition = new Vector3(0, 0, 0);

            rightObj.transform.localPosition = new Vector3(0, posY, 0);
        }

        public IEnumerator setSideTimeChange(BackGroundManager.TimeState time) //살려살려
        {
            yield return null;

            if (Mode.stadiumType == Mode.StadiumType.Dome)
            {
                yield return null;
            }
            else
            {
                left.SetActive(true);
                left.transform.localPosition = new Vector3(-3000, 0, 0);

                right.SetActive(true);
                right.transform.localPosition = new Vector3(3000, 0, 0);


                if (time == BackGroundManager.TimeState.Day)
                {
                    if(leftObj!=null)
                        leftObj.GetComponent<SideBackGroundManager>().setDay();
                    if (rightObj != null)
                        rightObj.GetComponent<SideBackGroundManager>().setDay();
                }
                if (time == BackGroundManager.TimeState.Evening)
                {
                    if (leftObj != null)
                        leftObj.GetComponent<SideBackGroundManager>().setEvening();
                    if (rightObj != null)
                        rightObj.GetComponent<SideBackGroundManager>().setEvening();
                }
                else if (time == BackGroundManager.TimeState.Night)
                {
                    if (leftObj != null)
                        leftObj.GetComponent<SideBackGroundManager>().setNight();
                    if (rightObj != null)
                        rightObj.GetComponent<SideBackGroundManager>().setNight();
                }

                yield return new WaitForSeconds(0.5f);

                left.SetActive(false);
                right.SetActive(false);
            }

        }



        //센터 앵글 초기화
        public void setInitAngle()
        {
            _stand.transform.localPosition = new Vector3(0, 226, 0);
            //_stand.transform.localScale = new Vector3(1, 1, 1);
            _field.transform.localScale = new Vector3(1, 0.5f, 1);
            _field.transform.localPosition = new Vector3(0, 0, -0.1f);
            _runnerPosition.transform.localPosition = new Vector3(0, 0, 0);
            pitcher.initPosition();
        }


        //센터 빅홈런
        public void setBigHomerunAngle()
        {
            StartCoroutine(bigHomerunAngleChange());
        }

        //센터 빅홈런 연출
        private IEnumerator bigHomerunAngleChange()
        {
            float maxFrame = 20.0f;

            float yPos1 = 226;
            float yPos2 = 0;
            float pitcherPos = 336;
            float fieldScale = 0.5f;

            float dy1 = (113.0f - yPos1) / maxFrame;
            float dy2 = (-105.0f - yPos2) / maxFrame;
            float pdy = (240.0f - pitcherPos) / maxFrame;
            float scaleDv = (0.25f - fieldScale) / maxFrame;

            int count = 0;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _stand.transform.localPosition = new Vector3(0, yPos1 += dy1, 0);
                _field.transform.localScale = new Vector3(1, fieldScale += scaleDv, 1);
                _runnerPosition.transform.localPosition = new Vector3(0, yPos2 += dy2, 0);
                pitcher.transform.localPosition = new Vector3(0, pitcherPos += pdy, -2);
                if (++count >= maxFrame) break;

            }
        }

        //레프티 앵글 초기화
        public void setLeftAngle()
        {            
            _leftStand.transform.localPosition = new Vector3(-25, 286, 0);
            _leftField.transform.localScale = new Vector3(-1, 0.5f, 1);
            _leftField.transform.localPosition = new Vector3(165, -20, 1);//_leftField.transform.localPosition = new Vector3(165, 66, 1);
            _leftRunnerPosition.transform.localPosition = new Vector3(0, 0, -1);
        }


        //레프트 빅홈런
        public void setLeftBigHomerunAngle()
        {
            StartCoroutine(leftBigHomerunAngleChange());
        }

        //레프트 빅홈런 연출
        private IEnumerator leftBigHomerunAngleChange()
        {
            float maxFrame = 30.0f;

            float yPos1 = 286;
            float yPos2 = 0;
            float yPos3 = -20;
            float fieldScale = 0.5f;

            float dy1 = (200.0f - yPos1) / maxFrame;
            float dy2 = (-80.0f - yPos2) / maxFrame;
            float dy3 = (20.0f - yPos3) / maxFrame;
            float scaleDv = (0.3f - fieldScale) / maxFrame;
            //float scaleDv2 = (0.95f - standScale) / maxFrame;

            int count = 0;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _leftStand.transform.localPosition = new Vector3(-25, yPos1 += dy1, 0);
                _leftField.transform.localScale = new Vector3(-1, fieldScale += scaleDv, 1);
                _leftField.transform.localPosition = new Vector3(165, yPos3 += dy3, 1);
                _leftRunnerPosition.transform.localPosition = new Vector3(0, yPos2 += dy2, 0);
                if (++count >= maxFrame) break;

            }
        }


        //라이트 앵글 초기화
        public void setRightAngle()
        {
            _rightStand.transform.localPosition = new Vector3(25, 286, 0);
            _rightField.transform.localScale = new Vector3(1, 0.5f, 1);
            _rightField.transform.localPosition = new Vector3(-165, -20, 1); //_rightField.transform.localPosition = new Vector3(-165, 66, 1);
            _rightRunnerPosition.transform.localPosition = new Vector3(0, 0, -1);
        }


        //라이트 빅홈런
        public void setRightBigHomerunAngle()
        {
            StartCoroutine(rightBigHomerunAngleChange());
        }

        //레프트 빅홈런 연출
        private IEnumerator rightBigHomerunAngleChange()
        {
            float maxFrame = 30.0f;

            float yPos1 = 286;
            float yPos2 = 0;
            float fieldScale = 0.5f;
            float yPos3 = -20;

            float dy1 = (200.0f - yPos1) / maxFrame;
            float dy2 = (-80.0f - yPos2) / maxFrame;
            float scaleDv = (0.3f - fieldScale) / maxFrame;
            //float scaleDv2 = (0.95f - standScale) / maxFrame;
            float dy3 = (20.0f - yPos3) / maxFrame;

            int count = 0;
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _rightStand.transform.localPosition = new Vector3(25, yPos1 += dy1, 0);
                _rightField.transform.localScale = new Vector3(1, fieldScale += scaleDv, 1);
                _rightField.transform.localPosition = new Vector3(-165, yPos3 += dy3, 1);
                _rightRunnerPosition.transform.localPosition = new Vector3(0, yPos2 += dy2, 0);
                if (++count >= maxFrame) break;

            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////
        //타격 이펙트
        //////////////////////////////////////////////////////////////////////////////////////////////////  

        //저스트 미트 이펙트
        public void setJustMeet(bool bActive, string clip = "hitfocus", float scale = 1, float alpha = 1)
        {
            justmeet.GetComponent<Renderer>().enabled = bActive;
            justmeet.transform.localScale = new Vector3(scale * 1.15f, scale * 1.15f, 1);
            justmeet.Play(justmeet.GetClipByName(clip));
            justmeet.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, alpha);

            if (bActive == false)
            {
                justmeet.transform.localPosition = new Vector3(0, 474, -0.5f);
            }
            meetEffectTime = 0;
            bFlip = false;
        }

        public void setJustMeet2(bool bActive, string clip, Vector3 scale, Vector3 pos, Color color)
        {
            justmeet.GetComponent<Renderer>().enabled = bActive;
            justmeet.transform.localScale = scale;
            justmeet.Play(justmeet.GetClipByName(clip));
            justmeet.GetComponent<tk2dSprite>().color = color;
            justmeet.transform.localPosition = pos;
            meetEffectTime = 0;
            bFlip = false;
        }

        //저스트 미트 홈런 이펙트
        float meetEffectTime;
        bool bFlip;
        public void justMeetHomerunEffect(float x, float y, float dz)
        {
            if (justmeet.GetComponent<Renderer>().enabled == true)
            {
                if (dz > 0)
                {
                    meetEffectTime += Time.deltaTime;
                    if (meetEffectTime > 0.15f)
                    {
                        bFlip = !bFlip;
                        meetEffectTime = 0;
                        justmeet.transform.localScale = new Vector3((bFlip ? 1.2f : -1.2f), 1.4f, 1);
                    }
                    justmeet.transform.position = new Vector3(x, y, 0);

                }
                else
                {
                    setJustMeet(false);
                }
            }
        }


        public IEnumerator justMeetHomerunEffect2()
        {
            float x = CameraManager.GetCameraPos().x;
            float y = CameraManager.GetCameraPos().y + 170;// (Mathf.Abs(field.ball.firstAngle) < 25 ? 200 : 150);
            justmeet.transform.position = new Vector3(x,y,-10);
            setJustMeet(true, "flyballfocus", 1.55f, 0.6f);// 1.8f, 0.5f);        
            while (true)
            {
                if (justmeet.GetComponent<Renderer>().enabled == true)
                {
                    bFlip = !bFlip;
                    justmeet.transform.localScale = new Vector3((bFlip ? 1.55f : -1.55f), 1.55f, 0.6f);
                }
                else
                {
                    break;
                }
                yield return new WaitForSeconds(0.0334f);
            }
        }



        public void setPitchingView(int sign)
        {
            pitcherviewObj.transform.Find("field").localScale = new Vector3(sign, 1, 1);

            pitcherviewObj.transform.localPosition = new Vector3(sign * 150, 0, 0);
        }


    }
}
