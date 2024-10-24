using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public RectTransform minimapRectTransform;
    public GameObject navigationMarker;
    public Button closeBtn;
    public Transform player; //다음에 GameManager로 참조 하기~
    public Vector2 expandedSize = new Vector2(1500, 1100);
    public float animationDuration = 0.5f;

    private bool isExpanded = false;
    private Vector2 originalSize;
    private Vector3 originalPosition;
    private Vector2 targetSize;
    private Vector3 targetPosition;

    private Canvas canvas;


    void Start()
    {
        originalSize = minimapRectTransform.sizeDelta;
        originalPosition = minimapRectTransform.anchoredPosition;

        targetSize = originalSize;
        targetPosition = originalPosition;

        canvas = minimapRectTransform.GetComponentInParent<Canvas>();
        closeBtn.onClick.AddListener(OnCloseButtonClick);
    }

    void Update()
    {
        minimapRectTransform.sizeDelta = Vector2.Lerp(minimapRectTransform.sizeDelta, targetSize, Time.deltaTime * 10);
        minimapRectTransform.anchoredPosition = Vector3.Lerp(minimapRectTransform.anchoredPosition, targetPosition, Time.deltaTime * 10);
    }

    public void OnMinimapClick()
    {
        if (!isExpanded)
        {
            targetSize = expandedSize;
            targetPosition = GetCanvasCenterPosition();
            closeBtn.gameObject.SetActive(true);
        }

        isExpanded = true;
    }

    public void OnCloseButtonClick()
    {
        targetSize = originalSize;
        targetPosition = originalPosition;
        isExpanded = false;
        closeBtn.gameObject.SetActive(false);
    }

    public void OnTargetAreaClick(Vector3 targetPosition)
    {
        if (isExpanded)
        {
            OnCloseButtonClick();
            
            navigationMarker.SetActive(true);
            Vector3 direction = targetPosition - player.position;
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg; // XZ 평면에서 방향 계산
            navigationMarker.transform.rotation = Quaternion.Euler(0, 0, -angle); // 마커를 회전시켜 방향을 가리키도록 설정
        }
    }

    private Vector3 GetCanvasCenterPosition()
    {
        if (canvas != null)
        {
            return Vector3.zero;
        }

        return originalPosition;
    }
}