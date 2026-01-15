using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Cần cái này để dùng List

public class StoryTrigger : MonoBehaviour
{
    [Header("Cấu hình Sự kiện")]
    public string eventID = "gap_me_lan_dau";
    public CutsceneManager cutsceneManager;

    [Header("Cấu hình Thời gian")]
    [Tooltip("Sau khi âm thanh vang lên, đợi bao nhiêu giây mới hiện chữ?")]
    public float thoiGianChoAmThanh = 2.0f;

    // --- 🔥 CẬP NHẬT MỚI: DÙNG LIST ĐỂ CHỨA NHIỀU CÂU ---
    [System.Serializable]
    public class CauThoaiChiTiet
    {
        [TextArea(3, 10)]
        public string noiDung;  // Nội dung câu nói
        public Sprite bieuCam;  // Cảm xúc riêng cho câu này (nếu để trống sẽ dùng cái mặc định hoặc cái cũ)
    }

    [Header("Hội thoại mở đầu")]
    public List<CauThoaiChiTiet> danhSachLoiThoai; // Thay thế cho biến string loiThoai cũ

    [Header("Âm thanh ban đầu")]
    public AudioClip amThanhSoc;
    public AudioSource audioSource;

    [Tooltip("0 = Mặc định. Số càng lớn chữ chạy càng chậm.")]
    public float tocDoRieng = 0f;

    private bool daKichHoat = false;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !daKichHoat)
        {
            if (SaveGameManager.Instance.CheckEvent(eventID))
                return;

            StartCoroutine(QuyTrinhKichHoat());
        }
    }

    IEnumerator QuyTrinhKichHoat()
    {
        daKichHoat = true;

        // 1. Phát âm thanh Sốc (Giữ nguyên logic cũ)
        if (audioSource != null && amThanhSoc != null)
        {
            audioSource.PlayOneShot(amThanhSoc);
        }

        // 2. Đợi âm thanh ngấm vào người chơi
        yield return new WaitForSeconds(thoiGianChoAmThanh);

        // 3. 🔥 LOGIC MỚI: Chạy vòng lặp từng câu thoại
        foreach (var cau in danhSachLoiThoai)
        {
            // Gọi hiển thị (truyền nội dung và biểu cảm của câu đó vào)
            InventoryManager.Instance.ShowDialogue(cau.noiDung, cau.bieuCam, tocDoRieng);

            yield return null; // Đợi 1 frame cho UI bật lên

            // Chờ người chơi đọc xong câu này mới qua câu kia
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // 4. Sau khi nói hết danh sách -> Gọi CutsceneManager
        if (cutsceneManager != null)
        {
            cutsceneManager.BatDauCutscene();
        }

        // 5. Lưu game
        SaveGameManager.Instance.CompleteEvent(eventID);
    }
}