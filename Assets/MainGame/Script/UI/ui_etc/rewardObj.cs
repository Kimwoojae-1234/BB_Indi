using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BaseBall.BallPlay
{
    public class rewardObj : MonoBehaviour
    {
        private int order;
        public GameObject _active;
        public GameObject light;
        public GameObject big, small;


        private GameObject itemName;

        public void Init(int id, int getNum, ItemSlot.ItemSize size, ItemSlot.ItemType type, int _order)
        {
            transform.localScale = Vector3.one;
           
            itemName.SetActive(false);

            StartCoroutine(setEffect(size));            
        }


        private IEnumerator setEffect(ItemSlot.ItemSize size)
        {
            yield return new WaitForSeconds(0.5f);
            TweenAlpha.Begin(light, 0.4f, 0.65f);
            yield return new WaitForSeconds(0.5f + (order*0.5f));
            _active.SetActive(true);
            if (size == ItemSlot.ItemSize.BIG) big.SetActive(true);
            else small.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            itemName.SetActive(true);
        }
    }
}
