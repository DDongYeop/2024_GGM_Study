using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSOScript : ScriptableObject
{
    [SerializeField]
    string str;
    void OnEnable()
    {
        name = "New ChildSOScript";
        Debug.Log(name);
    }
}
