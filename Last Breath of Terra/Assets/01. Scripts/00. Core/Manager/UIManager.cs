using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject cursorIndicator;
    public Light2D clickLight;
    public GameObject movingLightEffect;
    public GameObject jumpLightEffect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        UpdateCursorIndicator();
    }

    public void UpdateCursorIndicator()
    {
        if (cursorIndicator != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            cursorIndicator.transform.position = worldPosition;
            cursorIndicator.SetActive(true);
        }
    }

    public void HandleClickLight(Vector2 position)
    {
        // 커서 오브젝트 비활성화
        cursorIndicator.SetActive(false);

        // ClickLight 및 이동 효과 스프라이트 활성화 및 위치 설정
        clickLight.transform.position = position;
        clickLight.gameObject.SetActive(true);
        movingLightEffect.transform.position = position;
        movingLightEffect.SetActive(true);

        // 짧은 시간 후 커서를 다시 활성화하고, 빛 효과 비활성화
        Invoke("ReactivateCursor", 0.5f);
        Invoke("DeactivateClickLight", 1.0f); // 클릭 빛 효과 1초 후 비활성화
    }

    public void HandleJumpLight(Vector2 position)
    {
        // 점프 효과 스프라이트 활성화 및 위치 설정
        jumpLightEffect.transform.position = position;
        jumpLightEffect.SetActive(true);

        // 짧은 시간 후 점프 빛 효과 비활성화
        Invoke("DeactivateJumpLight", 1.0f); // 점프 빛 효과 1초 후 비활성화
    }

    private void ReactivateCursor()
    {
        cursorIndicator.SetActive(true);
    }

    private void DeactivateClickLight()
    {
        clickLight.gameObject.SetActive(false);
        movingLightEffect.SetActive(false);
    }

    private void DeactivateJumpLight()
    {
        jumpLightEffect.SetActive(false);
    }
}
