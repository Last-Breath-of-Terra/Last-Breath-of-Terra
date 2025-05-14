using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IcicleSpawner : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        StartCoroutine(SpawnIcicle());
    }

    IEnumerator SpawnIcicle()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            GameObject icicle = PoolManager.Instance.GetObject(IcicleManager.Instance.poolName);

            Vector2 center = boxCollider.bounds.center;
            float width = boxCollider.bounds.size.x;
            float randomX = Random.Range(center.x - width / 2f, center.x + width / 2f);
            Debug.Log(icicle);
            icicle.transform.position = new Vector3(randomX, boxCollider.bounds.max.y, transform.position.z);
        }
    }
}