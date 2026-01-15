using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    // Biến này để đảm bảo cutscene chỉ kích hoạt 1 lần duy nhất
    private bool daKichHoat = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Kiểm tra: Chưa kích hoạt lần nào AND Người chạm vào là Player
        if (!daKichHoat && other.CompareTag("Player"))
        {
            Debug.Log("Player đã bước vào vùng kích hoạt!");

            // 2. Khóa lại ngay lập tức để không bị lặp
            daKichHoat = true;

            // 3. Gọi lệnh bắt đầu bên CutsceneManager
            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.BatDauCutscene();
            }
            else
            {
                Debug.LogError("Lỗi: Không tìm thấy CutsceneManager trong Scene!");
            }

            // 4. (Tùy chọn) Tự hủy object trigger này đi cho nhẹ game
            // Destroy(gameObject); 
        }
    }
}