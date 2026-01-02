using UnityEngine;

public class MotherEvent : MonoBehaviour
{
    public CutsceneManager cutsceneManager; // Kéo GameObject chứa script CutsceneManager vào đây
    private bool daKichHoat = false; // Đảm bảo chỉ chạy 1 lần duy nhất

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nếu Player đi vào VÀ chưa từng kích hoạt trước đó
        if (other.CompareTag("Player") && !daKichHoat)
        {
            if (cutsceneManager != null)
            {
                cutsceneManager.BatDauCutscene(); // Gọi lệnh diễn phim
                daKichHoat = true; // Khóa lại, không cho chạy lần 2
            }
        }
    }
}