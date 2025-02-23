using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class RegionTransitonEffectWrapper : MonoBehaviour
{
    public void TransitRegion(int colorType)
    {
        RegionTransitionEffectManager.Instance.StartTransition(colorType, new Vector2(0.5f, 0.5f));
    }
}