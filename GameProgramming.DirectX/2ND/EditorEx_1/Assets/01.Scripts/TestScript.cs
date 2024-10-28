using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class TestScript : MonoBehaviour
{
    private void OnEnable() 
    {
#if UNITY_EDITOR
        var tex = EditorGUIUtility.Load("logo.png") as Texture;
        Debug.Log(tex);
#endif
    }
    
    // [InitializeOnLoadMethod]
    // static void GetBultinAssetNames()
    // {
    //     var flags = BindingFlags.Static | BindingFlags.NonPublic;
    //     var info = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", flags);
    //     var bundle = info.Invoke(null, new object[0]) as AssetBundle;
    //     foreach (var n in bundle.GetAllAssetNames())
    //     {
    //         Debug.Log(n);
    //     }
    // }
}
