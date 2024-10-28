using UnityEngine;
using UnityEngine.Serialization;

public class TestFormerlySerializedAs : MonoBehaviour
{
    // 최초에는 playerHealth 변수명으로 지정하고 값을 세팅
    [FormerlySerializedAs("playerHealth")]
    public int health;

    // 새 변수 이름
    public string playerName;

    void Start()
    {
        Debug.Log($"Player Health: {health}, Player Name: {playerName}");
    }
}
