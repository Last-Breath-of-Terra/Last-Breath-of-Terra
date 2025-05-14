using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    public int poolSize;
    public Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();


    public void CreatePool(string poolName, GameObject prefab, Transform spawnPoint)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            poolDictionary.Add(poolName, new Queue<GameObject>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                obj.SetActive(false);
                poolDictionary[poolName].Enqueue(obj);
            }
        }
    }

    public GameObject GetObject(string poolName)
    {
        if (poolDictionary.ContainsKey(poolName))
        {
            if (poolDictionary[poolName].Count > 0)
            {
                GameObject obj = poolDictionary[poolName].Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                Debug.Log("아이템 없음!!");
            }
        }

        return null;
    }

    public void ReturnObject(string poolName, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDictionary.ContainsKey(poolName) != null)
        {
            Debug.Log("return obj");
            poolDictionary[poolName].Enqueue(obj);
        }
    }

    public void DestroyPool(string poolName)
    {
        if (poolDictionary.ContainsKey(poolName))
        {
            while (poolDictionary[poolName].Count > 0)
            {
                GameObject obj = poolDictionary[poolName].Dequeue();
                Destroy(obj);
            }

            poolDictionary.Remove(poolName);
        }
    }
}