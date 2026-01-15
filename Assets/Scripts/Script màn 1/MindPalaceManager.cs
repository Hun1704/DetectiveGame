using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MindPalaceManager : MonoBehaviour
{
    public static MindPalaceManager Instance;

    [Header("HIỆU ỨNG DÂY NỐI (LINE RENDERER)")]
    public LineRenderer dayNoi; // 🔥 Kéo cái ConnectionLine vừa tạo vào đây
    public Camera uiCamera;

    [Header("UI Components")]
    public CanvasGroup mindPalacePanel;
    public RectTransform mindPalaceRect;
    public Transform leftContainer;
    public Transform rightContainer;
    public GameObject textPrefab;

    [Header("Hộp Thoại Nhân Vật")]
    public GameObject dialogBoxPanel;
    public TextMeshProUGUI dialogContentText;

    [Header("Hiệu ứng & Kết quả")]
    public Image effectOverlay;
    public TextMeshProUGUI ketLuanText;

    [System.Serializable]
    public class LoiKhaiQuan
    {
        [TextArea] public string noiDung;
        public int idDoiChieu;
        [TextArea] public string cauThoaiKhiDung;
    }

    [Header("Dữ liệu")]
    public List<LoiKhaiQuan> loiKhaiCuaQuan;

    private Dictionary<int, string> vatChungDaNhat = new Dictionary<int, string>();

    [Header("Cấu hình")]
    public int tongSoVatChungCanTim = 3;

    private MindPalaceItem itemDangChonBenTrai;
    private MindPalaceItem itemDangChonBenPhai;
    private bool dangXuLyHieuUng = false;
    private Vector3 originalPos;

    [Header("Thoại khi đủ manh mối")]
    [TextArea(2, 5)] public string cauThoaiDuManhMoi;

    private bool daKichHoatMindPalace = false;

    [Header("Kết luận Chapter")]
    [TextArea(3, 6)] public string cauThoaiKetLuanChapter;
    public GameObject panelKetThucChapter;

    [Header("--- ÂM THANH KẾT THÚC CHAPTER ---")]
    [Tooltip("Kéo AudioSource phát nhạc nền của màn chơi vào đây để tắt nó")]
    public AudioSource nhacNenCanTat;

    [Tooltip("Kéo AudioSource của chính MindPalace vào đây để phát nhạc chiến thắng")]
    public AudioSource audioSourceKetThuc;

    [Tooltip("Bài nhạc hào hùng khi phá án thành công")]
    public AudioClip nhacKetThucChapter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        mindPalacePanel.alpha = 0;
        mindPalacePanel.blocksRaycasts = false;

        if (mindPalaceRect) originalPos = mindPalaceRect.anchoredPosition;
        if (effectOverlay) effectOverlay.color = new Color(0, 0, 0, 0);
        if (ketLuanText) ketLuanText.text = "";

        // Restore vật chứng từ save
        if (SaveGameManager.Instance != null && SaveGameManager.Instance.vatChungDaNhat.Count > 0)
        {
            // Chuyển từ HashSet sang List để truyền vào hàm Restore
            List<int> listId = new List<int>(SaveGameManager.Instance.vatChungDaNhat);
            RestoreVatChung(listId);

            Debug.Log("MindPalace: Đã restore từ SaveGameManager (An toàn)");
        }
    }
    private void Update()
    {
        // 🔥 LOGIC VẼ DÂY THEO CHUỘT
        VeDayNoiTheoChuot();
    }
    void VeDayNoiTheoChuot()
    {
        if (dayNoi == null) return;

        // Trường hợp 1: Đang chọn 1 bên (Trái hoặc Phải), bên kia chưa chọn
        // -> Vẽ dây từ item đang chọn đến con chuột
        if ((itemDangChonBenTrai != null && itemDangChonBenPhai == null) ||
            (itemDangChonBenPhai != null && itemDangChonBenTrai == null))
        {
            dayNoi.enabled = true; // Bật dây lên

            // Xác định điểm bắt đầu (Start Point)
            MindPalaceItem itemDangCo = (itemDangChonBenTrai != null) ? itemDangChonBenTrai : itemDangChonBenPhai;
            Vector3 startPos = itemDangCo.transform.position;
            startPos.z = 0; // Đảm bảo dây nằm phẳng

            // Xác định điểm kết thúc (End Point - Con chuột)
            Vector3 mousePos = Input.mousePosition;
            if (uiCamera != null)
            {
                mousePos = uiCamera.ScreenToWorldPoint(mousePos);
            }
            mousePos.z = 0;

            // Vẽ dây
            dayNoi.SetPosition(0, startPos);
            dayNoi.SetPosition(1, mousePos);
        }
        // Trường hợp 2: Đã chọn cả 2 (Đang chờ check kết quả)
        // -> Vẽ dây nối cứng 2 điểm lại với nhau
        else if (itemDangChonBenTrai != null && itemDangChonBenPhai != null)
        {
            dayNoi.enabled = true;
            dayNoi.SetPosition(0, itemDangChonBenTrai.transform.position);
            dayNoi.SetPosition(1, itemDangChonBenPhai.transform.position);
        }
        // Trường hợp 3: Chưa chọn gì hoặc đã reset
        else
        {
            dayNoi.enabled = false; // Tắt dây
        }
    }

    // --- 1. Nhận dữ liệu ---
    public void NhatVatChung(string suyNghi, int id)
    {
        if (!vatChungDaNhat.ContainsKey(id))
            vatChungDaNhat.Add(id, suyNghi);

        if (vatChungDaNhat.Count >= tongSoVatChungCanTim && !daKichHoatMindPalace)
        {
            daKichHoatMindPalace = true;
            // 🔥 THAY ĐỔI: Không gọi Coroutine ngay, mà gọi hàm chờ
            StartCoroutine(ChoHoiThoaiXongRoiMoiKichHoat());
        }
    }
    IEnumerator ChoHoiThoaiXongRoiMoiKichHoat()
    {
        // Chờ 1 frame để chắc chắn hội thoại kia đã kịp bật lên
        yield return null;

        // Chờ cho đến khi InventoryManager bảo là "Hết hội thoại rồi"
        if (InventoryManager.Instance != null)
        {
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // Sau khi chờ xong mới bắt đầu quy trình suy luận
        StartCoroutine(QuyTrinhMoSuyLuan());
    }

    public void RestoreVatChung(List<int> ids)
    {
        vatChungDaNhat.Clear();

        foreach (int id in ids)
        {
            string noiDungHienThi = "(Đã thu thập)";

            if (InventoryManager.Instance != null)
            {
                var data = InventoryManager.Instance.GetVatChungDataByID(id);
                if (data != null)
                {
                    // 🔥 Dòng này bây giờ sẽ hết lỗi đỏ:
                    noiDungHienThi = string.IsNullOrEmpty(data.noiDungSuyLuan) ? data.moTa : data.noiDungSuyLuan;
                }
            }

            vatChungDaNhat[id] = noiDungHienThi;
        }

        if (vatChungDaNhat.Count >= tongSoVatChungCanTim)
        {
            daKichHoatMindPalace = true;
        }
    }

    // --- Coroutine mở Mind Palace ---
    IEnumerator QuyTrinhMoSuyLuan()
    {
        InventoryManager.Instance?.ShowDialogueByID("dan_chuyen", cauThoaiDuManhMoi);
        yield return new WaitUntil(() => InventoryManager.Instance != null && !InventoryManager.Instance.dangHoiThoai);
        StartCoroutine(KichHoatCheDoSuyLuan());
    }

    IEnumerator KichHoatCheDoSuyLuan()
    {
        yield return new WaitForSeconds(0.5f);
        mindPalacePanel.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2;
            mindPalacePanel.alpha = Mathf.SmoothStep(0, 1, t);
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

        foreach (var item in vatChungDaNhat)
        {
            GameObject obj = Instantiate(textPrefab, leftContainer);
            obj.GetComponent<MindPalaceItem>()?.SetupData("- " + item.Value, item.Key, true);
        }

        if (loiKhaiCuaQuan != null)
        {
            foreach (var loiKhai in loiKhaiCuaQuan)
            {
                GameObject obj = Instantiate(textPrefab, rightContainer);
                MindPalaceItem item = obj.GetComponent<MindPalaceItem>();
                item?.SetupData("- " + loiKhai.noiDung, loiKhai.idDoiChieu, false);

                TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.color = new Color(1f, 0.92f, 0.016f, 1f);
            }
        }
    }

    // --- 2. Chọn manh mối ---
    public void ChonManhMoi(MindPalaceItem item)
    {
        if (dangXuLyHieuUng) return;

        if (item.isBenTrai)
        {
            itemDangChonBenTrai?.SetHighlight(false);
            itemDangChonBenTrai = item;
            itemDangChonBenTrai?.SetHighlight(true);
        }
        else
        {
            itemDangChonBenPhai?.SetHighlight(false);
            itemDangChonBenPhai = item;
            itemDangChonBenPhai?.SetHighlight(true);
        }

        if (itemDangChonBenTrai != null && itemDangChonBenPhai != null)
            StartCoroutine(KiemTraKetQua());
    }

    IEnumerator KiemTraKetQua()
    {
        dangXuLyHieuUng = true;
        yield return new WaitForSeconds(0.5f);

        bool dung = itemDangChonBenTrai.idSuKien == itemDangChonBenPhai.idSuKien;

        if (dung)
        {
            string cauThoai = "Mối liên kết chính xác!";
            foreach (var lk in loiKhaiCuaQuan)
            {
                if (lk.idDoiChieu == itemDangChonBenTrai.idSuKien && !string.IsNullOrEmpty(lk.cauThoaiKhiDung))
                    cauThoai = lk.cauThoaiKhiDung;
            }

            StartCoroutine(HieuUngFlashSang());
            if (ketLuanText != null) ketLuanText.text = "SUY LUẬN CHÍNH XÁC!";

            if (dialogBoxPanel && dialogContentText)
            {
                dialogContentText.text = cauThoai;
                dialogBoxPanel.SetActive(true);
            }

            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

            dialogBoxPanel?.SetActive(false);
            if (ketLuanText != null) ketLuanText.text = "";

            Destroy(itemDangChonBenTrai.gameObject);
            Destroy(itemDangChonBenPhai.gameObject);
            itemDangChonBenTrai = itemDangChonBenPhai = null;

            Invoke(nameof(KiemTraHoanThanhMan), 0.2f);
        }
        else
        {
            if (ketLuanText != null) ketLuanText.text = "Không liên quan...";
            yield return StartCoroutine(HieuUngGlitchManHinh());
            if (ketLuanText != null) ketLuanText.text = "";
            itemDangChonBenTrai?.SetHighlight(false);
            itemDangChonBenPhai?.SetHighlight(false);
            itemDangChonBenTrai = itemDangChonBenPhai = null;
        }

        dangXuLyHieuUng = false;
    }

    void KiemTraHoanThanhMan()
    {
        if (leftContainer.childCount <= 0)
            StartCoroutine(QuyTrinhKetThucChapter());
    }

    IEnumerator QuyTrinhKetThucChapter()
    {
        // 1. Chờ 1 nhịp sau khi nối đúng dây
        yield return new WaitForSeconds(0.8f);

        // 2. Đóng bảng Mind Palace lại
        yield return StartCoroutine(TatBangSuyLuan());

        // 3. Tắt nhạc nền cũ (Fade Out)
        // Lúc này không gian sẽ dần trở nên im lặng để chuẩn bị cho câu nói quan trọng
        if (nhacNenCanTat != null && nhacNenCanTat.isPlaying)
        {
            StartCoroutine(FadeOutMusic(nhacNenCanTat, 1.5f));
        }

        // 4. Nhân vật chính đưa ra kết luận (Trong sự im lặng hoặc nhạc nền đang tắt dần)
        InventoryManager.Instance.ShowDialogue(cauThoaiKetLuanChapter);

        // Chờ 1 frame để UI bật lên
        yield return null;
        // 🔥 CHỜ NGƯỜI CHƠI ĐỌC XONG CÂU THOẠI
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 5. 🔥 PHÁT NHẠC CHIẾN THẮNG (Bây giờ mới phát!) 🔥
        if (audioSourceKetThuc != null && nhacKetThucChapter != null)
        {
            audioSourceKetThuc.clip = nhacKetThucChapter;
            audioSourceKetThuc.volume = 1f;
            audioSourceKetThuc.loop = true;
            audioSourceKetThuc.Play();
        }

        // 6. Hiện bảng kết thúc Chapter ngay lập tức cùng với nhạc
        if (panelKetThucChapter != null)
            panelKetThucChapter.SetActive(true);
    }

    IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            // Giảm volume từ mức hiện tại về 0
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        audioSource.Stop(); // Tắt hẳn
        audioSource.volume = startVolume; // Trả lại volume gốc để lần sau dùng tiếp
    }

    IEnumerator TatBangSuyLuan()
    {
        yield return new WaitForSeconds(0.3f);
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime;
            mindPalacePanel.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        mindPalacePanel.blocksRaycasts = false;
        mindPalacePanel.gameObject.SetActive(false);
    }

    // --- 3. Hiệu ứng ---
    IEnumerator HieuUngGlitchManHinh()
    {
        if (effectOverlay) effectOverlay.color = new Color(1, 0, 0, 0.4f);
        float duration = 0.5f, magnitude = 15f, elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (mindPalaceRect != null)
            {
                mindPalaceRect.anchoredPosition = originalPos + new Vector3(Random.Range(-1f, 1f) * magnitude, Random.Range(-1f, 1f) * magnitude, 0);
            }
            mindPalacePanel.alpha = Random.Range(0.5f, 1f);
            magnitude = Mathf.Lerp(15f, 0f, elapsed / duration);
            yield return null;
        }

        if (mindPalaceRect != null) mindPalaceRect.anchoredPosition = originalPos;
        mindPalacePanel.alpha = 1f;
        if (effectOverlay) effectOverlay.color = new Color(0, 0, 0, 0);
    }

    IEnumerator HieuUngFlashSang()
    {
        if (effectOverlay == null) yield break;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            effectOverlay.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 0.8f), t);
            yield return null;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            effectOverlay.color = Color.Lerp(new Color(1, 1, 1, 0.8f), new Color(1, 1, 1, 0), t);
            yield return null;
        }

        effectOverlay.color = new Color(1, 1, 1, 0);
    }
}
