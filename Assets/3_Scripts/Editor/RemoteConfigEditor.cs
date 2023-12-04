using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

/// <summary>Editor window used to change RemoteConfig values during runtime</summary>
class RemoteConfigEditor : EditorWindow
{
    // TODO : Add support for dynamic changing of RemoteConfig variables

    [MenuItem("Tools/Configuration Editor")]
    static void ShowWindow()
    {
        var window = GetWindow<RemoteConfigEditor>();
        window.titleContent = new GUIContent("Configuration editor");
        window.window = window;
        window.Show();
    }

    RemoteConfigEditor window;
    GUIStyle centerTitleLabel;
    FieldInfo[] fields;

    void OnGUI()
    {
        GenerateIfNeeded();

        EditorGUILayout.LabelField("Configuration editor", centerTitleLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Please start the game to use ths tool.", MessageType.Warning);
            fields = null;
        }
        else
        {
            if (fields == null)
                fields = typeof(RemoteConfig).GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (FieldInfo field in fields)
                AutoDisplay(field);
        }
    }

    void GenerateIfNeeded()
    {
        if (window == null)
            window = GetWindow<RemoteConfigEditor>();

        if (centerTitleLabel == null)
        {
            centerTitleLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }
    }

    void AutoDisplay(FieldInfo field)
    {
        string fieldName = NicifyFieldName(field);

        Action DisplayMethod = field.FieldType switch
        {
            Type t when t == typeof(bool) => () => field.SetValue(null, EditorGUILayout.Toggle(fieldName, (bool)field.GetValue(null))),
            Type t when t == typeof(int) => () => field.SetValue(null, EditorGUILayout.IntField(fieldName, (int)field.GetValue(null))),
            Type t when t == typeof(float) => () => field.SetValue(null, EditorGUILayout.FloatField(fieldName, (float)field.GetValue(null))),
            _ => () => EditorGUILayout.HelpBox("Unsupported type " + field.FieldType + " for field " + field.Name, MessageType.Error)
        };

        DisplayMethod?.Invoke();
    }

    string NicifyFieldName(FieldInfo field)
    {
        string result = string.Empty;
        string[] frags = field.Name.Split('_');

        for (int i = 1; i < frags.Length; i++)
        {
            string word = frags[i].ToLower();
            string firstLetter = i == 1 ? word[0].ToString().ToUpper() : word[0].ToString();
            word = word.TrimStart(word[0]).Insert(0, firstLetter);

            result += word + " ";
        }

        return result.TrimEnd(' ');
    }
}