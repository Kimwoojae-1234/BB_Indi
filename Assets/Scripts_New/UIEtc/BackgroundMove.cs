using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMove : MonoBehaviour
{
    private float PosY;
    private float dV;
    private RectTransform rectT;


    private float dX;
    private RectTransform [] childT;

    private void Awake()
    {
        rectT = GetComponent<RectTransform>();
        RectTransform[] childObj = GetComponentsInChildren<RectTransform>();
        int count = 0;
        if (childObj.Length > 1)
        {
            childT = new RectTransform[childObj.Length - 1];
            foreach (RectTransform child in childObj)
            {
                if (child != rectT)
                {
                    //Debug.Log(child.name);
                    childT[count] = child;
                    count++;
                }
            }
        }

        
        PosY = rectT.anchoredPosition.y;
        dX = dV = 31.25f * Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PosY += dV;
        if(PosY >= 250)
        {
            PosY = -1750;
        }
        rectT.anchoredPosition = new Vector2(0, PosY);


        for(int i = 0; i< childT.Length;i++)
        {
            float PosX = childT[i].anchoredPosition.x;
            float Y = childT[i].anchoredPosition.y;
            PosX += dX;
            if (PosX >= 4000)
            {
                PosX = 0;
            }
            childT[i].anchoredPosition = new Vector2(PosX, Y);
        }

    }
}
