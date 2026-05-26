using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class PitchIndicator : MonoBehaviour
    {
        public const int MAX_LINE = 25;
        private LineRenderer line;
        private int lineNum;
        private Vector3 startPos, finishPos;
        private bool bEnable;
        private Vector3[] curPosition;

        void Awake()
        {            
            line = GetComponent<LineRenderer>();
            init();
            line.enabled = false;
            bEnable = false;            
        }


        public void active(bool bActive)
        {
            line.enabled = bActive;
            bEnable = false;
        }

        public void init()
        {
            curPosition = new Vector3[MAX_LINE];
            //붉은색 버전
            //line.SetColors(new Color(1, 1, 1, 0.39f), new Color(1, 1, 1, 0.67f));            
            //line.SetWidth(1.5f,0.9f);            

            //흰색버전
            line.SetColors(new Color(1, 1, 1, 0.66f), new Color(1, 1, 1, 0.95f));
            line.SetWidth(1.5f,5.0f);            
        }

        public void makeLine(Vector3[] position, int num)
        {            
            lineNum = num;
            line.SetVertexCount(lineNum - 3);

            curPosition = position;
            for (int i = 3; i < lineNum; i++)
            {
                line.SetPosition(i - 3, position[i]);
            }

            active(true);
            bEnable = true;
        }



        public void updateLine(float aX, float aY)
        {
            if (bEnable == true)
            {   
                for (int i = 3; i < lineNum; i++)
                {
                    float rate = (float)(i + 1) / (float)lineNum;
                    Vector3 curPos = curPosition[i] + new Vector3(aX * rate, aY * rate, 0);
                    line.SetPosition(i-3, curPos);
                }
            }
        }

    }
}