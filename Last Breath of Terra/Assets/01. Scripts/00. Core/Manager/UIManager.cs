using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

/// <summary>
/// UI 관련 요소와 효과(광원, 커서, 미니맵 등)를 관리하는 클래스.
/// </summary>

public class UIManager : MonoBehaviour
{
    public MiniMapManager miniMapManager;

    public GameObject cursorIndicator;
    public Light2D clickLight;
    public GameObject movingLightEffect;
    public GameObject jumpLightEffect;

    private bool isHoldingClick;

    void FixedUpdate()
    {
        UpdateCursorIndicator();
    }

    private void UpdateCursorIndicator()
    {
        if (cursorIndicator != null && cursorIndicator.activeSelf)
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
        Color color = cursorIndicator.GetComponent<SpriteRenderer>().color;
        color.a = 0f;
        cursorIndicator.GetComponent<SpriteRenderer>().color = color;

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

            gameObject.GetComponent<AudioSource>().loop = true;
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

        AudioManager.instance.PlaySFX("spark_jump", gameObject.GetComponent<AudioSource>(), clickLight.transform);

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
        Color color = cursorIndicator.GetComponent<SpriteRenderer>().color;
        color.a = 100f;
        cursorIndicator.GetComponent<SpriteRenderer>().color = color;
        clickLight.gameObject.SetActive(false);
        gameObject.GetComponent<AudioSource>().loop = false;
        gameObject.GetComponent<AudioSource>().Stop();
    }
}
