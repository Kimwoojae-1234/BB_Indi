using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Text;

public class JsonHelper
{
    public static string SerializeObject(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T DeserializeObject<T>(string JsonData) 
    {
        return JsonConvert.DeserializeObject<T>(JsonData);
    }

    public static void DeleteJsonFile(string loadPath, string fileName)
    {
        string path = Path.Combine(loadPath, fileName + ".json");
        if (File.Exists(path) == true)
        {
            File.Delete(path);
        }
    }


    public static void CreateJsonFile(string createPath, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", createPath, fileName), FileMode.Create);
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(jsonData);
            fileStream.Write(data, 0, data.Length);
        }
        catch(Exception e)
        {
            Debug.LogError(string.Format("Create Json File Fail : {0}", e));
        }
        finally
        {
            fileStream.Close();
        }
        
    }

    public static T LoadJsonFile<T>(string loadPath, string fileName)
    {
        try
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string path = Path.Combine(loadPath, fileName + ".json");
            FileStream fileStream = new FileStream(path, FileMode.Open);
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
            fileStream.Close();
            string jsonData = Encoding.UTF8.GetString(data);
            return DeserializeObject<T>(jsonData);
        }
        catch(Exception e)
        {
            Debug.LogError(string.Format("Load Json File Fail : {0}", e));
            return default;
        }
    }

    public static string LoadJson(string loadPath, string fileName)
    {
        try
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string path = Path.Combine(loadPath, fileName + ".json");
            FileStream fileStream = new FileStream(path, FileMode.Open);
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
            fileStream.Close();
            string jsonData = Encoding.UTF8.GetString(data);
            return jsonData;
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Load Json File Fail : {0}", e));
            return null;
        }
    }
}
