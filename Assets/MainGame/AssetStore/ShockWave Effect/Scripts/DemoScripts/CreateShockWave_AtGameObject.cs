using UnityEngine;
using System.Collections;

public class CreateShockWave_AtGameObject : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        InvokeRepeating("CreateShockWave",0.5f,3f);
	}


    void CreateShockWave()
    {
        ShockWave.Get().StartIt(gameObject.transform.position,0.1f,1f, 0.5f);
    }
	
}
