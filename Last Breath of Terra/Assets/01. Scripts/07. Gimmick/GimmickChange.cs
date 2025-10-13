using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickChange : MonoBehaviour
{
    public GameObject[] gimmickSet;
    
    private int currentGimmick = 0;


    public void ChangeGimmick(int mapIndex)
    {
        if(gimmickSet[currentGimmick] != null)
            gimmickSet[currentGimmick].SetActive(false);
        if(gimmickSet[mapIndex] != null)
            gimmickSet[mapIndex].SetActive(true);
        currentGimmick = mapIndex;
    }
}
