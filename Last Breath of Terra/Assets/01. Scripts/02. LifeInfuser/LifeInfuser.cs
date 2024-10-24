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
        lifeInfuserData.canInfusion = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player") && lifeInfuserData.canInfusion)
        {
            lifeInfuserData.playerController = collision.GetComponent<PlayerController>();
            Invoke("PrepareInfusion", lifeInfuserData.infusionWaitTime);
        }
    }
    private void PrepareInfusion()
    {
        if (lifeInfuserData.playerController != null)
        {
            lifeInfuserData.playerController.SetCanMove(false);
        }
        lifeInfuserData.StartInfusion(infusionSlider);
        lifeInfuserData.SpawnObstacle(obstacleSprites);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        lifeInfuserData.StopInfusion(infusionSlider);
    }
}
