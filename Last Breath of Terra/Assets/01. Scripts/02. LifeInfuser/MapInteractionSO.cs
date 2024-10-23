using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "MapInteraction", menuName = "ScriptableObject/MapInteraction")]

public class MapInteractionSO : ScriptableObject
{
    public float gravityOffDuration;

    public void UseSimulated(Rigidbody2D rb)
    {
        rb.simulated = true;
    }
    public void StopUsingSimulated(Rigidbody2D rb)
    {
        rb.simulated = false;
    }
}
