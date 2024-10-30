using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GravityEffect : MonoBehaviour
{
    public MapInteractionSO mapInteraction;
    private Rigidbody2D rb;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            rb = collision.gameObject.GetComponent<Rigidbody2D>();
            mapInteraction.StopUsingSimulated(rb);
            Invoke("ReuseSimulated", mapInteraction.gravityOffDuration);
   
        }
        
    }

    private void ReuseSimulated()
    {
        mapInteraction.UseSimulated(rb);
    }
}
