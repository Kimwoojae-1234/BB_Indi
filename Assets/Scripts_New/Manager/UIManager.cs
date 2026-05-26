using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Background BG = null;
    private RectTransform Panel = null;
    private Dictionary<System.Type, GameObject> UIRegist = new Dictionary<System.Type, GameObject>();
    private List<UIBase> UIStack = new List<UIBase>();    

    public void Init()
    {
        //UI초기화
        ClearUI();
        GameObject canvas = GameObject.FindWithTag("MainUICanvas");
        if (canvas != null)
        {
            Panel = canvas.transform.Find("Panels").GetComponent<RectTransform>();
            
            UIWindow[] windows = Panel.GetComponentsInChildren<UIWindow>();
            foreach (UIWindow child in windows)
            {
                Debug.Log(child.name);
                System.Type type = System.Type.GetType(child.name);
                UIRegist.Add(type, child.gameObject);
                if(type == typeof(UI_LobbyRe))
                {
                    //최초 UI
                    child.gameObject.SetActive(true);
                    OpenWindow<UI_LobbyRe>();
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
            
        }
    }

    public void InitBackground(Background _bg)
    {
        BG = _bg;
    }


    public void ClearUI()
    {
        UIRegist.Clear();
        UIStack.Clear();
    }



    public T OpenWindow<T>() where T : UIWindow
    {        
        T UIwindow = null;
        CloseLastOpenWindow();
        if (UIRegist.ContainsKey(typeof(T)))
        {
            UIwindow = GetUIWindow<T>();
        }
        else
        {
            UIwindow = CreateUI<T>();
        }
        UIwindow.transform.localRotation = Quaternion.Euler(Vector3.zero);
        UIwindow.transform.localScale = Vector3.one;
        RectTransform trans = UIwindow.GetComponent<RectTransform>();
        trans.anchoredPosition3D = Vector3.zero;
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;

        PushStackUI(UIwindow);
        UIwindow.OpenWindow();
        return UIwindow;
    }

    public T GetUIWindow<T>() where T : UIWindow
    {
        if (UIRegist == null || UIRegist.Count <= 0)
            return null;

        GameObject windowObj = null;
        if (UIRegist.TryGetValue(typeof(T), out windowObj))
        {
            if (windowObj == null)
                return null;
            else
                return windowObj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    private T CreateUI<T>() where T : UIWindow
    {
        if (UIRegist == null)
            return null;

        GameObject windowObj = null;

        if (UIRegist.TryGetValue(typeof(T), out windowObj) == false)
        {
            windowObj = KOBManager.Resource.LoadUIWindow<T>();            
            if (windowObj != null)
            {
                windowObj.name = typeof(T).Name;
                windowObj.transform.parent = Panel.transform;                
            }
        }
        return RegistUIWindow<T>(windowObj);
    }

    public T RegistUIWindow<T>(GameObject obj) where T : UIWindow
    {
        if (UIRegist == null)
            return null;

        if (UIRegist.ContainsKey(typeof(T)) == false)
        {
            UIRegist.Add(typeof(T), obj);
        }
        return UIRegist[typeof(T)].GetComponent<T>();
    }

    public System.Type GetCurrentUIType()
    {
        if (UIStack.Count > 0)
        {
            UIWindow last = (UIWindow)UIStack[UIStack.Count - 1];
            if (last != null)
            {
                return System.Type.GetType(last.gameObject.name);
            }
        }

        return null;
    }

    public UIWindow GetCurrentUI()
    {
        if (UIStack.Count > 0)
        {
            UIWindow last = (UIWindow)UIStack[UIStack.Count - 1];
            return last;
        }

        return null;
    }


    public void PushStackUI(UIBase registUI)
    {
        if (UIStack == null)
            return;        
        UIStack.Add(registUI);

        if(UIStack.Count >= 5)
        {
            UIStack.RemoveAt(0);
        }
    }


    private void CloseLastOpenWindow()
    {
        if (UIStack.Count > 0)
        {
            UIBase last = UIStack[UIStack.Count - 1];
            if(last != null)
            {
                last.CloseUI();
            }
        }
    }



    public void BackToPreviousWindow(System.Type LastWindow)
    {
        /*if (UIStack.Count >= 2)
        {
            UIWindow last = (UIWindow)UIStack[UIStack.Count - 2];
            if (last != null)
            {
                Debug.Log("==============>> name " + last.gameObject.name);
                System.Type type = System.Type.GetType(last.gameObject.name);                
                if (type == typeof(UI_RTTS))
                {
                    OpenWindow<UI_RTTS>();
                }
                else if (type == typeof(UI_Tournament))
                {
                    OpenWindow<UI_Tournament>();
                }
                else
                {
                    OpenWindow<UI_Lobby>();
                }
            }
        }*/
        if(LastWindow == null)
        {
            OpenWindow<UI_LobbyRe>();
        }
        else
        {
            if (LastWindow == typeof(UI_RTTS))
            {
                OpenWindow<UI_RTTS>();
            }
            else if (LastWindow == typeof(UI_Tournament))
            {
                OpenWindow<UI_Tournament>();
            }
            else if (LastWindow == typeof(UI_BallersList))
            {
                OpenWindow<UI_BallersList>();
            }
            else if (LastWindow == typeof(UI_Ballers))
            {
                OpenWindow<UI_Ballers>();
            }            
            else
            {
                OpenWindow<UI_LobbyRe>();
            }
        }
    }

}
