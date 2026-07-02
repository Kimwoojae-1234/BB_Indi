using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class QuickRunner : MonoBehaviour
    {
        public enum BaseRunningType
        {
            Normal,
            OnemoreBaseOut,
            StealOut,
            StealSafe,
            PickOffOut,
            PickOffSafe
        }

        public bool bActive;

        public Transform[] pos;

        public int arrayIndex;
        public UILabel playerName, baseRunningLabel;
        public UISprite cap;
        public int curPos;
        private int addDestPos;
        private bool bMoving;
        
        public void init(int currentPos, string strName, int teamIndex)
        {
            baseRunningLabel.gameObject.SetActive(false);
            bActive = true;
            bMoving = false;
            addDestPos = -1;
            //gameObject.SetActive(true);
            playerName.text = strName;
            cap.spriteName = "minimap_team" + teamIndex;
            transform.localPosition = pos[currentPos].localPosition;
            curPos = currentPos;
        }

        public void deadRunner(int outCount, BaseRunningType runType = BaseRunningType.Normal)
        {
            int destPos = (curPos + 1) % 4;
            if (destPos != FieldParm.FIRSTBASE_INDEX)
            {
                if (destPos == FieldParm.HOMEBASE_INDEX || outCount >=2)
                {
                    deActive();
                }
                else
                {
                    moveRunner(destPos, false, runType);
                }
            }
        }


        public void moveRunner(int destPos, bool active = true, BaseRunningType runType = BaseRunningType.Normal)
        {
            if (bActive == true)
            {                
                if ((destPos != curPos) || (curPos == FieldParm.HOMEBASE_INDEX && destPos == FieldParm.HOMEBASE_INDEX))
                {
                    if (bMoving == true)
                    {
                        //Debug.Log("=========================>>add position");
                        addDestPos = destPos;
                    }
                    else
                    {
                        StartCoroutine(move(destPos, active, runType));
                    }
                }
            }
        }


        private IEnumerator move(int destPos, bool active, BaseRunningType runType)
        {
            setBaseRunningType1(runType);
            addDestPos = -1;
            bMoving = true;
            int next = destPos - curPos;
            if (next < 0) next += 4;

            ////Debug.Log("=======================>>> next = " + next);

            int position = curPos;
            transform.localPosition = pos[position].localPosition; //초기위치
            do
            {
                position = (position + 1) % 4;
                TweenPosition.Begin(gameObject, 0.4f, pos[position].localPosition);
                yield return new WaitForSeconds(0.45f);
                transform.localPosition = pos[position].localPosition;  //다음 베이스
                next--;
            } while (next > 0);

            curPos = destPos;
            bMoving = false;

            if (bActive == false || curPos == FieldParm.HOMEBASE_INDEX)
            {
                if (runType != BaseRunningType.Normal)
                {
                    //특수 주루 케이스
                    setBaseRunningType2(runType);
                    yield return new WaitForSeconds(0.6f);
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
                deActive(false);
            }
            else
            {
                if (addDestPos != -1)
                {
                    if (curPos != addDestPos)
                    {
                        int nextPos = addDestPos;
                        addDestPos = -1;
                        StartCoroutine(move(nextPos, active, runType));
                    }
                }
                else
                {
                    if (runType != BaseRunningType.Normal)
                    {
                        //특수 주루 케이스
                        setBaseRunningType2(runType);
                        yield return new WaitForSeconds(0.6f);
                        baseRunningLabel.gameObject.SetActive(false);
                        if (runType == BaseRunningType.StealOut || runType == BaseRunningType.OnemoreBaseOut)
                        {
                            deActive(false);
                        }
                    }
                }
            }
        }

        public void pickOffRunner(BaseRunningType runType = BaseRunningType.Normal)
        {
            if (bActive == true)
            {
                StartCoroutine(pickoff(runType));
            }
        }

        private IEnumerator pickoff(BaseRunningType runType)
        {
            setBaseRunningType2(runType);
            yield return new WaitForSeconds(0.5f);
            if (runType == BaseRunningType.StealOut) deActive(false);
            else baseRunningLabel.gameObject.SetActive(false);
        }


        public void deActive(bool bStopCoroutine = true)
        {
            if(bStopCoroutine == true) StopAllCoroutines();
            gameObject.SetActive(false);
            transform.localPosition = pos[FieldParm.HOMEBASE_INDEX].localPosition;
            bActive = false;
        }

        private void setBaseRunningType1(BaseRunningType type)
        {
            if (type == BaseRunningType.StealOut || type == BaseRunningType.StealSafe)
            {
                baseRunningLabel.text = "Steal";
                baseRunningLabel.gameObject.SetActive(true);
            }

        }

        private void setBaseRunningType2(BaseRunningType type)
        {
            if (type == BaseRunningType.PickOffOut)
            {
                baseRunningLabel.text = "Pick off";
            }
            else if (type == BaseRunningType.PickOffSafe)
            {
                baseRunningLabel.text = "Pick off Fail";
            }
            else if (type == BaseRunningType.StealOut)
            {
                baseRunningLabel.text = "Steal Fail";
            }
            else if (type == BaseRunningType.StealSafe)
            {
                baseRunningLabel.text = "Steal Success";
            }
            else
            {
                baseRunningLabel.text = "Out";
            }
            baseRunningLabel.gameObject.SetActive(true);
        }
    }
}
