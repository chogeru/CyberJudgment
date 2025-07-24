using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoScript))]
public class ScriptEditorWindow : Editor
{
    private string originalScriptContent;
    private string editedScriptContent;
    private Vector2 scrollPosition;
    private bool isModified;

    private void OnEnable()
    {
        // スクリプトの内容を初期化
        MonoScript script = (MonoScript)target;
        originalScriptContent = script.text;
        editedScriptContent = originalScriptContent;
        isModified = false;
    }

    private void OnDisable()
    {
        // クリア処理
        originalScriptContent = null;
        editedScriptContent = null;
        isModified = false;
    }

    public override void OnInspectorGUI()
    {
        MonoScript script = (MonoScript)target;

        // スクリプトの内容を表示
        GUILayout.Label("スクリプト内容", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500));

        // テキストエリアの背景色を変更
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.gray;
        string newContent = EditorGUILayout.TextArea(editedScriptContent, GUILayout.ExpandHeight(true));
        GUI.backgroundColor = oldColor;

        EditorGUILayout.EndScrollView();

        if (newContent != editedScriptContent)
        {
            Undo.RecordObject(script, "編集 " + script.name);
            editedScriptContent = newContent;
            isModified = !string.Equals(editedScriptContent, originalScriptContent);
        }

        // 変更の保存ボタン
        EditorGUILayout.Space();
        GUI.enabled = isModified;
        if (GUILayout.Button("スクリプトを保存"))
        {
            if (EditorUtility.DisplayDialog("スクリプトの保存",
                "変更を保存しますか？", "もちろん", "いやだ"))
            {
                SaveScript(script, editedScriptContent);
            }
        }
        GUI.enabled = true;

        // 再描画を呼び出す
        Repaint();
    }

    private void SaveScript(MonoScript script, string content)
    {
        string path = AssetDatabase.GetAssetPath(script);
        if (AssetDatabase.IsOpenForEdit(path) == false)
        {
            EditorUtility.DisplayDialog("読み取り専用スクリプト", "このスクリプトは読み取り専用のため、編集できません", "OK");
            return;
        }

        try
        {
            File.WriteAllText(path, content);
            AssetDatabase.Refresh();
            originalScriptContent = content;
            isModified = false;
            Debug.Log("スクリプトを保存");
        }
        catch (UnauthorizedAccessException e)
        {
            HandleSaveError("アクセス権限がない", e);
        }
        catch (IOException e)
        {
            HandleSaveError("I/Oエラーが発生しました", e);
        }
        catch (System.Exception e)
        {
            HandleSaveError(e.Message, e);
        }
    }

    private void HandleSaveError(string message, System.Exception e)
    {
        Debug.LogError($"スクリプトの保存に失敗: {message}");
        EditorUtility.DisplayDialog("エラー", $"スクリプトの保存に失敗: {message}", "OK");
    }

}