using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cấu hình (BẮT BUỘC)")]
    [Tooltip("Copy y hệt cái ID bên trong CutsceneManager dán vào đây")]
    public string cutsceneID;

    private bool daKichHoat = false;

    void Start()
    {
        // 1. Kiểm tra ngay khi vào màn chơi
        // Nếu sự kiện này đã xong rồi thì tự hủy Trigger này luôn -> Không bao giờ kích hoạt lại
        if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
        {
            if (SaveGameManager.Instance.CheckEvent(cutsceneID))
            {
                Destroy(gameObject); // Xóa vùng kích hoạt
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (daKichHoat) return;

        if (other.CompareTag("Player"))
        {
            // 2. Kiểm tra lần cuối (cho chắc ăn) trước khi chạy
            if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
            {
                if (SaveGameManager.Instance.CheckEvent(cutsceneID))
                {
                    Destroy(gameObject);
                    return;
                }
            }

            Debug.Log("Player đã bước vào vùng kích hoạt: " + cutsceneID);
            daKichHoat = true;

            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.BatDauCutscene();
            }
        }
    }
}