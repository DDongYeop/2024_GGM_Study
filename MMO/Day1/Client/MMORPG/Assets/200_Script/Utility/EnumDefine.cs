using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PC_AnimationStatus
{
    None = -1,

    Idle = 0,
    Run0 = 1,
    Run45,
    Run90,
    Run135,
    Run180,
    Run225,
    Run270,
    Run315 = 8,

}

public enum CharacterRotation
{
    vector0,
    vector45,
    vector90,
    vector135,
    vector180,
    vector225,
    vector270,
    vector315,
}
