using UnityEngine;
using System.Collections;

public class MaterialInit : MonoBehaviour {

    public Texture [] temp;
    public Material[] Mat;
	// Use this for initialization
	void Awake () {

        for (int i = 0; i < Mat.Length; i++)
        {
            Mat[i].mainTexture = temp[i];
        }

        Destroy(gameObject);

	}


}
