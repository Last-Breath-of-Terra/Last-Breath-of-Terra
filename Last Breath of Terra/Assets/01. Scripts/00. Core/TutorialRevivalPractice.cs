using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRevivalPractice : MonoBehaviour
{
    public GameObject obstacle;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().SetCanMove(false);
            obstacle.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}