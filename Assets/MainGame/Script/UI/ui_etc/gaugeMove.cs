//#define _Test_Version

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class gaugeMove : MonoBehaviour
    {
        private readonly int MAXLEVEL = 40;

        public UISprite firstGauge, secondGague;
        public UILabel percent, exp, level;

        private int maxValue;
        private bool bInit = false;        
        private int _curLevel, _curExp, _nextLevel, _nextExp;
        private int _getTotalExp = 0;



#if _Test_Version
        public int TEST_CUR_LEVEL = 1;
        public int TEST_CUR_EXP = 50;
        public int TEST_NEXT_LEVEL = 2;
        public int TEST_NEXT_EXP = 10;

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SetTeamExp(312, TEST_CUR_LEVEL, TEST_CUR_EXP, TEST_NEXT_LEVEL, TEST_NEXT_EXP);
                StartCoroutine(progressTeamExp());
            }
        }
#else
        void OnEnable()
        {
            if (bInit == true)
            {
                StartCoroutine(progressTeamExp());
                bInit = false;
            }
        }
#endif
        public void SetTeamExp(int max, int curLevel, int curExp, int nextLevel, int nextExp)
        {
            maxValue = max;
            _curLevel = curLevel;
            _curExp = curExp;
            _nextLevel = nextLevel;
            _nextExp = nextExp;
            
            int levelGab = nextLevel - curLevel;
            if (levelGab == 0)
            {
                _getTotalExp = _nextExp - _curExp;
            }
            else
            {
                int nextRealExp = _nextExp;
                for (int i = _curLevel; i < _nextLevel; i++)
                {
                    // DISABLED_MGRS: nextRealExp += Mgrs.GameData.GameDB_FindTeamLevel_Exp(i+1);
                }
                _getTotalExp = nextRealExp - _curExp;
            }


            //_getTotalExp = Util.getTotalExp(nextLevel, nextExp) - Util.getTotalExp(curLevel, curExp);
            Debug.Log("현재레벨 " + _curLevel + "    현재exp " + _curExp);
            Debug.Log("다음레벨 " + _nextLevel + "    다음exp " + _nextExp);
            Debug.Log("exp 차이 " + _getTotalExp);


            if (_curLevel >= MAXLEVEL)
            {
                setMaxLevel(-1);
            }
            else
            {
                int maxTeamExp = 100;// Mgrs.GameData.GameDB_FindTeamLevel_Exp(_curLevel + 1);
                float curPer = (float)((_curExp) * 100) / (float)maxTeamExp;
                firstGauge.gameObject.SetActive(true);
                firstGauge.SetDimensions(Mathf.Clamp((int)(maxValue * curPer / 100), 5, maxValue), 14);
                updateTeam(curPer, _curExp);

                bInit = true;
            }
        }

        private void updateTeam(float curPer, float curGetExp)
        {
            level.text = "TEAM LEVEL [95E943]" + _curLevel + "[-]";
            exp.text = "+EXP  " + curGetExp;
            percent.text = string.Format("{0:F2}%", curPer);

            int w = Mathf.Clamp((int)(maxValue * curPer / 100), 5, maxValue);
            secondGague.SetDimensions(w, 14);
        }

        private void setMaxLevel(float curGetExp)
        {
            level.text = "TEAM LEVEL [95E943]" + MAXLEVEL + "[-]";
            if (curGetExp > 0)
            {
                exp.text = "+EXP  " + curGetExp;
            }
            else
            {
                exp.gameObject.SetActive(false);
            }
            percent.text = "MAX";

            secondGague.SetDimensions(maxValue, 14);
        }

        private IEnumerator progressTeamExp()
        {
#if _Test_Version
            yield return new WaitForSeconds(1.0f);
#endif
            float frame = 30.0f + (_nextLevel - _curLevel) * 30.0f;
            float gabDv = _getTotalExp / frame;
            int curLevel = _curLevel;
            float curExp = _curExp;
            float totalExp = 0;

            while (true)
            {
                totalExp += gabDv;
                curExp += gabDv;

                int maxTeamExp = 100;//// DISABLED_MGRS: int maxTeamExp = Mgrs.GameData.GameDB_FindTeamLevel_Exp(curLevel + 1);
                if (totalExp > _getTotalExp)
                {
                    if (_curLevel < _nextLevel)
                    {
                        firstGauge.gameObject.SetActive(false);
                    }
                    totalExp = _getTotalExp;
                    float finalPer = (float)((_nextExp) * 100) / (float)maxTeamExp;
                    _curLevel = _nextLevel;
                    if (_curLevel >= MAXLEVEL)
                    {
                        setMaxLevel((int)totalExp);
                    }
                    else
                    {
                        updateTeam(finalPer, totalExp);
                    }
                    break;
                }                
                int exp = (int)curExp;
                if (exp > maxTeamExp)
                {
                    firstGauge.gameObject.SetActive(false);
                    curLevel++;
                    _curLevel = curLevel;
                    curExp = 0;
                    //이벤트 때려
                }

                if (curLevel >= MAXLEVEL)
                {
                    setMaxLevel((int)totalExp);
                    break;
                }
                else
                {
                    float curPer = (float)((exp) * 100) / (float)maxTeamExp;
                    updateTeam(curPer, (int)totalExp);
                }
                yield return new WaitForEndOfFrame();
            }
        }


    }
}
