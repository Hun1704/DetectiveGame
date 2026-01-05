using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceItem : MonoBehaviour
{
    [Header("CHỈ CẦN NHẬP SỐ NÀY")]
    public int idVatChung = 1;

    // --- CÁC BIẾN DƯỚI ĐÂY TỰ ĐỘNG LOAD, KHÔNG CẦN NHẬP TAY NỮA ---
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

        // 1. Kiểm tra xem đã nhặt chưa (Code cũ)
        if (SaveGameManager.Instance != null &&
            SaveGameManager.Instance.vatChungDaNhat.Contains(idVatChung))
        {
            Destroy(gameObject);
            return;
        }

        // 2. 🔥 TỰ ĐỘNG LẤY DỮ LIỆU TỪ MANAGER (FIX LỖI TRÙNG LẶP)
        if (InventoryManager.Instance != null)
        {
            var data = InventoryManager.Instance.GetVatChungDataByID(idVatChung);
            if (data != null)
            {
                tenVatChung = data.ten;
                iconVatChung = data.icon;
                suyNghiCuaNhanVat = data.moTa;

                // 🔥 Dòng này bây giờ sẽ hết lỗi đỏ:
                noiDungSuyLuan = string.IsNullOrEmpty(data.noiDungSuyLuan) ? data.moTa : data.noiDungSuyLuan;
            }
        }
        else
            {
                Debug.LogError($"QUÊN NHẬP DATA CHO ID {idVatChung} TRONG INVENTORY MANAGER!");
            }
        }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Cập nhật text UI trước khi hiện
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
        SaveGameManager.Instance.vatChungDaNhat.Add(idVatChung);

        // 1. Thêm vào túi (Dùng ID thay vì truyền tay) -> Sạch sẽ hơn
        InventoryManager.Instance.AddVatChungByID(idVatChung);

        // 2. Hiện hội thoại
        InventoryManager.Instance.ShowDialogue(suyNghiCuaNhanVat);

        // 3. Gửi sang Mind Palace
        if (MindPalaceManager.Instance != null)
        {
            MindPalaceManager.Instance.NhatVatChung(noiDungSuyLuan, idVatChung);
        }

        // 4. Cutscene
        if (kichHoatCutscene && boQuanLyCutscene != null)
        {
            boQuanLyCutscene.BatDauCutscene();
        }

        AnNutUI();
        Destroy(gameObject);
    }
}