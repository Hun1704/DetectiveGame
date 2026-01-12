using UnityEngine;
using System.Collections;

public class StoryTrigger : MonoBehaviour
{
    [Header("Cấu hình Sự kiện")]
    public string eventID = "gap_me_lan_dau"; // ID để lưu game
    public CutsceneManager cutsceneManager;     // Kéo CutsceneManager vào đây

    [Header("Cấu hình Thời gian (MỚI)")]
    [Tooltip("Sau khi âm thanh vang lên, đợi bao nhiêu giây mới hiện chữ?")]
    public float thoiGianChoAmThanh = 2.0f; // 🔥 MỚI: Mặc định đợi 2 giây

    [Header("Hội thoại mở đầu (Trước khi vào Cutscene)")]
    [TextArea(3, 10)]
    public string loiThoai; // Nội dung: "Ôi trời, cái gì thế này?"

    [Header("Cảm xúc & Âm thanh")]
    public Sprite anhCamXuc;      // Ảnh mặt sốc
    public AudioClip amThanhSoc;  // 🔥 MỚI: Tiếng "Hả?!", tiếng nhạc giật gân...
    public AudioSource audioSource; // Loa phát nhạc (tự tìm nếu quên kéo)

    [Tooltip("0 = Mặc định. Số càng lớn chữ chạy càng chậm.")]
    public float tocDoRieng = 0f;

    private bool daKichHoat = false;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ chạy khi Player bước vào VÀ chưa kích hoạt trong phiên chơi này
        if (other.CompareTag("Player") && !daKichHoat)
        {
            // Kiểm tra Save Game: Nếu làm rồi thì thôi
            if (SaveGameManager.Instance.CheckEvent(eventID))
                return;

            // Bắt đầu chuỗi hành động
            StartCoroutine(QuyTrinhKichHoat());
        }
    }

    IEnumerator QuyTrinhKichHoat()
    {
        daKichHoat = true; // Khóa lại ngay để không bị trigger 2 lần

        // 1. Phát âm thanh Sốc (nếu có)
        if (audioSource != null && amThanhSoc != null)
        {
            audioSource.PlayOneShot(amThanhSoc);
        }

        yield return new WaitForSeconds(thoiGianChoAmThanh);

        // 2. Hiện thoại nhân vật (Kèm ảnh cảm xúc)
        // Hệ thống Inventory sẽ tự động khóa di chuyển của Player
        InventoryManager.Instance.ShowDialogue(loiThoai, anhCamXuc, tocDoRieng);

        // 3. 🔥 QUAN TRỌNG: Chờ người chơi đọc xong thoại
        // (Đây là bước bạn bị thiếu trước đó)
        yield return null; // Chờ 1 frame để UI kịp bật
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 4. Sau khi thoại xong -> Mới gọi CutsceneManager làm việc
        if (cutsceneManager != null)
        {
            cutsceneManager.BatDauCutscene();
        }

        // 5. Lưu game là đã xong sự kiện này
        SaveGameManager.Instance.CompleteEvent(eventID);
    }
}