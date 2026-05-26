using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class changeController : MonoBehaviour
    {
        public UI_CardSmall card;
        public GameObject select;

        private UIPlayerChange changeManager;
        private BoxCollider col;
        //private UISprite backGround;

        private bool bSelected;

        private CPlayer curPlayer;

        public void Init(UIPlayerChange _changeManager, CPlayer player)
        {
            bSelected = false;
            changeManager = _changeManager;
            curPlayer = player;
            col = GetComponent<BoxCollider>();
            //backGround = GetComponent<UISprite>();

            CardData data = new CardData(player.getCard());
            card.SetCardInfo(data);

            select.SetActive(false);
            //backGround.enabled = false;
        }


        public void Select()
        {
            if (bSelected == false)
            {
                changeManager.unSelectCardAll();
                changeManager.setInPlayer(curPlayer);
                //backGround.enabled = true;
                select.SetActive(true);
                bSelected = true;
            }
            else
            {
                changeManager.setInPlayerNone();
                Unselect();                
            }
        }

        public void Unselect()
        {
            //backGround.enabled = false;
            select.SetActive(false);
            bSelected = false;
        }


        public void SelectChangePlayer(CPlayer player)
        {
            GetComponent<UISprite>().enabled = true;
            select.SetActive(true);
            card.gameObject.SetActive(true);
            CardData data = new CardData(player.getCard());
            card.SetCardInfo(data);
        }

        public void SelectNone()
        {
            select.SetActive(false);
            card.gameObject.SetActive(false);
        }

    }
}