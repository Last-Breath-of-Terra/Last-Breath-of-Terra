using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObject/PlayerData")]
public class PlayerSO : ScriptableObject
{
    public float baseSpeed = 1f;
    public float maxSpeed = 6f;
    public float accelerationTime = 5f;
    public float jumpForce = 5f;
    public float dashForce = 10f;
}