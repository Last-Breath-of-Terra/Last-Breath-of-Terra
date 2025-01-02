using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public int stage;
    
    public void ChangeStage(int stage)
    {
        SceneManager.LoadScene(stage);
    }
}
