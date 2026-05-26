using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class ResourceManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite LoadSprite(string pathName, string resourcename) 
    {
        string path = string.Format("{0}/{1}", pathName, resourcename);
        Sprite sprite = Resources.Load<Sprite>(path);                
        return sprite;
    }


    public GameObject LoadGameObject(string pathName, string resourcename)
    {
        string path = string.Format("{0}/{1}", pathName, resourcename);
        GameObject Obj = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        return Obj;
    }

    public GameObject LoadGameObject(string pathName, string resourcename, Transform par)
    {
        string path = string.Format("{0}/{1}", pathName, resourcename);
        GameObject Obj = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        Obj.transform.parent = par;
        Obj.transform.localPosition = Vector3.zero;
        Obj.transform.localScale = Vector3.one;
        return Obj;
    }


    public GameObject LoadUIWindow<T>() where T : UIWindow
    {        
        string path = string.Format("UI/Window/{0}", typeof(T).Name);
        GameObject LoadUIWindow = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        if (LoadUIWindow == null)
        {
            return null;
        }
        return LoadUIWindow;
    }

    public GameObject LoadPopup<T>() where T : UIPopup
    {
        string path = string.Format("UI/Popup/{0}", typeof(T).Name);
        GameObject LoadPopup = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        if (LoadPopup == null)
        {
            return null;
        }
        return LoadPopup;
    }

    public GameObject LoadFrontUI<T>() where T : UIFrontUI
    {
        string path = string.Format("UI/FrontUI/{0}", typeof(T).Name);
        GameObject LoadFrontUI = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        if (LoadFrontUI == null)
        {
            return null;
        }
        return LoadFrontUI;
    }

    public GameObject LoadBanner(string bannerName, Transform par = null)
    {
        string path = string.Format("UI/Banner/{0}", bannerName);
        GameObject LoadBanner = GameObject.Instantiate(Resources.Load(path), Vector3.zero, Quaternion.identity) as GameObject;
        if (LoadBanner == null)
        {
            return null;
        }

        if(par != null)
        {
            LoadBanner.transform.parent = par;
            LoadBanner.transform.localScale = Vector3.one;
        }


        return LoadBanner;
    }



    public GameObject LoadClone(GameObject cloneObj, Vector3 pos, Vector3 scale, Transform par = null)
    {
        GameObject clone = GameObject.Instantiate(cloneObj, pos, Quaternion.identity) as GameObject;
        if (par != null)
        {
            clone.transform.parent = par;
            clone.transform.localPosition = pos;
            clone.transform.localScale = scale;
            clone.transform.localEulerAngles = Vector3.zero;
        }

        return clone;
    }



    public void LoadMyLeagueLogo(Image logo)
    {
        if (logo != null)
        {
            int League = KOBManager.Rtts.League;    
            logo.sprite = KOBManager.Resource.LoadSprite("Sprite/LeagueLogo", string.Format("LeagueLogo{0}", League));
            logo.SetNativeSize();
        }
    }

    public void LoadLeagueLogo(Image logo, int idx)
    {
        if (logo != null)
        {
            logo.sprite = KOBManager.Resource.LoadSprite("Sprite/LeagueLogo", string.Format("LeagueLogo{0}", idx));
            logo.SetNativeSize();
        }
    }


    /// <summary>
    /// 내팀 로고를 로딩하여 매개변스 logo에 세팅
    /// </summary>
    /// <returns></returns>
    public void LoadMyTeamLogo(Image logo)
    {
        if (logo != null)
        {
            logo.sprite = LoadSprite("Sprite/TeamLogo", "team050"); //우선 임시로
            logo.SetNativeSize();
        }
    }

    public Sprite LoadMyTeamLogoSprite()
    {
        return LoadSprite("Sprite/TeamLogo", "team050"); //우선 임시로
    }


    /// <summary>
    /// 해당 팀인덱스의 팀 로고를 로딩하여 매개변수 logo에 세팅
    /// </summary>
    /// <param name="logo"></param>
    /// <param name="teamIdx"></param>
    public void LoadTeamLogo(Image logo, int teamIdx)
    {
        if (logo != null)
        {
            string logoName = string.Format("team{0:D3}", teamIdx);
            logo.sprite = LoadSprite("Sprite/TeamLogo", logoName);
            logo.SetNativeSize();
        }
    }

    public Sprite LoadTeamLogo(int teamIdx)
    {
        string logoName = string.Format("team{0:D3}", teamIdx);
        return LoadSprite("Sprite/TeamLogo", logoName);
    }


    public void LoadBallerPortrait(Image portrait, int idx)
    {
        if (portrait != null)
        {
            if(idx < 0) portrait.sprite = LoadSprite("BallerPortrait", "pic0000");
            else portrait.sprite = LoadSprite("BallerPortrait", string.Format("pic{0}", idx));
            portrait.SetNativeSize();
        }
    }

    public GameObject LoadBaller(int idx, Transform par = null)
    {
        if (par != null)
        {
            foreach (Transform child in par.transform)
            {
                Destroy(child.gameObject);
            }
        }

        GameObject baller = LoadGameObject("Ballers", "baller" + idx, par);

        return baller;
    }

}
