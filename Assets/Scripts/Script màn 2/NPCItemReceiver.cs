using UnityEngine;

public class NPCItemReceiver : MonoBehaviour
{
    [Header("Cấu hình")]
    public string tenNPC = "Quan Huyện";

    void Start()
    {
        // Đảm bảo NPC có Collider để chuột có thể nhận diện
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true; // Để chuột đi xuyên qua được
        }
    }

    // Hàm này được gọi bởi InventoryDragItem khi thả đồ vào
    public bool NhanVatPham(int itemID)
    {
        Debug.Log(tenNPC + " nhận được vật phẩm ID: " + itemID);

        // Chuyển tiếp cho CutsceneManager kiểm tra
        if (CutsceneManager.Instance != null)
        {
            return CutsceneManager.Instance.NopVatPham(itemID);
        }

        return false;
    }
}