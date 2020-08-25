using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorObjectsData : ScriptableObject
{
    public LayerMask RaycastMask;
    public float RaycastYOffset = 3.0f;
    public bool ShowRaycast = false;
}
