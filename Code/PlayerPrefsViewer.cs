using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private string newKey = "";
    private string newValue = "";
    private int selectedTypeIndex = 0;
    private string[] typeOptions = { "String", "Int", "Float" };
    private Dictionary<string, (string Value, PlayerPrefsManager.ValueType Type)> playerPrefsCache = new Dictionary<string, (string, PlayerPrefsManager.ValueType)>();
    private List<string> keysToRemove = new List<string>(); // 삭제할 키들을 저장할 리스트


    [MenuItem("Tools/Player Prefs Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerPrefsViewer>("Player Prefs Viewer");
        window.maxSize = new Vector2(495, window.maxSize.y);
        window.minSize = new Vector2(495, 300);
        window.Show();
    }

    void OnEnable() // 창이 열리거나 포커스를 받을 때 이벤트 구독
    {
        PlayerPrefsManager.OnPreferencesUpdated += RefreshPlayerPrefsCache;
    }

    void OnDisable() // 창이 닫히거나 포커스를 잃을 때 메모리 누수를 방지하기 위해 이벤트 구독 해제
    {
        PlayerPrefsManager.OnPreferencesUpdated -= RefreshPlayerPrefsCache;
    }

    void OnGUI()
    {
        GUILayout.Label("Ver. 1.1.0", EditorStyles.boldLabel);
        DrawLine();

        if (GUILayout.Button("Refresh List"))
        {
            RefreshPlayerPrefsCache();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        List<string> toRemove = new List<string>(); // 삭제할 항목을 임시 저장할 리스트

        foreach (var kvp in playerPrefsCache)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(kvp.Key, GUILayout.Width(100));
            GUILayout.Label(kvp.Value.Value, GUILayout.Width(200));
            GUILayout.Label(kvp.Value.Type.ToString(), GUILayout.Width(50));

            if (GUILayout.Button("Modify", GUILayout.Width(60)))
            {
                PlayerPrefsEditWindow.Open(kvp.Key, kvp.Value.Value, kvp.Value.Type);
            }


            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                bool confirm = EditorUtility.DisplayDialog("Confirm Removal", $"Are you sure you want to remove {kvp.Key}?", "Yes", "No");
                if (confirm)
                {
                    toRemove.Add(kvp.Key); // 삭제 리스트에 추가
                }
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        foreach (var key in toRemove)
        {
            PlayerPrefsManager.Instance.RemoveKey(key);
            playerPrefsCache.Remove(key); // 캐시에서도 삭제
        }

        DrawAddKeyValueSection();

        if (GUILayout.Button("Remove All"))
        {
            bool confirm = EditorUtility.DisplayDialog("Confirm Removal", "Are you sure you want to remove all PlayerPrefs?", "Yes", "No");
            if (confirm)
            {
                PlayerPrefsManager.Instance.ClearAll();
                RefreshPlayerPrefsCache();
            }
        }


        ProcessRemovals(); // 삭제할 키들을 리스트에 모은 다음, GUI 이벤트 처리가 모두 끝난 후에 한 번에 삭제를 처리
    }

    private void RefreshPlayerPrefsCache()
    {
        playerPrefsCache.Clear();
        var allKeys = PlayerPrefsManager.Instance.GetAllKeys();
        foreach (var key in allKeys)
        {
            var type = PlayerPrefsManager.Instance.GetType(key);
            string value = "";
            switch (type)
            {
                case PlayerPrefsManager.ValueType.String:
                    value = PlayerPrefsManager.Instance.GetString(key);
                    break;
                case PlayerPrefsManager.ValueType.Int:
                    value = PlayerPrefsManager.Instance.GetInt(key).ToString();
                    break;
                case PlayerPrefsManager.ValueType.Float:
                    value = PlayerPrefsManager.Instance.GetFloat(key).ToString();
                    break;
            }
            playerPrefsCache[key] = (value, type);
        }
    }

    private void DrawAddKeyValueSection()
    {
        GUILayout.Space(10);
        DrawLine();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Key", GUILayout.Width(100));
        GUILayout.Label("Value", GUILayout.Width(200));
        GUILayout.Label("Type", GUILayout.Width(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        newKey = GUILayout.TextField(newKey, GUILayout.Width(100));
        newValue = GUILayout.TextField(newValue, GUILayout.Width(200));
        selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, typeOptions, GUILayout.Width(70));

        if (GUILayout.Button("Add", GUILayout.Width(105)))
        {
            AddKeyValue();
        }
        GUILayout.EndHorizontal();
    }

    private void AddKeyValue()
    {
        if (!string.IsNullOrEmpty(newKey))
        {
            // 이미 존재하는 키인지 확인
            if (PlayerPrefsManager.Instance.GetAllKeys().Contains(newKey))
            {
                EditorUtility.DisplayDialog("Duplicate Key", "A key with the same name already exists. Please use a different key.", "OK");
                return; 
            }

            PlayerPrefsManager.ValueType selectedType = (PlayerPrefsManager.ValueType)Enum.Parse(typeof(PlayerPrefsManager.ValueType), typeOptions[selectedTypeIndex]);
            if (selectedType == PlayerPrefsManager.ValueType.Int)
            {
                if (int.TryParse(newValue, out int intValue))
                {
                    PlayerPrefsManager.Instance.SetInt(newKey, intValue);
                    PlayerPrefsManager.Instance.SetType(newKey, selectedType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Value", "For an integer value, please enter a valid integer.", "OK");
                    return;
                }
            }
            else if (selectedType == PlayerPrefsManager.ValueType.Float)
            {
                if (float.TryParse(newValue, out float floatValue))
                {
                    PlayerPrefsManager.Instance.SetFloat(newKey, floatValue);
                    PlayerPrefsManager.Instance.SetType(newKey, selectedType);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Value", "Please enter floating point values ​​correctly.", "OK");
                    return;
                }
            }
            else // String 타입 처리
            {
                PlayerPrefsManager.Instance.SetString(newKey, newValue);
                PlayerPrefsManager.Instance.SetType(newKey, selectedType);
            }

            // 새로운 키 추가 후 필드 초기화
            newKey = "";
            newValue = "";
            selectedTypeIndex = 0;
            RefreshPlayerPrefsCache(); // PlayerPrefs 캐시 갱신
        }
    }


    private void ProcessRemovals()
    {
        foreach (var key in keysToRemove)
        {
            PlayerPrefsManager.Instance.RemoveKey(key);
        }
        if (keysToRemove.Count > 0)
        {
            RefreshPlayerPrefsCache();
            keysToRemove.Clear();
        }
    }

    private void DrawLine()
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), Color.gray);
    }
}