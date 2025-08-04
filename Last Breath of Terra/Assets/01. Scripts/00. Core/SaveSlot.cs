using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SaveSlot : MonoBehaviour
{
    public Button[] slotButtons;
    public RectTransform selectArrow;
    public TitleSceneManager titleManager;

    private int currentIndex = 0;

    void Start()
    {
        HighlightCurrentSlot();

        for (int i = 0; i < slotButtons.Length; i++)
        {
            var btn = slotButtons[i].GetComponent<SaveSlotButton>();

            bool hasSave = DataManager.Instance.HasSave(i);

            if (btn != null)
            {
                btn.Setup(i);
                slotButtons[i].interactable = false; // 마우스 클릭 X
            }
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.D))
        {
            currentIndex = Mathf.Min(currentIndex + 1, slotButtons.Length - 1);
            HighlightCurrentSlot();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentIndex == 0)
            {
                titleManager.BackToIntroFromSave();
            }
            else
            {
                currentIndex = Mathf.Max(currentIndex - 1, 0);
                HighlightCurrentSlot();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            slotButtons[currentIndex].GetComponent<SaveSlotButton>().OnClick();
        }
    }

    void HighlightCurrentSlot()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var rect = slotButtons[i].GetComponent<RectTransform>();
            rect.DOComplete(); // 애니메이션 중복 방지
            rect.DOScale(i == currentIndex ? 8f : 6f, 0.2f).SetEase(Ease.OutQuad);
        }

        if (selectArrow != null)
        {
            RectTransform slotRect = slotButtons[currentIndex].GetComponent<RectTransform>();

            float targetX = slotRect.anchoredPosition.x;
            float fixedY = selectArrow.anchoredPosition.y; // 현재 y값 그대로 유지

            selectArrow.DOComplete();
            selectArrow.DOAnchorPos(new Vector2(targetX, fixedY), 0.2f).SetEase(Ease.OutQuad);
        }
    }
}
