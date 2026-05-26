using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItem : UIBase
{
    private Transform parentWindow = null;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Uninitialize()
    {
        base.Uninitialize();
    }

    protected override void Update()
    {
        base.Update();
    }

    public Transform GetParentWindow()
    {
        if (parentWindow == null)
            return null;
        return parentWindow;
    }

    public virtual float GetItemHeight()
    {
        RectTransform rectTrs = this.transform as RectTransform;
        return rectTrs.sizeDelta.y;
    }

    public virtual void SetParentWidnow(Transform parent_window)
    {
        parentWindow = parent_window;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localPosition = Vector3.zero;
    }
}
