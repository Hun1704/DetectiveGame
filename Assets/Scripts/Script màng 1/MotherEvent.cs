using UnityEngine;

public class MotherEvent : MonoBehaviour
{
    public string eventID = "gap_me_lan_dau";
    public CutsceneManager cutsceneManager; // Kéo GameObject chứa script CutsceneManager vào đây

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra trong File Save xem đã làm chưa
            if (SaveGameManager.Instance.CheckEvent(eventID))
                return; // Nếu làm rồi thì thôi, không chạy nữa

            if (cutsceneManager != null)
            {
                cutsceneManager.BatDauCutscene();

                // Đánh dấu đã hoàn thành vào File Save
                SaveGameManager.Instance.CompleteEvent(eventID);
            }
        }
    }
}