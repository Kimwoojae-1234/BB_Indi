using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        KOBManager.UI.Init();
        //KOBManager.Popup.Init();
        //KOBManager.FrontUI.Init();
        Destroy(gameObject, 1.0f);
    }

}
