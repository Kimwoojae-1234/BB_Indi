using System;
using System.Collections.Generic;
using static UIPopup;
/// <summary>
/// UIPopupActivity 에서 주로 사용된 Intent 클래스
/// 데이터 번들을 전달하는 기능이 주가 되겠지만 추가적으로
/// INTENT_ACTION 을 통해 데이터 번들이 아닌, 액션전달용으로도 사용합니다. ( 설계예정입니다 )
/// </summary>

public class Intent : IDisposable
{
    protected Dictionary<string, object> _intentData = new Dictionary<string, object>();
    protected const string CHARACTER_ID = "CHARACTER_ID";
    protected bool _disposed;

    public Intent()
    {
    }

    public Intent PrePopup()
    {
        AddIntentData<bool>(UIPopup.PRE_POPUP, true);
        return this;
    }

    public Intent SetOK(OnClickAction onOK)
    {
        AddIntentData<OnClickAction>(UIPopup.ON_OK, onOK);
        return this;
    }
    public Intent SetClose(OnClickAction onClose)
    {
        AddIntentData<OnClickAction>(UIPopup.ON_CLOSE, onClose);
        return this;
    }


    public virtual void Dispose()
    {
        _disposed = true;
        _intentData.Clear();
    }

    public bool Contains(string key)
    {
        return _intentData.ContainsKey(key);
    }

    // doo 추가 
    public bool Contains(System.Enum key)
    {
        return Contains(key.ToString());
    }

	public void RemoveIntentData( System.Enum key )
	{
		RemoveIntentData( key.ToString() );
	}

	public void RemoveIntentData( string key )
	{
		if( _intentData.ContainsKey( key ) == false )
			return;

		_intentData.Remove( key );
	}

    public void AddDestroyableUnityData<T>(string key, T data) where T : UnityEngine.Object
    {
        if (_disposed)
        {
            if (data) UnityEngine.Object.Destroy(data);
            return;
        }
        _intentData.Add( key, data );
    }

	public void AddIntentData<T>(string key, T value)
    {
        _intentData.Add(key, value);
    }

    
    public void SafeAddIntentData<T>(string key, T value)
    {
        _intentData[key] = value;
    }
    
    // doo 추가
    public void AddIntentData<T>(System.Enum key, T value)
    {
        _intentData.Add(key.ToString() , value);
    }

    public void AddIntentData(string key, object value)
    {
        _intentData.Add(key, value);
    }

    // doo 추가
    public void AddIntentData(System.Enum key, object value)
    {
        _intentData.Add(key.ToString() , value);
    }

    public object this[string key]
    {
        get
        {
            object data;
            _intentData.TryGetValue(key, out data);

            return data;
        }

        set
        {
            _intentData[key] = value;
        }
    }

    // doo : 추가 
    public object this[System.Enum key]
    {
        get
        {
            object data;
            _intentData.TryGetValue(key.ToString() , out data);

            return data;
        }

        set
        {
            _intentData[key.ToString()] = value;
        }
    }

    public void ChangeValue<T>(string key, T value)
    {
        if (false == _intentData.ContainsKey(key))
        {
            return;
        }

        if (_intentData[key].GetType() != typeof(T))
        {
            return;
        }

        _intentData[key] = value;
    }
    // doo 추가
    public void ChangeValue<T>(System.Enum key, T value)
    {
        ChangeValue<T>(key.ToString() , value);
    }

    public T GetValue<T>(string key)
    {
        object o = null;
        _intentData.TryGetValue(key, out o);

        if (o != null && o.GetType() == typeof(T))
        {
            return (T)o;
        }
        else
        {
            return default(T);
        }
    }

    // doo 추가
    public T GetValue<T>(System.Enum key)
    {
        return GetValue<T>(key.ToString());
    }

    public T PopValue<T>(string key)
    {
        if (_intentData.TryGetValue(key, out object o))
        {
            _intentData.Remove(key);
            return (T)o;
        }
        return default(T);
    }


    public void SetCharacterId(int id)
    {
        _intentData[CHARACTER_ID] = id;
    }
}

public class IntentEx : Intent
{
    public bool WasDisposed => this._disposed;
    public override void Dispose()
    {
        OnDynamicAdded = null;
        base.Dispose();
    }

    public event Action OnDynamicAdded;

    public void NotifyDynamicAdded()
    {
        if (_disposed) return;
        OnDynamicAdded?.Invoke();
    }
}
