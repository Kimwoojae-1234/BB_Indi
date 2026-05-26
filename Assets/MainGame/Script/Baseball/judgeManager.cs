using UnityEngine;
using System.Collections;


namespace BaseBall.BallPlay
{
    public class judgeManager : MonoBehaviour
    {
        Field field;
        BallPlayManager manager;
        public Judge[] judge;
        public int flyOutCallIndex;
        public bool bFairCheck;

        
        public void InitInstance(Field _field)
        {
            field = _field;
            manager = field.manager;
            judge = new Judge[4];

            for (int i = 0; i < 4; i++)
            {
                judge[i] = Util.Load("MainGame/prefabs/FieldViewPrefab/judgePrefab", transform, new Vector3(0, 0, Field._FielderZOrder)).GetComponent<Judge>();
                judge[i].InitInstance(field, i);
            }
        }

        public void InitPosition()
        {
            for (int i = 0; i < 4; i++)
            {
                judge[i].initPosition();
            }
        }


        public void setJudgeFieding(float firstAngle)
        {
            if (field.bFieldViewActive == false) return;// if (field.manager.playState != PlayState.PLAY_FIELDING_VIEW) return;


            if (field.flyCatchAvaiableCount > 0)
            {
                //플라이볼을 잡는 경우
                for (int i = 0; i < 4; i++)
                {
                    judge[i].setFlyball(field.flyCatchFielder, firstAngle);
                }
            }
            else
            {
                bFairCheck = false;
                if (firstAngle > 40 || firstAngle < -40)
                {
                    bFairCheck = true;
                }

                for (int i = 0; i < 4; i++)
                {
                    judge[i].setGrounder(field.groundCatchFielder, firstAngle, bFairCheck);
                }
            }

        }


        public void setJudgeStealFieding()
        {
            for (int i = 0; i < 4; i++)
            {
                judge[i].setGrounder(CPlayer._SHORTSTOP, 15, false);
            }
        }

        public void setCall(int index, CallType type)
        {
            int judgeIndex = -1;
            float delay = 0.01f;
            if (type == CallType._SAFE)
            {
                judgeIndex = index;

                if (judgeIndex != -1)
                {                    
                    judge[judgeIndex].callSafe();
                }

            }
            else if (type == CallType._OUT)
            {
                judgeIndex = index;                
                if (judgeIndex != -1)
                {
                    delay = judge[judgeIndex].callOut(type);
                }
            }
            else if (type == CallType._STRONGOUT)
            {
                judgeIndex = index;// FieldParm.FIRSTBASE_INDEX;
                
                if (judgeIndex != -1)
                {
                    delay = judge[judgeIndex].callOutStrong();
                }
            }
            else if (type == CallType._FLYOUT)
            {
                judgeIndex = flyOutCallIndex;// FieldParm.GetFlyOutJudge(index);
                if (judgeIndex != -1)
                {
                    judge[judgeIndex].callOut(type);
                }
            }
            else if (type == CallType._HOMERUN)
            {
                if (field.ball.firstAngle > 25) judgeIndex = FieldParm.THIRDBASE_INDEX;
                else if (field.ball.firstAngle < -25) judgeIndex = FieldParm.FIRSTBASE_INDEX;
                else judgeIndex = FieldParm.SECONDBASE_INDEX;
                if (judgeIndex != -1)
                {
                    judge[judgeIndex].callHomerun();                
                }
            }
            else if (type == CallType._LINECALL || type == CallType._FOUL)
            {
                judgeIndex = (field.ball.firstAngle > 0 ? FieldParm.THIRDBASE_INDEX : FieldParm.FIRSTBASE_INDEX);
                if (judgeIndex != -1)
                {
                    judge[judgeIndex].callFoul(type);
                }
            }

        }


    }
}
