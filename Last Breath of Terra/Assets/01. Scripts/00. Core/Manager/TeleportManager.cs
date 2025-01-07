using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using DG.Tweening;

public class TeleportManager : MonoBehaviour
{
    
    
    public static TeleportManager Instance;
    public GameObject[] teleportSet;
    public TeleportSO teleportSO;
    public PolygonCollider2D[] camBorders;
    public CinemachineVirtualCamera virtualCamera;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private GameObject player;
    private Animator animator;

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
        teleportSet = new GameObject[20];
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = player.GetComponent<Animator>();
    }

    public void ChangeCamera(int mapID)
    {
        var confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            // CinemachineConfiner가 없다면 추가하기
            confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
            Debug.Log("CinemachineConfiner component added.");
        }
        Debug.Log("mapID" + mapID);
        virtualCamera.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = camBorders[mapID];
    }

    public void CoFade(int targetID, Vector3 teleportDirection)
    {
        StartCoroutine(Fade(targetID, teleportDirection));
        
    }

    IEnumerator Fade(int targetID, Vector3 teleportDirection)
    {
        player.GetComponent<Rigidbody2D>().gravityScale = 0;

        int teleportOffset = 2;
        DOTween.To(() => player.transform.position, x => player.transform.position = x, player.transform.position + teleportDirection * 2, 1f);
        float f = 0f;
        while (f <= 1f)
        {
            f += 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }
        animator.SetBool("MoveToPortal", false);
        //animator.Play("Idle");
        if (teleportDirection == new Vector3(0, 1, 0))
        {
            teleportOffset = 3;
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;
            DOTween.To(() => player.transform.position, x => player.transform.position = x, player.transform.position + new Vector3(2, 0, 0), 2.5f);
        }
        else
        {
            player.transform.position = teleportSet[targetID].transform.position + teleportOffset * teleportDirection;

        }
        player.GetComponent<Rigidbody2D>().gravityScale = 3;
        ChangeCamera(teleportSet[targetID].GetComponent<Teleport>().mapID);
        yield return new WaitForSeconds(0.5f);
        while (f > 0f)
        {
            f -= 0.01f;
            yield return new WaitForSeconds(0.01f);
            fadeImage.color = new Color(0f, 0f, 0f, f);
        }

        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        
        player.GetComponent<PlayerController>().canMove = true;

    }

    public void MoveToPortal()
    {
        player.GetComponent<PlayerController>().canMove = false;
        Debug.Log("MoveToPortal called");
        animator.SetBool("MoveToPortal", true);

        
    }
    
}