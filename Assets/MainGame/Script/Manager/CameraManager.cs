using UnityEngine;
using System.Collections;

namespace BaseBall.BallPlay
{
    public class CameraManager : MonoBehaviour
    {
        private static CameraManager Instance_;

        //카메라 오브젝트
        public tk2dCamera _camera;           //메인 카메라(배팅뷰)        
        public tk2dCamera batterCamera;     //타자 카메라(메인카메라에 종속)
        public tk2dCamera fieldActiveCamera;//필드 카메라    
        public GameObject fieldOrigin;

        private Camera _fieldActiveCamera;  //필드 카메라의 카메라 컴퍼넌트

        public tk2dCamera zoomCamera;       //필드의 줌카메라

        //카메라 이미지 이펙트 효과
        //private ColorCorrectionCurves colorInvert;
        //private Blur [] blur = new Blur[3];
        //private ScreenOverlay [] screenOverlay = new ScreenOverlay[3];
        //private AntialiasingAsPostEffect anti;
        //private MotionBlur fieldMotionBlur;
        //private Blur fieldBlur;
        //private MotionBlur motionBlur;       

        private AmplifyMotionEffect fieldMotionBlur, zoomMotionBlur;
        //private AmplifyMotionCamera fieldMotionBlur, zoomMotionBlur;

        private Vector3 cameraInitPos;

        public SpriteRenderer tempTint;
        
        void Awake()
        {
            Instance_ = this;
        }
        
        void OnDestroy()
        {
            Instance_ = null;
        }

        // Use this for initialization
        void Start()
        {
            _camera = transform.Find("camera").gameObject.GetComponent<tk2dCamera>();
            //dof = camera.gameObject.GetComponent<DepthOfFieldScatter>();
            //bloom = camera.gameObject.GetComponent<FastBloom>();
            //colorInvert = _camera.gameObject.GetComponent<ColorCorrectionCurves>();
            
            //메인카메라 카메라 이펙트 세팅
            //blur[0] = _camera.gameObject.GetComponent<Blur>();
            //screenOverlay[0] = _camera.gameObject.GetComponent<ScreenOverlay>();

            //배터카메라 카메라 이펙트 세팅
            //blur[1] = batterCamera.gameObject.GetComponent<Blur>();
            //screenOverlay[1] = batterCamera.gameObject.GetComponent<ScreenOverlay>();

            //필드카메라 카메라 이펙트 세팅
            //blur[2] = fieldActiveCamera.gameObject.GetComponent<Blur>();
            //screenOverlay[2] = fieldActiveCamera.gameObject.GetComponent<ScreenOverlay>();

            //anti = camera.gameObject.GetComponent<AntialiasingAsPostEffect>();

            //motionBlur = camera.gameObject.GetComponent<MotionBlur>();
            //fieldMotionBlur = fieldCamera.gameObject.GetComponent<MotionBlur>();
            //fieldBlur = fieldCamera.gameObject.GetComponent<Blur>();
            //anti.enabled = false;

            //fieldMotionBlur = fieldActiveCamera.gameObject.GetComponent<AmplifyMotionEffect>();
            //zoomMotionBlur = zoomCamera.gameObject.GetComponent<AmplifyMotionEffect>();

            //fieldMotionBlur.enabled = false;
            //zoomMotionBlur.enabled = false;

            overlayChange = false;
            bZoomChange = false;
            //alphaCamera.gameObject.SetActive(false);
            cameraInitPos = Vector3.zero;
            cameraPosition(cameraInitPos);
            fieldActiveCameraInit();


        }

        // Update is called once per frame
        void Update()
        {
            /*
            if (overlayChange == true)
            {
                overlayChangeUpdate();
            }*/

            if (bZoomChange == true)
            {
                zoomFrame();
            }

            if (bCameraAngleChange == true)
            {
                angleFrame();
            }
        }

        public static CameraManager GetInstance()
        {
            return Instance_;
        }

        public static int GetCameraState()
        {
            return Instance_.cameraState;
        }

        /*
        public static void SetDepthOfField(bool bActive, float dis = 0, float size = 0, float aperture = 0, float maxSize = 0, Transform trans = null)
        {
            Instance_.setDepthOfField(bActive, dis, size, aperture, maxSize, trans);
        }

        public static void SetBlur(bool bActive, float blursize = 2, int downSample = 0, int iteration = 1)
        {
            Instance_.setBlur(bActive, blursize, downSample, iteration);
        }*/

        /*
        public static void SetBlurOff(float delay)
        {
            Instance_.setBlurOff(delay);
        }*/

        /// <summary>
        /// 블러 효과 세팅
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="blursize"></param>
        /// <param name="downSample"></param>
        /// <param name="iteration"></param>
        /*public static void SetBlur2(int index, bool bActive, float blursize = 2, int downSample = 0, int iteration = 1)
        {
            Instance_.setBlur(index, bActive, blursize, downSample, iteration);
        }*/


        /// <summary>
        /// 블러 사이즈 세팅
        /// </summary>
        /// <param name="dv"></param>
        /// <returns></returns>
        /*public static bool SetBlurSize(int index, float dv)
        {
            return Instance_.setBlurSize(index, dv);
        }*/


        public static void SetMotionBlur(bool bActive)
        {
            //Instance_.motionBlur.enabled = bActive;
        }

        /*
        public static void SetBloom(bool bActive, float threadhold = 0, float intensity = 0, float blursize = 0)
        {
            Instance_.setBloom(bActive, threadhold, intensity, blursize);
        }*/

        /*public static void SetInvert(bool bActive, bool bTurn = false)
        {
            Instance_.setInvert(bActive, bTurn);
        }*/


        /// <summary>
        /// 스크린 오버레이 세팅
        /// </summary>
        /// <param name="bActive"></param>
        /// <param name="blendMode"></param>
        /// <param name="intensity"></param>
        /*public static void SetScreenOverlay(int index, bool bActive, ScreenOverlay.OverlayBlendMode blendMode = ScreenOverlay.OverlayBlendMode.Multiply, float intensity = 1.0f)
        {
            Instance_.setScreenOverlay(index, bActive, blendMode, intensity);
        }*/


        /*public static void SetOverlayIntensityDV(int index, float intensity)
        {
            Instance_.setOverlayIntensityDV(index, intensity);
        }*/

        /*
        /// <summary>
        /// 스크린 오버레이 강도 세팅
        /// </summary>
        /// <param name="bIncrease"></param>
        /// <param name="intensity"></param>
        /// <param name="threadhold"></param>
        public static void SetScreenOverlayIntensity(bool bIncrease, float intensity, float threadhold)
        {
            Instance_.setScreenOverlayIntensity(bIncrease, intensity, threadhold);
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blendMode"></param>
        /// <param name="bIncrease"></param>
        /// <param name="intensity"></param>
        /// <param name="threadhold"></param>
        /// <param name="speed"></param>
        /*public static void SetScreenOverlay2(int index, ScreenOverlay.OverlayBlendMode blendMode, bool bIncrease, float intensity, float threadhold, float speed = 1.0f)
        {
            Instance_.setScreenOverlay2(index, blendMode, bIncrease, intensity, threadhold, speed);
        }*/

        /// <summary>
        /// 레이어 변환
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="name"></param>
        public static void ChangeLayersRecursively(Transform trans, string name)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(name);
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, name);
            }
        }

        /// <summary>
        /// 메인 카메라 줌 팩터값 얻어오기
        /// </summary>
        /// <returns></returns>
        public static float GetZoomFactor()
        {
            return Instance_._camera.ZoomFactor;
        }

        /// <summary>
        /// 메인카메라 줌 팩터값 세팅
        /// </summary>
        /// <param name="scale"></param>
        public static void SetZoomFactor(float scale)
        {
            Instance_._camera.ZoomFactor = scale;
        }

        /// <summary>
        /// 메인 카메라 줌 스케일 변환
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="remainTime"></param>
        public static void SetZoomTo(float scale, float remainTime)
        {
            Instance_.setZoomTo(scale, remainTime);
        }

        /// <summary>
        /// 필드 카메라  줌팩터 세팅
        /// </summary>
        /// <param name="scale"></param>
        public static void SetFieldZoomFactor(float scale)
        {
            Instance_.fieldActiveCamera.ZoomFactor = scale;
        }

        
        /// <summary>
        /// 카메라 이동
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="remainTime"></param>
        public static void SetPositionTo(Vector3 dst, float remainTime)
        {
            Instance_.setPositionTo(dst, remainTime);
        }

        /// <summary>
        /// 카메라 위치 초기화
        /// </summary>
        public static void CameraPositionInit()
        {
            Instance_.cameraPosition(Instance_.cameraInitPos);
        }

        public static void SetCameraInitPos(Vector3 pos)
        {
            Instance_.cameraInitPos = pos;
            Instance_.cameraPosition(pos);
        }



        /// <summary>
        /// 카메라 위치 세팅
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="bLocal"></param>
        public static void SetCameraPos(Vector3 pos, bool bLocal = false)
        {
            //Debug.Log("================>>pos = "+ pos);
            if (bLocal == false)
            {
                Instance_.transform.position = pos;
            }
            else
            {
                Instance_.transform.localPosition = pos;
            }
        }

        /// <summary>
        /// 카메라 위치값 얻어오기
        /// </summary>
        /// <param name="bLocal"></param>
        /// <returns></returns>
        public static Vector3 GetCameraPos(bool bLocal = false)
        {
            if (bLocal == false)
            {
                return Instance_.transform.position;
            }
            else
            {
                return Instance_.transform.localPosition;
            }
        }

        /// <summary>
        /// 카메라 흔들기
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="gab"></param>
        public static void CameraShake(float duration, float gab)
        {
            Instance_.cameraShake(duration, gab);
        }


        public static void FieldCameraShake(float duration, float gab)
        {
            Instance_.setFieldShake(duration, gab);
        }

        /// <summary>
        /// 카메라 상태 바꾸기
        /// </summary>
        /// <param name="state"></param>
        /// <param name="cameraX"></param>
        /// <param name="cameraY"></param>
        public static void ChangeCamera(int state, float cameraX, float cameraY)
        {
            Instance_.changeCamera(state, cameraX, cameraY);
        }


        /// <summary>
        /// 타자카메라 세팅
        /// </summary>
        /// <param name="bActive"></param>
        public static void SetBatterCamera(bool bActive)
        {
            Instance_.batterCamera.gameObject.SetActive(bActive);
        }

        /// <summary>
        /// 타자카메라 세팅
        /// </summary>
        /// <param name="bActive"></param>
        public static void SetBatterCameraZoomFactor(float scale)
        {
            Instance_.batterCamera.ZoomFactor = scale;
        }

        /// <summary>
        /// 카메라 레이어 세팅
        /// </summary>
        /// <param name="layer"></param>
        public static void SetCameraLayer(string layer)
        {
            Instance_.setCameraLayer(layer);
        }


        /// <summary>
        /// 필드카메라 초기 앵글 설정
        /// </summary>
        /// <param name="angle"></param>
        public static void SetActiveCameraInitAngle(float angle)
        {
            Instance_.setActiveCameraInitAngle(angle);
        }

        /// <summary>
        /// 필드 카메라 앵글 변화 설정
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="remainTime"></param>
        public static void SetActiveCameraDstAngle(float angle, float remainTime)
        {
            Instance_.setActiveCameraDstAngle(angle, remainTime);
        }

        /// <summary>
        /// 필드 카레라의 경계설정용 레이
        /// </summary>
        /// <param name="bTopLeft"></param>
        /// <returns></returns>
        public static Ray FieldCameray(bool bTopLeft)
        {
            return Instance_.screenPointToRay(bTopLeft);
        }


        public static Ray FieldCameray(int type)
        {
            return Instance_.screenPointToRay(type);
        }


        /*
        //뎁스오브필드
        private void setDepthOfField(bool bActive, float dis, float size, float aperture, float maxSize, Transform trans)
        {
            dof.enabled = bActive;
            if (bActive == true)
            {
                dof.focalLength = dis;
                dof.focalSize = size;
                dof.aperture = aperture;
                dof.maxBlurSize = maxSize;
                dof.focalTransform = trans;
            }

        }*/

        /*
        private void setFocalSize(float dv)
        {
            dof.focalSize += dv;
        }*/

        /*
        private void setApertuer(float dv)
        {
            if (dof.aperture >= 0)
            {
                dof.aperture += dv;
            }
            else
            {
                dof.enabled = false;
            }
        }*/

        
        //블러 효과
        /*private void setBlur(int index, bool bActive, float blursize, int downSample, int iteration)
        {

            //////Debug.Log("=================>>set blur :" + bActive);
            blur[index].enabled = bActive;
            //anti.enabled = !bActive;
            if (bActive == true)
            {
                blur[index].blurSize = blursize;
                blur[index].downsample = downSample;
                blur[index].blurIterations = iteration;
            }

        }*/

        //블러 사이즈
        /*private bool setBlurSize(int index, float dv)
        {
            if (blur[index].enabled == true)
            {
                blur[index].blurSize += dv;
                if (blur[index].blurSize < 0)
                {
                    setBlur(index, false, 0, 0, 0);
                    return false;
                }
            }
            return true;
        }*/

        /*
        //블룸효과
        private void setBloom(bool bActive, float threadhold, float intensity, float blursize)
        {
            bloom.enabled = bActive;
            //anti.enabled = !bActive;
            if (bActive == true)
            {
                bloom.threshhold = threadhold;
                bloom.intensity = intensity;
                bloom.blurSize = blursize;
            }
        }*/

        
        //인버트 효과
        bool invertAvail;
        /*private void setInvert(bool bActive, bool bTurn)
        {
            colorInvert.enabled = bActive;
            //anti.enabled = !bActive;
            invertAvail = bActive;

            if (bTurn == true)
            {
                StartCoroutine(setInvertTurn(1));
            }
        }*/

        //multiply효과
        /*private void setScreenOverlay(int index, bool bActive, ScreenOverlay.OverlayBlendMode blendMode = ScreenOverlay.OverlayBlendMode.Multiply, float intensity = 1.0f)
        {
            screenOverlay[index].enabled = bActive;
            //anti.enabled = !bActive;
            if (bActive == true)
            {
                screenOverlay[index].blendMode = blendMode;
                screenOverlay[index].intensity = intensity;
            }
        }*/

        /*
        //스크린 오버레이 강도
        private void setScreenOverlayIntensity(bool bIncrease, float intensity, float threadhold)
        {
            ////Debug.Log("================>>>setScreenOverlayIntensity");
            screenOverlay.intensity = intensity;
            if (bIncrease == true)
            {
                if (intensity > threadhold)
                {
                    overlayChange = false;
                    setScreenOverlay(false);
                    return;
                }
            }
            else
            {
                if (intensity < threadhold)
                {
                    overlayChange = false;
                    setScreenOverlay(false);
                    return;
                }
            }
            overlayChange = true;
        }*/

        //스크린 오버레이 세팅
        bool overlayChange = false;
        bool overlayIncrease;
        float overlayThreadhold;
        float overlaySpeed;
        /*private void setScreenOverlay2(int index, ScreenOverlay.OverlayBlendMode blendMode, bool bIncrease, float startIntensity, float threadhold, float speed = 1.0f)
        {
            overlayIncrease = bIncrease;
            screenOverlay[index].enabled = true;
            screenOverlay[index].blendMode = blendMode;
            overlayChange = true;
            screenOverlay[index].intensity = startIntensity;
            overlayThreadhold = threadhold;
            overlaySpeed = speed;
        }*/

        /// <summary>
        /// 스크린 오버레이 강도 설정
        /// </summary>
        /// <param name="intensity"></param>
        /*private void setOverlayIntensityDV(int index, float intensity)
        {
            screenOverlay[index].intensity += intensity;
        }*/

        /*
        //오버레이 강도 변화 업데이트
        private void overlayChangeUpdate()
        {
            if (overlayIncrease == true)
            {
                screenOverlay.intensity += (overlaySpeed * Time.deltaTime);
                if (screenOverlay.intensity > overlayThreadhold)
                {
                    overlayChange = false;
                    setScreenOverlay(false);
                }
            }
            else
            {
                screenOverlay.intensity -= (overlaySpeed * Time.deltaTime);
                if (screenOverlay.intensity < overlayThreadhold)
                {
                    overlayChange = false;
                    setScreenOverlay(false);
                }
            }
        }*/

        
        /*private IEnumerator setInvertTurn(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (invertAvail == true)
            {
                _camera.ZoomFactor = 2;
                bool active = colorInvert.enabled;
                colorInvert.enabled = !active;

                StartCoroutine(setInvertTurn(0.1f));
            }
        }*/

        private int cameraState;

        //카메라 상태 변화
        private void changeCamera(int state, float cameraX, float cameraY)
        {
            cameraState = state;
            int layerMask;
            if (state == BallPlayManager._BATTINGVIEW)
            {                
                ////Debug.Log("=============>>_BATTINGVIEW");
                IngameUI.GetFieldUI().SetActive(false);
                setActiveCameraInitAngle(InitAngleX);
                fieldActiveCamera.gameObject.SetActive(false);
                _camera.gameObject.SetActive(true);
                batterCamera.gameObject.SetActive(true);
                layerMask = (1 << LayerMask.NameToLayer("BATTINGVIEW_LAYER")) | (1 << LayerMask.NameToLayer("BATTINGFIELD_LAYER"));
                _camera.GetComponent<Camera>().cullingMask = layerMask;

            }
            else ///_FIELDVIEW
            {
                IngameUI.GetFieldUI().SetActive(true);
                FieldZoomState = false;
                _camera.gameObject.SetActive(false);
                zoomCamera.gameObject.SetActive(false);                
                fieldActiveCamera.gameObject.SetActive(true);
                layerMask = (1 << LayerMask.NameToLayer("FIELDINGVIEW_LAYER")) | (1 << LayerMask.NameToLayer("FIELDOBJECT_LAYER"));
                fieldActiveCamera.GetComponent<Camera>().cullingMask = layerMask;
                                
                //setMotionBlurDelay(0.4f);
            }
            transform.position = new Vector3(cameraX, cameraY, -200);
        }

        public void CameraOff()
        {
            _camera.gameObject.SetActive(false);
            zoomCamera.gameObject.SetActive(false);
            fieldActiveCamera.gameObject.SetActive(false);
        }

        //카메라 레이어 세팅
        private void setCameraLayer(string layer)
        {
            int layerMask = 1 << LayerMask.NameToLayer(layer);
            Camera unityCam = _camera.GetComponent<Camera>();
            unityCam.cullingMask = layerMask;
        }

        //메인 카메라 줌 변화 세팅
        private bool bZoomChange = false;
        private float dstZoom, zoomDV;
        private void setZoomTo(float scale, float remainTime)
        {
            dstZoom = scale;
            zoomDV = (dstZoom - _camera.ZoomFactor) / remainTime;
            bZoomChange = true;

        }

        //메인 카메라 줌 프레임
        private void zoomFrame()
        {
            _camera.ZoomFactor += (zoomDV * Time.deltaTime);
            batterCamera.ZoomFactor = _camera.ZoomFactor; //setFocusZoom(camera.ZoomFactor);
            if (zoomDV > 0)
            {
                if (_camera.ZoomFactor > dstZoom)
                {
                    batterCamera.ZoomFactor = _camera.ZoomFactor = dstZoom;
                    bZoomChange = false;
                }
            }
            else
            {
                if (_camera.ZoomFactor < dstZoom)
                {
                    batterCamera.ZoomFactor = _camera.ZoomFactor = dstZoom;
                    bZoomChange = false;
                }
            }

        }

        //메인 카메라 위치 이동
        private void setPositionTo(Vector3 dst, float remainTime)
        {
            TweenPosition.Begin(gameObject, remainTime, dst);
        }

        //메인 카메라 위치 초기화
        private void cameraPosition(Vector3 pos)
        {   
            /*
            _camera.transform.localPosition = pos;*/
            TweenPosition.Begin(_camera.gameObject, 0.01f, pos);

            //fieldCamera.transform.localPosition = new Vector3(0, 0, 0);
        }

        
        //카메라 쉐이크
        float shakeStep, shakeTime;
        const float _ShakeTimeGab = 0.025f;
        private void cameraShake(float duration, float gab)
        {
            shakeStep = 0;
            shakeTime = 0;
            _camera.transform.localPosition = cameraInitPos + new Vector3(0, gab, 0);
            StartCoroutine(shake(duration, gab));
        }
                

        //쉐이크
        private IEnumerator shake(float duration, float gab)
        {
            while (shakeTime < duration)
            {
                if (shakeStep == 0)
                {
                    TweenPosition.Begin(_camera.gameObject, _ShakeTimeGab, cameraInitPos + new Vector3(-gab, 0, 0));
                }
                else if (shakeStep == 1)
                {
                    TweenPosition.Begin(_camera.gameObject, _ShakeTimeGab, cameraInitPos + new Vector3(gab, 0, 0));
                }
                else
                {
                    TweenPosition.Begin(_camera.gameObject, _ShakeTimeGab, cameraInitPos + new Vector3(0, gab, 0));
                }
                yield return new WaitForSeconds(_ShakeTimeGab);
                shakeTime += _ShakeTimeGab;
                shakeStep = (shakeStep+1) % 3;
            }
            yield return new WaitForSeconds(0.02f);
            cameraPosition(cameraInitPos);
        }


        private void setFieldShake(float duration, float gab)
        {
            StartCoroutine(fieldShake(duration, gab));
        }


        private IEnumerator fieldShake(float duration, float gab)
        {
            float shakeTime1 = 0;
            int shakeStep1 = 0;

            while (shakeTime1 < duration)
            {
                if (shakeStep1 == 0)
                {
                    TweenPosition.Begin(fieldOrigin, _ShakeTimeGab, new Vector3(-gab, 0, 0));
                }
                else if (shakeStep1 == 1)
                {
                    TweenPosition.Begin(fieldOrigin, _ShakeTimeGab, new Vector3(gab, 0, 0));
                }
                else
                {
                    TweenPosition.Begin(fieldOrigin, _ShakeTimeGab, new Vector3(0, gab, 0));
                }
                yield return new WaitForSeconds(_ShakeTimeGab);
                shakeTime1 += _ShakeTimeGab;
                shakeStep1 = (shakeStep1 + 1) % 3;
            }
            yield return new WaitForSeconds(0.02f);
            fieldOrigin.transform.localPosition = Vector3.zero;
        }

        /*
        public static void SetFieldMotionBlur(float delay)
        {
            Instance_.setFieldMotionBlur(delay);
        }

        private void setFieldMotionBlur(float delay)
        {
            StartCoroutine(motionBlur(delay));
        }

        private IEnumerator motionBlur(float delay)
        {
            yield return new WaitForSeconds(0.05f);

            fieldMotionBlur.enabled = true;

            fieldMotionBlur.blurAmount = 0.65f;
            yield return new WaitForSeconds(delay);

            fieldMotionBlur.enabled = false;

        }
        */

        public static void SetMotionBlurDelay(float delay)
        {
            Instance_.setMotionBlurDelay(delay);
        }


        private void setFieldMotionBlur(bool bActive)
        {
            fieldMotionBlur.enabled = bActive;            
            //zoomMotionBlur.enabled = bActive;
        }

        private void setMotionBlurDelay(float delay)
        {
            StartCoroutine(motionBlurDelay(delay));
        }

        private IEnumerator motionBlurDelay(float delay)
        {
            setFieldMotionBlur(true);
            yield return new WaitForSeconds(delay);
            setFieldMotionBlur(false);
        }


        
        //액티브 카메라(필드)
        public static bool FieldZoomState = false;
        public static float FieldActiveAngleX = -30.0f;  //-12.5
        public static float FieldActivePosY = -200;  //-120
        private float FieldActivePosZ = -400;
        private bool bCameraAngleChange;
        private float dTheta, destAngle;
        private const float InitAngleX = -30;

        private const float ZoomAngleX = -40.0f;
        private float lastAngleX;

        //필드 카메라 위치 초기화
        private void fieldActiveCameraInit()
        {
            bCameraAngleChange = false;
            fieldActiveCamera.transform.localPosition = new Vector3(0, FieldActivePosY, FieldActivePosZ);
            fieldActiveCamera.transform.localEulerAngles = new Vector3(FieldActiveAngleX, 0, 0);
            _fieldActiveCamera = fieldActiveCamera.GetComponent<Camera>();
            _fieldActiveCamera.fieldOfView = 60;

            zoomCamera.transform.localPosition = new Vector3(0, -250, -100);
            zoomCamera.transform.localEulerAngles = new Vector3(ZoomAngleX, 0, 0);
            zoomCamera.GetComponent<Camera>().fieldOfView = 60;
            zoomCamera.ZoomFactor = 1f;

            FieldZoomState = false;


            top = _fieldActiveCamera.pixelHeight * 0.2f;
            bottom = _fieldActiveCamera.pixelHeight * 0.8f;
            left = _fieldActiveCamera.pixelWidth *0.2f;
            right = _fieldActiveCamera.pixelWidth * 0.8f;
        }

        //필드카메라 초기 앵글 세팅
        private void setActiveCameraInitAngle(float angle)
        {
            FieldActiveAngleX = angle;  //-12.5
            lastAngleX = FieldActiveAngleX;
            FieldActivePosY = FieldActiveAngleX * 10;// -200;  //-120
            FieldActivePosZ = -400;
            fieldActiveCamera.transform.localPosition = new Vector3(0, FieldActivePosY, FieldActivePosZ);
            fieldActiveCamera.transform.localEulerAngles = new Vector3(FieldActiveAngleX, 0, 0);
        }

        //필드카메라 각도 변화 세팅
        private void setActiveCameraDstAngle(float angle, float remainTime)
        {
            if (angle != FieldActiveAngleX)
            {
                fieldActiveCamera.transform.localPosition = new Vector3(0, FieldActivePosY, FieldActivePosZ);
                fieldActiveCamera.transform.localEulerAngles = new Vector3(FieldActiveAngleX, 0, 0);

                destAngle = angle;
                dTheta = (angle - FieldActiveAngleX) / remainTime;

                bCameraAngleChange = true;
            }
        }

        //각도 변화 프레임
        private void angleFrame()
        {
            FieldActiveAngleX += (dTheta * Time.deltaTime);
            if ((dTheta < 0 && FieldActiveAngleX < destAngle) ||
                (dTheta > 0 && FieldActiveAngleX > destAngle))
            {
                FieldActiveAngleX = destAngle;
                bCameraAngleChange = false;
            }
            FieldActivePosY = (FieldActiveAngleX * 10);
            fieldActiveCamera.transform.localPosition = new Vector3(0, FieldActivePosY, FieldActivePosZ);
            fieldActiveCamera.transform.localEulerAngles = new Vector3(FieldActiveAngleX, 0, 0);
        }

        //경계설정용 레이
        private Ray screenPointToRay(bool bTopLeft)
        {
            return _fieldActiveCamera.ScreenPointToRay(new Vector3(bTopLeft ? 0 : _fieldActiveCamera.pixelWidth, _fieldActiveCamera.pixelHeight, 0));
        }


        //경계설정용 레이
        float top, left, right, bottom;
        private Ray screenPointToRay(int type)
        {            
            if (type == 0)
            {
                return _fieldActiveCamera.ScreenPointToRay(new Vector3(left, top));
            }
            else if (type == 1)
            {
                return _fieldActiveCamera.ScreenPointToRay(new Vector3(left, bottom));

            }
            else if (type == 2)
            {
                return _fieldActiveCamera.ScreenPointToRay(new Vector3(right, top));
            }
            else
            {
                return _fieldActiveCamera.ScreenPointToRay(new Vector3(right, bottom));
            }
        }


        //필드 줌 설정
        private void setFieldZoomActive(int posIndex, float dstFactor)
        {
            if (FieldZoomState == false)
            {
                //zoomMotionBlur.enabled = true;
                FieldZoomState = true;
                fieldActiveCamera.gameObject.SetActive(false);
                zoomCamera.gameObject.SetActive(true);
                bCameraAngleChange = false;
                lastAngleX = (posIndex < CPlayer._LEFTFIELDER ? -5 : -15);
                zoomCamera.ZoomFactor = 0.7f;
                FieldActiveAngleX = ZoomAngleX;
                float dv = (dstFactor - 0.7f) / 40.0f;
                StartCoroutine(zoomCameraZoomFactor(dv, dstFactor));
            }
        }

        private IEnumerator zoomCameraZoomFactor(float dv, float max)
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                zoomCamera.ZoomFactor += dv;
                if (zoomCamera.ZoomFactor > max)
                {
                    //zoomMotionBlur.enabled = false;           
                    //UIFieldUI.SetFocus(false);
                    break;
                }
            }

        }


        private void setFieldZoomDeActive()
        {
            if (FieldZoomState == true)
            {
                StopCoroutine("zoomCameraZoomFactor");
                FieldZoomState = false;
                fieldActiveCamera.gameObject.SetActive(true);
                zoomCamera.gameObject.SetActive(false);
                setActiveCameraInitAngle(lastAngleX);
            }
        }


        public static void SetZoomActive(int posIndex, float dstFactor)
        {
            Instance_.setFieldZoomActive(posIndex, dstFactor);
        }

        public static void SetZoomDeActive()
        {
            Instance_.setFieldZoomDeActive();
        }


        public static void FieldShockWave(Vector3 pos, float radius, float speed, float amp)
        {
            Instance_.fieldShockWave(pos, radius, speed, amp);
        }


        private void fieldShockWave(Vector3 pos, float radius, float speed, float amp)
        {
            ShockWave wave;
            if (FieldZoomState == true)
            {
                wave = zoomCamera.gameObject.AddComponent<ShockWave>();
                wave.StartItCustom(zoomCamera.GetComponent<Camera>(), pos, radius, speed, amp);
            }
            else
            {
                wave = fieldActiveCamera.gameObject.AddComponent<ShockWave>();
                wave.StartItCustom(fieldActiveCamera.GetComponent<Camera>(),pos, radius, speed, amp);
            }
            
        }




        public static Vector3 fieldWorldToScreenPoint(Vector3 pos)
        {
            return Instance_._fieldActiveCamera.WorldToScreenPoint(pos);
        }

        public static Camera GetFieldCamera()
        {
            return Instance_._fieldActiveCamera;
        }



        public static void SetTint(bool bActive, Color color)
        {
            Instance_.setTint(bActive, color);
        }

        public static void SetTintValue(Color color)
        {
            Instance_.setTintValue(color);
        }


        private void setTint(bool bActive, Color color)
        {
            tempTint.gameObject.SetActive(bActive);
            if (bActive == true)
            {
                setTintValue(color);
            }
        }

        private void setTintValue(Color color)
        {
            color.a = 0.4f;
            tempTint.color = color;
        }



    }
}