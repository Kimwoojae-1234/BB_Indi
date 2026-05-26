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
    public class fieldSkillDisplayManager : MonoBehaviour
    {
        private static fieldSkillDisplayManager Instance_;

        private BallPlayManager manager;
        private Field field;        


        /// <summary>
        /// 이벤트 시작타임
        /// </summary>
        public enum FieldDisplayStep { Start, Catch, Throwing, CatcherTiming, OutFielderTiming, RunnerTiming };

        /// <summary>
        /// 포커스 캐릭터
        /// </summary>
        public enum FieldFocusPoint { Ball, Fielder, Runner };

        /// <summary>
        /// 연출 타입
        /// </summary>
        public enum FieldDisplayType { NoDisplay, UI, Blur, Multiply, Shake, BlackLine, WhiteLine, Slow, Zoom, FielderSpineEffect, RunnerSpineEffect, CutIn, Pause };


        [System.Serializable]
        public class FieldSkillEffectDisplay
        {   
            
            [System.Serializable]
            public class fieldEffectProperty
            {
                [Header("[a.연출대기시간(부가연출에만 설정)]")]
                public float waitTime = 0.0f;
                [Header("[b.연출타입]")]
                public FieldDisplayType displayType;
                [Header("[c.연출시 포커스 대상 (블러같은 이미지 이펙트에서 어디에 포커스 맞출지 여부)]")]
                public FieldFocusPoint focusPoint;
                [Header("[d.연출시간]")]
                [Range(0.0f, 3.0f)]
                public float duration = 1.0f;
                [Header("[e.설정값 또는 연출시 설정된 애니메이션의 스케일값]")]
                [Range(0.0f, 50.0f)]
                public float setValue = 1.0f;
                [Header("[f.애니메이션 설정시 연출되는 애니메이션 이름]")]
                public string animation;
                [Range(0.0f, 1.0f)]
                public float alpha;  
            }

            [System.Serializable]
            public class fieldEffectStep
            {
                [Header("[1.연출의 스텝]")]
                public FieldDisplayStep step;
                [Header("[2.UI제거 여부]")]
                public bool deleteUI;
                [Header("[3.첫 연출]")]
                public fieldEffectProperty mainEffect;
                [Header("[4.이후 연출]")]
                public fieldEffectProperty[] sideEffect = null;
            }

            public fieldEffectStep[] step;

            private SkillIndex _skill;
            private Fielder _curFielder;
            private Runner _curRunner;
            private bool _bFielder;

            public SkillIndex skill
            {
                get
                {
                    return _skill;
                }
                set
                {
                    _skill = value;
                }
            }

            public Fielder curFielder
            {
                get
                {
                    return _curFielder;
                }
                set
                {
                    _curFielder = value;
                }
            }

            public Runner curRunner
            {
                get
                {
                    return _curRunner;
                }
                set
                {
                    _curRunner = value;
                }
            }

            public bool bFielder
            {
                get
                {
                    return _bFielder;
                }
                set
                {
                    _bFielder = value;
                }
            }
        }

        [Header("[SKILL]")]
        [Header("1.제5의내야수")]
        public FieldSkillEffectDisplay Je5_NeaYasu;     //제5의 내야수
        [Header("2.견제왕")]
        public FieldSkillEffectDisplay GyeonJeWang;     //견제왕
        [Header("3.철벽수비")]
        public FieldSkillEffectDisplay CheolByeogSubi;  //철벽수비
        [Header("4.특급송구")]
        public FieldSkillEffectDisplay TeuggeubSongGu;  //특급송구
        [Header("5.쇠그물수비")]
        public FieldSkillEffectDisplay SoeGeumulSubi;   //쇠그물수비
        [Header("6.레이저송구")]
        public FieldSkillEffectDisplay LaserSongGu;     //레이저송구
        [Header("7.수비형포수")]
        public FieldSkillEffectDisplay SubihyeongPosu;  //수비형포수
        [Header("8.질주본능")]
        public FieldSkillEffectDisplay JiljuBonneung;   //질주본능
        [Header("9.주루센스")]
        public FieldSkillEffectDisplay JuluSense;       //주루센스



        private List<FieldSkillEffectDisplay> skillList = new List<FieldSkillEffectDisplay>();

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
        public static void AddSkill(GameObject player, CPlayer fieldPlayer, SkillIndex index)
        {
            Instance_.addSkill(player, fieldPlayer, index);
        }

        /// <summary>
        /// 연출할 스킬 제거
        /// </summary>
        /// <param name="player"></param>
        /// <param name="index"></param>
        public static void RemoveSkill(CPlayer fieldPlayer, SkillIndex index)
        {
            Instance_.removeSkill(fieldPlayer, index);
        }



        /// <summary>
        /// 연출을 보여줌
        /// </summary>
        /// <param name="displayStep"></param>
        /// <returns>총 연출 시간을 리턴</returns>
        public static float EffectDisplay(FieldDisplayStep displayStep)
        {
            return Instance_.effectDisplay(displayStep);
        }

        /// <summary>
        /// 인스턴스를 초기화 한다
        /// </summary>
        /// <param name="_manager"></param>
        private void initInstance(BallPlayManager _manager)
        {
            manager = _manager;
            field = _manager.field;
        }

        /// <summary>
        /// 스킬 연출 초기화
        /// </summary>
        private void initSkill()
        {
            lastIndex = SkillIndex.AssaultBall;
            skillList.Clear();
        }

        /// <summary>
        /// 연출할 스킬 추가
        /// </summary>
        /// <param name="index"></param>
        private SkillIndex lastIndex = SkillIndex.AssaultBall;
        private void addSkill(GameObject player, CPlayer fieldPlayer, SkillIndex index)
        {            
            int ID = fieldPlayer.getSkillValue(index).ID;

            if (lastIndex != index)
            {
                if (ID == 10001)
                {
                    setPlayer(Je5_NeaYasu, true, player, index);     //제5의 내야수
                }
                else if (ID == 10002)
                {
                    setPlayer(GyeonJeWang, true, player, index);     //견제왕
                }
                else if (ID == 20001)
                {
                    setPlayer(CheolByeogSubi, true, player, index);  //철벽수비
                }
                else if (ID == 20002 || ID == 21102)
                {
                    setPlayer(TeuggeubSongGu, true, player, index);  //특급송구, 평화송구
                }
                else if (ID == 20003)
                {
                    setPlayer(SoeGeumulSubi, true, player, index);   //쇠그물수비
                    effectDisplay(FieldDisplayStep.OutFielderTiming);
                }
                else if (ID == 20004)
                {
                    setPlayer(LaserSongGu, true, player, index);     //레이저 송구
                }
                else if (ID == 20006 || ID == 21106)
                {
                    setPlayer(SubihyeongPosu, true, player, index);   //수비형포수, 안방마님
                    effectDisplay(FieldDisplayStep.CatcherTiming);
                }
                else if (ID == 20007 || ID == 21107)
                {
                    setPlayer(JiljuBonneung, false, player, index);   //질주본능, 바람의아들
                    effectDisplay(FieldDisplayStep.RunnerTiming);
                }
                else if (ID == 20008)
                {
                    setPlayer(JuluSense, false, player, index);       //주루센스
                    effectDisplay(FieldDisplayStep.RunnerTiming);
                }

                lastIndex = index;
                
            }
        }


        private void removeSkill(CPlayer fieldPlayer, SkillIndex index)
        {            
            int ID = fieldPlayer.getSkillValue(index).ID;

            if (ID == 10001)
            {
                //제5의 내야수
                if (skillList.Contains(Je5_NeaYasu) == true) skillList.Remove(Je5_NeaYasu);
            }
            else if (ID == 10002)
            {
                //견제왕
                if (skillList.Contains(GyeonJeWang) == true) skillList.Remove(GyeonJeWang);
            }
            else if (ID == 20001)
            {
                //철벽수비
                if (skillList.Contains(CheolByeogSubi) == true) skillList.Remove(CheolByeogSubi);
            }
            else if (ID == 20002 || ID == 21102)
            {
                //특급송구, 평화송구
                if (skillList.Contains(TeuggeubSongGu) == true) skillList.Remove(TeuggeubSongGu);
            }
            else if (ID == 20003)
            {
                //쇠그물수비
                if (skillList.Contains(SoeGeumulSubi) == true) skillList.Remove(SoeGeumulSubi);
            }
            else if (ID == 20004)
            {
                //레이저송구
                if (skillList.Contains(LaserSongGu) == true) skillList.Remove(LaserSongGu);
            }
            else if (ID == 20006 || ID == 21106)
            {
                //수비형포수, 안방마님
                if (skillList.Contains(SubihyeongPosu) == true) skillList.Remove(SubihyeongPosu);
            }
            else if (ID == 20007 || ID == 21107)
            {
                //질주본능, 바람의아들
                if (skillList.Contains(JiljuBonneung) == true) skillList.Remove(JiljuBonneung);
            }
            else if (ID == 20008)
            {
                //주루센스
                if (skillList.Contains(JuluSense) == true) skillList.Remove(JuluSense);
            }

        }


        private void setPlayer(FieldSkillEffectDisplay display, bool bFielder, GameObject player, SkillIndex index)
        {
            display.skill = index;
            display.bFielder = bFielder;
            if (bFielder == true)
            {
                display.curFielder = player.GetComponent<Fielder>();
            }
            else
            {
                display.curRunner = player.GetComponent<Runner>();
            }
            skillList.Add(display); 
        }



        /// <summary>
        /// 연출을 보여줌
        /// </summary>
        /// <param name="displayStep"></param>
        /// <returns>총 연출 시간을 리턴</returns>
        public float effectDisplay(FieldDisplayStep displayStep)
        {
            bool bUIDelete = false;
            
            int length = skillList.Count;
            float delayTime = 0;

            for (int list = 0; list < length; list++)
            {
                FieldSkillEffectDisplay curSkillDisplay = skillList[list];

                if (curSkillDisplay != null)
                {
                    FieldSkillEffectDisplay.fieldEffectStep curStep = null;
                    int count = curSkillDisplay.step.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (displayStep == curSkillDisplay.step[i].step)
                        {
                            curStep = curSkillDisplay.step[i];
                            break;
                        }
                    }

                    if (curStep != null)
                    {
                        if (curStep.mainEffect.displayType != FieldDisplayType.NoDisplay)
                        {
                            if (curStep.deleteUI == true)
                            {
                                //UI제거
                                bUIDelete = true;
                            }
                            if (curStep.mainEffect.duration > delayTime)
                            {
                                delayTime = (curStep.mainEffect.duration + curStep.mainEffect.waitTime);
                            }
                            StartCoroutine(display(curSkillDisplay, curStep.mainEffect));
                            if (curStep.sideEffect != null)
                            {
                                float sideWaitTime = 0;
                                int numCount = curStep.sideEffect.Length;
                                for (int j = 0; j < numCount; j++)
                                {
                                    //setDefaultValue(curStep.sideEffect[j]);
                                    sideWaitTime += curStep.sideEffect[j].waitTime;
                                    float curDelay = (curStep.sideEffect[j].duration + sideWaitTime);
                                    if (curDelay > delayTime)
                                    {
                                        delayTime = curDelay;
                                    }
                                    StartCoroutine(display(curSkillDisplay, curStep.sideEffect[j]));
                                }
                            }
                        }
                    }

                }
            }

            if (bUIDelete == true)
            {
                //UI복원
                Invoke("setUI", delayTime);
            }

            return delayTime;

        }

        /// <summary>
        /// ui 복원
        /// </summary>
        private void setUI()
        {
            
        }

        /// <summary>
        /// 연출 코루틴
        /// </summary>
        /// <param name="curStep"></param>
        /// <returns></returns>
        private IEnumerator display(FieldSkillEffectDisplay display,  FieldSkillEffectDisplay.fieldEffectProperty curStep)
        {
            float waitTime = curStep.waitTime;

            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }

            //디스플레이
            CPlayer player = null;            
            bool bFielder = display.bFielder;
            SkillIndex skillIndex = display.skill;
            if (bFielder == true) player = display.curFielder.pFielder;
            else player = display.curRunner.pRunner;

            //스텝
            FieldDisplayType type = curStep.displayType;
            float value = curStep.setValue;
            float duration = curStep.duration;

            if (bFielder == true)
            {
                if (display.curFielder.actState == FielderAction._NOTHING_STATE)
                {
                    //Debug.Log("===========================>>야수 아무것도 아닌 상태가 와서 연출 스탑");
                    yield break;
                }
            }

            if (type == FieldDisplayType.UI)
            {
                CSkill _skill = player.getSkillValue(skillIndex);
                int ID = _skill.ID;
#if _Skill_Display
                int rank = UnityEngine.Random.Range(1, 5);
#else
                int rank = _skill.rank;
#endif
                if ((bFielder == false && manager.bMyTurn == true) || (bFielder == true && manager.bMyTurn == false))
                {
                    //내측                  
                    IngameUI.GetMySkillUI().init(ID, rank);
                }
                else
                {
                    //상대측
                    IngameUI.GetCpuSkillUI().init(ID, rank);
                }
            }
            else if (type == FieldDisplayType.Zoom)
            {
                if (field.ball.bBallDeadState == false)
                {
                    if (curStep.focusPoint == FieldFocusPoint.Runner)
                    {
                        field.ball.setRunnerFocus(display.curRunner.arrayIndex);
                    }
                    else if (curStep.focusPoint == FieldFocusPoint.Fielder)
                    {
                        field.ball.setFielderFocus(display.curFielder.posIndex);
                    }
                    field.setZoomTo(value, duration);
                }
            }
            else if (type == FieldDisplayType.Pause)
            {
                field.setTimeScale(0);
                yield return new WaitForSeconds(duration);
                field.setTimeScale(Field.INIT_TIME_SCALE);
            }
            else if(type == FieldDisplayType.WhiteLine)
            {
                IngameUI.GetFieldUI().SetLineEffect(true);
                yield return new WaitForSeconds(duration);
                IngameUI.GetFieldUI().SetLineEffect(false);
            }
        }









        public void saveData()
        {
#if UNITY_EDITOR
            //Debug.Log("==============>>데이터를 저장");

            
            if (File.Exists(Application.persistentDataPath + "/FieldEffectData.dat") == true)
            {
                File.Delete(Application.persistentDataPath + "/FieldEffectData.dat");
            }
                        
            List<FieldSkillEffectDisplay> temp = new List<FieldSkillEffectDisplay>();

            //예외처리
            for (int i = 0; i < 9; i++)
            {
                FieldSkillEffectDisplay tempDisplay = null;

                if (i == 0) tempDisplay = Je5_NeaYasu;
                else if (i == 1) tempDisplay = GyeonJeWang;
                else if (i == 2) tempDisplay = CheolByeogSubi;
                else if (i == 3) tempDisplay = TeuggeubSongGu;
                else if (i == 4) tempDisplay = SoeGeumulSubi;
                else if (i == 5) tempDisplay = LaserSongGu;
                else if (i == 6) tempDisplay = SubihyeongPosu;
                else if (i == 7) tempDisplay = JiljuBonneung;
                else if (i == 8) tempDisplay = JuluSense;

                if (tempDisplay.step == null || tempDisplay.step.Length == 0)
                {
                    tempDisplay.step = new FieldSkillEffectDisplay.fieldEffectStep[1];
                    tempDisplay.step[0] = new FieldSkillEffectDisplay.fieldEffectStep();
                }

                if (tempDisplay.step[0].mainEffect == null)
                {
                    tempDisplay.step[0].mainEffect = new FieldSkillEffectDisplay.fieldEffectProperty();
                    tempDisplay.step[0].mainEffect.displayType = FieldDisplayType.NoDisplay;
                }

                if (tempDisplay.step[0].sideEffect == null || tempDisplay.step[0].sideEffect.Length == 0)
                {
                    tempDisplay.step[0].sideEffect = new FieldSkillEffectDisplay.fieldEffectProperty[1];
                    tempDisplay.step[0].sideEffect[0] = new FieldSkillEffectDisplay.fieldEffectProperty();
                    tempDisplay.step[0].sideEffect[0].displayType = FieldDisplayType.NoDisplay;
                }

                tempDisplay.skill = SkillIndex.AssaultBall;

                temp.Add(tempDisplay);
            }            

            string filePath = Application.persistentDataPath + "/FieldEffectData.dat";

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

            string filePath = Application.persistentDataPath + "/FieldEffectData.dat";

            if (File.Exists(filePath) == false)
            {
                Debug.Log("file doesn't exist");
                return;
            }

            try
            {
                FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<FieldSkillEffectDisplay>));

                List<FieldSkillEffectDisplay> temp = serializer.ReadObject(fileStream) as List<FieldSkillEffectDisplay>;
            
                if (temp != null)
                {
                    if (temp.Count > 0)
                    {                        
                        Je5_NeaYasu = temp[0];
                        GyeonJeWang = temp[1];
                        CheolByeogSubi = temp[2];
                        TeuggeubSongGu = temp[3];
                        SoeGeumulSubi = temp[4];
                        LaserSongGu = temp[5];
                        SubihyeongPosu = temp[6];
                        JiljuBonneung = temp[7];
                        JuluSense = temp[8];
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