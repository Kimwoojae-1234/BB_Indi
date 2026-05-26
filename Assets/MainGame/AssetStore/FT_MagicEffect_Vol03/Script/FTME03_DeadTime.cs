using UnityEngine;
using System.Collections;

public class FTME03_DeadTime : MonoBehaviour {
	public float deadTime;

	void Awake () {
		Destroy (gameObject, deadTime);	
	}
	
	void Update () {
	
	}
}
