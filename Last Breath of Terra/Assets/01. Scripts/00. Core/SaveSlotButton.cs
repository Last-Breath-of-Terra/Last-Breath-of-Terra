using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    public TitleSceneManager titleManager;
    public GameObject flameEffect;

    private int slotIndex;

    public void Setup(int index)
    {
        slotIndex = index;
    }

    public void OnClick()
    {
        Debug.Log($"슬롯 선택됨");
        DataManager.Instance.playerIndex = slotIndex;
        titleManager.OnSaveSlotSelected();
    }

    void SetFlameScale(float scale)
    {
        var ps = flameEffect.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = scale;
        }
    }

    // public void SetFlameVisible(bool visible)
    // {
    //     if (flameEffect == null) return;

    //     flameEffect.SetActive(true); // 일단 켜기

    //     StopAllCoroutines();
    //     StartCoroutine(ScaleFlame(flameEffect.transform, visible ? Vector3.zero : Vector3.one, visible ? Vector3.one : Vector3.zero, 0.5f));
    // }

}
