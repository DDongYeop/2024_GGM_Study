using UnityEngine;

public class TestContextMenu : MonoBehaviour
{
    [Range(0, 100)]
    public int number;

    [ContextMenu("RandomNumber")]
    void RandomNumber()
    {
        number = Random.Range(0, 100);
    }

    [ContextMenu("ResetNumber")]
    void ResetNumber()
    {
        number = 0;
    }
}
