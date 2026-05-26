using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class gaugePlayerMove : MonoBehaviour
    {
        public bool bMaxLevel;

        public UISprite firstGauge, secondGague;
        public UILabel level;

        private GameObject maxLevel;

        private readonly int MAX_LEVEL = 20;
        private readonly int maxValue = 110;
        private bool bInit = false;
        private int _curLevel, _curExp, _nextLevel, _nextExp, _grade;
        private int _getTotalExp;

        private float delayTime;

#if _Test_Version
        public int TEST_CUR_LEVEL = 1;
        public int TEST_CUR_EXP = 50;
        public int TEST_NEXT_LEVEL = 2;
        public int TEST_NEXT_EXP = 10;
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SetTeamExp(312, 2, 10, 7, 30);
                StartCoroutine(progressTeamExp());
            }
        }
#else
        void OnEnable()
        {
            if (bInit == true)
            {
                StartCoroutine(progressPlayerExp());
                bInit = false;
            }
        }
#endif
        

        public void SetPlayerExp(int grade, int curLevel, int curExp, int nextLevel, int nextExp, float delay)
        {
            bMaxLevel = false;
            delayTime = delay;
            _curLevel = curLevel;
            _curExp = curExp;
            _nextLevel = nextLevel;
            _nextExp = nextExp;
            _grade = grade;

            int levelGab = nextLevel - curLevel;
            _getTotalExp = Util.getPlayerTotalExp(nextLevel-1, grade, nextExp) - Util.getPlayerTotalExp(curLevel-1,grade, curExp);

            int maxPlayerExp = 100;//// DISABLED_MGRS: int maxPlayerExp = Mgrs.GameData.FindCardExpDemand(curLevel, grade);
            float curPer = (float)((_curExp) * 100) / (float)maxPlayerExp;
            firstGauge.gameObject.SetActive(true);
            firstGauge.SetDimensions((int)(maxValue * curPer / 100), 14);
            updatePlayer(curPer);

            bInit = true;
        }

        private void updatePlayer(float curPer)
        {
            level.text = "LV " + _curLevel;
            //percent.text = string.Format("{0:F2}%", curPer);
            int w = (int)(maxValue * curPer / 100);
            secondGague.SetDimensions(w, 14);
        }

        public void setMaxLevel(bool bAready, bool bFinalLevel = false)
        {
            bMaxLevel = true;
            level.gameObject.SetActive(false);
            if (bFinalLevel == false)
            {
                transform.Find("max").gameObject.SetActive(true);
            }
            else
            {
                bMaxLevel = false;
                transform.Find("maxfinal").gameObject.SetActive(true);
            }
            if (bAready == false)
            {
                //실시간으로 맥스레벨달성
                secondGague.SetDimensions(maxValue, 14);
            }
            else
            {
                //이미 맥스레벨 달성한 경우
                secondGague.gameObject.SetActive(false);
                firstGauge.SetDimensions(maxValue, 14);
            }
        }

        private IEnumerator progressPlayerExp()
        {
            yield return new WaitForSeconds(delayTime);


            float frame = 30.0f + (_nextLevel - _curLevel) * 30.0f;
            float gabDv = _getTotalExp / frame;
            int curLevel = _curLevel;
            float curExp = _curExp;
            float totalExp = 0;

            while (true)
            {
                totalExp += gabDv;
                curExp += gabDv;

                int maxPlayerExp = 100;//// DISABLED_MGRS: int maxPlayerExp = Mgrs.GameData.FindCardExpDemand(curLevel, _grade);
                if (totalExp > _getTotalExp)
                {
                    totalExp = _getTotalExp;
                    float finalPer = (float)((_nextExp) * 100) / (float)maxPlayerExp;
                    _curLevel = _nextLevel;
                    if (_curLevel >= MAX_LEVEL)
                    {
                        bool bFinalGrade = (_grade == resultGrowBar.MAX_GRADE ? true : false);
                        setMaxLevel(false, bFinalGrade);
                    }
                    else
                    {
                        updatePlayer(finalPer);
                    }
                    break;
                }
                int exp = (int)curExp;
                if (exp > maxPlayerExp)
                {
                    firstGauge.gameObject.SetActive(false);
                    curLevel++;
                    _curLevel = curLevel;
                    curExp = 0;                    
                    //이벤트 때려
                }
                if (_curLevel >= MAX_LEVEL)
                {
                    bool bFinalGrade = (_grade == resultGrowBar.MAX_GRADE ? true : false);
                    setMaxLevel(false, bFinalGrade);
                    break;
                }
                else
                {
                    float curPer = (float)((exp) * 100) / (float)maxPlayerExp;
                    updatePlayer(curPer);
                }
                yield return new WaitForEndOfFrame();
            }
        }


    }
}
