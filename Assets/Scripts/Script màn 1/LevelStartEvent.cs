using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelStartEvent : MonoBehaviour
{
    [Header("Cấu hình Chung")]
    [Tooltip("Nếu TÍCH: Chỉ chạy 1 lần duy nhất (Màn 1).\nNếu BỎ TÍCH: Luôn chạy mỗi khi vào lại Màn.")]
    public bool canLuuTrangThai = true;

    [Header("--- LIÊN KẾT CUTSCENE ---")]
    [Tooltip("Kéo CutsceneManager vào đây (nếu sau khi thoại xong muốn chuyển cảnh ngay).")]
    public CutsceneManager cutsceneTiepTheo;

    [Tooltip("Đợi bao nhiêu giây sau khi thoại xong mới chuyển sang Cutscene?")]
    public float thoiGianNghiChuyenCanh = 1.0f;

    [Header("Cấu hình Sự kiện")]
    public string eventID = "intro_man_1";
    public float thoiGianCho = 1.5f;

    // --- 🔥 CẬP NHẬT CẤU TRÚC LỜI THOẠI ---
    [System.Serializable]
    public class LoiThoaiDauGame
    {
        [Header("Ai nói?")]
        [Tooltip("Để TRỐNG = Nhân vật chính (Player).\nNhập ID (VD: 'quan', 'ngoc_khue') = NPC nói.")]
        public string characterID;

        [TextArea(2, 5)]
        public string noiDung;

        public Sprite bieuCam; // Ảnh cảm xúc (nếu có)
        public AudioClip sfxKemTheo; // Âm thanh (nếu có)
    }

    [Header("Nội dung Hội thoại")]
    public List<LoiThoaiDauGame> danhSachLoiThoai;

    [Header("--- SFX ---")]
    public AudioSource sfxAudioSource;

    void Start()
    {
        // Kiểm tra Save Game: Nếu làm rồi thì thôi
        if (canLuuTrangThai)
        {
            if (SaveGameManager.Instance != null && SaveGameManager.Instance.CheckEvent(eventID))
            {
                // Nếu sự kiện này đã xong, ta hủy object này luôn để đỡ nặng game
                Destroy(gameObject);
                return;
            }
        }

        if (sfxAudioSource == null) sfxAudioSource = GetComponent<AudioSource>();

        StartCoroutine(ChayKichBanDauGame());
    }

    IEnumerator ChayKichBanDauGame()
    {
        // Bước A: Chờ một chút cho màn hình sáng lên hẳn
        yield return new WaitForSeconds(thoiGianCho);

        // Bước B: Chạy danh sách thoại
        foreach (var dong in danhSachLoiThoai)
        {
            // 1. Phát âm thanh (nếu có)
            if (dong.sfxKemTheo != null && sfxAudioSource != null)
                sfxAudioSource.PlayOneShot(dong.sfxKemTheo);

            // 2. 🔥 HIỂN THỊ HỘI THOẠI DỰA TRÊN ID
            if (InventoryManager.Instance != null)
            {
                if (string.IsNullOrEmpty(dong.characterID))
                {
                    // Trường hợp 1: ID trống -> Là PLAYER nói (Dùng hàm cũ)
                    InventoryManager.Instance.ShowDialogue(dong.noiDung, dong.bieuCam);
                }
                else
                {
                    // Trường hợp 2: Có ID -> Là NPC nói (Dùng hàm ByID)
                    InventoryManager.Instance.ShowDialogueByID(dong.characterID, dong.noiDung, dong.bieuCam);
                }
            }

            // 3. Chờ đợi
            yield return null; // Chờ 1 frame để UI bật lên

            // Chờ người chơi bấm chuột xong câu này mới qua câu sau
            if (InventoryManager.Instance != null)
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // Bước C: Lưu trạng thái (nếu cần)
        if (canLuuTrangThai)
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.CompleteEvent(eventID);
                // Auto Save nhẹ một cái để nhớ là đã xem intro rồi
                SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
            }
        }

        // Bước D: Xử lý chuyển tiếp (Nếu có Cutscene nối tiếp)
        if (cutsceneTiepTheo != null)
        {
            if (thoiGianNghiChuyenCanh > 0)
                yield return new WaitForSeconds(thoiGianNghiChuyenCanh);

            Debug.Log("Hết thời gian nghỉ -> Chuyển sang Cutscene tiếp theo...");
            cutsceneTiepTheo.BatDauCutscene();
        }
    }
}