using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "SO_StageLifeInfuser", menuName = "ScriptableObject/StageLifeInfuser")]
public class StageLifeInfuserSO : LifeInfuserSO
{
    public string stageName;
    public Image[] infuserStatusUI;
    public GameObject targetInfuser;
    public Sprite InfuserActiveImage;
    public Sprite InfuserInactiveImage;
    public bool[] canInfusion;
    public GameObject[] infuser;
    public int totalInfuser;
    
    [SerializeField]
    private bool[] isInfuser;

    public override void CompleteInfusion(int infuserNumber)
    {
        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage;
        infuserStatusUI[infuserNumber].sprite = infusionActiveUI;
        base.CompleteInfusion(infuserNumber);
        isInfuser[infuserNumber] = true;
        canInfusion[infuserNumber] = false;
        
    }

    /*
    public override void StartInfusion(int infuserNumber)
    {
        base.CompleteInfusion(infuserNumber);
        AudioManager.instance.PlaySFX("breath_action_start", targetInfuser.GetComponent<AudioSource>(), targetInfuser.GetComponent<Transform>());

    }
    */

}
