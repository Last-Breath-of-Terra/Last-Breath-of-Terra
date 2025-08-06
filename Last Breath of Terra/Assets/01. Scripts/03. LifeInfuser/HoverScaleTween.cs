using DG.Tweening;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HoverScaleTween : MonoBehaviour
{
    [Header("Scale Settings")]
    public RectTransform targetRect;
    public Vector3 hoverScale = Vector3.one;
    public Vector3 normalScale = new Vector3(0.5f, 0.5f, 0.5f);
    public float tweenDuration = 2f;
    public float exitDelay = 5f;

    [Header("참조할 ScriptableObject")]
    [Tooltip("인스펙터에서 할당할 LifeInfuserSO 에셋")]
    public LifeInfuserSO lifeInfuserSO;

    private Tween exitTween;
    private bool isHovered = false;

    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        targetRect.localScale = normalScale;
        _raycaster = GetComponentInParent<Canvas>()?.GetComponent<GraphicRaycaster>();
        _eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (_raycaster == null || _eventSystem == null) return;

        // 1) 마우스 위치로 Raycast
        var pointerData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };
        var results = new List<RaycastResult>();
        _raycaster.Raycast(pointerData, results);

        // 2) 자신의 오브젝트가 hit 됐는지 검사
        bool nowOver = false;
        foreach (var res in results)
        {
            if (res.gameObject == gameObject)
            {
                nowOver = true;
                break;
            }
        }

        // 3) 상태 변경 시 로직 분기
        if (nowOver != isHovered)
        {
            isHovered = nowOver;
            Debug.Log($"[HoverScaleTween] Hover 상태: {isHovered}");

            if (isHovered)
            {
                if (lifeInfuserSO != null)
                    lifeInfuserSO.SetUIForInfuserStatus(true);
            }
            else
            {
                if (lifeInfuserSO != null)
                    lifeInfuserSO.SetUIForInfuserStatus(false);
            }
        }
    }
}
