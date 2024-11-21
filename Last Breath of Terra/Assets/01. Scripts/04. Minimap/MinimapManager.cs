using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 미니맵 활성화/비활성화 상태를 관리하고,
/// 외부에서의 미니맵 제어 요청(강제 비활성화 등)을 처리하는 클래스.
///
/// 주요 기능:
/// 1. 사용자의 입력(Action)에 따라 미니맵을 토글.
/// 2. 외부 요청에 따라 미니맵을 강제로 비활성화.
/// 3. 플레이어 이동 상태와 미니맵 상태를 연동.
/// </summary>

public class MiniMapManager : MonoBehaviour
{
    public GameObject fullMapUI;

    private InputAction minimapAction;
    private bool isFullMapActive = false;

    private void OnEnable()
    {
        var playerInput = GameManager.Instance.playerTr.GetComponent<PlayerInput>();
        minimapAction = playerInput.actions["Minimap"];
        minimapAction.performed += ToggleMap;
        minimapAction.Enable();
    }

    private void OnDisable()
    {
        minimapAction.performed -= ToggleMap;
        minimapAction.Disable();
    }

    private void ToggleMap(InputAction.CallbackContext context)
    {
        SetMapActive(!isFullMapActive);
    }

    private void SetMapActive(bool active)
    {
        isFullMapActive = active;
        fullMapUI.SetActive(isFullMapActive);

        GameManager.Instance.playerTr.GetComponent<PlayerController>().SetCanMove(!isFullMapActive);
    }

    public void ForceCloseMap()
    {
        if (isFullMapActive)
        {
            SetMapActive(false);
        }
    }
}