using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceItem : MonoBehaviour
{
    [Header("CHỈ CẦN NHẬP SỐ NÀY")]
    public int idVatChung = 1;

    [Header("CẤU HÌNH LOẠI VẬT PHẨM (MỚI)")]
    [Tooltip("Nếu TÍCH: Nhặt xong sẽ bay vào bảng suy luận (Dùng cho màn hiện tại).\nNếu BỎ TÍCH: Chỉ nằm trong túi đồ (Dùng cho vật phẩm màn sau).")]
    public bool guiVaoMindPalace = true; // 🔥 Mặc định là True (như cũ)

    // --- CÁC BIẾN DƯỚI ĐÂY TỰ ĐỘNG LOAD ---
    private string tenVatChung;
    private Sprite iconVatChung;
    private string suyNghiCuaNhanVat;
    private string noiDungSuyLuan;

    [Header("UI & References")]
    public Button nutVatChung;
    public TMP_Text textHienThi;

    [Header("Sự kiện đặc biệt")]
    public bool kichHoatCutscene = false;
    public CutsceneManager boQuanLyCutscene;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // 1. Kiểm tra xem đã nhặt chưa
        if (SaveGameManager.Instance != null &&
            SaveGameManager.Instance.vatChungDaNhat.Contains(idVatChung))
        {
            Destroy(gameObject);
            return;
        }

        // 2. Tự động lấy dữ liệu
        if (InventoryManager.Instance != null)
        {
            var data = InventoryManager.Instance.GetVatChungDataByID(idVatChung);
            if (data != null)
            {
                tenVatChung = data.ten;
                iconVatChung = data.icon;
                suyNghiCuaNhanVat = data.moTa;
                noiDungSuyLuan = string.IsNullOrEmpty(data.noiDungSuyLuan) ? data.moTa : data.noiDungSuyLuan;
            }
            else
            {
                Debug.LogError($"QUÊN NHẬP DATA CHO ID {idVatChung} TRONG INVENTORY MANAGER!");
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

        // Lưu vào SaveGame (Để qua màn 2 vẫn còn)
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.vatChungDaNhat.Add(idVatChung);
        }

        // 1. Thêm vào túi đồ (Inventory)
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddVatChungByID(idVatChung);

            // Hiện hội thoại suy nghĩ của nhân vật
            InventoryManager.Instance.ShowDialogue(suyNghiCuaNhanVat);
        }

        // 2. 🔥 LOGIC MỚI: Chỉ gửi vào Mind Palace nếu được phép
        if (guiVaoMindPalace)
        {
            if (MindPalaceManager.Instance != null)
            {
                MindPalaceManager.Instance.NhatVatChung(noiDungSuyLuan, idVatChung);
            }
        }
        else
        {
            Debug.Log("Vật phẩm này chỉ vào túi, KHÔNG vào Mind Palace (Dành cho màn sau).");
        }

        // 3. Cutscene (nếu có)
        if (kichHoatCutscene && boQuanLyCutscene != null)
        {
            boQuanLyCutscene.BatDauCutscene();
        }

        AnNutUI();
        Destroy(gameObject);
    }
}