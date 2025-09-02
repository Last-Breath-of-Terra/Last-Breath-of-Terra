using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaleTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public LifeInfuserSO lifeInfuserSO;

    private bool isHovered = false;
    private bool isScaledUp = false;
    private Coroutine _coroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("HoverScaleTween OnPointerEnter");
        isHovered = true;
        if (_coroutine != null) { StopCoroutine(_coroutine); _coroutine = null; }

        if (lifeInfuserSO != null && !isScaledUp)
        {
            lifeInfuserSO.SetUIForInfuserStatus(true);
            isScaledUp = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(UIManager.Instance.hideDelay);
        if (!isHovered && lifeInfuserSO != null && isScaledUp)
        {
            lifeInfuserSO.SetUIForInfuserStatus(false);
            isScaledUp = false;
        }
        _coroutine = null;
    }

    private void OnDisable()
    {
        if (_coroutine != null) { StopCoroutine(_coroutine); _coroutine = null; }
    }
}