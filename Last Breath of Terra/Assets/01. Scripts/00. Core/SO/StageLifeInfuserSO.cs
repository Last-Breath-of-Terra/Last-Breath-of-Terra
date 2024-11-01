using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "SO_StageLifeInfuser", menuName = "ScriptableObject/StageLifeInfuser")]
public class StageLifeInfuserSO : LifeInfuserSO
{
    public string stageName;
    public Image[] infuserStatusUI;
    public bool[] canInfusion;

    [SerializeField]
    private bool[] isInfuser;

    public override void CompleteInfusion(Slider infusionSlider, int infuserNumber)
    {
        base.CompleteInfusion(infusionSlider, infuserNumber);
        isInfuser[infuserNumber] = true;
        canInfusion[infuserNumber] = false;
        infuserStatusUI[infuserNumber].color = Color.white;
    }
}
