using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceItem : MonoBehaviour
{
    [Header("ID Đối Chiếu (Quan trọng)")]
    [Tooltip("ID này phải trùng với ID lời khai của Quan trong MindPalaceManager")]
    public int idVatChung = 1; // Ví dụ: Cây trâm là ID 1

    public string tenVatChung = "";
    public Sprite iconVatChung;

    public Button nutVatChung;  // Kéo cái Btn_VatChung vào đây
    public TMP_Text textHienThi; // Kéo cái Text con của nút vào đây

    [Header("Hội thoại khi nhặt")]
    [TextArea(3, 10)]
    public string suyNghiCuaNhanVat = ""; // Câu nói ngay lúc nhặt (VD: "Cái gì đây?")

    [Header("Dữ liệu Suy Luận (MỚI)")]
    [Tooltip("Câu chữ sẽ hiện trong màn hình suy luận (Mind Palace). Nếu để trống sẽ dùng câu bên trên.")]
    [TextArea(3, 10)]
    public string noiDungSuyLuan = ""; // Câu chốt hạ trong đầu (VD: "Vật này không bị dính nước mưa.")

    [Header("Cài đặt Sự kiện đặc biệt (Dành cho Mẹ)")]
    public bool kichHoatCutscene = false;
    public CutsceneManager boQuanLyCutscene;

    void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HienNutUI();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AnNutUI();
        }
    }

    void HienNutUI()
    {
        textHienThi.text = tenVatChung;
        nutVatChung.gameObject.SetActive(true);
        nutVatChung.onClick.RemoveAllListeners();
        nutVatChung.onClick.AddListener(XuLyKhiClick);
    }

    void AnNutUI()
    {
        nutVatChung.gameObject.SetActive(false);
    }

    // --- HÀM XỬ LÝ CHÍNH ĐÃ CẬP NHẬT ---
    void XuLyKhiClick()
    {
        Debug.Log("Đã thu thập: " + tenVatChung);

        // 1. Thêm vào túi đồ (Giữ nguyên)
        InventoryManager.Instance.AddItem(tenVatChung, iconVatChung, suyNghiCuaNhanVat);

        // 2. Hiện hội thoại ngay lập tức (Giữ nguyên)
        InventoryManager.Instance.ShowDialogue(suyNghiCuaNhanVat);

        // 3. --- GỬI SANG MIND PALACE (CODE MỚI THÊM VÀO) ---
        if (MindPalaceManager.Instance != null)
        {
            // Kiểm tra: Nếu bạn quên nhập noiDungSuyLuan, code sẽ lấy luôn suyNghiCuaNhanVat dùng tạm
            string noiDungDeGhiNho = "";
            if (string.IsNullOrEmpty(noiDungSuyLuan))
            {
                noiDungDeGhiNho = suyNghiCuaNhanVat;
            }
            else
            {
                noiDungDeGhiNho = noiDungSuyLuan;
            }

            // Gửi dữ liệu đi
            MindPalaceManager.Instance.NhatVatChung(noiDungDeGhiNho, idVatChung);
        }
        // ---------------------------------------------------

        // 4. Kiểm tra Cutscene (Giữ nguyên)
        if (kichHoatCutscene && boQuanLyCutscene != null)
        {
            boQuanLyCutscene.BatDauCutscene();
        }

        // 5. Dọn dẹp
        AnNutUI();
        Destroy(gameObject);
    }
}