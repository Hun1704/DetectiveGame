using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleport : MonoBehaviour
{
    [Header("Cấu hình Chuyển Cảnh")]
    [Tooltip("Tên chính xác của Scene muốn đến (VD: Man2, Man2_5)")]
    public string tenSceneTiepTheo;

    [Tooltip("Tên của điểm muốn xuất hiện ở màn sau (VD: CuaTuMan2_5)")]
    public string tenDiemDen; // <-- THÊM MỚI

    [Header("Cấu hình Save Game (Tùy chọn)")]
    public bool canSaveGame = true;
    public bool laDauChapterMoi = false;
    public int chapterHienTai = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChuyenCanh();
        }
    }

    void ChuyenCanh()
    {
        Debug.Log("Đang chuyển sang: " + tenSceneTiepTheo + " tại điểm: " + tenDiemDen);

        // 1. Lưu lại cái tên điểm đến vào bộ nhớ tạm (PlayerPrefs)
        // Đây là "tờ giấy nhớ" để mang sang màn sau đọc
        if (!string.IsNullOrEmpty(tenDiemDen))
        {
            PlayerPrefs.SetString("LastSpawnPoint", tenDiemDen);
        }

        // 2. Logic Save Game cũ của bạn (Giữ nguyên nếu bạn muốn save dữ liệu)
        if (canSaveGame && SaveGameManager.Instance != null)
        {
            if (laDauChapterMoi)
            {
                SaveGameManager.Instance.SaveAsNewChapter(chapterHienTai + 1, tenSceneTiepTheo);
            }
            else
            {
                // Lưu ý: Nếu chỉ chuyển phòng nhỏ thì thường không cần SaveAsNewChapter
                // Nhưng nếu logic game bạn yêu cầu thì cứ giữ nguyên.
                SaveGameManager.Instance.SaveAsNewChapter(chapterHienTai, tenSceneTiepTheo);
            }
        }

        // 3. Chuyển cảnh
        SceneManager.LoadScene(tenSceneTiepTheo);
    }
}