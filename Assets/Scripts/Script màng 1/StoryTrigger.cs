using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    public string eventID = "gap_me_lan_dau";
    public CutsceneManager cutsceneManager;

    [Header("Nội dung cốt truyện")]
    [TextArea(3, 10)]
    public string loiThoai;

    [Header("Cảm xúc nhân vật")]
    public Sprite anhCamXuc; // <-- THÊM DÒNG NÀY: Kéo ảnh Khóc/Sốc vào đây

    [Tooltip("0 = Mặc định. Số càng lớn chữ chạy càng chậm (Buồn).")]
    public float tocDoRieng = 0f;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra trong File Save xem đã làm chưa
            if (SaveGameManager.Instance.CheckEvent(eventID))
                return; // Nếu làm rồi thì thôi, không chạy nữa

            if (cutsceneManager != null)
            {
                cutsceneManager.BatDauCutscene();

                // Đánh dấu đã hoàn thành vào File Save
                SaveGameManager.Instance.CompleteEvent(eventID);
            }
        }
    }
}