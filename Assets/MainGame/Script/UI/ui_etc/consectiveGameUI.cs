using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaseBall.BallPlay
{
    public class consectiveGameUI : MonoBehaviour
    {
        public void Init()
        {
            transform.localScale = Vector3.one;
            transform.Find("Label").GetComponent<UILabel>().text = (11 - Mode.ConsecutiveNum) + "/10";
        }

        public void setConsectiveQuit()
        {
            // DISABLED_MGRS: Mgrs.userData.SetUserGameMode(DefineEnum.EGameMode.Season);
            Destroy(gameObject, 0.22f);
        }
    }
}
