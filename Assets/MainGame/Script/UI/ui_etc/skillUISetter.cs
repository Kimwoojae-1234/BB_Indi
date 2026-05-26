using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace BaseBall.BallPlay
{
    public class skillUISetter : MonoBehaviour
    {
        public enum BgType
        {
            None,
            Earth,
            Line,
            Circle,
            Star,
            Tornado,
            Light
        }

        public GameObject _active;
        public bool bLeftPosition;
        public GameObject front, back;
        public SkillSlot skillStot;
        public UISprite[] line;

        public GameObject backSpr;

        public Camera captureCamera;
        public SpriteRenderer catureTexture;

        private SkeletonAnimation anim;
        private GameObject bg = null;
        private BgType bgType;


        private Transform textPos;

        private readonly float backUITime = 0.2f;
        private readonly float spineWaitTime = 0.2f;
        private readonly float remainTime = 1.5f;


        private readonly int leftInitPos = -1156;
        private readonly int rightInitPos = 1156;

        private readonly int leftPos = -641;
        private readonly int rightPos = 641;



        private IEnumerator coroutine = null;


        private bool bVsState;

        public void init(int skillID, int rank, bool bVs = false)
        {
            bVsState = bVs;
            if (_active.activeSelf == true)
            {
                //중첩 발동된경우                
                StartCoroutine(startAnim2(skillID, rank));
            }
            else
            {
                //애니메이션 시작
                coroutine = startAnim(skillID, rank);
                StartCoroutine(coroutine);
            }
        }

        private bool bLegendSkill;
        private int getSkillID(int realSkillID)
        {
            bLegendSkill = false;
            if (((realSkillID / 100) % 100) > 0)
            {
                bLegendSkill = true;
            }

            return (realSkillID / 10000) * 10000 + (realSkillID % 100);
        }

        
        private IEnumerator startAnim(int realSkillID, int rank)
        {
            int skillID = getSkillID(realSkillID);

            if (skillID == (int)SkillID.gang_chul_shoulder)
            {
                //강철어깨
                yield break;
            }

            //뒷배경 세팅
            bgSetting(skillID);
            
            rank = Mathf.Clamp(rank, 1, 5);
            //슬롯세팅
            WebConnector.CardSkill skillinfo = new WebConnector.CardSkill();
            skillinfo.skillId = skillID;
            skillinfo.rank = rank;
            OldCode.SkillData curSkillData = new OldCode.SkillData(skillinfo);
            skillStot.SetSkillSlotSkillUI(curSkillData);
            skillStot.gameObject.SetActive(false);

            //라인세팅
            int lineIndex = (rank == 3 ? 2 : rank);
            line[0].spriteName = "skill_line_top" + lineIndex;
            line[1].spriteName = "skill_line_down" + lineIndex;

            //스킬 애니메이션 세팅
            textPos = null;
            ////Debug.Log("=============>> skillID = " + skillID);
            GameObject skillAnim = Util.Load("MainGame/prefabs/skillUI/skill" + skillID, front.transform, Vector3.zero);
            if (skillAnim == null)
            {                
                yield break;
            }
            skillAnim.transform.localScale = new Vector3(100, 100, 100);
            textPos = skillAnim.transform.Find("Root");//.FindChild("root").FindChild("text_01").FindChild("position").transform;
            if (textPos != null)
            {
                Transform skillPos = textPos.Find("root").Find("text_01").Find(bLeftPosition?"left":"right").transform;
                skillStot.transform.parent = skillPos;
                skillStot.transform.localScale = Vector3.one;
                skillStot.transform.localPosition = Vector3.zero;
                skillStot.transform.localEulerAngles = Vector3.zero;
            }
            anim = skillAnim.GetComponent<SkeletonAnimation>();

            //초기 포지션 세팅
            transform.localPosition = new Vector3(bLeftPosition == true ? leftInitPos : rightInitPos, 0, 0);
            gameObject.GetComponent<UIPanel>().alpha = 1.0f;            
            _active.SetActive(true);
            backSpr.SetActive(true);
            backSpr.GetComponent<UISprite>().alpha = 0.0f;
            TweenAlpha.Begin(backSpr, 0.2f, 1);    

            //Util.SetTweenerStart(gameObject);
            TweenPosition.Begin(gameObject, backUITime, new Vector3(bLeftPosition == true ? leftPos : rightPos, 0, 0));
            if (bg != null)
            {
                UITweener tweener = bg.transform.Find("spr").GetComponent<UITweener>();
                if (tweener != null)
                {
                    tweener.enabled = true;
                }

            }
            if (textPos == null)
            {
                TweenPosition.Begin(skillStot.gameObject, spineWaitTime - 0.05f, new Vector3((bLeftPosition == true ?55:-55), 52, 0));
            }
            yield return new WaitForSeconds(spineWaitTime);
            
            //anim.state.ClearTracks();
            anim.skeleton.SetToSetupPose();
            if (bLegendSkill == true)
            {
                //레전드용
                //Debug.Log("==============================>> 레전드용 스킬 발동 ID :  " + realSkillID);
                anim.state.SetAnimation(0, (bLeftPosition == true ? "PLAY_L_" : "PLAY_R_") + realSkillID, false);
            }
            else
            {
                anim.state.SetAnimation(0, (bLeftPosition == true ? "PLAY_L" : "PLAY_R"), false);
            }
            anim.timeScale = 1.0f;

            yield return new WaitForSeconds(0.2f);
            skillStot.gameObject.SetActive(true);

            if (bVsState == true)
            {
                //VS모드시
                //반짝 효과
                yield return new WaitForSeconds(0.2f);
                setCature(captureCamera, Color.white);
                UITweener tween = catureTexture.gameObject.GetComponent<UITweener>();
                tween.ResetToBeginning();
                tween.PlayForward();
                yield return new WaitForSeconds(0.8f);
                catureTexture.sprite = null;
                catureTexture.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(remainTime);


            TweenPosition.Begin(gameObject, 0.2f, new Vector3(bLeftPosition == true ? leftInitPos : rightInitPos, 0, 0));
            TweenAlpha.Begin(backSpr, 0.2f, 0);

            yield return new WaitForSeconds(0.5f);

            destroyObject();

            
            backSpr.SetActive(false);
            catureTexture.gameObject.SetActive(false);
            _active.SetActive(false);
        }

        private IEnumerator startAnim2(int realSkillID, int rank)
        {
            yield return new WaitForSeconds(0.6f);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            float alpha = 1.0f;
            while (alpha > 0)
            {
                alpha -= 0.1f;
                gameObject.GetComponent<UIPanel>().alpha = alpha;
                if(anim!= null) anim.skeleton.A = alpha;
                yield return new WaitForSeconds(0.02f);            
            }  
            destroyObject();
            _active.SetActive(false);
            yield return new WaitForEndOfFrame();
            coroutine = startAnim(realSkillID, rank);
            StartCoroutine(coroutine);
        }


        private void destroyObject()
        {
            skillStot.transform.parent = front.transform;
            skillStot.transform.localPosition = new Vector3(bLeftPosition?-250:250, 52, 0);
            skillStot.transform.localEulerAngles = Vector3.zero;
            skillStot.gameObject.SetActive(false);

            if (anim != null)
            {
                Destroy(anim.gameObject);
                anim = null;
            }
            if (bg != null)
            {
                Destroy(bg.gameObject);
                bg = null;
            }
        }


        private void bgSetting(int skillID)
        {
            bg = null;
            bgType  = BgType.None;
            switch (skillID)
            {
                case 20011: //강습타구 
                case 20012: //찬스맨
                case 10012: //닥터K
                case 10013: //필승의지
                    bgType = BgType.Line;
                    break;
                case 20014: //뜬금포
                case 10007: //회심의 일격
                case 20003: //쇠그물수비
                    bgType = BgType.Earth;
                    break;
                case 20009: //매의눈        
                case 10002: //견제왕
                    bgType = BgType.Circle;
                    break;
                case 10003: //선두타자승부
                case 20008: //주루센스
                    bgType = BgType.Star;
                    break;
                case 10008: //매혹
                case 20005: //도발꾼
                    bgType = BgType.Tornado;
                    break;
                case 10011: //카리스마
                case 20004: //레이저
                    bgType = BgType.Light;
                    break;
                default: 
                    //타자위압
                    //번트신공
                    //추격본능 
                    //불꽃투혼 
                    //강심장 
                    //투수위압
                    //제5의 내야수
                    //수비형포수
                    //질주본능
                    //철벽수비
                    //특급수비
                    bgType = BgType.None;
                    break;
            }
            if (bgType != BgType.None)
            {
                //Debug.Log("name = " + "bg_" + bgType.ToString());
                bg = Util.Load("MainGame/prefabs/skillUI/bg/bg_" + bgType.ToString(), back.transform, Vector3.zero);
                bg.transform.localScale = new Vector3((bLeftPosition ? 1 : -1), 1, 1);
            }
        }


        /// <summary>
        /// 지는 연출
        /// </summary>
        public void lose(GameObject burnEffect)
        {
            StartCoroutine(loseAnim(burnEffect));
        }

        /// <summary>
        /// 지는 연출 애니메이션
        /// </summary>
        /// <returns></returns>
        private IEnumerator loseAnim(GameObject burnEffect)
        {
            yield return new WaitForSeconds(0.8f);

            float color = 1.0f;
            while (color > 0.5f)
            {
                anim.skeleton.R = color;
                anim.skeleton.G = color;
                anim.skeleton.B = color;
                color -= 0.0334f;
                yield return new WaitForSeconds(0.02f);
            }
            
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            //그레이 스케일로 캡쳐
            setCature(captureCamera,Color.gray);
            catureTexture.color = new Color(1, 1, 1, 1);
            burnEffect.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            destroyObject();            
            
            yield return new WaitForSeconds(0.5f);

            //사라짐
            TweenAlpha.Begin(backSpr, 0.2f, 0);
            TweenAlpha.Begin(catureTexture.gameObject, 0.2f, 0);
            TweenAlpha.Begin(gameObject, 0.2f, 0);
            burnEffect.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            catureTexture.sprite = null;
            catureTexture.gameObject.SetActive(false);
            backSpr.SetActive(false);
            _active.SetActive(false);
        }

        public void setCature(Camera camera, Color color)
        {
            ////Debug.Log("============>>텍스쳐 캡쳐");
            camera.gameObject.SetActive(true);


            if (color == null)
            {
                //원본
                catureTexture.sprite = Util.MakeCaptureSprite(camera, 640, 360);
            }            
            else 
            {
                //솔리드 칼라
                catureTexture.sprite = Util.MakeSolidColorCapture(camera, color, 640, 360);
            }
            
            camera.gameObject.SetActive(false);
            catureTexture.gameObject.SetActive(true);

            
        }
    }
}