using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebConnector;

namespace BaseBall.BallPlay
{
    public class UIResultOtherTeam : MonoBehaviour
    {
        public GameObject _active;

        public otherResultSetting [] resultSetting;


        public void init(bool bInit)
        {
            
            _active.SetActive(true);
        }


        public void quit()
        {
            ResultUI.BackFromPopup();
            _active.SetActive(false);
        }


    }
}
