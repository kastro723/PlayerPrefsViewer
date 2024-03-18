using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditWindow : EditorWindow
{
    private string originalKey;
    private string originalValue; // 원래 값 보관을 위한 변수
    private PlayerPrefsManager.ValueType originalType;
    private string key;
    private string value;
    private PlayerPrefsManager.ValueType selectedType;

    public static void Open(string key, string value, PlayerPrefsManager.ValueType type)
    {
        var window = GetWindow<PlayerPrefsEditWindow>("Modify PlayerPrefs");
        window.originalKey = key;

        window.originalValue = value; // 원래 값 초기화
        window.originalType = type; // 원래 타입 초기화

        window.key = key;
        window.value = value;
        window.selectedType = type;
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Current Key-Value Pair", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Key: " + originalKey);
        EditorGUILayout.LabelField("Value: " + originalValue);
        EditorGUILayout.LabelField("Type: " + originalType.ToString());


        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Modify Key-Value Pair", EditorStyles.boldLabel);

        GUI.enabled = false;
        key = EditorGUILayout.TextField("Key", key);
        GUI.enabled = true;

        value = EditorGUILayout.TextField("Value", value);
        selectedType = (PlayerPrefsManager.ValueType)EditorGUILayout.EnumPopup("Type", selectedType);

        if (GUILayout.Button("Modify"))
        {
            if (EditorUtility.DisplayDialog("Confirm Modification", "Do you want to modify this PlayerPrefs entry?", "Yes", "No"))
            {
                ModifyPlayerPrefs();
            }
        }
    }

    private void ModifyPlayerPrefs()
    {

        if (selectedType == PlayerPrefsManager.ValueType.String)
        {
            PlayerPrefsManager.Instance.SetString(key, value);
            PlayerPrefsManager.Instance.SetType(key, PlayerPrefsManager.ValueType.String);
        }
        else if (selectedType == PlayerPrefsManager.ValueType.Int && int.TryParse(value, out int intValue))
        {
            PlayerPrefsManager.Instance.SetInt(key, intValue);
            PlayerPrefsManager.Instance.SetType(key, PlayerPrefsManager.ValueType.Int);
        }
        else if (selectedType == PlayerPrefsManager.ValueType.Float && float.TryParse(value, out float floatValue))
        {
            PlayerPrefsManager.Instance.SetFloat(key, floatValue);
            PlayerPrefsManager.Instance.SetType(key, PlayerPrefsManager.ValueType.Float); // 타입 정보 업데이트
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid Input", "The provided value does not match the selected type.", "OK");
            return;
        }

        PlayerPrefsManager.TriggerPreferencesUpdated();
        Close();
    }

}