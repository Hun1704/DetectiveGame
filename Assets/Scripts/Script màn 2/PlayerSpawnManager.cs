using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        // 1. Đọc tên điểm đến từ bộ nhớ tạm
        string spawnPointName = PlayerPrefs.GetString("LastSpawnPoint");

        // 2. Nếu có dữ liệu (tức là vừa chuyển cảnh tới)
        if (!string.IsNullOrEmpty(spawnPointName))
        {
            // Tìm Game Object có tên trùng với tên đã lưu
            GameObject targetPoint = GameObject.Find(spawnPointName);

            if (targetPoint != null)
            {
                // Dịch chuyển nhân vật tới vị trí đó
                transform.position = targetPoint.transform.position;

                // (Tùy chọn) Nếu muốn nhân vật quay mặt đúng hướng, 
                // bạn có thể lấy scale của targetPoint gán cho player luôn.
            }
            else
            {
                Debug.LogWarning("Không tìm thấy điểm spawn có tên: " + spawnPointName);
            }

            // 3. Xóa dữ liệu để tránh lỗi khi reload game bình thường
            PlayerPrefs.DeleteKey("LastSpawnPoint");
        }
    }
}