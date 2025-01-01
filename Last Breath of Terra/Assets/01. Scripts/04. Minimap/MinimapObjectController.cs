using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapObjectController : MonoBehaviour
{
    private Material material;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    public void SetActive(bool isActive)
    {
        if (material != null)
        {
            material.SetFloat("_isActive", isActive ? 1f : 0f);
        }
    }
}
