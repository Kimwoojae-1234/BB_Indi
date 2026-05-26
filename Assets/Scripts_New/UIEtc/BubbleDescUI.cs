using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BubbleDescUI : MonoBehaviour
{
    
    private float _time = 0;

    // Start is called before the first frame update
    public void Init(Transform pos)
    {
        transform.position = pos.position;
        
        _time = 0;




    }


    private void Update()
    {
        _time += Time.deltaTime;
        //if (_time > 0.1f)
        {
            if (Input.GetMouseButton(0))
            {
                _time = 0;
                gameObject.SetActive(false);
            }
        }
    }
}
