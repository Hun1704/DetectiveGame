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
    public RectTransform mindPalaceRect;
    public Transform leftContainer;
    public Transform rightContainer;
    public GameObject textPrefab;

    [Header("Hộp Thoại Nhân Vật (MỚI)")]
    public GameObject dialogBoxPanel;       // Kéo cái Panel hộp thoại vào đây
    public TextMeshProUGUI dialogContentText; // Kéo cái Text hiển thị nội dung vào đây

    [Header("Hiệu ứng & Kết quả")]
    public Image effectOverlay;
    public TextMeshProUGUI ketLuanText;

    [System.Serializable]
    public class LoiKhaiQuan
    {
        [TextArea] public string noiDung; // Nội dung hiển thị trên nút
        public int idDoiChieu;            // ID khớp nối
        [TextArea] public string cauThoaiKhiDung; // 🔥 MỚI: Câu thoại hiện ra khi nối đúng
    }

    [Header("Dữ liệu")]
    public List<LoiKhaiQuan> loiKhaiCuaQuan;

    private Dictionary<int, string> vatChungDaNhat = new Dictionary<int, string>();

    [Header("Cấu hình")]
    public int tongSoVatChungCanTim = 3;

    // Biến xử lý logic
    private MindPalaceItem itemDangChonBenTrai;
    private MindPalaceItem itemDangChonBenPhai;
    private bool dangXuLyHieuUng = false;
    private Vector3 originalPos; // Lưu vị trí gốc để rung xong trả về cũ

    [Header("Thoại khi đủ manh mối")]
    [TextArea(2, 5)]
    public string cauThoaiDuManhMoi =
    "Đã đủ các mảnh manh mối...\nĐã đến lúc ghép chúng lại để đưa ra kết luận.";

    private bool daKichHoatMindPalace = false;

    [Header("Kết luận Chapter")]
    [TextArea(3, 6)]
    public string cauThoaiKetLuanChapter =
    "Mọi manh mối đã khớp với nhau.\nSự thật dần lộ diện...\nTa đã biết hung thủ là ai.";

    public GameObject panelKetThucChapter; // UI: CHAPTER 1 END


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    private void Start()
    {
        mindPalacePanel.alpha = 0;
        mindPalacePanel.blocksRaycasts = false;

        if (mindPalaceRect) originalPos = mindPalaceRect.anchoredPosition;

        if (effectOverlay)
        {
            effectOverlay.color = new Color(0, 0, 0, 0);
            effectOverlay.raycastTarget = false;
        }

        if (ketLuanText) ketLuanText.text = "";

        // 🔥 RESTORE VẬT CHỨNG SAU KHI MỌI THỨ SẴN SÀNG
        if (SaveGameManager.Instance != null &&
            SaveGameManager.Instance.pendingVatChungRestore != null)
        {
            RestoreVatChung(
                SaveGameManager.Instance.pendingVatChungRestore
            );

            SaveGameManager.Instance.pendingVatChungRestore = null;

            Debug.Log("MindPalace: Đã restore vật chứng từ save");
        }
    }

    // --- 1. NHẬN DỮ LIỆU ---
    public void NhatVatChung(string suyNghi, int id)
    {
        if (!vatChungDaNhat.ContainsKey(id))
            vatChungDaNhat.Add(id, suyNghi);

        if (vatChungDaNhat.Count >= tongSoVatChungCanTim && !daKichHoatMindPalace)
        {
            daKichHoatMindPalace = true;
            StartCoroutine(QuyTrinhMoSuyLuan());
        }
    }

    public List<int> GetDanhSachVatChungID()
    {
        return new List<int>(vatChungDaNhat.Keys);
    }

    public void RestoreVatChung(List<int> ids)
    {
        vatChungDaNhat.Clear();

        foreach (int id in ids)
        {
            vatChungDaNhat[id] = "(Đã thu thập trước đó)";
        }

        // 🔥 QUAN TRỌNG
        if (vatChungDaNhat.Count >= tongSoVatChungCanTim)
        {
            daKichHoatMindPalace = true;
        }
    }


    IEnumerator QuyTrinhMoSuyLuan()
    {
        // 1. Người dẫn chuyện nói
        InventoryManager.Instance.ShowDialogueByID(
            "narrator",
            cauThoaiDuManhMoi
        );

        yield return null;

        // 2. Chờ người chơi click xong thoại
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 3. MỚI mở Mind Palace
        StartCoroutine(KichHoatCheDoSuyLuan());
    }


    IEnumerator KichHoatCheDoSuyLuan()
    {
        yield return new WaitForSeconds(1f);
        mindPalacePanel.gameObject.SetActive(true);

        // Hiệu ứng Fade In mượt mà
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2; // Nhân 2 để hiện nhanh hơn chút
            mindPalacePanel.alpha = Mathf.SmoothStep(0, 1, t); // SmoothStep giúp mượt hơn linear
            yield return null;
        }

        mindPalacePanel.blocksRaycasts = true;
        HienThiThongTin();
    }

    void HienThiThongTin()
    {
        if (leftContainer == null || rightContainer == null || textPrefab == null) return;

        foreach (Transform child in leftContainer) Destroy(child.gameObject);
        foreach (Transform child in rightContainer) Destroy(child.gameObject);

        // Bên Trái
        foreach (var item in vatChungDaNhat)
        {
            GameObject obj = Instantiate(textPrefab, leftContainer);
            MindPalaceItem scriptItem = obj.GetComponent<MindPalaceItem>();
            if (scriptItem) scriptItem.SetupData("- " + item.Value, item.Key, true);
        }

        // Bên Phải
        if (loiKhaiCuaQuan != null)
        {
            foreach (var loiKhai in loiKhaiCuaQuan)
            {
                GameObject obj = Instantiate(textPrefab, rightContainer);
                MindPalaceItem scriptItem = obj.GetComponent<MindPalaceItem>();
                if (scriptItem) scriptItem.SetupData("- " + loiKhai.noiDung, loiKhai.idDoiChieu, false);

                TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.color = new Color(1f, 0.92f, 0.016f, 1f); // Màu vàng chuẩn
            }
        }
    }

    // --- 2. XỬ LÝ CLICK ---
    public void ChonManhMoi(MindPalaceItem item)
    {
        if (dangXuLyHieuUng) return;

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

        if (itemDangChonBenTrai != null && itemDangChonBenPhai != null)
        {
            StartCoroutine(KiemTraKetQua());
        }
    }

    IEnumerator KiemTraKetQua()
    {
        dangXuLyHieuUng = true;
        yield return new WaitForSeconds(0.3f);

        if (itemDangChonBenTrai.idSuKien == itemDangChonBenPhai.idSuKien)
        {
            // --- ĐÚNG ---
            Debug.Log("CHÍNH XÁC!");

            // 1. Lấy nội dung thoại từ List
            string cauThoai = "Mối liên kết này chính xác!";
            foreach (var lk in loiKhaiCuaQuan)
            {
                if (lk.idDoiChieu == itemDangChonBenTrai.idSuKien)
                {
                    if (!string.IsNullOrEmpty(lk.cauThoaiKhiDung))
                        cauThoai = lk.cauThoaiKhiDung;
                    break;
                }
            }

            // 2. Chạy hiệu ứng Flash trước
            StartCoroutine(HieuUngFlashSang());
            if (ketLuanText) ketLuanText.text = "SUY LUẬN CHÍNH XÁC!";

            // 3. HIỆN HỘP THOẠI NHÂN VẬT
            if (dialogBoxPanel && dialogContentText)
            {
                dialogContentText.text = cauThoai; // Điền chữ
                dialogBoxPanel.SetActive(true);    // Bật bảng lên
            }

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            // 5. Tắt hộp thoại & Xóa vật phẩm
            if (dialogBoxPanel) dialogBoxPanel.SetActive(false);
            if (ketLuanText) ketLuanText.text = "";

            if (itemDangChonBenTrai) Destroy(itemDangChonBenTrai.gameObject);
            if (itemDangChonBenPhai) Destroy(itemDangChonBenPhai.gameObject);

            itemDangChonBenTrai = null;
            itemDangChonBenPhai = null;

            Invoke("KiemTraHoanThanhMan", 0.2f);
        }
        else
        {
            // --- SAI ---
            Debug.Log("SAI RỒI!");
            if (ketLuanText) ketLuanText.text = "Không liên quan...";

            yield return StartCoroutine(HieuUngGlitchManHinh());

            if (ketLuanText) ketLuanText.text = "";
            if (itemDangChonBenTrai) itemDangChonBenTrai.SetHighlight(false);
            if (itemDangChonBenPhai) itemDangChonBenPhai.SetHighlight(false);
            itemDangChonBenTrai = null;
            itemDangChonBenPhai = null;
        }

        dangXuLyHieuUng = false;
    }

    void KiemTraHoanThanhMan()
    {
        if (leftContainer.childCount <= 0)
        {
            StartCoroutine(QuyTrinhKetThucChapter());

        }
    }

    IEnumerator QuyTrinhKetThucChapter()
    {
        // 1. Chờ 1 nhịp cho người chơi "thở"
        yield return new WaitForSeconds(0.8f);

        // 2. Đóng Mind Palace trước
        yield return StartCoroutine(TatBangSuyLuan());

        // 3. Nhân vật chính đưa ra kết luận
        InventoryManager.Instance.ShowDialogue(cauThoaiKetLuanChapter);

        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 4. Hiện kết thúc Chapter
        if (panelKetThucChapter != null)
            panelKetThucChapter.SetActive(true);
    }


    IEnumerator TatBangSuyLuan()
    {
        yield return new WaitForSeconds(1f);
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            mindPalacePanel.alpha = t;
            yield return null;
        }
        mindPalacePanel.blocksRaycasts = false;
        mindPalacePanel.gameObject.SetActive(false);
    }

    // --- 3. HIỆU ỨNG VFX NÂNG CẤP ---

    // Hiệu ứng RUNG LẮC (Shake) kết hợp nháy đỏ
    IEnumerator HieuUngGlitchManHinh()
    {
        // Bật lớp phủ đỏ
        if (effectOverlay) effectOverlay.color = new Color(1, 0, 0, 0.4f);

        float duration = 0.5f; // Thời gian rung
        float magnitude = 15f; // Độ mạnh của rung (Pixel)

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 1. Rung vị trí ngẫu nhiên
            if (mindPalaceRect)
            {
                float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
                float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;
                mindPalaceRect.anchoredPosition = new Vector3(x, y, originalPos.z);
            }

            // 2. Nháy độ sáng Panel (Glitch)
            mindPalacePanel.alpha = Random.Range(0.5f, 1f);

            // Giảm dần độ mạnh theo thời gian (cho mượt)
            magnitude = Mathf.Lerp(15f, 0f, elapsed / duration);

            yield return null;
        }

        // Reset về trạng thái ban đầu
        if (mindPalaceRect) mindPalaceRect.anchoredPosition = originalPos;
        mindPalacePanel.alpha = 1f;
        if (effectOverlay) effectOverlay.color = new Color(0, 0, 0, 0);
    }

    // Hiệu ứng FLASH SÁNG (Mượt mà hơn dùng SmoothStep)
    IEnumerator HieuUngFlashSang()
    {
        if (effectOverlay)
        {
            // Pha 1: Sáng lên cực nhanh
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f; // Tốc độ sáng
                // Dùng màu vàng nhạt hoặc trắng tinh khôi
                effectOverlay.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.8f), t);
                yield return null;
            }

            // Pha 2: Mờ dần từ từ (Chill)
            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f; // Tốc độ tắt chậm hơn
                effectOverlay.color = Color.Lerp(new Color(1, 1, 1, 0.8f), new Color(1, 1, 1, 0), t);
                yield return null;
            }
            effectOverlay.color = new Color(1, 1, 1, 0);
        }
    }
}