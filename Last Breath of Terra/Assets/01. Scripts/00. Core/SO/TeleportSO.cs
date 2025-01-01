using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageTeleportData", menuName = "ScriptableObject/Teleport")]
public class TeleportSO : ScriptableObject
{
    public Vector2[] portals;
}
