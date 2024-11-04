using UnityEngine;
using UnityEditor;
public class EditorWindowSizeExample : EditorWindow
{
    const string SIZE_WIDTH_KEY = "ExampleWindow size width";
    const string SIZE_HEIGHT_KEY = "ExampleWindow size height";

    [MenuItem("Window/WindowSizeExample")]
    static void Open()
    {
        GetWindow<EditorWindowSizeExample>();
    }

    void OnEnable()
    {
        var width = EditorPrefs.GetFloat(SIZE_WIDTH_KEY, 600);
        var height = EditorPrefs.GetFloat(SIZE_HEIGHT_KEY, 400);
        this.position = new Rect(this.position.x, this.position.y, width, height);

        Debug.Log(this.position);
    }

    void OnDisable()
    {
        EditorPrefs.SetFloat(SIZE_WIDTH_KEY, this.position.width);
        EditorPrefs.SetFloat(SIZE_HEIGHT_KEY, this.position.height);

        Debug.Log(this.position);
    }
}