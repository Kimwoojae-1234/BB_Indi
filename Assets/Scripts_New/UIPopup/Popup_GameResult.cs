using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Popup_GameResult : UIPopup
{
    [SerializeField] private GameObject[] Obj;


    

    /*
    public void Set(SimulMain simul)
    {
        for(int i =0; i < Obj.Length; i++) {  Obj[i].gameObject.SetActive(false); }

        //
        resultInfo = MakeResultInfo(simul); 
        Obj[0].gameObject.SetActive(true);
        Obj[0].GetComponent<ResultTeam>().SetResultTeam(resultInfo);

        Obj[2].GetComponent<ResultStat>().bInit = false;
    }*/


    public override void Set(Intent it = null)
    {
        
    }


}
