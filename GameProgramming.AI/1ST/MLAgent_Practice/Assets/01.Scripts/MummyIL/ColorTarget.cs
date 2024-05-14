using UnityEngine;
using Random = UnityEngine.Random;

public class ColorTarget : MonoBehaviour
{
    public enum TARGET_COLOR
    {
        Black = 0,
        Blue = 1,
        Green = 2,
        Red = 3 
    }

    public TARGET_COLOR TargetColor;
    [SerializeField] private Material[] _targetColorMat;

    private Renderer _hintRenderer;
    private int _preColorIndex = -1;

    private void Awake()
    {
        _hintRenderer = transform.Find("Hint").GetComponent<Renderer>();
    }

    public void TargetingColor()
    {
        int currentColorIndex;

        do
        {
            currentColorIndex = Random.Range(0, _targetColorMat.Length);
        } while (currentColorIndex == _preColorIndex);
        _preColorIndex = currentColorIndex;

        TargetColor = (TARGET_COLOR)currentColorIndex;
        _hintRenderer.material = _targetColorMat[currentColorIndex];
    }
}
