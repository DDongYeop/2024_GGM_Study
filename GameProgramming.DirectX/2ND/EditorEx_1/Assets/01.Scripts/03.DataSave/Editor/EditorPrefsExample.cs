using UnityEditor;
using UnityEngine;

public class EditorPrefsExample : EditorWindow
{
    [MenuItem("Window/EditorPrefs Example")]
    public static void ShowWindow()
    {
        GetWindow<EditorPrefsExample>("EditorPrefs Example");
    }

    void OnGUI()
    {
        // 저장 버튼
        if (GUILayout.Button("설정 저장"))
        {
            EditorPrefs.SetInt("SavedValue", 100);
            Debug.Log("저장된 값: 100");
        }

        // 불러오기 버튼
        if (GUILayout.Button("설정 불러오기"))
        {
            int value = EditorPrefs.GetInt("SavedValue", 0);
            Debug.Log("불러온 값: " + value);
        }

        // 삭제 버튼
        if (GUILayout.Button("설정 삭제"))
        {
            EditorPrefs.DeleteKey("SavedValue");
            Debug.Log("저장된 값 삭제됨");
        }
    }
}