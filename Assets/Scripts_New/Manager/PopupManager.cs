using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    private RectTransform Panel = null;
    private Dictionary<System.Type, GameObject> PopRegist = new Dictionary<System.Type, GameObject>();
    private List<System.Type> Stack = new List<System.Type>();

    public void Init()
    {
        //UI초기화
        PopRegist.Clear();
        GameObject canvas = GameObject.FindWithTag("PopupCanvas");
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



    public T OpenPopup<T>() where T : UIPopup
    {
        T UIPopup = null;
        CloseLastOpenPopup();        
        if (PopRegist.ContainsKey(typeof(T)))
        {
            UIPopup = GetPopup<T>();
        }
        else
        {
            CheckDestoryOldPopup();
            UIPopup = CreatePopup<T>();
        }
        UIPopup.transform.localRotation = Quaternion.Euler(Vector3.zero);
        UIPopup.transform.localScale = Vector3.one;
        RectTransform trans = UIPopup.GetComponent<RectTransform>();
        trans.anchoredPosition3D = Vector3.zero;
        trans.offsetMin = Vector2.zero;
        trans.offsetMax = Vector2.zero;        
        UIPopup.Open();
        return UIPopup;
    }

    public T GetPopup<T>() where T : UIPopup
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

    private T CreatePopup<T>() where T : UIPopup
    {
        if (PopRegist == null)
            return null;

        GameObject windowObj = null;

        if (PopRegist.TryGetValue(typeof(T), out windowObj) == false)
        {
            windowObj = KOBManager.Resource.LoadPopup<T>();
            if (windowObj != null)
            {
                windowObj.name = typeof(T).Name;
                windowObj.transform.parent = Panel.transform;
            }
        }
        return RegistPopup<T>(windowObj);
    }


    public T RegistPopup<T>(GameObject obj) where T : UIPopup
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
                if(popup.Value.activeSelf == true)
                {
                    popup.Value.GetComponent<UIPopup>().Close();
                }
            }
        }
    }

    private void CheckDestoryOldPopup()
    {
        if (Stack.Count > 5)
        {
            System.Type Key = Stack[0];
            Debug.Log("key : " + Key);
            if (PopRegist.ContainsKey(Key))
            {
                Destroy(PopRegist[Key]);
                PopRegist.Remove(Key);
                Stack.RemoveAt(0);
                return;
            }
        }
    }


    public void BackToLastPopup()
    {
        if(Stack.Count >= 2)
        {
            System.Type Key = Stack[Stack.Count - 2];
            if (PopRegist.ContainsKey(Key))
            {
                PopRegist[Key].gameObject.SetActive(true);
            }
        }
    }




#if UNITY_EDITOR

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.F1))
        {
            GoldRewardTest();
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            //KOBManager.Backend.GameData.KOBRttsInfo.UpdateLeague(true);
            //KOBManager.Backend.UpdateAllGameData(null);
        }
    }




    public void GoldRewardTest()
    {        
        RewardSetting goldSetting = new RewardSetting();
        goldSetting.rewardData = RewardType.Bat;
        goldSetting.index = 1;
        goldSetting.quantity = 10;
        
        /*
        RewardSetting goldSetting2 = new RewardSetting();
        goldSetting2.rewardData = RewardType.PowerTP;
        goldSetting2.index = 0;
        goldSetting2.quantity = 50;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting2);
          */      
        /*
        RewardSetting goldSetting3 = new RewardSetting();
        goldSetting3.rewardData = RewardType.Gem;
        goldSetting3.index = 0;
        goldSetting3.quantity = 5;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting3);*/

        /*
        RewardSetting goldSetting4 = new RewardSetting();
        goldSetting4.rewardData = RewardType.SkillCard;
        goldSetting4.index = 10526;
        goldSetting4.quantity = 0;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting4);//*/

        /*
        RewardSetting goldSetting4 = new RewardSetting();
        goldSetting4.rewardData = RewardType.SkillPoint;
        goldSetting4.index = 0;
        goldSetting4.quantity = 100000;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting4);*/

        /*
        RewardSetting goldSetting4 = new RewardSetting();
        goldSetting4.rewardData = RewardType.Gold;
        goldSetting4.index = 0;
        goldSetting4.quantity = 1000000;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting4); //*/

        /*
        RewardSetting goldSetting4 = new RewardSetting();
        goldSetting4.rewardData = RewardType.SkillCard;
        goldSetting4.index = 10529;
        goldSetting4.quantity = 0;
        KOBManager.MyInfo.AddToRewardCashe(goldSetting4); //*/


    }

#endif
}
