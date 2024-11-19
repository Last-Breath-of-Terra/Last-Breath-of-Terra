using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniMapManager : MonoBehaviour
{
    [Header("References")]
    public GameObject fullMapUI;

    private InputAction minimapAction;
    private bool isFullMapActive = false;

    void OnEnable()
    {
        var playerInput = GameManager.Instance.player.GetComponent<PlayerInput>();
        minimapAction = playerInput.actions["Minimap"];
        minimapAction.performed += ToggleMap;
        minimapAction.Enable();
    }

    void OnDisable()
    {
        minimapAction.performed -= ToggleMap;
        minimapAction.Disable();
    }

    private void ToggleMap(InputAction.CallbackContext context)
    {
        isFullMapActive = !isFullMapActive;
        fullMapUI.SetActive(isFullMapActive);

        GameManager.Instance.player.GetComponent<PlayerController>().SetCanMove(!isFullMapActive);
    }
}