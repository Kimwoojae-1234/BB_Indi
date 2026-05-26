using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFXManager : MonoBehaviour
{
    [SerializeField] private ItemAcquireFx Gold;
    [SerializeField] private ItemAcquireFx Gem;
    [SerializeField] private ItemAcquireFx GameToken;




    public void GoldGet(Transform par, Transform source, Transform target, GameConfig.Callback callback, float Size)
    {
        int randCount = 6;// Random.Range(1, 10);
        for (int i = 0; i < randCount; ++i)
        {
            var itemFx = GameObject.Instantiate<ItemAcquireFx>(Gold, par.transform);
            itemFx.transform.localScale = Vector3.one;
            if (i == randCount - 1)
            {
                itemFx.Explosion(source.position, target.position, Size, callback);
            }
            else
            {
                itemFx.Explosion(source.position, target.position, Size);
            }
        }
    }

    public void GemGet(Transform par, Transform source, Transform target, GameConfig.Callback callback, float Size)
    {
        int randCount = 6;// Random.Range(1, 10);
        for (int i = 0; i < randCount; ++i)
        {
            var itemFx = GameObject.Instantiate<ItemAcquireFx>(Gem, par.transform);
            itemFx.transform.localScale = Vector3.one;
            if (i == randCount - 1)
            {
                itemFx.Explosion(source.position, target.position, Size, callback);
            }
            else
            {
                itemFx.Explosion(source.position, target.position, Size);
            }
        }
    }

    public void GameTokenGet(Transform par, Transform source, Transform target, GameConfig.Callback callback, float Size)
    {
        int randCount = 6;// Random.Range(1, 10);
        for (int i = 0; i < randCount; ++i)
        {
            var itemFx = GameObject.Instantiate<ItemAcquireFx>(GameToken, par.transform);
            itemFx.transform.localScale = Vector3.one;
            if (i == randCount - 1)
            {
                itemFx.Explosion(source.position, target.position, Size, callback);
            }
            else
            {
                itemFx.Explosion(source.position, target.position, Size);
            }
        }
    }

}
