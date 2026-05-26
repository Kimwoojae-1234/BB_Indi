using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public interface UIEventHandler
{
    void OnRecieveEvent(GameDefine.eUIEvnet uiEvent);
}
public class UIBase : MonoBehaviour, UIEventHandler
{
    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// UI 생성 될때 여기서 초기화 처리 (SangHyuk)
    /// </summary>
    public virtual void Initialize()
    {
        return;
    }

    /// <summary>
    /// 삭제될 때 메모리 해제 되어야 하는 녀석들 여기서 처리 (SangHyuk)
    /// </summary>
    public virtual void Uninitialize()
    {
        Destroy(this);
        return;
    }

    public virtual void OpenUI()
    {
    }

    public virtual void CloseUI()
    {
        //MainManager.UI.PopStackUI(this.gameObject);
    }

    protected virtual void Start()
    {

    }
    
    protected virtual void Update()
    {

    }

    private void OnDestroy()
    {
        Uninitialize();
    }

    public virtual void OnRecieveEvent(GameDefine.eUIEvnet uiEvent)
    {
    }
}
