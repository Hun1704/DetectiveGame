using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MindPalaceManager : MonoBehaviour
{
    public static MindPalaceManager Instance;

    [Header("UI Components")]
    public CanvasGroup mindPalacePanel;
    public Transform leftContainer;
    public Transform rightContainer;
    public GameObject textPrefab; // LƯU Ý: Prefab này phải gắn script MindPalaceItem và có Button

    [Header("Hiệu ứng & Kết quả (MỚI)")]
    public Image effectOverlay; // Kéo một Image trắng full màn hình vào đây (để làm chớp nháy)
    public TextMeshProUGUI ketLuanText; // Kéo Text hiển thị kết quả "Chính xác" vào đây

    // --- CẤU TRÚC DỮ LIỆU MỚI ---
    [System.Serializable]
    public class LoiKhaiQuan
    {
        [TextArea] public string noiDung;
        public int idDoiChieu; // ID này phải trùng với ID vật chứng
    }

    [Header("Dữ liệu Lời Quan")]
    public List<LoiKhaiQuan> loiKhaiCuaQuan; // Nhập dữ liệu và ID vào đây

    // Dictionary để lưu ID vật chứng (Thay cho List<string> cũ)
    private Dictionary<int, string> vatChungDaNhat = new Dictionary<int, string>();

    [Header("Cấu hình")]
    public int tongSoVatChungCanTim = 3;

    // Biến xử lý logic nối
    private MindPalaceItem itemDangChonBenTrai;
    private MindPalaceItem itemDangChonBenPhai;
    private bool dangXuLyHieuUng = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mindPalacePanel.alpha = 0;
        mindPalacePanel.blocksRaycasts = false;

        if (effectOverlay)
        {
            effectOverlay.color = new Color(0, 0, 0, 0);
            effectOverlay.raycastTarget = false; // 🔥 RẤT QUAN TRỌNG
        }

        if (ketLuanText) ketLuanText.text = "";
    }


    // --- 1. NHẬN DỮ LIỆU TỪ EVIDENCE ITEM ---
    // Cập nhật thêm tham số ID
    public void NhatVatChung(string suyNghi, int id)
    {
        if (!vatChungDaNhat.ContainsKey(id))
        {
            vatChungDaNhat.Add(id, suyNghi);
        }

        if (vatChungDaNhat.Count >= tongSoVatChungCanTim)
        {
            StartCoroutine(KichHoatCheDoSuyLuan());
        }
    }

    IEnumerator KichHoatCheDoSuyLuan()
    {
        yield return new WaitForSeconds(1f);
        mindPalacePanel.gameObject.SetActive(true);

        float t = 0;
        while (t < 1) { t += Time.deltaTime; mindPalacePanel.alpha = t; yield return null; }

        mindPalacePanel.blocksRaycasts = true;
        HienThiThongTin();
    }

    void HienThiThongTin()
    {
        // 1. Kiểm tra biến môi trường trước
        if (leftContainer == null || rightContainer == null || textPrefab == null)
        {
            Debug.LogError("LỖI: Chưa kéo thả leftContainer, rightContainer hoặc textPrefab trong Inspector!");
            return;
        }

        // Xóa nội dung cũ
        foreach (Transform child in leftContainer) Destroy(child.gameObject);
        foreach (Transform child in rightContainer) Destroy(child.gameObject);

        // --- BÊN TRÁI: VẬT CHỨNG ---
        foreach (var item in vatChungDaNhat)
        {
            GameObject obj = Instantiate(textPrefab, leftContainer);

            // Kiểm tra script MindPalaceItem
            MindPalaceItem scriptItem = obj.GetComponent<MindPalaceItem>();
            if (scriptItem != null)
            {
                scriptItem.SetupData("- " + item.Value, item.Key, true);
            }
            else
            {
                Debug.LogError("LỖI: Prefab chưa gắn script 'MindPalaceItem'!");
            }
        }

        // --- BÊN PHẢI: LỜI QUAN (Khu vực dòng 104 cũ) ---
        if (loiKhaiCuaQuan == null)
        {
            Debug.LogError("LỖI: List 'loiKhaiCuaQuan' chưa được khởi tạo hoặc chưa nhập dữ liệu!");
            return;
        }

        foreach (var loiKhai in loiKhaiCuaQuan)
        {
            GameObject obj = Instantiate(textPrefab, rightContainer);

            // Xử lý an toàn cho Script MindPalaceItem
            MindPalaceItem scriptItem = obj.GetComponent<MindPalaceItem>();
            if (scriptItem != null)
            {
                scriptItem.SetupData("- " + loiKhai.noiDung, loiKhai.idDoiChieu, false);
            }

            // Xử lý an toàn cho việc đổi màu chữ
            // Dùng GetComponentInChildren để tìm Text kể cả khi nó nằm ở object con
            TextMeshProUGUI textMesh = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.color = Color.yellow;
            }
            else
            {
                Debug.LogError("LỖI: Prefab không tìm thấy component TextMeshProUGUI (ở cha hoặc con)!");
            }
        }
    }

    // --- 2. XỬ LÝ CLICK CHỌN (ĐƯỢC GỌI TỪ MINDPALACEITEM) ---
    public void ChonManhMoi(MindPalaceItem item)
    {
        if (dangXuLyHieuUng) return; // Đang chạy hiệu ứng thì khóa click

        // Logic Highlight (Chọn cái này thì bỏ chọn cái kia cùng phía)
        if (item.isBenTrai)
        {
            if (itemDangChonBenTrai != null) itemDangChonBenTrai.SetHighlight(false);
            itemDangChonBenTrai = item;
            itemDangChonBenTrai.SetHighlight(true);
        }
        else
        {
            if (itemDangChonBenPhai != null) itemDangChonBenPhai.SetHighlight(false);
            itemDangChonBenPhai = item;
            itemDangChonBenPhai.SetHighlight(true);
        }

        // Nếu đã chọn đủ 1 cặp Trái - Phải -> KIỂM TRA NGAY
        if (itemDangChonBenTrai != null && itemDangChonBenPhai != null)
        {
            StartCoroutine(KiemTraKetQua());
        }
    }

    IEnumerator KiemTraKetQua()
    {
        dangXuLyHieuUng = true;
        yield return new WaitForSeconds(0.5f); // Chờ 0.5s tạo kịch tính

        // SO SÁNH ID
        if (itemDangChonBenTrai.idSuKien == itemDangChonBenPhai.idSuKien)
        {
            // --- ĐÚNG ---
            Debug.Log("CHÍNH XÁC!");
            yield return StartCoroutine(HieuUngFlashSang());

            // Hiện kết luận thành công
            if (ketLuanText) ketLuanText.text = "SUY LUẬN CHÍNH XÁC:\nLời khai mâu thuẫn với vật chứng!";

            // Ở đây bạn có thể gọi hàm qua màn hoặc tắt MindPalace
        }
        else
        {
            // --- SAI ---
            Debug.Log("SAI RỒI!");
            yield return StartCoroutine(HieuUngGlitchManHinh());

            // Reset lựa chọn để người chơi chọn lại
            itemDangChonBenTrai.SetHighlight(false);
            itemDangChonBenPhai.SetHighlight(false);
            itemDangChonBenTrai = null;
            itemDangChonBenPhai = null;
        }

        dangXuLyHieuUng = false;
    }

    // --- 3. HIỆU ỨNG VFX ---

    // Hiệu ứng Rung lắc/Nháy tắt mở (Khi Sai)
    IEnumerator HieuUngGlitchManHinh()
    {
        if (effectOverlay) effectOverlay.color = new Color(1, 0, 0, 0.3f); // Đỏ nhẹ

        // Nháy tắt mở 3 lần
        for (int i = 0; i < 3; i++)
        {
            mindPalacePanel.alpha = 0.2f; // Tối đi
            yield return new WaitForSeconds(0.1f);
            mindPalacePanel.alpha = 1f;   // Sáng lại
            yield return new WaitForSeconds(0.1f);
        }

        if (effectOverlay) effectOverlay.color = new Color(0, 0, 0, 0); // Reset
    }

    // Hiệu ứng Chớp sáng trắng (Khi Đúng)
    IEnumerator HieuUngFlashSang()
    {
        if (effectOverlay)
        {
            effectOverlay.color = Color.white;
            // Sáng rực lên
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                effectOverlay.color = new Color(1, 1, 1, t * 3);
                yield return null;
            }
            // Mờ dần đi
            while (t > 0)
            {
                t -= Time.deltaTime;
                effectOverlay.color = new Color(1, 1, 1, t);
                yield return null;
            }
            effectOverlay.color = new Color(1, 1, 1, 0);
        }
    }
}