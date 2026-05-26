
using UnityEngine;
using UnityEngine.UI;

public static class UIUtil
{
    public static void ScrollTo(ScrollRect scrollRect, int index)
    {
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        int itemCount = content.childCount;

        if (itemCount == 0) return;

        // 인덱스 범위 제한
        index = Mathf.Clamp(index, 0, itemCount - 1);

        RectTransform target = content.GetChild(index) as RectTransform;

        // 스크롤 가능한 범위
        float scrollableHeight = content.rect.height - viewport.rect.height;
        if (scrollableHeight <= 0)
        {
            scrollRect.verticalNormalizedPosition = 1; // 스크롤 필요 없음
            return;
        }

        // 타겟 위치를 중앙 기준으로 계산
        // Content pivot이 상단(1,1) 기준이라면 localPosition.y는 음수 증가
        float targetPosY = -target.localPosition.y + (target.rect.height / 2) - (viewport.rect.height / 2);

        // normalizedPosition 계산 (1 = 맨 위, 0 = 맨 아래)
        float normalized = 1 - (targetPosY / scrollableHeight);
        normalized = Mathf.Clamp01(normalized);

        scrollRect.verticalNormalizedPosition = normalized;
    }



    public static void RemoveChild(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child != null)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}
