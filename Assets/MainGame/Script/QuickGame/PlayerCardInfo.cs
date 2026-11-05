using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCardInfo : MonoBehaviour
{
    [SerializeField] UILabel PlayerName;
    [SerializeField] UITexture Logo;
    [SerializeField] UITexture pic;
    public void SetRttsInfo(BaseBall.BallPlay.CPlayer player)
    {
        pic.mainTexture = RttsLiveRoster.Portrait(player);
        Logo.mainTexture = KOBManager.Resource.LoadLogoTemp(player.RttsTeamId + 1);
        PlayerName.text = player.getName();
    }

    public void SetInfo(int idx, string name, int logoIndex)
    {
        pic.mainTexture =  KOBManager.Resource.LoadBallerPortraitTemp(idx);
        Logo.mainTexture = KOBManager.Resource.LoadLogoTemp(logoIndex);
        PlayerName.text = name;
    }
}
