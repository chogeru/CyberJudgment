using UnityEngine;
using UnityEditor;

public class RemoveMissingComponentsEditor
{
    [MenuItem("HAYASHI/選択したオブジェクトの全ての子からMissingコンポーネントを削除")]
    public static void RemoveMissingComponentsFromChildren()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていない");
            return;
        }

        foreach (GameObject obj in selectedObjects)
        {
            RemoveMissingRecursively(obj);
        }

        Debug.Log("Missingコンポーネントの削除を完了");
    }

    private static void RemoveMissingRecursively(GameObject obj)
    {
        Undo.RegisterCompleteObjectUndo(obj, "Missingコンポーネント削除");

        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);

        if (removedCount > 0)
        {
            Debug.Log($"{obj.name} から {removedCount} 個のMissingコンポーネントを削除");
        }

        foreach (Transform child in obj.transform)
        {
            RemoveMissingRecursively(child.gameObject);
        }
    }
}
