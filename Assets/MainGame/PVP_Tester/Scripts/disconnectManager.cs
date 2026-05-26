using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class disconnectManager : MonoBehaviour {

    bool bInit = false;
	// Use this for initialization
	void Start () {
        bInit = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetMouseButtonDown(0))
        {
            if (bInit == false)
            {
                Debug.Log("GetMouseButtonDown");
                bInit = true;
                destroyObject();
                Invoke("loadScene", 1.0f);
            }
        }
	}

    void destroyObject()
    {
        MusicManager.Get().StopMusic();

        GameObject obj1 = GameObject.Find("skillEffectDisplayManager").gameObject;
        if (obj1 != null) Destroy(obj1);
        GameObject obj2 = GameObject.Find("simulator").gameObject;
        if (obj2 != null) Destroy(obj2);
        GameObject obj3 = GameObject.Find("Managers").gameObject;
        if (obj3 != null) Destroy(obj3);
        GameObject obj4 = GameObject.Find("pvpmanager").gameObject;
        if (obj4 != null) Destroy(obj4);
        GameObject obj5 = GameObject.Find("PhotonManager").gameObject;
        if (obj5 != null) Destroy(obj5);
        GameObject obj6 = GameObject.Find("SoundManager").gameObject;
        if (obj6 != null) Destroy(obj6);
        GameObject obj7 = GameObject.Find("MusicManager").gameObject;
        if (obj7 != null) Destroy(obj7);
    }

    private void loadScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("Login");
    }
}
