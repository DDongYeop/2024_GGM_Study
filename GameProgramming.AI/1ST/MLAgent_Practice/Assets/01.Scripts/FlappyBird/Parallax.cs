using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float animationSpeed = 1f;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        meshRenderer.material.mainTextureOffset += new Vector2(animationSpeed * Time.fixedDeltaTime, 0);
    }

}
