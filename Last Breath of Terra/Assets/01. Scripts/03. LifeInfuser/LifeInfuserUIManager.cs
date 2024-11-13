using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LifeInfuserUIManager : MonoBehaviour
{
    public StageLifeInfuserSO lifeInfuserData;
    public GameObject infuserStatus;
    public Canvas infuserActivationCanvas;
    public GameObject InfuserStatusUI;
    public Image infuserActivationUI;
    

    //private Image[] infuserStatusUI;

    

    private void Start()
    {
        lifeInfuserData.InfuserStatusUI = InfuserStatusUI;
        lifeInfuserData.infuserActivationUI = infuserActivationUI;
        lifeInfuserData.infuserActivationCanvas = infuserActivationCanvas;
        lifeInfuserData.infuserStatusUI = new Image[infuserStatus.transform.childCount];
        for (int i = 0; i < infuserStatus.transform.childCount; i++)
        {
            lifeInfuserData.infuserStatusUI[i] = infuserStatus.transform.GetChild(i).GetComponent<Image>();
        }
    }

    
}