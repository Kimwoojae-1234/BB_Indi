using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class SimulFielder //: MonoBehaviour
    {
        public CPlayer fielder;
        
      

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void setFielder(CPlayer _fielder)
        {
            fielder = _fielder;
        }

        public CPlayer getFielder()
        {
            return fielder;
        }

        /// <summary>
        /// 야수의 수비 능력치 얻어오기
        /// </summary>
        public int getFielding()
        {
            return fielder.getFielding() + fielder.getFieldBonusValue();//
        }
        /// <summary>
        /// 야수의 어꺠 능력치 얻어오기
        /// </summary>
        public int getThrowing()
        {
            return fielder.getThrowing() + fielder.getFieldBonusValue();//
        }

        /// <summary>
        /// 야수의 주력 능력치 얻어오기
        /// </summary>
        public int getSpeed()
        {
            return fielder.getSpeed() + fielder.getFieldBonusValue();//
        }


        /// <summary>
        /// 시뮬레이션상에서 야수의 오버롤 수비 능력치
        /// </summary>
        public int getFieldingAbil()
        {
            return (getFielding() * 70 + getThrowing() * 30) / 100;
        }


        /// <summary>
        /// 수비범위, 타구판단 스킬에 따른 시뮬레이션 상에서 야수의 능력치 상승시켜줌
        /// </summary>
        public int addFieldingAbil(int value)
        {
            float addValue = (float)value * (0.03f);
            int totalAdd = 0;            
          
            /*
            //내야 타구 범위
            if (skillAvailable(SkillIndex.InfieldRange) == true)
            {
                //3%증가
                totalAdd += (int)addValue;
            }
            //외야 타구 범위
            if (skillAvailable(SkillIndex.OutfieldRange) == true)
            {
                //3%증가
                totalAdd += (int)addValue;
            }*/
            
            return totalAdd;

        }

        

        //////////////////////////////////////////////////////////
        //스킬
        //////////////////////////////////////////////////////////
        /// <summary>
        /// 스킬 발동 여부를 체크
        /// </summary>
        /// <returns>true를 리턴한경우 스킬이 발동함</returns>
        public bool checkSkillOn(SkillIndex index)
        {
            return fielder.fieldSkillSuccess(index);
        }

        //////////////////////////////////////////////////////////
        //에러
        //////////////////////////////////////////////////////////
        //포구에러
        public bool isCatchError()
        {
            int errorPer = 2;   //임시
            if (Util.GetPercent(errorPer) == true)
            {
                return true;
            }
            return false;
        }

        //송구에러
        public bool isThrowError()
        {
            int errorPer = 2;   //임시
            if (Util.GetPercent(errorPer) == true)
            {
                return true;
            }
            return false;
        }
    }
}
