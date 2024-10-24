using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LifeInfuser : MonoBehaviour
{
    public Slider infusionSlider;
    public LifeInfuserSO lifeInfuserData;
    public GameObject[] obstacleSprites;
    
    [SerializeField]
    private bool canInfusion;

    private void Start()
    {
        canInfusion = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player") && canInfusion)
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetCanMove(false);
            }
            
            Invoke("PrepareInfusion", lifeInfuserData.infusionWaitTime);
        }
    }
    private void PrepareInfusion()
    {
        Debug.Log("starting infusion");
        lifeInfuserData.StartInfusion(infusionSlider, ref canInfusion);
        lifeInfuserData.SpawnObstacle(obstacleSprites);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        lifeInfuserData.StopInfusion(infusionSlider);
    }
}
