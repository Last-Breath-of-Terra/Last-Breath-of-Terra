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
    public GameObject player;
    public CinemachineVirtualCamera camera;
    public Canvas infuserActivationCanvas;
    public Image infuserActivationUI;
    
    private PlayerInput playerInput;

    //private Image[] infuserStatusUI;
    private int currentIndex = 0;

    private void Awake()
    {
        playerInput = player.GetComponent<PlayerInput>();
    }

    private void Start()
    {
        lifeInfuserData.infuserActivationUI = infuserActivationUI;
        lifeInfuserData.infuserActivationCanvas = infuserActivationCanvas;
        lifeInfuserData.virtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        lifeInfuserData.infuserStatusUI = new Image[infuserStatus.transform.childCount];
        for (int i = 0; i < infuserStatus.transform.childCount; i++)
        {
            lifeInfuserData.infuserStatusUI[i] = infuserStatus.transform.GetChild(i).GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Select/Left"].performed += OnLeftSelect;
            playerInput.actions["Select/Right"].performed += OnRightSelect;
            playerInput.actions["Select/Select"].performed += OnSelect;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.actions["Select/Left"].performed -= OnLeftSelect;
            playerInput.actions["Select/Right"].performed -= OnRightSelect;
            playerInput.actions["Select/Select"].performed -= OnSelect;
        }
    }

    public void OnLeftSelect(InputAction.CallbackContext context)
    {
        lifeInfuserData.infuserStatusUI[currentIndex].color = Color.gray;
        currentIndex = Mathf.Clamp(currentIndex - 1, 0, lifeInfuserData.totalInfuser);
        lifeInfuserData.infuserStatusUI[currentIndex].color = Color.cyan;
        camera.Follow = lifeInfuserData.infuser[currentIndex].transform;
        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
    }

    public void OnRightSelect(InputAction.CallbackContext context)
    {
        lifeInfuserData.infuserStatusUI[currentIndex].color = Color.gray;
        currentIndex = Mathf.Clamp(currentIndex + 1, 0, lifeInfuserData.totalInfuser);
        lifeInfuserData.infuserStatusUI[currentIndex].color = Color.cyan;
        camera.Follow = lifeInfuserData.infuser[currentIndex].transform;
        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        camera.Follow = player.transform;
    }

    public void SelectReviveLife()
    {
        currentIndex = 0;
        Debug.Log("현재 infuser은? " + lifeInfuserData.infuserStatusUI[currentIndex].name);
    }
}