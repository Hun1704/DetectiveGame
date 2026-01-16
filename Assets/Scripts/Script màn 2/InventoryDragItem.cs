using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int itemID;
    [HideInInspector] public string itemName;

    private Transform parentGoc;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 🔥 [ĐÃ SỬA] Tìm Canvas gốc (Root) để đảm bảo không bị lỗi layer con
        Canvas[] allCanvases = GetComponentsInParent<Canvas>();
        if (allCanvases.Length > 0)
        {
            // Lấy cái Canvas to nhất ngoài cùng
            canvas = allCanvases[allCanvases.Length - 1];
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return; // An toàn

        parentGoc = transform.parent;
        transform.SetParent(canvas.transform, true); // Đưa ra ngoài cùng
        canvasGroup.blocksRaycasts = false;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetPanelVisibility(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetPanelVisibility(true);

        // Logic Raycast tìm NPC
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        bool daDuaDuoc = false;

        if (hit.collider != null)
        {
            NPCItemReceiver npc = hit.collider.GetComponent<NPCItemReceiver>();
            if (npc != null) daDuaDuoc = npc.NhanVatPham(itemID);
        }

        if (daDuaDuoc)
        {
            Destroy(gameObject);
        }
        else
        {
            // 🔥 [ĐÃ SỬA] Reset vị trí chuẩn xác hơn
            transform.SetParent(parentGoc);
            transform.localScale = Vector3.one; // Đảm bảo không bị méo hình
            rectTransform.anchoredPosition = Vector3.zero;
        }
    }
}