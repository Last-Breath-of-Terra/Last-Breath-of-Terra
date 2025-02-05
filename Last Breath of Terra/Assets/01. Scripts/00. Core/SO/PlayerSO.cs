using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObject/PlayerData")]
public class PlayerSO : ScriptableObject
{
    public float hp = 100f;
    public float maxSpeed = 8f;
    public float accelerationTime = 5f;
    public float jumpForce = 5f;
    public float climbSpeed = 3f;
    public float moveDelayAfterFall = 0.5f;
}