using BaseBall.BallPlay.UGUI;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCardInfo : MonoBehaviour
{
    [SerializeField] GameUIElement PlayerName;
    [SerializeField] GameUIElement Logo;
    [SerializeField] GameUIElement pic;

    public void SetInfo(int idx, string name, int logoIndex)
    {
        pic.mainTexture =  KOBManager.Resource.LoadBallerPortraitTemp(idx);
        Logo.mainTexture = KOBManager.Resource.LoadLogoTemp(logoIndex);
        PlayerName.text = name;
    }
}
