using UnityEditor;
public class TestSaveData
{
    [InitializeOnLoadMethod]
    static void SaveConfig()
    {
        EditorUserSettings.SetConfigValue("Data 1", "text1");
        EditorUserSettings.SetConfigValue("Data 2", "text2");
        EditorUserSettings.SetConfigValue("Data 3", "text3");
    }
}
