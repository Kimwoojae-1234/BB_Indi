using BaseBall.BallPlay.UGUI;
using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class resultLeagueLeader : MonoBehaviour
    {
        public UI_CardSmall card;
        public GameUIElement teamName, result;
        public GameUIElement logo;

        public void init(CPlayer player)
        {
            transform.localScale = Vector3.one;
        }
    }
}