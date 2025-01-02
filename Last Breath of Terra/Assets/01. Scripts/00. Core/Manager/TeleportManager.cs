using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
    public TeleportSO teleportSO;
    public PolygonCollider2D[] camBorders;
    public CinemachineVirtualCamera virtualCamera;
    public Image fadeImage;
    public float fadeDuration = 1f;

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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void ChangeCamera(int index)
    {
        var confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            // CinemachineConfiner가 없다면 추가하기
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
            Debug.Log("CinemachineConfiner component added.");
        }

        virtualCamera.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = camBorders[index];
    }

    public void CoFade(int targetID)
    {
        StartCoroutine(Fade(targetID));
        
    }

    IEnumerator Fade(int targetID)
    {
        float f = 0f;
        while (f <= 1f)
        {
            f += 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }
        player.transform.position = teleportSO.portals[targetID];
        ChangeCamera(targetID / 2);
        yield return new WaitForSeconds(0.5f);
        while (f > 0f)
        {
            f -= 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        
        
    }
    
}