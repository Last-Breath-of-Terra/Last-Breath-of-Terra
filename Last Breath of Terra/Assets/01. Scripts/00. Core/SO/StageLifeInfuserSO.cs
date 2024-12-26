using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;


[CreateAssetMenu(fileName = "SO_StageLifeInfuser", menuName = "ScriptableObject/StageLifeInfuser")]
public class StageLifeInfuserSO : LifeInfuserSO
{
    //public Image[] infuserStatusUI;
    public Sprite InfuserActiveImage;
    public Sprite InfuserInactiveImage;
    //public int totalInfuser;
    //public bool[] isInfuser;
    
    public override void StartInfusion(int infuserNumber, GameObject targetInfuser)
    {
        InfuserManager.Instance.infuserActivationCanvas.gameObject.transform.position = targetInfuser.transform.position;
        base.StartInfusion(infuserNumber, targetInfuser);
        AudioManager.instance.PanSoundLeftToRight("breath_action_being", infusionDuration);
        
    }
    public override void CompleteInfusion(int infuserNumber, GameObject targetInfuser)
    {
        AudioManager.instance.PlayPlayer("breath_action_end", 0f);
        targetInfuser.GetComponent<SpriteRenderer>().sprite = InfuserActiveImage;
        InfuserManager.Instance.infuserStatusChild[infuserNumber].GetComponent<Image>().color = new Color(1, 1, 1, 0.8f);
        base.CompleteInfusion(infuserNumber, targetInfuser);
        InfuserManager.Instance.activatedInfusers[infuserNumber] = true;
    }

    
}
