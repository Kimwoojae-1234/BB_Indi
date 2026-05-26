using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BaseBall.BallPlay
{
    public class SkillEffectDisplayManager : MonoBehaviour
    {
        private static SkillEffectDisplayManager Instance_;

        private BallPlayManager manager;

        void Awake()
        {
            Instance_ = this;
            DontDestroyOnLoad(gameObject);
        }


        void OnDestroy()
        {
            Instance_ = null;
        }

        /// <summary>
        /// 플레이 제약 여부
        /// 항상, 내배팅시, 상대배팅시
        /// </summary>
        public enum PlayRestriction { Always, MyBatting, CpuBatting };

        /// <summary>
        /// 디스플레이 타입
        /// </summary>
        public enum DisplayType { NoDisplay, UI, Blur, Multiply, Additive, Shake, BlackLine, WhiteLine, Slow, ColorTint, Zoom, PitcherSpineEffect, BatterSpineEffect, BatAuraEffect, CutIn, PitcherAnim, BatterAnim, Pause};
        
        /// <summary>
        /// 이벤트 시작타임
        /// </summary>
        public enum DisplayStep { Start, Pitch, Release, Swing, Batting, Field };
        
        /// <summary>
        /// 이펙트 플레이 타입
        /// </summary>
        public enum PlayType { Set, Move, PingPong };

        /// <summary>
        /// 포커스
        /// </summary>
        public enum FocusType { MyCharacter = 0, BattingViewNofocus = 1, FieldViewNofocus = 2 };


        [System.Serializable]
        public class SkillEffectDisplay
        {
            public SkillIndex skill;
            public int ID;

            [System.Serializable]
            public class animVector
            {
                public float posX, posY;
                public float scaleX, scaleY;
            }

            [System.Serializable]
            public class effectProperty
            {
                [Header("[a.연출대기시간(부가연출에만 설정)]")]
                public float waitTime = 0.0f;
                [Header("[b.연출타입]")]
                public DisplayType displayType;
                [Header("[c.연출의 플레이 타입 (점진적인 변화가 요구되는 연출에 사용)]")]
                public PlayType playType = PlayType.Set;
                [Header("[d.연출시 포커스 대상 (블러같은 이미지 이펙트에서 어디에 포커스 맞출지 여부)]")]
                public FocusType focustType = FocusType.MyCharacter;
                [Header("[e.연출시간]")]
                [Range(0.0f, 4.0f)]
                public float duration = 1.0f;
                [Header("[f.설정값 또는 연출시 설정된 애니메이션의 스케일값]")]
                [Range(0.0f, 50.0f)]
                public float setValue = 1.0f;
                [Header("[g.시작값 설정(시작값이 있는경우만)]")]
                [Range(0.0f, 50.0f)]
                public float startValue = 0.0f;                
                [Header("[i.애니메이션 설정시 연출되는 애니메이션 이름]")]
                public string animation;
                public animVector vector;
                [Range(0.0f, 1.0f)]
                public float alpha;      
                          

            }
            
            [System.Serializable]
            public class effectStep
            {
                [Header("[1.연출의 스텝]")]
                public DisplayStep step;
                [Header("[2.연출의 제한설정]")]
                public PlayRestriction play = PlayRestriction.Always;
                [Header("[3.UI제거 여부]")]
                public bool deleteUI;
                [Header("[4.첫 연출]")]
                public effectProperty mainEffect;
                [Header("[5.이후 연출]")]
                public effectProperty[] sideEffect = null;
            }

            public effectStep[] step;
        }

        [Header("[PITCHER SKILL]")]
        [Header("1.선두타자승부")]
        public SkillEffectDisplay sun_du_ta_ja;
        [Header("2.추격본능")]
        public SkillEffectDisplay chu_gyeog_bon_neung;
        [Header("3.불꽃투혼")]
        public SkillEffectDisplay bul_kkot_tu_hon;
        [Header("4.강심장")]
        public SkillEffectDisplay kang_sim_jang;
        [Header("5.회심의일격")]
        public SkillEffectDisplay hoe_sim_il_gyeog;
        [Header("6.매혹")]
        public SkillEffectDisplay mea_hog;
        [Header("7.투수위압")]
        public SkillEffectDisplay too_soo_wi_ab;
        [Header("8.카리스마")]
        public SkillEffectDisplay chrisma;

        [Space(40)]
        [Header("[BATTER SKILL]")]
        [Header("1.매의눈")]
        public SkillEffectDisplay mae_noon;
        [Header("2.타자위압")]
        public SkillEffectDisplay ta_ja_ei_ab;
        [Header("3.강습타구")]
        public SkillEffectDisplay gang_seup_ta_gu;
        [Header("4.찬스맨")]
        public SkillEffectDisplay chance_man;
        [Header("5.번트의신")]
        public SkillEffectDisplay bunt_sin;
        [Header("6.뜬금포")]
        public SkillEffectDisplay tteun_geum_po;


        private List<SkillEffectDisplay> pSkillList = new List<SkillEffectDisplay>();
        private List<SkillEffectDisplay> bSkillList = new List<SkillEffectDisplay>();

        /// <summary>
        /// 인스턴스 초기화
        /// </summary>
        /// <param name="_manager"></param>
        public static void InitInstance(BallPlayManager _manager)
        {
            Instance_.initInstance(_manager);
        }

        /// <summary>
        /// 스킬 연출 초기화
        /// </summary>
        public static void InitSkill()
        {
            Instance_.initSkill();
        }

        /// <summary>
        /// 연출할 스킬 추가
        /// </summary>
        /// <param name="index"></param>
        public static void AddSkill(CSkill skill)// SkillIndex index)
        {
            Instance_.addSkill(skill);
        }

         
        /// <summary>
        /// 연출을 보여줌
        /// </summary>
        /// <param name="displayStep"></param>
        /// <returns>총 연출 시간을 리턴</returns>
        public static float EffectDisplay(DisplayStep displayStep, bool bPitcher, bool bVs = false)
        {
            return Instance_.effectDisplay(displayStep, bPitcher, bVs);
        }

        public static void Destroy()
        {
            if (Instance_ != null)
            {
                Destroy(Instance_.gameObject);
            }
        }


        /// <summary>
        /// 인스턴스를 초기화 한다
        /// </summary>
        /// <param name="_manager"></param>
        private void initInstance(BallPlayManager _manager)
        {
            manager = _manager;
        }

        /// <summary>
        /// 스킬 연출 초기화
        /// </summary>
        private void initSkill()
        {
            pSkillList.Clear();
            bSkillList.Clear();
        }

        /// <summary>
        /// 연출할 스킬 추가
        /// </summary>
        /// <param name="index"></param>
        private void addSkill(CSkill skill)
        {
            SkillIndex index = skill.effectIndex;
            if (index == SkillIndex.SunduKiller)
            {
                //선두타자승부
                sun_du_ta_ja.ID = skill.ID;
                sun_du_ta_ja.skill = index;
                pSkillList.Add(sun_du_ta_ja);
            }
            else if (index == SkillIndex.ChaseInstinct)
            {
                //추격본능
                chu_gyeog_bon_neung.ID = skill.ID;
                chu_gyeog_bon_neung.skill = index;
                pSkillList.Add(chu_gyeog_bon_neung);
            }
            else if (index == SkillIndex.FrameFight)
            {
                //불꽃투혼
                bul_kkot_tu_hon.ID = skill.ID;
                bul_kkot_tu_hon.skill = index;
                pSkillList.Add(bul_kkot_tu_hon);
            }
            else if (index == SkillIndex.SteelHeart)
            {
                //강심장
                kang_sim_jang.ID = skill.ID;
                kang_sim_jang.skill = index;
                pSkillList.Add(kang_sim_jang);
            }
            else if (index == SkillIndex.TenderStroke)
            {
                //회심일격
                hoe_sim_il_gyeog.ID = skill.ID;
                hoe_sim_il_gyeog.skill = index;
                pSkillList.Add(hoe_sim_il_gyeog);
            }
            else if (index == SkillIndex.Charm)
            {
                //매혹
                mea_hog.ID = skill.ID;
                mea_hog.skill = index;
                pSkillList.Add(mea_hog);
            }
            else if (index == SkillIndex.PitcherOverwhelming)
            {
                //투수위압
                too_soo_wi_ab.ID = skill.ID;
                too_soo_wi_ab.skill = index;
                pSkillList.Add(too_soo_wi_ab);
            }
            else if (index == SkillIndex.Charisma)
            {
                //카리스마
                chrisma.ID = skill.ID;
                chrisma.skill = index;
                pSkillList.Add(chrisma);
            }
            else if (index == SkillIndex.FalconEye)
            {
                //매의눈
                mae_noon.ID = skill.ID;
                mae_noon.skill = index;
                bSkillList.Add(mae_noon);
            }
            else if (index == SkillIndex.BatterOverwhelming)
            {
                //타자위압
                ta_ja_ei_ab.ID = skill.ID;
                ta_ja_ei_ab.skill = index;
                bSkillList.Add(ta_ja_ei_ab);
            }
            else if (index == SkillIndex.AssaultBall)
            {
                //강습타구
                gang_seup_ta_gu.ID = skill.ID;
                gang_seup_ta_gu.skill = index;
                bSkillList.Add(gang_seup_ta_gu);
            }
            else if (index == SkillIndex.ChanceMan)
            {
                //찬스맨
                chance_man.ID = skill.ID;
                chance_man.skill = index;
                bSkillList.Add(chance_man);
            }
            else if (index == SkillIndex.GodOfBunt)
            {
                //번트의신
                bunt_sin.ID = skill.ID;
                bunt_sin.skill = index;
                bSkillList.Add(bunt_sin);
            }
            else if (index == SkillIndex.Unexpected)
            {
                //뜬금포
                tteun_geum_po.ID = skill.ID;
                tteun_geum_po.skill = index;
                bSkillList.Add(tteun_geum_po);
            }            
        }

        /// <summary>
        /// 때에 맞추어 지정한 스킬 연출 보여줌
        /// </summary>
        /// <param name="displayStep"></param>
        private bool bVsType;
        private float effectDisplay(DisplayStep displayStep, bool bPitcher, bool bVs)
        {
            bool bUIDelete = false;
            StopAllCoroutines();

            bVsType = bVs;

            int length = (bPitcher ? pSkillList.Count : bSkillList.Count);
            float delayTime = 0;

            for (int list = 0; list < length; list++)
            {
                SkillEffectDisplay curSkillDisplay = (bPitcher ? pSkillList[list] : bSkillList[list]);
                if (curSkillDisplay != null)
                {
                    SkillEffectDisplay.effectStep curStep = null;
                    int count = curSkillDisplay.step.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (displayStep == curSkillDisplay.step[i].step)
                        {
                            curStep = curSkillDisplay.step[i];
                            if (curStep != null)
                            {
                                if (curStep.mainEffect.displayType != DisplayType.NoDisplay)
                                {
                                    if (displayPossible(curStep) == true)
                                    {
                                        if (curStep.deleteUI == true)
                                        {
                                            IngameUI.GetScoreBoard().SetActive(false);
                                            IngameUI.GetPlayerInfo().SetActive(false);
                                            bUIDelete = true;
                                        }
                                        setDefaultValue(curStep.mainEffect);
                                        if (curStep.mainEffect.duration > delayTime)
                                        {
                                            delayTime = curStep.mainEffect.duration;
                                        }
                                        StartCoroutine(display(curStep.mainEffect, curSkillDisplay.ID, curSkillDisplay.skill));

                                        if (curStep.sideEffect != null)
                                        {
                                            if (Mode.bPvpMode == false)
                                            {
                                                float sideWaitTime = 0;
                                                int numCount = curStep.sideEffect.Length;
                                                for (int j = 0; j < numCount; j++)
                                                {
                                                    setDefaultValue(curStep.sideEffect[j]);
                                                    sideWaitTime += curStep.sideEffect[j].waitTime;
                                                    float curDelay = (curStep.sideEffect[j].duration + sideWaitTime);
                                                    if (curDelay > delayTime)
                                                    {
                                                        delayTime = curDelay;
                                                    }
                                                    StartCoroutine(display(curStep.sideEffect[j], curSkillDisplay.ID, curSkillDisplay.skill, sideWaitTime));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (bUIDelete == true)
            {
                Invoke("setUI", delayTime);
            }

            return delayTime;
        }


        private void setUI()
        {
            if (manager.batter.bHitted == false)
            {
                IngameUI.GetScoreBoard().SetActive(true);
                IngameUI.GetPlayerInfo().Active();//.SetActive(true);
            }
        }

        /// <summary>
        /// 연출 코루틴
        /// </summary>
        /// <param name="curStep"></param>
        /// <returns></returns>
        private IEnumerator display(SkillEffectDisplay.effectProperty curStep, int skillID, SkillIndex index, float waitTime = 0)
        {
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            float delay = curStep.duration;
            float value = curStep.setValue;
            float startValue = curStep.startValue;
            DisplayType type = curStep.displayType;

            float startTime = 0;
            float dv = ((value - startValue) / delay) * 0.1f;

            bool bStart = true;
            if (curStep.playType == PlayType.PingPong) dv = dv * 2.0f;

            int focusIndex = (int)curStep.focustType;

            if (type == DisplayType.Blur)
            {                
                if (curStep.playType == PlayType.Set)
                {
                    //CameraManager.SetBlur2(focusIndex, true, value);
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    //CameraManager.SetBlur2(focusIndex, true, startValue);
                    while (startTime < delay)
                    {
                        if (curStep.playType == PlayType.PingPong)
                        {
                            if (bStart == true)
                            {
                                if (startTime > delay * 0.5f)
                                {
                                    dv = -dv;
                                    bStart = false;
                                }
                            }
                        }
                        //CameraManager.SetBlurSize(focusIndex, dv);
                        yield return new WaitForSeconds(0.1f);
                        startTime += 0.1f;
                    }

                }
                //CameraManager.SetBlur2(focusIndex, false);
            }
            else if (type == DisplayType.Zoom)
            {
                float curZoom = CameraManager.GetZoomFactor();
                if (curStep.playType == PlayType.Set)
                {
                    //셋
                    CameraManager.SetZoomTo(value, 0.1f);
                    yield return new WaitForSeconds(delay);
                    CameraManager.SetZoomTo(curZoom, 0.1f);
                }
                else if (curStep.playType == PlayType.Move)
                {
                    //무브
                    CameraManager.SetZoomTo(value, delay);
                    yield return new WaitForSeconds(delay);
                    CameraManager.SetZoomTo(curZoom, 0.1f);
                }
                else
                {
                    //핑퐁
                    CameraManager.SetZoomTo(value, delay*0.5f);
                    yield return new WaitForSeconds(delay * 0.5f);
                    CameraManager.SetZoomTo(curZoom, delay * 0.5f);
                }
            }
            else if (type == DisplayType.CutIn)
            {
                //UIFieldCall.CutSceneEffect(true, curStep.animation, value);
                //yield return new WaitForSeconds(delay);
                //UIFieldCall.CutSceneEffect(false, curStep.animation, value);
            }
            else if (type == DisplayType.PitcherSpineEffect)
            {
                Vector3 pos = new Vector3(curStep.vector.posX, curStep.vector.posY, -0.01f);
                Vector3 sacle = new Vector3(curStep.vector.scaleX, curStep.vector.scaleY, 1);
                manager.pitcher.AuraEffect(true, curStep.animation, pos, sacle);
                yield return new WaitForSeconds(delay);
                manager.pitcher.AuraEffect(false, curStep.animation, pos, sacle);
            }
            else if (type == DisplayType.BatterSpineEffect)
            {
                Vector3 pos = new Vector3(curStep.vector.posX, curStep.vector.posY, -0.01f);
                Vector3 sacle = new Vector3(curStep.vector.scaleX, curStep.vector.scaleY, 1);
                manager.batter.AuraEffect(true, curStep.animation, pos, sacle);
                yield return new WaitForSeconds(delay);
                manager.batter.AuraEffect(false, curStep.animation, pos, sacle);
            }
            else if (type == DisplayType.PitcherAnim)
            {
                manager.pitcher.AnimEffect(true, curStep.animation);
                yield return new WaitForSeconds(delay);
                manager.pitcher.AnimEffect(false, "");
            }
            else if (type == DisplayType.BatAuraEffect)
            {
                Vector3 pos = new Vector3(curStep.vector.posX, curStep.vector.posY, -0.01f);
                Vector3 sacle = new Vector3(curStep.vector.scaleX, curStep.vector.scaleY, 1);
                manager.batter.BatAuraEffect(true, curStep.animation, pos, sacle);
                yield return new WaitForSeconds(delay);
                manager.batter.BatAuraEffect(false, curStep.animation, pos, sacle);
            }
            else if (type == DisplayType.BatterAnim)
            {
                manager.batter.AnimEffect(true, curStep.animation);
                yield return new WaitForSeconds(delay);
                manager.batter.AnimEffect(false, "");
            }
            else if (type == DisplayType.Shake)
            {
                CameraManager.CameraShake(delay, value);
                yield return new WaitForSeconds(delay);
                CameraManager.CameraPositionInit();
            }
            else if (type == DisplayType.BlackLine)
            {
                manager.battingview.setJustMeet(true, "hitfocus", 1.2f, curStep.alpha);
                yield return new WaitForSeconds(delay);
                manager.battingview.setJustMeet(false);
            }
            else if (type == DisplayType.WhiteLine)
            {
                manager.battingview.setJustMeet(true, "hitfocus2", 1.2f, curStep.alpha);
                yield return new WaitForSeconds(delay);
                manager.battingview.setJustMeet(false);
            }
            else if (type == DisplayType.Pause)
            {
                Mode.bPauseGame = true;
                yield return new WaitForSeconds(delay);
                Mode.bPauseGame = false;

                /*
                if (curStep.playType != PlayType.Set)
                {
                    float curScale = (startValue <= 0 ? 0.1f : startValue);
                    float dt = (value <= 0 ? 0.2f : value);
                    while (curScale < 1.0f)
                    {
                        Time.timeScale = curScale;
                        float curDt = dt * curScale;
                        yield return new WaitForSeconds(curDt);
                        curScale += dt;
                    }
                    Time.timeScale = 1;
                }*/
                


            }
            else if (type == DisplayType.Multiply)
            {
                if (curStep.playType == PlayType.Set)
                {
                    //CameraManager.SetScreenOverlay(focusIndex, true, ScreenOverlay.OverlayBlendMode.Multiply, value);
                    yield return new WaitForSeconds(delay);
                    float curTime = 0;
                    float dv2 = (2 - value) / 0.1f * 0.05f;
                    while(curTime < 0.1f)
                    {
                        //CameraManager.SetOverlayIntensityDV(focusIndex, dv2);
                        yield return new WaitForSeconds(0.05f);
                        curTime += 0.05f;
                    }
                }
                else
                {
                    //CameraManager.SetScreenOverlay(focusIndex, true, ScreenOverlay.OverlayBlendMode.Multiply, startValue);
                    while (startTime < delay)
                    {
                        if (curStep.playType == PlayType.PingPong)
                        {
                            if (bStart == true)
                            {
                                if (startTime > delay * 0.5f)
                                {
                                    dv = -dv;
                                    bStart = false;
                                }
                            }
                        }
                        //CameraManager.SetOverlayIntensityDV(focusIndex, dv);
                        yield return new WaitForSeconds(0.1f);
                        startTime += 0.1f;
                    }
                }
                //CameraManager.SetScreenOverlay(focusIndex, false);
            }
            else if (type == DisplayType.Additive)
            {
                if (curStep.playType == PlayType.Set)
                {
                    //CameraManager.SetScreenOverlay(focusIndex, true, ScreenOverlay.OverlayBlendMode.Additive, value);
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    //CameraManager.SetScreenOverlay(focusIndex, true, ScreenOverlay.OverlayBlendMode.Additive, startValue);
                    while (startTime < delay)
                    {
                        if (curStep.playType == PlayType.PingPong)
                        {
                            if (bStart == true)
                            {
                                if (startTime > delay * 0.5f)
                                {
                                    dv = -dv;
                                    bStart = false;
                                }
                            }
                        }
                        //CameraManager.SetOverlayIntensityDV(focusIndex, dv);
                        yield return new WaitForSeconds(0.1f);
                        startTime += 0.1f;
                    }
                }
                //CameraManager.SetScreenOverlay(focusIndex, false);
            }
            else if (type == DisplayType.Slow)
            {
                if (value < 0.01f) value = 0.01f;
                Time.timeScale = value;
                float realDelay = delay * value;
                yield return new WaitForSeconds(realDelay);
                Time.timeScale = 1.0f;
            }
            else if (type == DisplayType.UI)
            {
                if (bVsType == false)
                {
                    bool bPitcherSkill = checkPitcherSkill(index);
                    
                    if ((manager.bMyTurn == true && bPitcherSkill == false) || (manager.bMyTurn == false && bPitcherSkill == true))                        
                    {
                        //내스킬
#if _Test_Local
                        int rank = UnityEngine.Random.Range(1, 5);
#else
                        int rank = manager.pitcher.pPitcher.getSkillRank(index);
#endif
                        IngameUI.GetMySkillUI().init(skillID, rank);
                    }
                    else
                    {
                        //상대스킬
#if _Test_Local
                        int rank = UnityEngine.Random.Range(1, 5);
#else
                        int rank = manager.batter.pBatter.getSkillRank(index);
#endif
                        IngameUI.GetCpuSkillUI().init(skillID, rank);
                    }
                }
                bVsType = false;
            }
            else if (type == DisplayType.ColorTint)
            {
            /*    if (curStep.playType == PlayType.Set)
                {
                    CameraManager.SetTint(true, curStep.colorValue);
                    yield return new WaitForSeconds(delay);
                }
                else
                {
                    CameraManager.SetTint(true, Color.white);
                    float r = 1, g = 1, b = 1;
                    float dvRatio = 0.1f * (curStep.playType == PlayType.PingPong ? 2 : 1);

                    float dr = (curStep.colorValue.r - r) * dvRatio;
                    float dg = (curStep.colorValue.g - g) * dvRatio;
                    float db = (curStep.colorValue.b - b) * dvRatio;

                    while (startTime < delay)
                    {
                        if (curStep.playType == PlayType.PingPong)
                        {
                            if (bStart == true)
                            {
                                if (startTime > delay * 0.5f)
                                {
                                    dr = -dr;
                                    dg = -dg;
                                    db = -db;
                                    bStart = false;
                                }
                            }
                        }

                        r += dr;
                        g += dg;
                        b += db;


                        CameraManager.SetTintValue(new Color(r, g, b));
                        yield return new WaitForSeconds(0.1f);
                        startTime += 0.1f;
                    }
                }*/
                CameraManager.SetTint(false, Color.white);
            }

        }

        /// <summary>
        /// 현시점에서 연출 가능한지 여부
        /// </summary>
        /// <param name="curStep"></param>
        /// <returns></returns>
        private bool displayPossible(SkillEffectDisplay.effectStep curStep)
        {
            if (curStep.play == PlayRestriction.Always)
            {
                return true;
            }
            else
            {
                if (curStep.play == PlayRestriction.MyBatting)
                {
                    return manager.bMyTurn;
                }
                else
                {
                    return (!manager.bMyTurn);
                }
            }
        }


        /// <summary>
        /// 해당스킬이 피처스킬인지 여부
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool checkPitcherSkill(SkillIndex index)
        {
            if (index < SkillIndex.SpecialCatch) return true;
            return false;
        }

        /// <summary>
        /// 스킬 시전 플레이어 네임
        /// </summary>
        /// <param name="bPitcher"></param>
        /// <returns></returns>
        private string getName(bool bPitcher)
        {
            return (bPitcher ? manager.pitcher.pPitcher.getName() : manager.batter.pBatter.getName());
        }



        private void setDefaultValue(SkillEffectDisplay.effectProperty type)
        {
            /*
            if (type.colorValue == new Color(0,0,0,0))
            {
                //디폴트 컬러
                type.colorValue = Color.white;
            }*/

            if (type.alpha == 0)
            {
                type.alpha = 1;
            }

            //
            if (type.vector.scaleX == 0)
            {
                //디폴트 스케일
                type.vector.scaleX = 1;
            }

            if (type.vector.scaleY == 0)
            {
                //디폴트 스케일
                type.vector.scaleY = 1;
            }

            
            if (type.duration <= 0)
            {
                //플레이 시간
                type.duration = 1;
            }

            if (type.displayType == DisplayType.CutIn
              || type.displayType == DisplayType.PitcherAnim || type.displayType == DisplayType.BatterAnim
              || type.displayType == DisplayType.BatterSpineEffect || type.displayType == DisplayType.PitcherSpineEffect
              || type.displayType == DisplayType.Slow
              || type.displayType == DisplayType.Blur)
            {
                if (type.setValue <= 0)
                {
                    //디폴트 밸류
                    type.setValue = 1.0f;
                }
            }


            if (type.displayType == DisplayType.Shake)
            {
                if (type.setValue <= 0)
                {
                    //디폴드 셰이크
                    type.setValue = 10.0f;
                }
            }




            //Blur, Multiply, Additive, Shake, BlackLine, WhiteLine, Slow, ColorTint, Zoom, PitcherSpineEffect, BatterSpineEffect, CutIn, PitcherAnim, BatterAnim
        }



        public void saveData()
        {
#if UNITY_EDITOR
            //Debug.Log("==============>>데이터를 저장");

            
            if (File.Exists(Application.persistentDataPath + "/SkillEffectData.dat") == true)
            {
                File.Delete(Application.persistentDataPath + "/SkillEffectData.dat");
            }
                        
            List<SkillEffectDisplay> temp = new List<SkillEffectDisplay>();

            //예외처리
            for (int i = 0; i < 14; i++)
            {
                SkillEffectDisplay tempDisplay = sun_du_ta_ja;

                if (i == 0) tempDisplay = sun_du_ta_ja;
                else if (i == 1) tempDisplay = chu_gyeog_bon_neung;
                else if (i == 2) tempDisplay = bul_kkot_tu_hon;
                else if (i == 3) tempDisplay = kang_sim_jang;
                else if (i == 4) tempDisplay = hoe_sim_il_gyeog;
                else if (i == 5) tempDisplay = mea_hog;
                else if (i == 6) tempDisplay = too_soo_wi_ab;
                else if (i == 7) tempDisplay = chrisma;

                else if (i == 8) tempDisplay = mae_noon;
                else if (i == 9) tempDisplay = ta_ja_ei_ab;
                else if (i == 10) tempDisplay = gang_seup_ta_gu;
                else if (i == 11) tempDisplay = chance_man;
                else if (i == 12) tempDisplay = bunt_sin;
                else if (i == 13) tempDisplay = tteun_geum_po;

                if (tempDisplay.step == null || tempDisplay.step.Length == 0)
                {
                    tempDisplay.step = new SkillEffectDisplay.effectStep[1];
                    tempDisplay.step[0] = new SkillEffectDisplay.effectStep();
                }

                if (tempDisplay.step[0].mainEffect == null)
                {
                    tempDisplay.step[0].mainEffect = new SkillEffectDisplay.effectProperty();
                    tempDisplay.step[0].mainEffect.displayType = DisplayType.NoDisplay;
                }

                if (tempDisplay.step[0].sideEffect == null || tempDisplay.step[0].sideEffect.Length == 0)
                {
                    tempDisplay.step[0].sideEffect = new SkillEffectDisplay.effectProperty[1];
                    tempDisplay.step[0].sideEffect[0] = new SkillEffectDisplay.effectProperty();
                    tempDisplay.step[0].sideEffect[0].displayType = DisplayType.NoDisplay;
                }

                temp.Add(tempDisplay);
            }            

            string filePath = Application.persistentDataPath + "/SkillEffectData.dat";

            Debug.Log(Application.persistentDataPath);
            
            if (File.Exists(filePath) == true)
            {
                File.Delete(filePath);
            }
            
            try
            {
                FileStream file = File.Create(filePath);
                //Serialize to xml
                DataContractSerializer bf = new DataContractSerializer(temp.GetType());
                MemoryStream streamer = new MemoryStream();

                //Serialize the file
                bf.WriteObject(streamer, temp);
                streamer.Seek(0, SeekOrigin.Begin);

                //Save to disk
                file.Write(streamer.GetBuffer(), 0, streamer.GetBuffer().Length);

                // Close the file to prevent any corruptions
                file.Close();
                string result = XElement.Parse(Encoding.ASCII.GetString(streamer.GetBuffer()).Replace("\0", "")).ToString();
                Debug.Log("Serialized Result: " + result);

            }
            catch (System.Exception e) {
                Debug.LogError("Procedure: Load failed with message: " + e.Message);   
            }

            
#endif 
        }


        public void loadData()
        {
#if UNITY_EDITOR
            //Debug.Log("==============>>데이터를 업데이트");

            string filePath = Application.persistentDataPath + "/SkillEffectData.dat";

            if (File.Exists(filePath) == false)
            {
                Debug.Log("file doesn't exist");
            }

            try
            {
                FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<SkillEffectDisplay>));

                List<SkillEffectDisplay> temp = serializer.ReadObject(fileStream) as List<SkillEffectDisplay>;
            
                if (temp != null)
                {
                    if (temp.Count > 0)
                    {
                        sun_du_ta_ja = temp[0];
                        chu_gyeog_bon_neung = temp[1];
                        bul_kkot_tu_hon = temp[2];
                        kang_sim_jang = temp[3];
                        hoe_sim_il_gyeog = temp[4];
                        mea_hog = temp[5];
                        too_soo_wi_ab = temp[6];
                        chrisma = temp[7];

                        mae_noon = temp[8];
                        ta_ja_ei_ab = temp[9];
                        gang_seup_ta_gu = temp[10];
                        chance_man = temp[11];
                        bunt_sin = temp[12];
                        tteun_geum_po = temp[13];
                    }
                }
                fileStream.Close();

                UnityEditor.PrefabUtility.ReplacePrefab(gameObject, UnityEditor.PrefabUtility.GetPrefabParent(gameObject), UnityEditor.ReplacePrefabOptions.ConnectToPrefab);

            }
            catch (System.Exception e)
            {
                Debug.LogError("Procedure: Load failed with message: " + e.Message);
            }
#endif
        }
    }
}