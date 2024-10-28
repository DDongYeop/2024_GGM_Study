using UnityEngine;

[SelectionBaseAttribute]
public class TestSectionBase : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("이 오브젝트가 베이스이다!");
    }
}