using UnityEngine;
using UnityEngine.EventSystems; // Thư viện bắt sự kiện chuột
using TMPro; // Thư viện TextMeshPro

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI btnText;

    [Header("Màu sắc")]
    public Color normalColor = new Color32(50, 50, 50, 255);   // Màu xám đen (mặc định)
    public Color hoverColor = new Color32(180, 0, 0, 255);     // Màu đỏ sậm (khi chuột vào)

    [Header("Hiệu ứng Scale (Tùy chọn)")]
    public bool enableScale = true;
    public float hoverScale = 1.1f; // Phóng to 10%

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        btnText = GetComponentInChildren<TextMeshProUGUI>();
        if (btnText) btnText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btnText) btnText.color = hoverColor;
        if (enableScale)
            transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (btnText) btnText.color = normalColor;
        if (enableScale)
            transform.localScale = originalScale;
    }
}