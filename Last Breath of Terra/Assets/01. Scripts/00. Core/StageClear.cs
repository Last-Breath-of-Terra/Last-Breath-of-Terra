using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;



public class StageClear : MonoBehaviour
{
    public DataManager dataManager;
    
    private void OnTriggerEnter2D(Collider2D collision) 
    {
        int count = InfuserManager.Instance.activatedInfusers.Count(x => x);
        Debug.Log(count + " infusers activated");
        if (count == 3) //추후 변경 필요
        {
            Debug.Log("stage cleared");
            DataManager.Instance.ModifyPlayerData(DataManager.Instance.playerIndex, 0, true);
        }
    }
}
