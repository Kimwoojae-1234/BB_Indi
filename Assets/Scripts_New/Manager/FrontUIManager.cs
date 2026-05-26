using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontUIManager : MonoBehaviour
{
    private RectTransform Panel = null;
    private Dictionary<System.Type, GameObject> PopRegist = new Dictionary<System.Type, GameObject>();
    private List<System.Type> Stack = new List<System.Type>();

    public void Init()
    {
        //UI초기화
        PopRegist.Clear();
        GameObject canvas = GameObject.FindWithTag("FrontUICanvas");
        if (canvas != null)
        {
            Panel = canvas.transform.Find("Panels").GetComponent<RectTransform>();
        }
    }

    public bool CheckPopupOpen()
    {
        foreach (KeyValuePair<System.Type, GameObject> popup in PopRegist)
        {
            if (popup.Value.activeSelf == true)
            {
                return true;
            }
        }

        return false;
    }



    public T OpenPopup<T>() where T : UIFrontUI
    {
        T UIFrontUI = null;
        CloseLastOpenPopup();
        if (PopRegist.ContainsKey(typeof(T)))
        {
            UIFrontUI = GetPopup<T>();
        }
        else
        {
            UIFrontUI = CreatePopup<T>();
        }
        UIFrontUI.transform.localRotation = Quaternion.Euler(Vector3.zero);
        UIFrontUI.transform.localScale = Vector3.one;
        RectTransform trans = UIFrontUI.GetComponent<RectTransform>();
        trans.anchoredPosition3D = Vector3.zero;
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;
        UIFrontUI.Open();
        return UIFrontUI;
    }

    public T GetPopup<T>() where T : UIFrontUI
    {
        if (PopRegist == null || PopRegist.Count <= 0)
            return null;

        GameObject popupObj = null;
        if (PopRegist.TryGetValue(typeof(T), out popupObj))
        {
            if (popupObj == null)
                return null;
            else
                return popupObj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    private T CreatePopup<T>() where T : UIFrontUI
    {
        if (PopRegist == null)
            return null;

        GameObject windowObj = null;

        if (PopRegist.TryGetValue(typeof(T), out windowObj) == false)
        {
            windowObj = KOBManager.Resource.LoadFrontUI<T>();
            if (windowObj != null)
            {
                windowObj.name = typeof(T).Name;
                windowObj.transform.parent = Panel.transform;
            }
        }
        return RegistPopup<T>(windowObj);
    }

    public T RegistPopup<T>(GameObject obj) where T : UIFrontUI
    {
        if (PopRegist == null)
            return null;

        if (PopRegist.ContainsKey(typeof(T)) == false)
        {
            PopRegist.Add(typeof(T), obj);
            Stack.Add(typeof(T));
        }
        return PopRegist[typeof(T)].GetComponent<T>();
    }

    private void CloseLastOpenPopup()
    {
        if (PopRegist.Count > 0)
        {
            foreach (KeyValuePair<System.Type, GameObject> popup in PopRegist)
            {
                Debug.Log("Key = " + popup.Key);
                /*if (popup.Key == typeof(FrontUI_Tutorial5))
                {
                    Debug.Log("이놈은 예외처리");
                }
                else if (popup.Key == typeof(FrontUI_Tutorial7))
                {
                    Debug.Log("이놈은 예외처리");
                }
                else if (popup.Key == typeof(FrontUI_Tutorial9))
                {
                    Debug.Log("이놈은 예외처리");
                }
                else*/
                {
                    if (popup.Value.activeSelf == true)
                    {
                        popup.Value.GetComponent<UIFrontUI>().Close();
                    }
                }
            }
        }
    }


    public void BackToLobby()
    {
        GameObject obj = KOBManager.Resource.LoadGameObject("UI/Loading", "UI_Loading", Panel);
        if(obj != null)
        {
            obj.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            obj.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            UI_Loading loading = obj.GetComponent<UI_Loading>();
            loading.BackToLobby();
        }
    }
}
