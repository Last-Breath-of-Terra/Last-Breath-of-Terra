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
    

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.CompareTag("Player"))
        {
          lifeInfuserData.StartInfusion(infusionSlider);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        lifeInfuserData.StopInfusion(infusionSlider);
    }
}
