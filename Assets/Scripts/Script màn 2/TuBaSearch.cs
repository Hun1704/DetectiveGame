using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TuBaSearch : MonoBehaviour
{
    [Header("--- CẤU HÌNH VẬT PHẨM ---")]
    [Tooltip("Nhập ID của vật phẩm trong Database (VD: 100 - Chìa Khóa).")]
    public int idVatPham = 0;

    [Header("--- CẤU HÌNH UI ---")]
    public GameObject nutLucSoat; // Nút UI hiện lên khi lại gần
    public ChapterEndPanel bangKetThuc; // Kéo bảng ChapterEndPanel vào đây

    // --- 🔥 CẤU TRÚC MỚI: HỖ TRỢ NHIỀU NHÂN VẬT NÓI ---
    [System.Serializable]
    public class LoiThoaiKet
    {
        [Header("Thông tin người nói")]
        [Tooltip("Để TRỐNG = Nhân vật chính. Nhập ID (VD: 'linh', 'ngoc_khue') = NPC nói.")]
        public string characterID;

        [TextArea(2, 5)]
        public string noiDung;

        [Tooltip("Ảnh cảm xúc (nếu có)")]
        public Sprite bieuCam;
    }

    [Header("--- KỊCH BẢN KẾT THÚC ---")]
    public List<LoiThoaiKet> danhSachLoiThoai;

    // Biến kiểm soát
    private bool daLucSoat = false;

    void Start()
    {
        if (nutLucSoat != null) nutLucSoat.SetActive(false);
        if (bangKetThuc != null) bangKetThuc.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !daLucSoat)
        {
            if (nutLucSoat != null) nutLucSoat.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (nutLucSoat != null) nutLucSoat.SetActive(false);
        }
    }

    public void BamNutLucSoat()
    {
        daLucSoat = true;
        if (nutLucSoat != null) nutLucSoat.SetActive(false);

        StartCoroutine(QuyTrinhLayDoVaKetThuc());
    }

    IEnumerator QuyTrinhLayDoVaKetThuc()
    {
        // ====================================================
        // 1. THÊM VẬT PHẨM VÀO TÚI & LƯU GAME
        // ====================================================
        if (InventoryManager.Instance != null)
        {
            // 🔥 [FIX 1] Đảm bảo hàm này đã có trong InventoryManager (Xem Bước 2 bên dưới)
            InventoryManager.Instance.AddVatChungByID(idVatPham);

            // 🔥 [FIX 2] Sửa dòng này: Gộp tên và nội dung thành 1 chuỗi
            // Thay vì: ShowDialogue("Hệ thống", "...") -> Dễ gây lỗi thiếu hàm
            InventoryManager.Instance.ShowDialogue("Bạn tìm thấy một vật phẩm quan trọng trên người Tú Bà.");

            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        if (SaveGameManager.Instance != null)
        {
            if (!SaveGameManager.Instance.vatChungDaNhat.Contains(idVatPham))
                SaveGameManager.Instance.vatChungDaNhat.Add(idVatPham);

            SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
        }

        // ====================================================
        // 2. CHẠY DANH SÁCH HỘI THOẠI
        // ====================================================
        if (danhSachLoiThoai != null && danhSachLoiThoai.Count > 0)
        {
            foreach (var line in danhSachLoiThoai)
            {
                if (InventoryManager.Instance != null)
                {
                    if (string.IsNullOrEmpty(line.characterID))
                    {
                        InventoryManager.Instance.ShowDialogue(line.noiDung, line.bieuCam);
                    }
                    else
                    {
                        InventoryManager.Instance.ShowDialogueByID(line.characterID, line.noiDung, line.bieuCam);
                    }
                }
                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
        }

        // ====================================================
        // 3. HIỆN BẢNG KẾT THÚC
        // ====================================================
        yield return new WaitForSeconds(0.5f);
        if (bangKetThuc != null) bangKetThuc.gameObject.SetActive(true);
    }
}