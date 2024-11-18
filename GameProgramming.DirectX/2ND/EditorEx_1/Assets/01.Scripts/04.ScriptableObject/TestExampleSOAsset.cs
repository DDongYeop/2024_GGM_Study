using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Example/Create ExampleAsset Instance")]
public class TestExampleSOAsset : ScriptableObject
{
    [SerializeField]
    string str;
    [SerializeField, Range(0, 10)]
    int number; 

    // 1. 인스턴스화
    // 2. 파일로 저장
    // 3. 파일을 로드

    // 1. 무언가를 '인스턴스화' 시키다?
    // 유니티의 "직렬화 매커니즘"을 통해 객체를 생성
    [MenuItem("SO Examples/Create TestExampleSOAsset Instance")]
    static void CreateExampleAssetInstance()
    {
        var exampleAsset = CreateInstance<TestExampleSOAsset>();

        // 2. 파일로 저장
        AssetDatabase.CreateAsset(exampleAsset, "Assets/06.ScriptableObjects/Test/ExampleAsset.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("SO Examples/Load TestExampleSOAsset")]
    static void LoadExampleAsset()
    {
        var exampleAsset = AssetDatabase.LoadAssetAtPath<TestExampleSOAsset>("Assets/06.ScriptableObjects/Test/ExampleAsset.asset");
        Debug.Log(exampleAsset);
    }
}
 