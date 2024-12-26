using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class InfuserManager : MonoBehaviour
{
    public static InfuserManager Instance;

    public LifeInfuserSO LifeInfuserSO;
    public CinemachineVirtualCamera virtualCamera;
    public GameObject[] infuser;
    public bool[] activatedInfusers;
    public bool[] canInfusion;

    [Header("UI")] public GameObject infuserStatus; //활성화 여부 확인 상단 UI
    public Transform[] infuserStatusChild;
    public Canvas infuserActivationCanvas;
    public Image infuserActivation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        infuser = new GameObject[3];
        activatedInfusers = new bool[3];
        canInfusion = new bool[3];
    }

    private void Start()
    {
        foreach (Transform child in infuserStatusChild)
        {
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                child.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
            }
        }
    }
}