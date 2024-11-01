using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;

public class InfuserUISetter : MonoBehaviour
{
    public StageLifeInfuserSO stageLifeInfuserSO;
    public int infuserUINumber;

    private void Awake()
    {
        stageLifeInfuserSO.infuserStatusUI[infuserUINumber] = gameObject.GetComponent<Image>();
    }
}
