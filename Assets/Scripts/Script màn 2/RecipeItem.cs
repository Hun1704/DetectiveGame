using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 1. Thêm thư viện này

// 2. Thêm IPointerClickHandler vào đây
public class RecipeZoom : MonoBehaviour, IPointerClickHandler
{
    [Header("Cài đặt UI")]
    public GameObject recipePanel;   
    public Image displayImage;       
    public Sprite recipeSprite;      

    [Header("Cài đặt Nút Đóng")]
    public Button closeButton;       

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        // Tự động thêm Collider nếu quên (Chỉ chạy khi là Sprite, không phải UI)
        if (GetComponent<Collider2D>() == null && GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    // 3. Thay OnMouseDown bằng hàm này
    public void OnPointerClick(PointerEventData eventData)
    {
        ShowRecipe();
    }

    void ShowRecipe()
    {
        if (recipePanel != null && displayImage != null)
        {
            recipePanel.SetActive(true);      
            displayImage.sprite = recipeSprite; 
            
            // Đưa panel lên trên cùng để không bị che
            recipePanel.transform.SetAsLastSibling(); 
        }
        else
        {
            Debug.LogError("Chưa kéo Recipe Panel hoặc Display Image vào script!");
        }
    }

    void ClosePanel()
    {
        if (recipePanel != null) recipePanel.SetActive(false);     
    }
}