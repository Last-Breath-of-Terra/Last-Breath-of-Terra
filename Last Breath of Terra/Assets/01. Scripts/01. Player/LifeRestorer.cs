using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LifeRestorer : MonoBehaviour
{
    public PlayerSO playerData;

    private InputActionMap selectMap;

    public void RestoreHealth()
    {
        gameObject.GetComponent<PlayerInput>().currentActionMap = selectMap;
    }

    public void GameOver()
    {
        SceneManager.LoadScene("StageSelection");
    }
}
