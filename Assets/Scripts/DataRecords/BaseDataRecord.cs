using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// 기본 데이터 레코드 이것을 상속받아 사용
/// </summary>
[Serializable]
public class BaseDataRecord
{
    public virtual bool Initialize()
    {
        return true;
    }

    public virtual bool Uninitialize()
    {
        return true;
    }
}
