using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelStartEvent : MonoBehaviour
{
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

    [Header("--- SFX (Tiếng động) ---")]
    public AudioSource sfxAudioSource;

    void Start()
    {
        // Kiểm tra Save Game: Nếu sự kiện này đã xảy ra rồi thì thôi
        if (SaveGameManager.Instance != null && SaveGameManager.Instance.CheckEvent(eventID))
        {
            return;
        }

        if (sfxAudioSource == null) sfxAudioSource = GetComponent<AudioSource>();

        StartCoroutine(ChayKichBanDauGame());
    }

    IEnumerator ChayKichBanDauGame()
    {
        // Bước A: Chờ người chơi nhìn ngắm
        yield return new WaitForSeconds(thoiGianCho);

        // Bước B: Chạy từng dòng thoại nội tâm
        foreach (var dong in danhSachLoiThoai)
        {
            if (dong.sfxKemTheo != null && sfxAudioSource != null)
                sfxAudioSource.PlayOneShot(dong.sfxKemTheo);

            InventoryManager.Instance.ShowDialogue(dong.noiDung, dong.bieuCam);

            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // Bước C: Lưu lại là "Đã xem xong"
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.CompleteEvent(eventID);
        }
    }
}