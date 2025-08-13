using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 미니맵 활성화/비활성화 상태를 관리하고,
/// 외부에서의 미니맵 제어 요청(강제 비활성화 등)을 처리하는 클래스.
/// </summary>

[System.Serializable]
public class MiniMapRegion
{
    public string mapID;
    public Image mapImage;
    public bool hasInfuser = false;
    public bool alwaysVisible = false;
    [HideInInspector] public bool isRevealed = false;
}

public class StageMinimapManager : MonoBehaviour
{
    public MiniMapUIController fadeController;

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

    [SerializeField] private List<MiniMapRegion> mapRegions;
    
    private Dictionary<string, MiniMapRegion> regionDict = new Dictionary<string, MiniMapRegion>();
    //private List<Image> minimapIcons = new List<Image>();

    private class MinimapInfuserIcon
    {
        public GameObject iconObject;
        public string mapID;
    }

    private List<MinimapInfuserIcon> infuserIcons = new List<MinimapInfuserIcon>();

    private void Awake()
    {
        foreach (var region in mapRegions)
        {
            if (region.alwaysVisible)
            {
                region.mapImage.enabled = true;
                region.isRevealed = true;
            }
            else
            {
                region.mapImage.enabled = false;
            }

            regionDict[region.mapID] = region;
        }
    }

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

            for (int i = 0; i < InfuserManager.Instance.infuser.Length; i++)
            {
                UpdateObjectIconPosition(InfuserManager.Instance.infuser[i], InfuserManager.Instance.infuser[i].transform.position);
            }
        }
    }

    private void ToggleMap(InputAction.CallbackContext context)
    {
        SetMapActive(!isFullMapActive);
    }

    private void SetMapActive(bool active)
    {
        isFullMapActive = active;

        if (isFullMapActive)
        {
            // 미니맵 켜짐 애니메이션 실행
            fadeController.ShowMinimap();
        }
        else
        {
            // 꺼질 땐 그냥 바로 UI 비활성화
            fadeController.HideMinimap();
        }

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
                iconObject.SetActive(false);
                Image iconImage = iconObject.GetComponent<Image>();
                if (iconImage != null)
                {
                    bool isActive = InfuserManager.Instance.activatedInfusers[i];
                    iconImage.sprite = isActive ? InfuserManager.Instance.LifeInfuserSO.activeIcon : InfuserManager.Instance.LifeInfuserSO.inactiveIcon;

                    string mapID = InfuserManager.Instance.infuser[i].GetComponent<LifeInfuser>().mapID ?? "Unknown";
                    infuserIcons.Add(new MinimapInfuserIcon
                    {
                        iconObject = iconObject,
                        mapID = mapID
                    });

                    UpdateObjectIconPosition(iconObject, InfuserManager.Instance.infuser[i].transform.position);
                }
            }
        }
    }

    private void UpdateObjectIcons()
    {
        for (int i = 0; i < infuserIcons.Count; i++)
        {
            var icon = infuserIcons[i];
            if (regionDict.ContainsKey(icon.mapID) && regionDict[icon.mapID].isRevealed)
            {
                icon.iconObject.SetActive(true);
                bool isActive = InfuserManager.Instance.activatedInfusers[i];
                icon.iconObject.GetComponent<Image>().sprite = isActive ? InfuserManager.Instance.LifeInfuserSO.activeIcon : InfuserManager.Instance.LifeInfuserSO.inactiveIcon;
            }
            else
            {
                icon.iconObject.SetActive(false);
            }
        }
    }

    private void UpdateObjectIconPosition(GameObject icon, Vector3 worldPosition)
    {
        Vector2 mapArea = new Vector2(
        right.position.x - left.position.x, 
        top.position.y - bottom.position.y
        );

        Vector2 objPos = new Vector2(
            worldPosition.x - left.position.x, 
            worldPosition.y - bottom.position.y
        );

        Vector2 normalPos = new Vector2(
            Mathf.Clamp01(objPos.x / mapArea.x),
            Mathf.Clamp01(objPos.y / mapArea.y)
        );

        RectTransform iconRect = icon.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            Vector2 minimapSize = minimapImage.rectTransform.sizeDelta;
            iconRect.anchoredPosition = new Vector2(
                minimapSize.x * normalPos.x, 
                minimapSize.y * normalPos.y
            );
        }
    }

    public void OnMapEntered(string mapID)
    {
        if (regionDict.ContainsKey(mapID) && !regionDict[mapID].isRevealed && !regionDict[mapID].hasInfuser)
        {
            regionDict[mapID].mapImage.enabled = true;
            regionDict[mapID].isRevealed = true;
        }
    }

    public void RevealMapFromInfuser(string mapID)
    {
        if (regionDict.ContainsKey(mapID) && !regionDict[mapID].isRevealed)
        {
            regionDict[mapID].mapImage.enabled = true;
            regionDict[mapID].isRevealed = true;
        }
    }
}