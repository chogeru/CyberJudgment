using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;


public class BatchComponentDestroyer : EditorWindow
{
    //愛らしいオブジェクトを格納するための変数ですわ
    private GameObject m_SelectedObject;
    //優雅な選択されたコンポーネントを格納するための変数ですの
    private Component m_SelectedComponent;
    //オブジェクトの全コンポーネントを保持するリストですわ
    private List<Component> m_AllComponents = new List<Component>();
    //GUIのポップアップで選択されたコンポーネントのインデックスですわね
    private int m_SelectedComponentIndex = 0;
    //コンポーネントの名前を保持する配列ですのよ
    private string[] m_ComponentNames;

    [MenuItem("HAYASHI/一括コンポーネント破壊ヤー")]
    public static void ShowWindow()
    {
        //エディタウィンドウを繊細に表示するのですわ
        GetWindow<BatchComponentDestroyer>("一括コンポーネント破壊ヤー"); 
    }

    private void OnEnable()
    {
        //ウィンドウが有効になった際に、選択されたオブジェクトを優雅に更新するのですわ
        UpdateSelectedObject(); 
    }

    private void OnGUI()
    {
        if (GUILayout.Button("選択されたオブジェクトを更新"))
        {
            //選択されたオブジェクトを更新するボタンですわ
            UpdateSelectedObject();
        }

        if (m_SelectedObject != null)
        {
            //選択されたオブジェクトの名前を表示するのですわ
            EditorGUILayout.LabelField("選択されたオブジェクト: " + m_SelectedObject.name);

            if (m_AllComponents.Any())
            {
                //コンポーネントを選択するポップアップメニューですわ
                m_SelectedComponentIndex = EditorGUILayout.Popup("コンポーネント選択", m_SelectedComponentIndex, m_ComponentNames);
                //選択されたコンポーネントを更新するのです
                m_SelectedComponent = m_AllComponents[Mathf.Clamp(m_SelectedComponentIndex, 0, m_AllComponents.Count - 1)];

                if (GUILayout.Button("選択したコンポーネントと同名の全コンポーネントを削除"))
                {
                    //選択されたコンポーネントと同名のコンポーネントを全て削除するのですわ
                    RemoveComponentsWithSameName();
                }
            }
            else
            {
                //コンポーネントが存在しない場合の対処ですわ
                EditorGUILayout.LabelField("コンポーネントが見つからないですわ");
            }
        }
        else
        {
            //オブジェクトが選択されていない場合のメッセージですわ
            EditorGUILayout.LabelField("オブジェクトが選択されてませんこと");
        }
    }

    private void UpdateSelectedObject()
    {
        //アクティブなゲームオブジェクトを選択するのですわ
        m_SelectedObject = Selection.activeGameObject;
        //コンポーネントリストを優雅に更新するのですわ
        UpdateComponentList();
    }


    private void UpdateComponentList()
    {
        if (m_SelectedObject != null)
        {
            m_AllComponents = m_SelectedObject.GetComponentsInChildren<Component>().ToList();
            var uniqueComponentTypes = m_AllComponents.Select(c => c.GetType()).Distinct()
                                                    .Where(t => t != typeof(Transform)).ToList(); //Transformコンポーネントは除外するのですわ

            m_ComponentNames = uniqueComponentTypes.Select(t => t.Name).ToArray(); //コンポーネントの名前の配列を更新するのです
            //各コンポーネントタイプに対して、子オブジェクトからそのタイプのコンポーネントを取得してリストを更新しますわ
            m_AllComponents = uniqueComponentTypes.Select(t => m_SelectedObject.GetComponentInChildren(t)).ToList();
        }
    }
    private void RemoveComponentsWithSameName()
    {
        if (m_SelectedComponent != null)
        {
            //選択されたコンポーネントの名前を取得するのですわ
            string componentName = m_SelectedComponent.GetType().Name;
            //選択された名前と同じ名前のコンポーネントをすべて取得し、Transform コンポーネントは除外するのです
            Component[] componentsToRemove = m_SelectedObject.GetComponentsInChildren<Component>()
                .Where(c => c.GetType().Name == componentName && !(c is Transform)).ToArray();

            foreach (Component comp in componentsToRemove)
            {
                //優雅にコンポーネントを破壊するのですわ
                DestroyImmediate(comp);
            }
            //コンポーネントリストを再度更新するのですわ
            UpdateComponentList();
            //オブジェクトの変更をエディタに通知するのです
            EditorUtility.SetDirty(m_SelectedObject);

            //インデックスを適切に調整するのですわ
            if (m_AllComponents.Count == 0)
            {
                //コンポーネントがない場合は0にリセットするのですわ
                m_SelectedComponentIndex = 0;
            }
            else
            {
                //他にコンポーネントが存在する場合は、選択インデックスを適切な範囲内で調整するのですわ
                m_SelectedComponentIndex = Mathf.Min(m_SelectedComponentIndex, m_AllComponents.Count - 1);
            }
        }
    }
}