using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;


public class Tutorial : MonoBehaviour
{
    public GameObject door;
    public GameObject obstacle;
    private void Update()
    {
        int count = InfuserManager.Instance.activatedInfusers.Count(x => x);
        Debug.Log(count + " infusers activated");
        if (count >= 5) //추후 변경 필요
        {
            Debug.Log("stage cleared");
            //DataManager.Instance.ModifyPlayerData(DataManager.Instance.playerIndex, 0, true);
            //SceneManager.LoadScene(2);
            door.SetActive(false);
        }
        else
        {
            door.SetActive(true);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "RevivalPractice")
        {
            gameObject.GetComponent<PlayerController>().canMove = false;
            obstacle.SetActive(true);
            other.gameObject.SetActive(false);
        }
        
    }
}
