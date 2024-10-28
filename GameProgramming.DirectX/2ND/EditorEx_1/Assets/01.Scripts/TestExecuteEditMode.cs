using UnityEngine;

// 유니티 플레이중이 아니더라도 모노비헤이비어를 상속받은 코드는 에디터를 사용하는 중에도 돌아간다
[ExecuteInEditMode]
public class TestExecuteEditMode : MonoBehaviour
{
    [Range(0, 10)]
    public int number;

    // void Awake()
    // {
    //     Debug.Log("ExecuteEditMode Awake");
    // }
    // void Start()
    // {
    //     Debug.Log("ExecuteEditMode Start");
    // }
    // void Update()
    // {
    //     Debug.Log("ExecuteEditMode Update");
    // }
}