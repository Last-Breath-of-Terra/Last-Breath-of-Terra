using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WindData", menuName = "ScriptableObject/Gimmick/Wind")]

public class WindSO : ScriptableObject
{
    
    //바람 빨라지고 느려지는 정도
    public float fastRate = 1.5f;
    public float slowRate = 0.5f;

    //얼마나 올라갈지
    public float liftHeight = 0.5f;
    public float activationTime = 0.5f;
    public float deactivationTime = 0.5f;
}
