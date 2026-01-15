using UnityEngine;

public class TableTrigger : MonoBehaviour
{
    public GameObject nutMoBan;       // Nút UI nhỏ (hình bàn tay)
    public GameObject banPhaChePanel; // Cái Panel to đùng

    void Start()
    {
        // Kiểm tra kỹ trước khi tắt lúc đầu game
        if (nutMoBan != null) nutMoBan.SetActive(false);
        if (banPhaChePanel != null) banPhaChePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // SỬA LỖI: Luôn kiểm tra xem nút có còn tồn tại không trước khi bật
            if (nutMoBan != null)
            {
                nutMoBan.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // SỬA LỖI: Kiểm tra null để tránh lỗi MissingReferenceException
            if (nutMoBan != null)
            {
                nutMoBan.SetActive(false);
            }

            if (banPhaChePanel != null)
            {
                banPhaChePanel.SetActive(false); // Đi xa tự đóng bàn
            }
        }
    }

    // Gán hàm này vào nút "Mở Bàn"
    public void MoBanPhaChe()
    {
        if (banPhaChePanel != null) banPhaChePanel.SetActive(true);
        if (nutMoBan != null) nutMoBan.SetActive(false);
    }

    // Gán hàm này vào nút "X" (Close)
    public void DongBan()
    {
        if (banPhaChePanel != null) banPhaChePanel.SetActive(false);
        if (nutMoBan != null) nutMoBan.SetActive(true);
    }
}