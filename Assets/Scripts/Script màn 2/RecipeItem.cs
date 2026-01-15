using UnityEngine;
using UnityEngine.UI; // Cần dòng này để chỉnh UI

public class RecipeZoom : MonoBehaviour
{
    [Header("Cài đặt UI")]
    public GameObject recipePanel;   // Cái khung UI đen mờ
    public Image displayImage;       // Cái ảnh để hiển thị công thức
    public Sprite recipeSprite;      // Ảnh công thức của vật phẩm này

    [Header("Cài đặt Nút Đóng")]
    public Button closeButton;       // Nút X để tắt

    void Start()
    {
        // Gán sự kiện cho nút đóng (nếu chưa gán ở inspector)
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners(); // Xóa sự kiện cũ tránh lỗi
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    void OnMouseDown()
    {
        ShowRecipe();
    }

    void ShowRecipe()
    {
        recipePanel.SetActive(true);      // Bật bảng lên
        displayImage.sprite = recipeSprite; // Đổi ảnh thành công thức này
    }

    void ClosePanel()
    {
        recipePanel.SetActive(false);     // Tắt bảng đi
    }
}