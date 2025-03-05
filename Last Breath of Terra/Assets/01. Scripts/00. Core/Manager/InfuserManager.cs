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
    public int infusionCount;
    public ParticleSystem objectParticle;
    public float radius = 30f;


    [Header("UI")] public GameObject infuserStatus; //활성화 여부 확인 상단 UI
    public Transform[] infuserStatusChild;
    public Image infuserActivation;
    
    [Header("Arc")]
    public GameObject ArcEffect;
    public LineRenderer glowLineRenderer;  
    public LineRenderer brightLineRenderer;  
    public LineRenderer backLineRenderer;
    public ParticleSystem gaugeParticle;   

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

        infuser = new GameObject[infusionCount];
        activatedInfusers = new bool[infusionCount];
        canInfusion = new bool[infusionCount];
    }

    private void Start()
    {
        
        //활성화 게이지 쉐이더 초기화
        glowLineRenderer.positionCount = 0;
        brightLineRenderer.positionCount = 0;

        if (GetComponent<ParticleSystem>() != null)
        {
            GetComponent<ParticleSystem>().Stop();
        }
        
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