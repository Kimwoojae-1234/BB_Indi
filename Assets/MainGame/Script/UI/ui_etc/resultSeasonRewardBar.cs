using UnityEngine;
using System.Collections;
namespace BaseBall.BallPlay
{
    public class resultSeasonRewardBar : MonoBehaviour
    {
        public GameObject bg;
        public UILabel label, rank, gold;

        public void set(string _label, string _rank, int _gold, bool bBg)
        {
            transform.localScale = Vector3.one;
            bg.SetActive(bBg);
            label.text = _label;
            rank.text = _rank;
            gold.text = string.Format("{0:#,###}", _gold);
        }
    }
}