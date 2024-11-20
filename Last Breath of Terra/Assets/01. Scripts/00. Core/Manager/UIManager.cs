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

    private bool isHoldingClick;

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

    void FixedUpdate()
    {
        UpdateCursorIndicator();
    }

    private void UpdateCursorIndicator()
    {
        if (cursorIndicator != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            cursorIndicator.transform.position = worldPosition;
            cursorIndicator.SetActive(true);
        }
    }

    #region LightSystem
    public void HandleClickLight(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
        if (hit.collider != null)
        {
            Vector3 hitPosition = hit.point;
            clickLight.transform.position = new Vector3(hitPosition.x, hitPosition.y, clickLight.transform.position.z);
        }
        else
        {
            clickLight.transform.position = new Vector3(position.x, position.y, clickLight.transform.position.z);
        }

        clickLight.gameObject.SetActive(true);

        movingLightEffect.transform.position = new Vector3(position.x, position.y, movingLightEffect.transform.position.z);
        movingLightEffect.SetActive(true);

        if (!isHoldingClick)
        {
            AudioManager.instance.PlaySFX("light_start", gameObject.GetComponent<AudioSource>(), clickLight.transform);
            
            AudioManager.instance.PlayCancelable("light_being", gameObject.GetComponent<AudioSource>(), clickLight.transform);
        }

        isHoldingClick = true;
    }

    public void HandleJumpLight(Vector2 position)
    {
        if(!isHoldingClick)
        {
            clickLight.transform.position = position;
            clickLight.gameObject.SetActive(true);
        }

        jumpLightEffect.transform.position = position;
        jumpLightEffect.SetActive(true);

        Invoke("DeactivateJumpLight", 0.5f);
    }

    private void DeactivateJumpLight()
    {
        jumpLightEffect.gameObject.SetActive(false);
        if (!isHoldingClick)
        {
            clickLight.gameObject.SetActive(false);
        }
    }
    #endregion

    public void ReleaseClick()
    {
        isHoldingClick = false;
        clickLight.gameObject.SetActive(false);
    }
}
