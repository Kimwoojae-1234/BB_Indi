using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Setting : UIPopup
{
    


    public void OnClickLanguageSetting()
    {
        KOBManager.Popup.OpenPopup<Popup_Setting_Language>().BackToPrevPopup = true;
    }


}
