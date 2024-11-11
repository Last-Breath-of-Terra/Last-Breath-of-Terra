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
        cursorIndicator.SetActive(false);

        clickLight.transform.position = new Vector3(position.x, position.y, 0);
        clickLight.gameObject.SetActive(true);

        movingLightEffect.transform.position = new Vector3(position.x, position.y, 0);
        movingLightEffect.SetActive(true);
    }

    public void HandleJumpLight(Vector2 position)
    {
        cursorIndicator.SetActive(false);

        clickLight.transform.position = position;
        clickLight.gameObject.SetActive(true);

        jumpLightEffect.transform.position = position;
        jumpLightEffect.SetActive(true);

        Invoke("DeactivateJumpLight", 0.5f);
    }

    public void ReactivateCursor()
    {
        cursorIndicator.SetActive(true);
    }

    public void DeactivateClickLight()
    {
        clickLight.gameObject.SetActive(false);
        movingLightEffect.SetActive(false);
    }

    private void DeactivateJumpLight()
    {
        clickLight.gameObject.SetActive(false);
        jumpLightEffect.SetActive(false);
    }
}
