using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    public TitleSceneManager titleManager;
    public Sprite savedSlotSprite;
    public Sprite emptySlotSprite;

    private Image slotImage;
    private int slotIndex;

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    public void Setup(int index)
    {
        slotIndex = index;

        bool hasSave = DataManager.Instance.HasSave(index);
        if (slotImage != null)
        {
            slotImage.sprite = hasSave ? savedSlotSprite : emptySlotSprite;
        }
    }

    public void OnClick()
    {
        Debug.Log($"슬롯 선택됨");
        titleManager.SelectSlot(slotIndex);
    }

    public void Refresh()
    {
        Setup(slotIndex);
    }

    public int SlotIndex() => slotIndex;
}
