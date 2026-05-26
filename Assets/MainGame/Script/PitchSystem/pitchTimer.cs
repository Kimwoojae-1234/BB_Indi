using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pitchTimer : MonoBehaviour {

    public Transform scale;
    private float scaleValue;
    private float dv, da;

    private bool bUpdateStart = false;

	// Use this for initialization
	void Awake () {
        

    }
	
	// Update is called once per frame
	void Update ()
    {
        if(bUpdateStart == true)
        {
            scaleUpdate();
        }
		
	}

    private void scaleUpdate()
    {
        scaleValue = scaleValue - (dv * Time.deltaTime);

        dv += (da * Time.deltaTime);

        //Debug.Log("scaleValue = " + scaleValue);

        if (scaleValue <= 0)
        {
            resetScale();
        }
        scale.localScale = new Vector3(scaleValue, scaleValue);
    }

    private void resetScale()
    {
        scaleValue = 1.0f;
        dv = 0.4f;
        da = 1.0f;
    }


    public void init()
    {
        Debug.Log("timer init");
        resetScale();
        scale.localScale = new Vector3(scaleValue, scaleValue);
        scale.gameObject.SetActive(true);
        bUpdateStart = true;
    }

    public float release()
    {
        Debug.Log("timer release");
        scale.gameObject.SetActive(false);
        bUpdateStart = false;
        return scaleValue;
    }



}
