using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Bắt buộc để dùng tính năng di chuột

public class JarHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cài đặt Hình ảnh")]
    public Image imageHienThi; // Kéo component Image của cái nút vào đây

    public Sprite anhDongNap;  // Ảnh lúc bình thường (Đóng nắp)
    public Sprite anhMoNap;    // Ảnh khi di chuột vào (Mở nắp)

    [Header("Tùy chọn Âm thanh (Nếu thích)")]
    public AudioSource audioSource;
    public AudioClip amThanhMoNap;

    void Start()
    {
        // Đảm bảo lúc đầu game luôn là ảnh đóng nắp
        if (imageHienThi != null && anhDongNap != null)
        {
            imageHienThi.sprite = anhDongNap;
        }
    }

    // --- KHI CHUỘT DI VÀO (HOVER) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (imageHienThi != null && anhMoNap != null)
        {
            imageHienThi.sprite = anhMoNap; // Đổi sang ảnh mở nắp

            // Chơi âm thanh nhẹ (nếu có)
            if (audioSource != null && amThanhMoNap != null)
                audioSource.PlayOneShot(amThanhMoNap);
        }
    }

    // --- KHI CHUỘT DI RA (EXIT) ---
    public void OnPointerExit(PointerEventData eventData)
    {
        if (imageHienThi != null && anhDongNap != null)
        {
            imageHienThi.sprite = anhDongNap; // Trả về ảnh đóng nắp
        }
    }
}