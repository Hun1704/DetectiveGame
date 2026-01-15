using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int itemID;
    [HideInInspector] public string itemName;

    private Transform parentGoc; // Vị trí ô chứa ban đầu
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Tìm Canvas tổng để khi kéo item không bị che
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentGoc = transform.parent; // Nhớ vị trí ô cũ

        // 1. Đưa item ra ngoài cùng (làm con của Canvas) để nó nổi lên trên tất cả
        transform.SetParent(canvas.transform, true);

        // 2. Cho phép tia chuột xuyên qua item để chạm vào NPC bên dưới
        canvasGroup.blocksRaycasts = false;

        // 3. 🔥 QUAN TRỌNG: Ẩn túi đồ đi để nhìn thấy màn hình game
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetPanelVisibility(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Di chuyển item theo chuột
        if (canvas != null)
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // 4. 🔥 QUAN TRỌNG: Hiện lại túi đồ ngay lập tức khi thả tay
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetPanelVisibility(true);
        }

        // 5. Kiểm tra xem có thả trúng NPC không
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        bool daDuaDuoc = false;

        if (hit.collider != null)
        {
            NPCItemReceiver npc = hit.collider.GetComponent<NPCItemReceiver>();
            if (npc != null)
            {
                // Gửi ID vật phẩm cho NPC xử lý
                daDuaDuoc = npc.NhanVatPham(itemID);
            }
        }

        // 6. Xử lý kết quả
        if (daDuaDuoc)
        {
            // Nếu NPC nhận (đưa đúng đồ) -> Hủy item này đi
            Destroy(gameObject);
        }
        else
        {
            // Nếu đưa sai hoặc thả ra ngoài không trung -> Bay về ô cũ
            transform.SetParent(parentGoc);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}