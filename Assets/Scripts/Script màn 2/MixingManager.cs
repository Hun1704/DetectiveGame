using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // BẮT BUỘC ĐỂ DÙNG TEXT MESH PRO

public class MixingManager : MonoBehaviour
{
    public static MixingManager Instance;

    [Header("--- 1. CẤU HÌNH CƠ BẢN ---")]
    public Transform diemXuatHien; // Vị trí SpawnPoint giữa tô

    [Header("--- 2. CẤU HÌNH MUỖNG ---")]
    public GameObject nutCayMuong; // Nút cây muỗng trên bàn
    public Texture2D cursorMuong;  // Ảnh con trỏ chuột hình muỗng
    private bool daCoMuong = false;

    [Header("--- 3. UI CÁI TÔ ---")]
    public Image imageCaiTo;       // Image hiển thị cái tô
    public Button nutCaiTo;        // Component Button của cái tô (Để bấm lấy thuốc)

    [Header("--- 4. ẢNH TRẠNG THÁI TÔ ---")]
    public Sprite toRong;          // Ảnh lúc đầu
    public Sprite toThanhCong;     // Ảnh pha ĐÚNG
    public Sprite toCoDoc;         // Ảnh có Thạch Tín
    public Sprite toCoRuou;        // Ảnh có Rượu
    public Sprite toSaiKhac;       // Ảnh sai

    [Header("--- 5. HỆ THỐNG HỘI THOẠI (TMP) ---")]
    public GameObject hopThoaiPanel;
    public TMP_Text tenNhanVatText;  // SỬA THÀNH TMP_Text
    public TMP_Text noiDungThoaiText;// SỬA THÀNH TMP_Text

    [Header("--- 6. HIỆU ỨNG KHÁC ---")]
    public GameObject manHinhDo;     // Panel màu đỏ cảnh báo độc

    // Dữ liệu nội bộ
    private List<string> nguyenLieuDaBo = new List<string>();
    private bool daNauXong = false;
    private int ketQuaPhaChe = 0;    // 1: Đúng, 2: Độc, 3: Rượu, 4: Sai

    void Awake()
    {
        Instance = this;
        ResetManChoi();
    }

    // --- HÀM 1: NHẶT MUỖNG ---
    public void NhatMuong()
    {
        daCoMuong = true;
        if (nutCayMuong != null) nutCayMuong.SetActive(false);
        if (cursorMuong != null) Cursor.SetCursor(cursorMuong, Vector2.zero, CursorMode.ForceSoftware);
    }

    // --- HÀM 2: THÊM NGUYÊN LIỆU ---
    public void ThemVaoTo(string id, GameObject asset)
    {
        if (!daCoMuong)
        {
            HienHopThoai("Tôi", "Tôi cần tìm một cái muỗng trước đã.");
            return;
        }
        if (daNauXong) return;

        // Sinh ra hình ảnh trong tô
        if (asset != null && diemXuatHien != null)
        {
            GameObject itemMoi = Instantiate(asset, diemXuatHien.position, Quaternion.identity, diemXuatHien);

            // [QUAN TRỌNG] Tắt Raycast của vật phẩm mới sinh ra để không che nút bấm cái tô
            Image img = itemMoi.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
        }

        nguyenLieuDaBo.Add(id);
        KiemTraLogic();
    }

    // --- HÀM 3: LOGIC KIỂM TRA ---
    void KiemTraLogic()
    {
        // 1. CÓ ĐỘC (Ưu tiên cao nhất)
        if (nguyenLieuDaBo.Contains("ThachTin")) { XuLyKetQua(2); return; }

        // 2. CÓ RƯỢU
        if (nguyenLieuDaBo.Contains("Ruou")) { XuLyKetQua(3); return; }

        // 3. PHA ĐÚNG (Nước + Mật Ong + Tâm Sen)
        if (nguyenLieuDaBo.Contains("Nuoc") && nguyenLieuDaBo.Contains("MatOng") && nguyenLieuDaBo.Contains("TamSen"))
        {
            if (nguyenLieuDaBo.Count == 3) XuLyKetQua(1); // Chuẩn 3 món
            else XuLyKetQua(4); // Thừa món khác
            return;
        }

        // 4. QUÁ SỐ LƯỢNG
        if (nguyenLieuDaBo.Count >= 3) XuLyKetQua(4);
    }

    void XuLyKetQua(int caseID)
    {
        daNauXong = true;
        ketQuaPhaChe = caseID;

        // Xóa hình ảnh nguyên liệu con cho đẹp
        foreach (Transform child in diemXuatHien) Destroy(child.gameObject);

        switch (caseID)
        {
            case 1: // ĐÚNG
                imageCaiTo.sprite = toThanhCong;
                HienHopThoai("Nhân Vật Chính", "Thuốc đã sắc xong. Mau đem cho Tú Bà.");
                // Cho phép bấm vào tô
                if (nutCaiTo != null) nutCaiTo.interactable = true;
                break;

            case 2: // ĐỘC
                imageCaiTo.sprite = toCoDoc;
                if (manHinhDo != null) manHinhDo.SetActive(true);
                HienHopThoai("Nhân Vật Chính", "Nguy hiểm! Có thạch tín! Đổ đi ngay!");
                break;

            case 3: // RƯỢU
                imageCaiTo.sprite = toCoRuou;
                HienHopThoai("Nhân Vật Chính", "Mùi rượu nồng quá. Làm lại thôi.");
                break;

            case 4: // SAI
                imageCaiTo.sprite = toSaiKhac;
                HienHopThoai("Nhân Vật Chính", "Hỗn hợp này nhìn kỳ quá...");
                break;
        }
    }

    // --- HÀM 4: LẤY THUỐC (Gán vào nút Cái Tô) ---
    // Giữ nguyên các phần trên, chỉ chú ý hàm ClickVaoCaiTo
    // --- COPY ĐÈ ĐOẠN NÀY VÀO MixingManager.cs ---

    public void ClickVaoCaiTo()
    {
        // Nếu chưa nấu xong thì bấm không có tác dụng
        if (!daNauXong) return;

        // ====================================================
        // TRƯỜNG HỢP 1: PHA CHẾ THÀNH CÔNG (Thuốc An Thần)
        // ====================================================
        if (ketQuaPhaChe == 1)
        {
            Debug.Log("MixingManager: Thu hoạch thuốc thành công!");

            if (InventoryManager.Instance != null)
            {
                // 1. Thêm thuốc vào túi đồ
                // LƯU Ý: Bạn phải chắc chắn đã thêm item ID 99 tên "ThuocAnThan" vào Database trong Inspector
                InventoryManager.Instance.AddItemByNameWithDatabaseCheck(
                    "Thuốc An Thần",
                    toThanhCong,
                    "Một bát thuốc an thần sắc đúng vị, có mùi thơm nhẹ của Tâm Sen."
                );

                // 2. Nhân vật chính tự thoại (Thông báo cho người chơi biết bước tiếp theo)
                // Hàm ShowDialogue chỉ nhận 1 tham số là nội dung (vì mặc định là Player nói)
                InventoryManager.Instance.ShowDialogue("Cuối cùng cũng xong. Phải mang ngay cho Ngọc Khuê mới được.");

                // 3. Đánh dấu sự kiện "Đã nấu xong" để Ngọc Khuê hiện nút nói chuyện
                if (SaveGameManager.Instance != null)
                {
                    SaveGameManager.Instance.CompleteEvent("da_nau_thuoc_xong");

                    // Lưu game ngay lập tức để tránh lỗi mất dữ liệu nếu tắt game
                    SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);

                    Debug.Log("MixingManager: Đã lưu sự kiện 'da_nau_thuoc_xong'");
                }
            }

            // 4. Ẩn cái tô đi (Để người chơi không bấm được nữa)
            if (imageCaiTo != null)
                imageCaiTo.gameObject.SetActive(false);

            // 5. Tắt giao diện bàn pha chế (Cho nhân vật di chuyển)
            TableTrigger table = FindFirstObjectByType<TableTrigger>();
            if (table != null)
                table.DongBan();
        }

        // ====================================================
        // TRƯỜNG HỢP 2: THẤT BẠI (Có độc, có rượu hoặc sai công thức)
        // ====================================================
        else
        {
            // Dùng hàm hiện thoại nội bộ của MixingManager (hiện ở góc màn hình pha chế)
            HienHopThoai("Nhân Vật Chính", "Thứ này hỏng rồi, đổ đi làm lại thôi.");

            // Tự động reset lại nguyên liệu sau 1 giây
            Invoke(nameof(ResetManChoi), 1.0f);
        }
    }

    // --- HÀM 5: HIỆN HỘP THOẠI ---
    void HienHopThoai(string ten, string noiDung)
    {
        if (hopThoaiPanel != null)
        {
            hopThoaiPanel.SetActive(true);
            if (tenNhanVatText != null) tenNhanVatText.text = ten;
            if (noiDungThoaiText != null) noiDungThoaiText.text = noiDung;
        }
    }

    // --- HÀM 6: RESET ---
    public void ResetManChoi()
    {
        daNauXong = false;
        ketQuaPhaChe = 0;
        nguyenLieuDaBo.Clear();

        foreach (Transform child in diemXuatHien) Destroy(child.gameObject);

        imageCaiTo.gameObject.SetActive(true);
        imageCaiTo.sprite = toRong;

        // Mặc định tô vẫn bấm được (để lỡ sai còn bấm vào reset), nhưng logic bên trong sẽ chặn
        if (nutCaiTo != null) nutCaiTo.interactable = true;

        if (manHinhDo != null) manHinhDo.SetActive(false);
        if (hopThoaiPanel != null) hopThoaiPanel.SetActive(false);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}