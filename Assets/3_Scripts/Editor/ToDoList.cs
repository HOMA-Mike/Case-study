using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Editor window to get all "TODO" tasks from scripts</summary>
class ToDoList : EditorWindow
{
    ExcludedList _excludedScripts;
    ExcludedList excludedScripts
    {
        get
        {
            if (_excludedScripts == null)
            {
                if (PlayerPrefs.HasKey("excludedScriptsToDo"))
                {
                    _excludedScripts = JsonUtility.FromJson<ExcludedList>(
                        PlayerPrefs.GetString("excludedScriptsToDo")
                    );
                }
                else
                    _excludedScripts = new ExcludedList();
            }

            return _excludedScripts;
        }
    }

    GUIStyle boldCenteredStyle;
    GUIStyle centeredStyle;
    GUIStyle boldStyle;
    GUIStyle frameStyle;
    GUIStyle boldButtonStyle;

    List<TextAsset> scripts;
    List<TextAsset> texts;
    ToDoList window;
    Vector2 scrollPos;
    Vector2 excludeScroll;
    int toDoCount;
    int selectedScriptIndex;

    [MenuItem("Tools/ToDoList")]
    static void ShowWindow()
    {
        ToDoList toDoList = GetWindow<ToDoList>();
        toDoList.titleContent = new GUIContent("ToDoList");

        toDoList.GetAllProjectScriptsWithToDos();

        toDoList.window = toDoList;
        toDoList.Show();
    }

    void GenerateRequirement()
    {
        if (window == null)
            window = GetWindow<ToDoList>();

        if (boldCenteredStyle == null)
        {
            boldCenteredStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        if (centeredStyle == null)
            centeredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        if (boldStyle == null)
            boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

        if (frameStyle == null)
        {
            frameStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                richText = true
            };
        }

        if (boldButtonStyle == null)
        {
            boldButtonStyle = new GUIStyle(GUI.skin.box)
            {
                fontStyle = FontStyle.Bold,
                stretchWidth = true,
                alignment = TextAnchor.MiddleLeft
            };
        }
    }

    void OnGUI()
    {
        GenerateRequirement();

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("ToDo list", boldCenteredStyle);
            EditorGUILayout.Space();

            DisplayList();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Task count : " + toDoCount, centeredStyle);

            EditorGUILayout.Space();

            ShowExclusions();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Refresh"))
                    GetAllProjectScriptsWithToDos();

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }
        EditorGUILayout.EndVertical();
    }

    void GetAllProjectScriptsWithToDos()
    {
        string[] assetsPaths = AssetDatabase.GetAllAssetPaths();

        scripts = new List<TextAsset>();
        texts = new List<TextAsset>();

        foreach (string assetPath in assetsPaths)
        {
            if (!assetPath.Contains("Packages") && !assetPath.Contains(GetType().ToString()))
            {
                if (assetPath.EndsWith(".cs"))
                {
                    TextAsset script = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

                    if (!excludedScripts.scriptsNames.Contains(script.name) && ScriptContainsToDo(script.text))
                        scripts.Add(script);
                }

                if (assetPath.EndsWith(".todo"))
                {
                    TextAsset textFile = new TextAsset(File.ReadAllText(assetPath));

                    string[] lines = assetPath.Split('/');
                    textFile.name = lines[lines.Length - 1];

                    if (TextContainsToDo(textFile.text))
                        texts.Add(textFile);
                }
            }
        }
    }

    void DisplayList()
    {
        if (scripts == null || texts == null || (scripts.Count == 0 && texts.Count == 0))
            GetAllProjectScriptsWithToDos();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);
        {
            EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
            {
                toDoCount = 0;

                foreach (TextAsset text in texts)
                {
                    if (text == null)
                        continue;

                    if (GUILayout.Button(text.name, boldButtonStyle))
                        AssetDatabase.OpenAsset(text);

                    string[] toDos = GetTextToDo(text);
                    string content = string.Empty;

                    foreach (string toDo in toDos)
                    {
                        toDoCount++;
                        content += toDo + "\n";
                    }

                    if (content.Length > 0)
                        content = content.TrimEnd('\n');

                    EditorGUILayout.TextArea(content, frameStyle, GUILayout.MaxWidth(window.position.width));
                    EditorGUILayout.Space();
                }

                foreach (TextAsset script in scripts)
                {
                    if (script == null)
                        continue;

                    if (GUILayout.Button(script.name + ".cs", boldButtonStyle))
                        AssetDatabase.OpenAsset(script);

                    Todo[] toDos = GetScriptToDo(script);

                    foreach (Todo toDo in toDos)
                    {
                        toDoCount++;

                        if (GUILayout.Button(
                            "- " + toDo.text,
                            frameStyle,
                            GUILayout.MaxWidth(window.position.width)
                        ))
                            AssetDatabase.OpenAsset(script, toDo.lineIndex);
                    }

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }

    void ShowExclusions()
    {
        EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
        {
            EditorGUILayout.LabelField("Excluded scripts", boldCenteredStyle);
            EditorGUILayout.Space();

            excludeScroll = EditorGUILayout.BeginScrollView(excludeScroll);
            {
                List<string> toRemove = new List<string>();

                foreach (string script in excludedScripts.scriptsNames)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(script);

                        if (GUILayout.Button("Remove"))
                            toRemove.Add(script);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (toRemove.Count > 0)
                {
                    toRemove.ForEach(script => excludedScripts.scriptsNames.Remove(script));
                    SaveExcludes();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                string[] allAssetsPaths = AssetDatabase.GetAllAssetPaths();
                List<string> fileNames = new List<string>();

                foreach (string assetPath in allAssetsPaths)
                {
                    if (!assetPath.Contains("Packages") && assetPath.EndsWith(".cs"))
                    {
                        TextAsset script = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

                        if (!excludedScripts.scriptsNames.Contains(script.name))
                            fileNames.Add(script.name);
                    }
                }

                selectedScriptIndex = EditorGUILayout.Popup(selectedScriptIndex, fileNames.ToArray());

                if (GUILayout.Button("Add exception"))
                {
                    excludedScripts.scriptsNames.Add(fileNames[selectedScriptIndex]);
                    selectedScriptIndex = 0;

                    TextAsset selected = scripts.Find(item => item.name == fileNames[selectedScriptIndex]);

                    if (selected != null)
                        scripts.Remove(selected);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    void SaveExcludes()
    {
        PlayerPrefs.SetString("excludedScriptsToDo", JsonUtility.ToJson(excludedScripts));
    }

    Todo[] GetScriptToDo(TextAsset script)
    {
        string[] lines = script.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<Todo> toDo = new List<Todo>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (ScriptContainsToDo(lines[i]))
            {
                string[] words = lines[i].Split(new string[] { "TODO", ":" }, System.StringSplitOptions.RemoveEmptyEntries);

                if (words[1] == " ")
                    toDo.Add(new Todo(words[2].TrimStart(' '), i + 1));
                else
                    toDo.Add(new Todo(words[1].Trim(' '), i + 1));
            }
        }

        return toDo.ToArray();
    }

    string[] GetTextToDo(TextAsset text)
    {
        string[] lines = text.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        List<string> toDo = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i][0] == '-')
                toDo.Add(lines[i]);
        }

        return toDo.ToArray();
    }

    bool ScriptContainsToDo(string text)
    {
        return text.Replace(" ", "").Contains("//TODO:");
    }

    bool TextContainsToDo(string text)
    {
        return text.Contains("\n- ");
    }

    /// <summary>Stores excluded script names</summary>
    [Serializable]
    class ExcludedList
    {
        public List<string> scriptsNames;

        public ExcludedList() => scriptsNames = new List<string>();
    }

    class Todo
    {
        public string text;
        public int lineIndex;

        public Todo(string text, int lineIndex)
        {
            this.text = text;
            this.lineIndex = lineIndex;
        }
    }
}