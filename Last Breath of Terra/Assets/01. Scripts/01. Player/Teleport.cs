using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Teleport : MonoBehaviour
{
    public enum PortalDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    private static Dictionary<PortalDirection, Vector3> directionOffsets = new Dictionary<PortalDirection, Vector3>
    {
        { PortalDirection.Up, new Vector3(0, 1, 0) },
        { PortalDirection.Down, new Vector3(0, -1, 0) },
        { PortalDirection.Left, new Vector3(-1, 0, 0) },
        { PortalDirection.Right, new Vector3(1, 0, 0) }
    };
    
    public PortalDirection portalDirection;
    public int mapID;
    public int teleportID;
    public int targetID;
    public bool isRight;
    
    //※※※※※※※※※※※※※※※임시 코드※※※※※※※※※※※※※※※※※※※※※※※※※
    public Transform targetPos;
    private bool isTeleporting = false;

    //※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※

    private GameObject player;

    private void Start()
    {
        TeleportManager.Instance.teleportSet[teleportID] = gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isTeleporting = true;
            TeleportManager.Instance.MoveToPortal();
            TeleportManager.Instance.CoFade(targetID, directionOffsets[portalDirection]);
            //※※※※※※※※※※※※※※※임시 코드※※※※※※※※※※※※※※※※※※※※※※※※※
            //StartCoroutine(ResetTeleportingFlag());
            //※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
            //player.transform.position = TeleportManager.Instance.teleportSO.portals[targetID];
            //TeleportManager.Instance.ChangeCamera(targetID / 2);
        }
    }

    //※※※※※※※※※※※※※※※임시 코드※※※※※※※※※※※※※※※※※※※※※※※※※
    private IEnumerator ResetTeleportingFlag()
    {
        yield return new WaitForSeconds(1.5f);
        isTeleporting = false;
    }
    //※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※
}