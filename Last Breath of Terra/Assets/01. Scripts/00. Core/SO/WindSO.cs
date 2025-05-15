using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WindData", menuName = "ScriptableObject/Gimmick/Wind")]

public class WindSO : ScriptableObject
{
    public float activationTime = 0.5f;
    public float deactivationTime = 0.5f;
    
    public float fastRate = 0.5f;
    public float slowRate = 0.5f;

    public float liftHeight = 0.5f;
    public float liftDuration = 0.5f;
}
