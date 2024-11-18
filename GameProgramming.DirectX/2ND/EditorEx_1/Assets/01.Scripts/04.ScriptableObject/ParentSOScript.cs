using UnityEditor;
using UnityEngine;

public class ParentSOScript : ScriptableObject
{
    const string PATH = "Assets/New ParentSOScript.asset";

    [SerializeField] ChildSOScript child1;

    [MenuItem("Example/Create ParentSOAsset")]
    static void CreateScriptableObject()
    {
        // 1. 부모를 인스턴스화
        var parent = CreateInstance<ParentSOScript>();

        // 2. 자식을 인스턴스화
        parent.child1 = CreateInstance<ChildSOScript>();

        // 3. 부모를 에셋으로 저장
        AssetDatabase.CreateAsset(parent, PATH);

        // 변경사항을 저장& 갱신
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    } 
} 
