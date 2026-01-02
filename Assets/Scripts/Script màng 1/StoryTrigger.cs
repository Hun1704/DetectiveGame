using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [Header("Nội dung cốt truyện")]
    [TextArea(3, 10)]
    public string loiThoai;

    [Header("Cảm xúc nhân vật")]
    public Sprite anhCamXuc; // <-- THÊM DÒNG NÀY: Kéo ảnh Khóc/Sốc vào đây

    [Tooltip("0 = Mặc định. Số càng lớn chữ chạy càng chậm (Buồn).")]
    public float tocDoRieng = 0f;

    private bool daKichHoat = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !daKichHoat)
        {
            // Gửi thêm tocDoRieng vào hàm
            InventoryManager.Instance.ShowDialogue(loiThoai, anhCamXuc, tocDoRieng);

            daKichHoat = true;
            Debug.Log("Đã kích hoạt sự kiện cốt truyện: " + gameObject.name);
        }
    }
}