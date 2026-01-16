using UnityEngine;

public class NPCItemReceiver : MonoBehaviour
{
    [Header("Cấu hình")]
    public string tenNPC = "Tú Bà";

    void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }
    }

    // Hàm này được gọi bởi InventoryDragItem khi thả đồ vào
    public bool NhanVatPham(int itemID)
    {
        Debug.Log(tenNPC + " nhận được vật phẩm ID: " + itemID);

        // Gọi sang CutsceneManager để kiểm tra
        if (CutsceneManager.Instance != null)
        {
            // Hàm CheckVatPhamNopVao sẽ trả về True nếu đúng đồ đang cần
            bool ketQua = CutsceneManager.Instance.CheckVatPhamNopVao(itemID);
            return ketQua;
        }

        return false;
    }
}