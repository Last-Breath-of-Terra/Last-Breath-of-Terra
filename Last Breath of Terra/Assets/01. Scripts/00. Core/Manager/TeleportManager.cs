using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
    public TeleportSO teleportSO;
    public PolygonCollider2D[] camBorders;
    public CinemachineVirtualCamera virtualCamera;

    private GameObject player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeCamera(int index)
    {
        var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
        if (confiner == null)
        {
            // CinemachineConfiner가 없다면 추가하기
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
            Debug.Log("CinemachineConfiner component added.");
        }
        virtualCamera.GetComponent<CinemachineConfiner>().m_BoundingShape2D = camBorders[index];
    }
}