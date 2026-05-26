using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Inbox : UIPopup
{
    [SerializeField] private InboxComponent clone;
    [SerializeField] private Transform origin;

    [SerializeField] private GameObject ClaimAll;
    [SerializeField] private GameObject NoMail;


    public override void Initialize()
    {
        base.Initialize();
        PostObjInit();
    }

    public override void Open()
    {
        base.Open();
    }


    private void PostObjInit()
    {
        Dictionary<int, PostData> PostList = KOBManager.Backend.PostList;
        SetClaimAllBtn(PostList.Count);
        foreach (KeyValuePair<int, PostData> post in PostList)
        {
            GameObject postObj = GameObject.Instantiate(clone.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
            postObj.gameObject.SetActive(true);
            postObj.GetComponent<InboxComponent>().Init(post.Value);
            postObj.transform.parent = origin;
            postObj.transform.localScale = Vector3.one;
        }
    }

    public void SetClaimAllBtn(int count)
    {
        bool bInBox = count > 0;
        ClaimAll.gameObject.SetActive(bInBox);
        NoMail.gameObject.SetActive(!bInBox);
    }



    public void OnClickClaimAll()
    {
        KOBManager.FrontUI.OpenPopup<FrontUI_NetworkLoading>();
        KOBManager.Backend.PostReceive(BackEnd.PostType.Admin, -1, (obj) =>
        {
            KOBManager.FrontUI.GetPopup<FrontUI_NetworkLoading>()?.Close();
            Dictionary<int, PostData> PostList = (Dictionary<int, PostData>)obj;
            //PostObjInit();
            foreach (Transform child in origin.transform)
            {
                Destroy(child.gameObject);
            }
            SetClaimAllBtn(0);
        });
    }




}
