using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex = 1;

    [Header("UI")]
    public TextMeshProUGUI slotText;
    public Button slotButton;
    public CanvasGroup canvasGroup;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (SaveGameManager.Instance == null) return;

        var data = SaveGameManager.Instance.GetSaveData(slotIndex);

        if (data == null)
        {
            // --- SLOT TRỐNG ---
            slotText.text = $"Slot {slotIndex}\n<color=#888888>Trống</color>";
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            // --- SLOT ĐÃ LƯU ---
            slotText.text =
                $"Slot {slotIndex}\n" +
                $"Chapter {data.chapter}\n" +
                $"<size=70%>{data.saveTime}</size>";

            canvasGroup.alpha = 1f;
        }
    }
}
