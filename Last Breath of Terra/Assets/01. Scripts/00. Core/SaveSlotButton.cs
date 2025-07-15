using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TitleSceneManager titleManager;
    public GameObject flameEffect;

    private int slotIndex;

    public void Setup(int index)
    {
        slotIndex = index;
        nameText.text = $"저장 {index + 1}";
    }

    public void OnClick()
    {
        Debug.Log($"슬롯 선택됨");
        DataManager.Instance.playerIndex = slotIndex;
        titleManager.OnSaveSlotSelected();
    }

    public void SetFlameVisible(bool visible)
    {
        if (flameEffect == null) return;

        flameEffect.SetActive(true); // 일단 켜기

        StopAllCoroutines();
        StartCoroutine(ScaleFlame(flameEffect.transform, visible ? Vector3.zero : Vector3.one, visible ? Vector3.one : Vector3.zero, 0.5f));
    }

    private IEnumerator ScaleFlame(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;
        target.localScale = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, time / duration);
            yield return null;
        }

        target.localScale = to;

        if (to == Vector3.zero) target.gameObject.SetActive(false); // 다시 숨기기
    }

}
