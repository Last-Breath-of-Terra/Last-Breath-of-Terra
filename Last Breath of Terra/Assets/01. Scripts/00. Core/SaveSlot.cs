using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlot : MonoBehaviour
{
    public Button[] slotButtons;
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
                btn.SetFlameVisible(hasSave);
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
            slotButtons[currentIndex].onClick.Invoke();
        }
    }

    void HighlightCurrentSlot()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            var txt = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            txt.color = (i == currentIndex) ? Color.yellow : Color.white;
        }
    }
}
