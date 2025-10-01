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

    [Header("Minimap")] public string targetMapID;

    private GameObject player;

    private void Start()
    {
        TeleportManager.Instance.teleportSet[teleportID] = gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            TeleportManager.Instance.MoveToPortal();
            GameManager.Instance._stageminimapManager.OnMapEntered(targetMapID);
            TeleportManager.Instance.CoFade(targetID, directionOffsets[portalDirection]);
        }
    }
}