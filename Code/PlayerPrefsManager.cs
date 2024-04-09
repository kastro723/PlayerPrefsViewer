using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeysWrapper
{
    public List<string> keys;
}

public class PlayerPrefsManager
{
    private static PlayerPrefsManager instance;
    public static PlayerPrefsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PlayerPrefsManager();
                instance.LoadKeys();
            }
            return instance;
        }
    }


    public static event Action OnPreferencesUpdated;

    // 이벤트를 트리거하는 메서드 추가 (캡슐화로 인해 직접적인 호출이 불가능하기 때문)
    public static void TriggerPreferencesUpdated()
    {
        OnPreferencesUpdated?.Invoke();
    }

    /// 

    private HashSet<string> keys = new HashSet<string>();
    private const string KeyStore = "PlayerPrefsKeys";

    public enum ValueType
    {
        String,
        Int,
        Float
    }

    private PlayerPrefsManager() { }

    public void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        SetType(key, ValueType.String);
        AddKey(key);
    }

    public string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        SetType(key,ValueType.Int);
        AddKey(key);
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    public void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        SetType(key,ValueType.Float);
        AddKey(key);
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public void RemoveKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        keys.Remove(key);
        SaveKeys();
    }

    public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        keys.Clear();
        SaveKeys();
    }

    public HashSet<string> GetAllKeys()
    {
        return keys;
    }

    private void AddKey(string key)
    {
        if (keys.Add(key))
        {
            SaveKeys();
        }
    }

    private void SaveKeys()
    {
        KeysWrapper keysWrapper = new KeysWrapper { keys = new List<string>(keys) };
        var json = JsonUtility.ToJson(keysWrapper);
        PlayerPrefs.SetString(KeyStore, json);
        PlayerPrefs.Save();

        TriggerPreferencesUpdated();
    }

    private void LoadKeys()
    {
        var json = PlayerPrefs.GetString(KeyStore, "{}");
        KeysWrapper keysWrapper = JsonUtility.FromJson<KeysWrapper>(json);
        if (keysWrapper != null && keysWrapper.keys != null)
        {
            keys = new HashSet<string>(keysWrapper.keys);
        }
        else
        {
            keys = new HashSet<string>();
        }
    }

    private void SetType(string key, ValueType type)
    {
        PlayerPrefs.SetString(key + "_type", type.ToString());
    }

    public ValueType GetType(string key)
    {
        var typeStr = PlayerPrefs.GetString(key + "_type", ValueType.String.ToString());
        return (ValueType)Enum.Parse(typeof(ValueType), typeStr);
        
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    public bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
   
}