using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class resultLeagueLeader : MonoBehaviour
    {
        public UI_CardSmall card;
        public UILabel teamName, result;
        public UISprite logo;

        public void init(CPlayer player)
        {
            transform.localScale = Vector3.one;
        }
    }
}