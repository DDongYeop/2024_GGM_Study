using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class TestSO : ScriptableObject
{
    [Range(0, 10)]
    public int number = 3;
    public bool toggle = false;
    public string[] texts = new string[5];
}