using UnityEngine;
using UnityEngine.UI; // Thêm thư viện UI để dùng Button
using UnityEngine.SceneManagement; // Thêm thư viện để chuyển Scene
using System.Collections;

public class ChapterEndPanel : MonoBehaviour
{
    [Header("--- HIỆU ỨNG HÌNH ẢNH ---")]
    public CanvasGroup canvasGroup;
    public float tocDoHien = 1.0f; // Tốc độ hiện bảng (số càng lớn hiện càng nhanh)

    [Header("--- LOGIC CHUYỂN MÀN ---")]
    public Button nutTiepTuc;        // Kéo nút "Tiếp Tục" vào đây
    public string tenSceneTiepTheo = "Man2"; // Tên Scene của màn 2
    public int soChapterTiepTheo = 2;        // Số thứ tự Chapter mới (để lưu vào Save)

    // Hàm này chạy mỗi khi GameObject được SetActive(true)
    void OnEnable()
    {
        // 1. Reset Alpha về 0 để bắt đầu hiện
        if (canvasGroup != null) canvasGroup.alpha = 0;

        // 2. Chạy hiệu ứng hiện từ từ
        StartCoroutine(FadeIn());
    }

    void Start()
    {
        // 3. Gán sự kiện click cho nút Tiếp Tục
        if (nutTiepTuc != null)
        {
            nutTiepTuc.onClick.RemoveAllListeners(); // Xóa sự kiện cũ cho chắc
            nutTiepTuc.onClick.AddListener(OnContinueClick);
        }
    }

    // --- PHẦN 1: HIỆU ỨNG HÌNH ẢNH ---
    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.blocksRaycasts = true; // Chặn chuột click xuyên qua

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * tocDoHien;
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    // --- PHẦN 2: LOGIC CHUYỂN CẢNH & SAVE ---
    void OnContinueClick()
    {
        Debug.Log($"Người chơi bấm Tiếp Tục. Đang chuyển sang: {tenSceneTiepTheo}");

        if (SaveGameManager.Instance != null)
        {
            // 🔥 QUAN TRỌNG: Gọi hàm Save đặc biệt cho chuyển màn
            // Hàm này giúp Reset vị trí nhân vật về điểm xuất phát của màn mới
            // nhưng vẫn giữ nguyên Túi đồ (Inventory)
            SaveGameManager.Instance.SaveAsNewChapter(soChapterTiepTheo, tenSceneTiepTheo);
        }

        // Chuyển sang màn 2
        SceneManager.LoadScene(tenSceneTiepTheo);
    }
}