using System;
using System.Collections.Generic;
using System.IO;
using Server;

// Struct for location
public struct CFLocation
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

// 기본 오브젝트 클래스
public abstract class CObject
{
    public string Name { get; set; }
    public int Index { get; set; }
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public CFLocation Pos { get; set; }

    public float Direction { get; set; }
    public virtual void Update()
    {
        // 기본 업데이트 로직
    }
}

