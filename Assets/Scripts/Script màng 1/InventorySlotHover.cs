using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string itemNameData; // Tên của vật phẩm này (sẽ được Manager gán vào)

    // Khi chuột di vào icon này
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Chuột đã chạm vào Icon!");
        // Gọi Manager để hiện Tooltip với tên tương ứng
        InventoryManager.Instance.ShowTooltip(itemNameData, Input.mousePosition);
    }

    // Khi chuột di ra khỏi icon này
    public void OnPointerExit(PointerEventData eventData)
    {
        // Gọi Manager để ẩn Tooltip
        InventoryManager.Instance.HideTooltip();
    }
}