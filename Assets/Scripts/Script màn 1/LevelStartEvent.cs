using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelStartEvent : MonoBehaviour
{
    [Header("Cấu hình Chung")]
    [Tooltip("Nếu TÍCH: Chỉ chạy 1 lần duy nhất (Màn 1).\nNếu BỎ TÍCH: Luôn chạy (Màn 2).")]
    public bool canLuuTrangThai = true;

    [Header("--- LIÊN KẾT CUTSCENE ---")]
    [Tooltip("Kéo CutsceneManager vào đây.")]
    public CutsceneManager cutsceneTiepTheo;

    [Tooltip("Đợi bao nhiêu giây sau khi thoại xong mới chuyển sang Cutscene?")]
    public float thoiGianNghiChuyenCanh = 1.0f; // 🔥 MỚI: Biến chỉnh thời gian nghỉ

    [Header("Cấu hình Sự kiện")]
    public string eventID = "intro_man_1";
    public float thoiGianCho = 1.5f;

    [System.Serializable]
    public class LoiThoaiDauGame
    {
        [TextArea(2, 5)]
        public string noiDung;
        public Sprite bieuCam;
        public AudioClip sfxKemTheo;
    }

    [Header("Nội dung Hội thoại")]
    public List<LoiThoaiDauGame> danhSachLoiThoai;

    [Header("--- SFX ---")]
    public AudioSource sfxAudioSource;

    void Start()
    {
        if (canLuuTrangThai)
        {
            if (SaveGameManager.Instance != null && SaveGameManager.Instance.CheckEvent(eventID))
            {
                return;
            }
        }

        if (sfxAudioSource == null) sfxAudioSource = GetComponent<AudioSource>();

        StartCoroutine(ChayKichBanDauGame());
    }

    IEnumerator ChayKichBanDauGame()
    {
        // Bước A: Chờ lúc đầu
        yield return new WaitForSeconds(thoiGianCho);

        // Bước B: Chạy thoại nội tâm
        foreach (var dong in danhSachLoiThoai)
        {
            if (dong.sfxKemTheo != null && sfxAudioSource != null)
                sfxAudioSource.PlayOneShot(dong.sfxKemTheo);

            InventoryManager.Instance.ShowDialogue(dong.noiDung, dong.bieuCam);

            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // Bước C: Lưu (nếu cần)
        if (canLuuTrangThai)
        {
            if (SaveGameManager.Instance != null)
            {
                SaveGameManager.Instance.CompleteEvent(eventID);
            }
        }

        // 🔥 BƯỚC D: XỬ LÝ CHUYỂN TIẾP (CÓ NGHỈ)
        if (cutsceneTiepTheo != null)
        {
            // Nghỉ một chút cho người chơi thấm câu thoại cuối
            if (thoiGianNghiChuyenCanh > 0)
            {
                yield return new WaitForSeconds(thoiGianNghiChuyenCanh);
            }

            Debug.Log("Hết thời gian nghỉ -> Chuyển sang Cutscene...");
            cutsceneTiepTheo.BatDauCutscene();
        }
    }
}