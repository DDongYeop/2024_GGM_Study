using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TestInspector : MonoBehaviour
{
    /// <summary>
    /// Range
    /// </summary>
    [Header("Range")]
    [Range(1, 10)]
    public int num1;
    [Range(1, 10)]
    public float num2;
    [Range(1, 10)]
    public long num3;
    [Range(1, 10)]
    public double num4;

    /// <summary>
    /// String
    /// </summary>
    [Header("String")]
    [Multiline(5)]
    public string multiline;
    [TextArea(3, 5)]
    public string textArea;

    /// <summary>
    /// ContextMenuItem
    /// </summary>
    [Header("ContextMenuItem")]
    [ContextMenuItem("Random", "RandomNumber")]
    [ContextMenuItem("Reset", "ResetNumber")]
    public int number;
    void RandomNumber()
    {
        number = UnityEngine.Random.Range(0, 100);
    }
    void ResetNumber()
    {
        number = 0;
    }

    /// <summary>
    /// Color
    /// </summary>
    [Header("Color")]
    public Color color1;

    [ColorUsage(false)]
    public Color color2;

    [ColorUsageAttribute(true, true)]
    public Color color3;

    /// <summary>
    /// Header
    /// </summary>
    [Header("Header")]
    [Header("Player Settings")]
    public Player player;
    [Serializable]
    public class Player
    {
        public string name;
        [Range(1, 100)]
        public int hp;
    }
    [Header("Game Settings")]
    public Color background;

    /// <summary>
    /// Space & Tooltop
    /// </summary>
    [Header("Space & Tooltop")]
    [Space(16)]
    public string str1;
    [Space(48)]
    public string str2;

    [Tooltip("이것은 툴팁입니다")]
    public long tooltip;

    [Header("Components")]
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
}
