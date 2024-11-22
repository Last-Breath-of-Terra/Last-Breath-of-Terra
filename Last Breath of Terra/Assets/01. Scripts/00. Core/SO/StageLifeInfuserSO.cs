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
    public bool[] isInfuser;
    
    public override void StartInfusion(int infuserNumber)
    {
        infuserActivationCanvas.gameObject.transform.position = targetInfuser.transform.position;
        base.StartInfusion(infuserNumber);
        AudioManager.instance.PanSoundLeftToRight("breath_action_being", infusionDuration);
        
    }
    public override void CompleteInfusion(int infuserNumber)
    {
        GameObject player = AudioManager.instance.player;
        AudioManager.instance.PlayPlayer("breath_action_end", 0f);
        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage;
        infuserStatusUI[infuserNumber].GetComponent<Image>().color = Color.white;
        base.CompleteInfusion(infuserNumber);
        isInfuser[infuserNumber] = true;
        canInfusion[infuserNumber] = false;
        
    }

    
}
