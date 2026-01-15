using UnityEngine;
using UnityEngine.EventSystems;

// Thêm IPointerClickHandler để nhận biết cú click chuột
public class InventorySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public string itemNameData;

    // 1. Xử lý di chuột vào (như cũ)
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryManager.Instance.ShowTooltip(itemNameData, Input.mousePosition);
    }

    // 2. Xử lý di chuột ra (như cũ)
    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryManager.Instance.HideTooltip();
    }

    // 3. Xử lý CLICK CHUỘT (Mới thêm)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Khi bấm vào icon, gọi Manager để xử lý việc trao đồ
        Debug.Log("Đã bấm vào vật phẩm: " + itemNameData);

        // Gọi hàm kiểm tra và trao đồ bên Manager
        InventoryManager.Instance.OnItemClicked(itemNameData);

        // Ẩn luôn tooltip cho đỡ vướng
        InventoryManager.Instance.HideTooltip();
    }
}