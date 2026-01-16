using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceItem : MonoBehaviour
{
    [Header("1. QUAN TRỌNG: ID DUY NHẤT")]
    [Tooltip("Mỗi món đồ trong CẢ GAME phải có 1 số riêng. VD: Màn 1 dùng 1-10, Màn 2 dùng 11-20")]
    public int idVatChung = 1;

    [Header("2. CẤU HÌNH")]
    public bool guiVaoMindPalace = true;

    [Header("3. UI & References")]
    public Button nutVatChung;
    public TMP_Text textHienThi;
    public CutsceneManager boQuanLyCutscene;
    public bool kichHoatCutscene = false;

    // Biến nội bộ
    private string tenVatChung;
    private string suyNghiCuaNhanVat;
    private string noiDungSuyLuan;

    void Start()
    {
        // --- SỬA ĐOẠN NÀY ---

        // 1. Thử lấy Collider ra
        Collider2D col = GetComponent<Collider2D>();

        // 2. Nếu chưa có (null) thì tự động thêm BoxCollider2D vào
        if (col == null)
        {
            Debug.LogWarning($"GameObject '{gameObject.name}' thiếu Collider2D! Đang tự động thêm vào...");
            col = gameObject.AddComponent<BoxCollider2D>();
        }

        // 3. Set là Trigger
        col.isTrigger = true;
        // --------------------

        // --- PHẦN LOGIC CŨ GIỮ NGUYÊN ---

        // BƯỚC 1: KIỂM TRA ĐÃ NHẶT CHƯA
        if (SaveGameManager.Instance != null)
        {
            if (SaveGameManager.Instance.vatChungDaNhat.Contains(idVatChung))
            {
                Destroy(gameObject);
                return;
            }
        }

        // BƯỚC 2: LOAD DỮ LIỆU TỪ INVENTORY
        if (InventoryManager.Instance != null)
        {
            var data = InventoryManager.Instance.GetVatChungDataByID(idVatChung);
            if (data != null)
            {
                tenVatChung = data.ten;
                suyNghiCuaNhanVat = data.moTa;
                noiDungSuyLuan = string.IsNullOrEmpty(data.noiDungSuyLuan) ? data.moTa : data.noiDungSuyLuan;
            }
            else
            {
                // Tắt cảnh báo lỗi đỏ nếu quên nhập data, chỉ ẩn đi thôi
                if (SaveGameManager.Instance != null && !SaveGameManager.Instance.vatChungDaNhat.Contains(idVatChung))
                {
                    // Chỉ log warning nếu chưa nhặt
                    // Debug.LogWarning($"Lỗi nhẹ: ID {idVatChung} chưa có trong Database Inventory.");
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (textHienThi != null) textHienThi.text = tenVatChung;
            HienNutUI();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) AnNutUI();
    }

    void HienNutUI()
    {
        if (nutVatChung)
        {
            nutVatChung.gameObject.SetActive(true);
            nutVatChung.onClick.RemoveAllListeners();
            nutVatChung.onClick.AddListener(XuLyKhiClick);
        }
    }

    void AnNutUI()
    {
        if (nutVatChung) nutVatChung.gameObject.SetActive(false);
    }

    void XuLyKhiClick()
    {
        Debug.Log("Đã thu thập: " + tenVatChung);

        // --- 1. LƯU VÀO SAVE GAME (QUAN TRỌNG: Check trùng) ---
        if (SaveGameManager.Instance != null)
        {
            // Chỉ thêm nếu chưa có (tránh lỗi 1 món bị add 2 lần)
            if (!SaveGameManager.Instance.vatChungDaNhat.Contains(idVatChung))
            {
                SaveGameManager.Instance.vatChungDaNhat.Add(idVatChung);
            }
        }

        // --- 2. THÊM VÀO TÚI ĐỒ ---
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddVatChungByID(idVatChung);

            // Hiện suy nghĩ nếu có
            if (!string.IsNullOrEmpty(suyNghiCuaNhanVat))
            {
                InventoryManager.Instance.ShowDialogue(suyNghiCuaNhanVat);
            }
        }

        // --- 3. GỬI VÀO MIND PALACE ---
        if (guiVaoMindPalace && MindPalaceManager.Instance != null)
        {
            MindPalaceManager.Instance.NhatVatChung(noiDungSuyLuan, idVatChung);
        }

        // --- 4. CUTSCENE ---
        if (kichHoatCutscene && boQuanLyCutscene != null)
        {
            boQuanLyCutscene.BatDauCutscene();
        }

        AnNutUI();
        Destroy(gameObject);
    }
}