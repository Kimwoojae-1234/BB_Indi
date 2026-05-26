using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class changeController2 : MonoBehaviour
    {
        public UILabel overRoll;
        public UILabel nameLabel;
        public GameObject stamina;
        public UISprite gauge;
        public GameObject selected;
        public GameObject caution;


        private UIPlayerChange changeManager;
        private CPlayer curPlayer;
        private bool bPitcher;
        private int baseIndex;


        private UIPlayerChange.PlayerChangeType changeType;

        public void Init(UIPlayerChange _changeManager, CPlayer player, int position, UIPlayerChange.PlayerChangeType _type)
        {
            changeManager = _changeManager;
            curPlayer = player;
            changeType = _type;


            bPitcher = (position == CPlayer._PITCHER ? true : false);
            stamina.SetActive(bPitcher);
            nameLabel.text = player.getName();
            if (bPitcher == true)
            {
                //피처 세팅
                int w =(int)((132 * player.getCurrentStamina()) /100.0f);
                gauge.SetDimensions(w, 8);
                overRoll.text = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities).ToString();
                caution.SetActive(false);
            }
            else
            {
                //야수 세팅
                overRoll.text = Utils.TeamPowerUtils.calCardPower(player.getCard().abilities).ToString();
                caution.SetActive(player.getMissMatch());
            }
            selected.SetActive(false); 
        }


        public void Select()
        {
            changeManager.unSelectAll(changeType);
            changeManager.setOutPlayer(curPlayer, baseIndex);
            selected.SetActive(true);
        }

        public void Unselect()
        {
            selected.SetActive(false);
        }


        public void SetBaseIndex(int index)
        {
            baseIndex = index;
        }

    }
}