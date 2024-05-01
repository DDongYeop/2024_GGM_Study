using UnityEngine;

public class Pipes : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public float speed = 5f;

    private float leftEdge = -25f;

    private void FixedUpdate()
    {
        transform.position += speed * Time.fixedDeltaTime * Vector3.left;

        if (transform.localPosition.x <= leftEdge) {
            Destroy(gameObject);
        }
    }
}
