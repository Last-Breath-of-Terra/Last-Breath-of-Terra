using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 미니맵 활성화/비활성화 상태를 관리하고,
/// 외부에서의 미니맵 제어 요청(강제 비활성화 등)을 처리하는 클래스.
///
/// 주요 기능:
/// 1. 사용자의 입력(Action)에 따라 미니맵을 토글.
/// 2. 외부 요청에 따라 미니맵을 강제로 비활성화.
/// 3. 플레이어 이동 상태와 미니맵 상태를 연동.
/// 4. 활성화 오브젝트의 상태를 미니맵에 반영.
/// </summary>

public class MiniMapManager : MonoBehaviour
{
    public GameObject fullMapUI;

    private InputAction minimapAction;
    private bool isFullMapActive = false;
    private bool isInitialized = false;

    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private Image minimapImage;
    [SerializeField] private Image minimapPlayerImage;
    [SerializeField] private GameObject minimapIconPrefab;

    private List<Image> minimapIcons = new List<Image>();
    

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

    private void Update()
    {
        if (!isInitialized)
        {
            if (InfuserManager.Instance != null && InfuserManager.Instance.infuser != null && InfuserManager.Instance.infuser.Length > 0)
            {
                InitializeMinimapObjects();
                isInitialized = true;
            }
        }

        if (!isFullMapActive)
        {
            UpdatePlayerIconPosition();
            UpdateObjectIcons();
        }
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

    private void UpdatePlayerIconPosition()
    {
        Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));
        Vector2 charPos = new Vector2(Vector3.Distance(left.position, new Vector3(GameManager.Instance.playerTr.position.x, 0f, 0f)), Vector3.Distance(bottom.position, new Vector3(0f, GameManager.Instance.playerTr.position.y, 0f)));
        Vector2 normalPos = new Vector2(charPos.x / mapArea.x, charPos.y / mapArea.y);

        minimapPlayerImage.rectTransform.anchoredPosition = new Vector2(minimapImage.rectTransform.sizeDelta.x * normalPos.x, minimapImage.rectTransform.sizeDelta.y * normalPos.y);
    }

    private void InitializeMinimapObjects()
    {
        for (int i = 0; i < InfuserManager.Instance.infuser.Length; i++)
        {
            if (InfuserManager.Instance.infuser[i] != null)
            {
                GameObject iconObject = Instantiate(minimapIconPrefab, minimapImage.transform);
                Image iconImage = iconObject.GetComponent<Image>();
                if (iconImage != null)
                {
                    bool isActive = InfuserManager.Instance.activatedInfusers[i];
                    iconImage.sprite = isActive
                        ? InfuserManager.Instance.LifeInfuserSO.activeIcon
                        : InfuserManager.Instance.LifeInfuserSO.inactiveIcon;

                    minimapIcons.Add(iconImage);

                    UpdateObjectIconPosition(iconObject, InfuserManager.Instance.infuser[i].transform.position);
                }
            }
        }
    }

    private void UpdateObjectIcons()
    {
        for (int i = 0; i < minimapIcons.Count; i++)
        {
            if (InfuserManager.Instance.infuser[i] != null)
            {
                bool isActive = InfuserManager.Instance.activatedInfusers[i];

                minimapIcons[i].GetComponent<Image>().sprite = isActive
                    ? InfuserManager.Instance.LifeInfuserSO.activeIcon
                    : InfuserManager.Instance.LifeInfuserSO.inactiveIcon;
            }
        }
        // for (int i = 0; i < minimapObjectIcons.Count; i++)
        // {
        //     MinimapObjectController controller = minimapObjectIcons[i].GetComponent<MinimapObjectController>();
        //     controller.SetActive(InfuserManager.Instance.activatedInfusers[i]);
        // }
    }

    private void UpdateObjectIconPosition(GameObject icon, Vector3 worldPosition)
    {
        // Vector2 mapArea = new Vector2(Vector3.Distance(left.position, right.position), Vector3.Distance(bottom.position, top.position));
        // Vector2 objPos = new Vector2(Vector3.Distance(left.position, new Vector3(worldPosition.x, 0f, 0f)), Vector3.Distance(bottom.position, new Vector3(0f, worldPosition.y, 0f)));
        // Vector2 normalPos = new Vector2(objPos.x / mapArea.x, objPos.y / mapArea.y);

        // icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(
        //     minimapImage.rectTransform.sizeDelta.x * normalPos.x,
        //     minimapImage.rectTransform.sizeDelta.y * normalPos.y
        // );
        Vector2 mapArea = new Vector2(
        right.position.x - left.position.x, 
        top.position.y - bottom.position.y
        );

        // 월드 좌표 -> 미니맵 상대 좌표 변환
        Vector2 objPos = new Vector2(
            worldPosition.x - left.position.x, 
            worldPosition.y - bottom.position.y
        );

        // 정규화된 좌표 계산
        Vector2 normalPos = new Vector2(
            objPos.x / mapArea.x, 
            objPos.y / mapArea.y
        );

        // UI 좌표 변환
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            Vector2 minimapSize = minimapImage.rectTransform.sizeDelta; // 크기 캐싱
            iconRect.anchoredPosition = new Vector2(
                minimapSize.x * normalPos.x, 
                minimapSize.y * normalPos.y
            );
        }
    }
}