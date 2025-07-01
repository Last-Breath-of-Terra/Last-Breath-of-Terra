using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().hp -= damage;
            Debug.Log("hp : " + other.GetComponent<PlayerController>().hp);

        }
    }
}