using LitJson;
using System;
using System.Collections.Generic;

public static class KOBTableUtil 
{
    /// <summary>
    /// JsonData를 Dictionary<TKey, TValue>로 변환합니다.
    /// </summary>
    /// <typeparam name="TKey">딕셔너리 키 타입 (예: int, string, long 등)</typeparam>
    /// <typeparam name="TValue">딕셔너리 값 타입</typeparam>
    /// <param name="jsonData">변환할 JsonData 객체</param>
    /// <param name="keyParser">문자열 키를 TKey 타입으로 변환하는 함수</param>
    /// <param name="valueParser">JsonData를 TValue 타입으로 변환하는 함수</param>
    /// <returns>Dictionary<TKey, TValue> 객체</returns>
    public static Dictionary<TKey, TValue> DeserializeDictionary<TKey, TValue>(
        JsonData jsonData,
        Func<string, TKey> keyParser,
        Func<JsonData, TValue> valueParser)
    {
        if (jsonData == null || !jsonData.IsObject)
        {
            //throw new ArgumentException("Invalid jsonData: not an object");
            UnityEngine.Debug.LogError("Invalid jsonData: not an object");
            return null;
        }

        var result = new Dictionary<TKey, TValue>();

        foreach (var key in jsonData.Keys)
        {
            TKey parsedKey = keyParser(key);
            TValue parsedValue = valueParser(jsonData[key]);
            result[parsedKey] = parsedValue;
        }

        return result;
    }

    /// <summary>
    /// JsonData 배열을 List<T>로 변환합니다.
    /// </summary>
    /// <typeparam name="T">리스트의 요소 타입</typeparam>
    /// <param name="jsonData">JsonData 배열</param>
    /// <param name="elementParser">각 요소를 T 타입으로 변환하는 함수</param>
    /// <returns>List<T> 객체</returns>
    public static List<T> DeserializeList<T>(
        JsonData jsonData,
        Func<JsonData, T> elementParser)
    {
        var result = new List<T>();

        if (jsonData == null || !jsonData.IsArray)
            throw new ArgumentException("Invalid jsonData: not an array");

        foreach (JsonData item in jsonData)
        {
            T parsedItem = elementParser(item);
            result.Add(parsedItem);
        }

        return result;
    }

    public static int ParseSafeInt(JsonData json, string key)
    {
        return json.ContainsKey(key) && !string.IsNullOrEmpty(json[key]?.ToString())
            ? int.Parse(json[key].ToString())
            : 0; // 기본값 0
    }

    public static string ParseSafeString(JsonData json, string key)
    {
        return json.ContainsKey(key) && !string.IsNullOrEmpty(json[key]?.ToString())
            ? json[key].ToString()
            : null; // 기본값 0
    }

    public static T ParseEnumFromJson<T>(JsonData json, string key, T defaultValue = default) where T : struct, Enum
    {
        if (json == null || !json.ContainsKey(key) || json[key] == null)
            return defaultValue;

        string value = json[key].ToString();

        return Enum.TryParse<T>(value, out T result) ? result : defaultValue;
    }
}
