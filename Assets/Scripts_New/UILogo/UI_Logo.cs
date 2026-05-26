//#define _433Logo
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Logo : MonoBehaviour
{    
    [SerializeField] private UI_Loading loading = null;
    [SerializeField] private GameObject[] logo = null;
    [SerializeField] private Image bg = null;

    bool bCloseLogo = false;


    private void Awake()
    {
        bg.color = Color.white;
        logo[0].gameObject.SetActive(true);            
    }

    public void CloseLogo()
    {
        bCloseLogo = true;
    }


    private void Update()
    {
        if(bCloseLogo == true)
        {
            if(LoginManager.LoginSuccess == true)
            {
                loading.StartLoading();
                bCloseLogo = false;
            }
        }
    }
}
