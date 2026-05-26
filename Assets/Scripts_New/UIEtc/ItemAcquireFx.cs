using UnityEngine;
using DG.Tweening;

public class ItemAcquireFx : MonoBehaviour
{
    private GameConfig.Callback CallBack = null;

    public void Explosion(Vector2 from, Vector2 to, float explo_range, GameConfig.Callback callback = null)
    {
        CallBack = callback;
        transform.position = from;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(from + Random.insideUnitCircle * explo_range, 0.25f).SetEase(Ease.OutCubic));
        sequence.Append(transform.DOMove(to, 0.5f).SetEase(Ease.InCubic));
        sequence.AppendCallback(() => { DestroyObj(); });
    }


    private void DestroyObj()
    {
        gameObject.SetActive(false);
        Destroy(gameObject, 0.3f);
        if(CallBack != null)
        {
            CallBack();
        }
        CallBack = null;
    }
}