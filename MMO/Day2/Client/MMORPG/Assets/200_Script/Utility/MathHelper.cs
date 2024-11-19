using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static float GetNearestOne(this float value)
    {
        if ( -0.1f < value && value < 0.1f )
            return 0;
        else if (value > 0)
            return 1f;
        else
            return -1f;
    }
}
