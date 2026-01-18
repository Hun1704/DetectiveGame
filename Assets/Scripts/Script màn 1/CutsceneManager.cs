using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("--- [QUAN TRỌNG] LƯU TRẠNG THÁI ---")]
    [Tooltip("Đặt tên duy nhất cho sự kiện này (VD: GapNgocKhue_M2). Đừng để trống!")]
    public string cutsceneID;

    [Header("--- CÀI ĐẶT ÂM THANH ---")]
    public AudioSource sfxAudioSource;

    [Header("--- LIÊN KẾT TIẾNG GIÓ ---")]
    public DoorWindSound tiengGioCua;

    [Header("--- [MỚI] UI KẾT THÚC CHAPTER (BAD END) ---")]
    [Tooltip("Kéo cái Panel Kết Thúc (Victory/Game Over) vào đây")]
    public GameObject panelKetThucChapter;

    [Header("--- [MỚI] NHÂN VẬT CHÍNH (DÙNG ĐỂ DI CHUYỂN CUỐI GAME) ---")]
    [Tooltip("Kéo nhân vật chính (người sẽ đi về phía gương) vào đây")]
    public GameObject nhanVatChinh;

    [Header("--- [MỚI] PANEL KẾT THÚC (SAU KHI HẾT THOẠI) ---")]
    [Tooltip("Kéo Panel 'Hoàn Thành Chapter' vào đây. Nó sẽ hiện ra khi chạy xong hết kịch bản.")]
    public GameObject panelHoanThanhChapter;

    // --- STRUCT LỰA CHỌN (Giữ nguyên) ---
    [System.Serializable]
    public class LuaChon
    {
        public string noiDungNut;
        [TextArea] public string mcTraLoi;
        public Sprite camXucMC;
        public AudioClip sfxMC;

        // --- 🔥 [MỚI] OPTION 1: KẾT THÚC CHAPTER LUÔN (BAD END) ---
        [Header("🔥 OPTION 1: KẾT THÚC (GƯƠNG VỠ)")]
        public bool ketThucChapterLuon = false;
        [Tooltip("Vị trí nhân vật sẽ đi tới trước khi màn hình tối")]
        public Transform viTriKetThuc;
        [Tooltip("Âm thanh gương vỡ")]
        public AudioClip sfxGuongVo;

        // --- 🔥 [MỚI] OPTION 2: CHUYỂN SCENE KHÁC ---
        [Header("🔥 OPTION 2: CHUYỂN SCENE")]
        public bool chuyenSceneKhac = false;
        public string tenSceneTiepTheo;

        // --- OPTION THƯỜNG: NPC TRẢ LỜI (Giữ nguyên) ---
        [Header("--- OPTION THƯỜNG: NPC TRẢ LỜI ---")]
        public string idNguoiDapLai;
        [TextArea] public string npcDapLai;
        public Sprite camXucNPC;
        public AudioClip sfxNPC;
    }

    [System.Serializable]
    public class TrangHoiTuong
    {
        [Tooltip("Ảnh minh họa cho đoạn hồi tưởng này")]
        public Sprite hinhAnh;

        [TextArea(3, 5)]
        [Tooltip("Dòng suy nghĩ/lời thoại đi kèm ảnh")]
        public string noiDung;
    }

    [System.Serializable]
    public class DongHoiThoai
    {
        public bool laNhanVatChinh;
        public string npcID;
        [TextArea(2, 5)] public string noiDung;
        public Sprite anhCamXuc;
        public AudioClip amThanhKem;
        public List<LuaChon> cacLuaChon;

        // 🔥 [MỚI] Tích vào đây ở dòng thoại TRƯỚC KHI Tú Bà xuất hiện
        [Header("🔥 KỊCH BẢN ĐẶC BIỆT")]
        public bool kichHoatTuBaXuatHien = false;
        public bool kichHoatTuBaNgatXiu = false;

        [Header("🔥 KỊCH BẢN NGẤT XỈU (MỚI)")]
        [Tooltip("Tích vào đây: Màn hình tối -> Nhân vật 1 chuyển sang nằm -> Màn hình sáng.")]
        public bool kichHoatNgatXiu_Lan1 = false;

        [Tooltip("Tích vào đây: Màn hình tối -> Nhân vật 2 chuyển sang nằm -> Màn hình sáng.")]
        public bool kichHoatNgatXiu_Lan2 = false;

        [Header("🔥 HỒI TƯỞNG (FLASHBACK)")]
        [Tooltip("Tích vào đây để bắt đầu chuỗi hồi tưởng")]
        public bool kichHoatHoiTuong = false;
        [Tooltip("Danh sách các trang ảnh + chữ sẽ hiện ra")]
        public List<TrangHoiTuong> danhSachHoiTuong;

        [Header("🔥 YÊU CẦU VẬT PHẨM")]
        public int idVatPhamYeuCau = 0;
        public string cauThoaiSauKhiNhan;

        [Header("🔥 CHUYỂN CẢNH (FLASHBACK)")]
        [Tooltip("Tích vào đây nếu muốn sau khi nói xong câu này thì chuyển Scene ngay.")]
        public bool chuyenCanhSauCauNay = false;
        [Tooltip("Tên Scene muốn chuyển tới (VD: Man3_QuaKhu).")]
        public string tenSceneChuyenToi;
    }

    [Header("--- [MỚI] UI HỒI TƯỞNG ---")]
    public GameObject flashbackPanel;      // Panel tổng (chứa ảnh + chữ)
    public Image flashbackImage;           // Nơi hiện ảnh
    public TMP_Text flashbackText;         // Nơi hiện chữ
    public Button flashbackNextButton;     // Nút "Tiếp tục" trên panel hồi tưởng

    [Header("UI Màn hình đen & Chữ")]
    public CanvasGroup fadePanel;
    public GameObject timeText;

    // 🔥 [MỚI] Biến riêng cho lúc chuyển cảnh đi
    [Header("--- [MỚI] PANEL CHUYỂN CẢNH (KẾT THÚC) ---")]
    [Tooltip("Kéo Panel đen dùng để che màn hình khi CHUYỂN SCENE vào đây.")]
    public CanvasGroup panelChuyenCanh;

    // 🔥 [MỚI] Màn che riêng cho đoạn Tú Bà xuất hiện (Tránh xung đột với fadePanel)
    [Header("--- [MỚI] MÀN CHE TÚ BÀ ---")]
    public CanvasGroup manCheTuBa;

    [Header("UI Lựa Chọn")]
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    [Header("Nhân vật")]
    public GameObject quan;
    public GameObject linh;
    public float tocDoDiChuyen = 3f;

    [Header("--- [MỚI] NGỌC KHUÊ NGỒI/ĐỨNG ---")]
    public GameObject ngocKhueNgoi;
    public GameObject ngocKhueDung;

    // 🔥 [MỚI] Thêm Tú Bà vào đây để ẩn hiện
    public GameObject tuBaObject;
    public GameObject tuBaNgatXiu;

    [Header("--- [MỚI] CẤU HÌNH NGƯỜI NGẤT (LẦN 1) ---")]
    public GameObject nguoi1_Dung; // Kéo nhân vật A lúc đứng
    public GameObject nguoi1_Nam;  // Kéo nhân vật A lúc nằm

    [Header("--- [MỚI] CẤU HÌNH NGƯỜI NGẤT (LẦN 2) ---")]
    public GameObject nguoi2_Dung; // Kéo nhân vật B lúc đứng
    public GameObject nguoi2_Nam;

    [Header("Điểm đến & Đi về")]
    public Transform viTriDungQuan;
    public Transform viTriDungLinh;
    public Transform loiRaQuan;
    public Transform loiRaLinh;

    [Header("KỊCH BẢN HỘI THOẠI")]
    public List<DongHoiThoai> kichBanHoiThoai;

    [Header("Kết thúc")]
    [TextArea(2, 5)] public List<string> loiTuThoaiKetThuc;
    public List<GameObject> danhSachVatChung;

    [Header("UI THÔNG BÁO GIỮA MÀN HÌNH")]
    public CanvasGroup thongBaoPanel;
    public TMP_Text noiDungThongBao;

    // --- BIẾN KIỂM SOÁT ---
    private bool daChonXong = false;
    private bool dangChoVatPham = false;
    private int idVatPhamDangCan = 0;

    // 🔥 [MỚI] Biến tạm để xử lý logic chờ nộp đồ
    private int idDoDangCan = 0;
    private bool daBamTiepTucHoiTuong = false;
    public bool IsPlaying { get; private set; }


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // =========================================================
        // 1. KIỂM TRA SAVE GAME: NẾU ĐÃ XONG THÌ TẮT HẾT VÀ THOÁT
        // =========================================================
        if (!string.IsNullOrEmpty(cutsceneID) && SaveGameManager.Instance != null)
        {
            if (SaveGameManager.Instance.CheckEvent(cutsceneID))
            {
                Debug.Log($"[CutsceneManager] Sự kiện '{cutsceneID}' đã hoàn thành. Tắt hiển thị.");

                if (GetComponent<Collider2D>() != null)
                    GetComponent<Collider2D>().enabled = false;

                if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(false);
                if (ngocKhueDung != null) ngocKhueDung.SetActive(false);
                if (quan != null) quan.SetActive(false);
                if (linh != null) linh.SetActive(false);
                if (nguoi1_Dung != null) nguoi1_Dung.SetActive(false);
                if (nguoi1_Nam != null) nguoi1_Nam.SetActive(true);

                if (nguoi2_Dung != null) nguoi2_Dung.SetActive(false);
                if (nguoi2_Nam != null) nguoi2_Nam.SetActive(true);

                // 🔥 [MỚI] Ẩn luôn Tú Bà nếu đã xong
                if (tuBaObject != null) tuBaObject.SetActive(false);

                HienTatCaVatChung();

                if (tiengGioCua != null) tiengGioCua.TatVinhVien();
                if (flashbackPanel != null) flashbackPanel.SetActive(false);

                // Gán sự kiện cho nút tiếp tục của hồi tưởng
                if (flashbackNextButton != null)
                {
                    flashbackNextButton.onClick.RemoveAllListeners();
                    flashbackNextButton.onClick.AddListener(() => daBamTiepTucHoiTuong = true);
                }
            return;
            }
        }
        // =========================================================

        if (SaveGameManager.Instance != null && SaveGameManager.Instance.daKichHoatHienVatChung)
        {
            HienTatCaVatChung();
            if (tiengGioCua != null) tiengGioCua.TatVinhVien();
        }

        if (sfxAudioSource == null) sfxAudioSource = GetComponent<AudioSource>();
    }

    public void BatDauCutscene()
    {
        StartCoroutine(QuyTrinhChuyenCanh());
    }

    IEnumerator QuyTrinhChuyenCanh()
    {
        if (InventoryManager.Instance != null)
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // --- PHẦN 1: MÀN HÌNH ĐEN ---
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.blocksRaycasts = true;
            float t = 0;
            while (t < 1) { t += Time.deltaTime; fadePanel.alpha = t; yield return null; }
            fadePanel.alpha = 1;

            if (timeText != null) timeText.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            if (timeText != null) timeText.SetActive(false);

            t = 0;
            while (t < 1) { t += Time.deltaTime; fadePanel.alpha = 1 - t; yield return null; }
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }

        // --- PHẦN 2: NHÂN VẬT XUẤT HIỆN ---
        if (quan != null) quan.SetActive(true);
        if (linh != null) linh.SetActive(true);
        if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(true);
        if (ngocKhueDung != null) ngocKhueDung.SetActive(false);

        // --- PHẦN 3: DI CHUYỂN VÀO ---
        if (quan != null && viTriDungQuan != null)
            StartCoroutine(DiChuyenNhanVat(quan, viTriDungQuan));
        if (linh != null && viTriDungLinh != null)
            yield return StartCoroutine(DiChuyenNhanVat(linh, viTriDungLinh));

        // --- 4. VÒNG LẶP HỘI THOẠI (MAIN LOOP) ---
        Debug.Log("Bắt đầu hội thoại...");
        for (int i = 0; i < kichBanHoiThoai.Count; i++)
        {
            DongHoiThoai dong = kichBanHoiThoai[i];

            // ... (Xử lý Ngất xỉu, Tú Bà... giữ nguyên) ...
            if (dong.kichHoatNgatXiu_Lan1)
            {
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 0, 1, 1f));
                else yield return new WaitForSeconds(1f);
                if (nguoi1_Dung != null) nguoi1_Dung.SetActive(false);
                if (nguoi1_Nam != null) nguoi1_Nam.SetActive(true);
                yield return new WaitForSeconds(1f);
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 1, 0, 1f));
            }
            if (dong.kichHoatNgatXiu_Lan2)
            {
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 0, 1, 1f));
                else yield return new WaitForSeconds(1f);
                if (nguoi2_Dung != null) nguoi2_Dung.SetActive(false);
                if (nguoi2_Nam != null) nguoi2_Nam.SetActive(true);
                yield return new WaitForSeconds(1f);
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 1, 0, 1f));
            }
            if (dong.kichHoatTuBaXuatHien)
            {
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 0, 1, 1f));
                else yield return new WaitForSeconds(1f);
                if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(false);
                if (ngocKhueDung != null) ngocKhueDung.SetActive(false);
                if (tuBaObject != null) tuBaObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 1, 0, 1f));
            }
            if (dong.kichHoatTuBaNgatXiu)
            {
                if (tuBaObject != null) tuBaObject.SetActive(false);
                if (tuBaNgatXiu != null) tuBaNgatXiu.SetActive(true);
            }

            // --- XỬ LÝ HỒI TƯỞNG ---
            if (dong.kichHoatHoiTuong)
            {
                if (InventoryManager.Instance != null) InventoryManager.Instance.CloseDialogue();
                yield return StartCoroutine(ChaySlideHoiTuong(dong.danhSachHoiTuong));
            }

            // --- HIỆN THOẠI VÀ CHỜ NGƯỜI CHƠI ĐỌC XONG ---
            if (!string.IsNullOrEmpty(dong.noiDung))
            {
                HienThiHoiThoai(dong);

                // 🔥 [QUAN TRỌNG] Thêm dòng này để game DỪNG LẠI chờ người chơi bấm đọc xong câu này
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }

            // --- XỬ LÝ YÊU CẦU VẬT PHẨM ---
            if (dong.idVatPhamYeuCau > 0)
            {
                dangChoVatPham = true;
                idVatPhamDangCan = dong.idVatPhamYeuCau;
                idDoDangCan = dong.idVatPhamYeuCau;
                while (dangChoVatPham) yield return null;

                if (!string.IsNullOrEmpty(dong.cauThoaiSauKhiNhan))
                {
                    if (InventoryManager.Instance != null)
                        InventoryManager.Instance.ShowDialogue(dong.cauThoaiSauKhiNhan);
                    yield return null;
                    yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
                }
            }
            else
            {
                // Nếu dòng này không có nội dung thoại nhưng cũng không phải vật phẩm, chờ 1 frame
                if (string.IsNullOrEmpty(dong.noiDung)) yield return null;
            }

            // --- XỬ LÝ LỰA CHỌN ---
            if (dong.cacLuaChon != null && dong.cacLuaChon.Count > 0)
            {
                daChonXong = false;
                StartCoroutine(HieuUngHienNut(dong.cacLuaChon));
                yield return new WaitUntil(() => daChonXong);
            }

            // --- XỬ LÝ CHUYỂN CẢNH (Option đặc biệt) ---
            if (dong.chuyenCanhSauCauNay)
            {
                if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
                {
                    SaveGameManager.Instance.CompleteEvent(cutsceneID);
                    SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
                }
                if (panelChuyenCanh != null)
                {
                    panelChuyenCanh.gameObject.SetActive(true);
                    panelChuyenCanh.blocksRaycasts = true;
                    yield return StartCoroutine(FadeCanvasGroup(panelChuyenCanh, 0, 1, 2f));
                    yield return new WaitForSeconds(1f);
                }
                if (!string.IsNullOrEmpty(dong.tenSceneChuyenToi))
                {
                    SceneManager.LoadScene(dong.tenSceneChuyenToi);
                }
                yield break;
            }
        } // --- 🔥 KẾT THÚC VÒNG LẶP HỘI THOẠI TẠI ĐÂY ---


        // ==========================================================
        // 🔥 [SỬA LỖI] ĐƯA PHẦN DI CHUYỂN RA NGOÀI VÒNG LẶP
        // Chỉ khi nói hết tất cả các câu thì mới đi ra
        // ==========================================================

        // --- TRÁO ẢNH NGỒI -> ĐỨNG (Nếu cần) ---
        if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(false);
        if (ngocKhueDung != null)
        {
            ngocKhueDung.SetActive(true);
            if (ngocKhueNgoi != null) ngocKhueDung.transform.position = ngocKhueNgoi.transform.position;
        }

        // --- PHẦN 5: DI CHUYỂN RA ---
        // Ưu tiên Ngọc Khuê đi, nếu không thì Quan đi (logic cũ của bạn)
        GameObject objToMove = (ngocKhueDung != null && ngocKhueDung.activeSelf) ? ngocKhueDung : quan;

        // Biến để theo dõi tiến trình di chuyển
        Coroutine hanhDongDiCuaQuan = null;
        Coroutine hanhDongDiCuaLinh = null;

        // 1. RA LỆNH CHO QUAN ĐI (Nhưng chưa chờ vội)
        if (objToMove != null && loiRaQuan != null)
        {
            // StartCoroutine mà KHÔNG có 'yield return' ở trước -> Chạy song song
            hanhDongDiCuaQuan = StartCoroutine(DiChuyenNhanVat(objToMove, loiRaQuan));
        }

        // 2. RA LỆNH CHO LINH ĐI NGAY LẬP TỨC (Chạy song song với Quan)
        if (linh != null && loiRaLinh != null)
        {
            hanhDongDiCuaLinh = StartCoroutine(DiChuyenNhanVat(linh, loiRaLinh));
        }

        // 3. BÂY GIỜ MỚI BẮT ĐẦU CHỜ (Đợi cả 2 đi đến nơi)

        // Chờ Quan đi xong
        if (hanhDongDiCuaQuan != null) yield return hanhDongDiCuaQuan;
        if (objToMove != null) objToMove.SetActive(false); // Quan đi xong thì ẩn

        // Chờ Linh đi xong (Nếu Quan tới trước thì chờ nốt Linh, nếu Linh tới trước rồi thì dòng này qua luôn)
        if (hanhDongDiCuaLinh != null) yield return hanhDongDiCuaLinh;
        if (linh != null) linh.SetActive(false); // Linh đi xong thì ẩn

        // --- PHẦN 6: TỰ THOẠI KẾT THÚC ---
        if (loiTuThoaiKetThuc != null && loiTuThoaiKetThuc.Count > 0)
        {
            foreach (string cauNoi in loiTuThoaiKetThuc)
            {
                InventoryManager.Instance.ShowDialogue(cauNoi);
                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
        }

        // =========================================================
        // 2. LƯU LẠI LÀ ĐÃ XONG
        // =========================================================
        if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
        {
            SaveGameManager.Instance.CompleteEvent(cutsceneID);
            SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
        }
        if (panelHoanThanhChapter != null)
        {
            // Bật Panel lên
            panelHoanThanhChapter.SetActive(true);
            Debug.Log("Đã hiện Panel kết thúc Chapter!");
        }
        else
        {
            Debug.LogWarning("Bạn chưa kéo Panel Kết Thúc vào ô 'panelHoanThanhChapter' trong Inspector!");
        }

        // --- KẾT THÚC ---
        if (tuBaObject != null) tuBaObject.SetActive(false);
        if (tiengGioCua != null) tiengGioCua.TatVinhVien();
        if (SaveGameManager.Instance != null) SaveGameManager.Instance.daKichHoatHienVatChung = true;
        HienTatCaVatChung();
    
    IEnumerator ChaySlideHoiTuong(List<TrangHoiTuong> danhSach)
        {
            if (flashbackPanel == null || danhSach == null || danhSach.Count == 0)
            {
                Debug.LogError("Thiếu UI Flashback hoặc Danh sách rỗng!");
                yield break;
            }

            // 1. Hiện Panel
            flashbackPanel.SetActive(true);
            CanvasGroup panelCG = flashbackPanel.GetComponent<CanvasGroup>();
            if (panelCG == null) panelCG = flashbackPanel.AddComponent<CanvasGroup>();

            // Fade In
            panelCG.alpha = 0;
            float t = 0;
            while (t < 1) { t += Time.deltaTime * 2f; panelCG.alpha = t; yield return null; }
            panelCG.alpha = 1;

            // 2. Duyệt từng trang
            foreach (var trang in danhSach)
            {
                daBamTiepTucHoiTuong = false;

                // Hiện ảnh (Giữ nguyên code cũ của bạn)
                if (flashbackImage != null)
                {
                    flashbackImage.color = new Color(1, 1, 1, 0);
                    flashbackImage.sprite = trang.hinhAnh;
                    // Nếu muốn giữ tỷ lệ ảnh thì để true, không thì để false tùy bạn
                    flashbackImage.preserveAspect = true;

                    float fadeImg = 0;
                    while (fadeImg < 1)
                    {
                        fadeImg += Time.deltaTime * 1.5f;
                        flashbackImage.color = new Color(1, 1, 1, fadeImg);
                        yield return null;
                    }
                }

                // Hiện chữ
                if (flashbackText != null)
                    flashbackText.text = trang.noiDung;

                // Chờ 1 frame cho ổn định
                yield return null;

                // 🔥 [SỬA LỖI NÚT KHÔNG ĂN TẠI ĐÂY]
                // Cho phép: Bấm nút (biến daBam...) HOẶC Click chuột trái (GetMouseButton) HOẶC Phím Space
                yield return new WaitUntil(() => daBamTiepTucHoiTuong || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
            }

            // 3. Fade Out
            t = 1;
            while (t > 0) { t -= Time.deltaTime * 2f; panelCG.alpha = t; yield return null; }

            flashbackPanel.SetActive(false);
        }
    }

    // --- 🔥 [MỚI] HÀM NÀY ĐỂ NPCItemReceiver GỌI KHI CÓ NGƯỜI THẢ ĐỒ VÀO ---
    public bool CheckVatPhamNopVao(int itemID)
    {
        // Nếu không đang chờ, hoặc đưa sai đồ
        if (!dangChoVatPham || itemID != idDoDangCan)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ShowTooltip("Sai vật phẩm rồi!", Input.mousePosition);
                Invoke(nameof(AnTooltip), 2f);
            }
            return false;
        }

        // NẾU ĐÚNG ĐỒ
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItemByID(itemID); // Trừ đồ

        StartCoroutine(HienThongBao("Đã đưa vật phẩm thành công!"));

        dangChoVatPham = false; // 🔥 QUAN TRỌNG: Ngắt vòng lặp while để hội thoại chạy tiếp
        idVatPhamDangCan = 0;

        return true;
    }

    // 🔥 [MỚI] Hàm hỗ trợ Fade riêng cho màn che Tú Bà
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        cg.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        cg.alpha = end;
        if (end == 0) cg.blocksRaycasts = false;
        else cg.blocksRaycasts = true;
    }

    // --- CÁC HÀM HỖ TRỢ GIỮ NGUYÊN (NopVatPham cũ có thể giữ hoặc bỏ, nhưng tôi giữ lại để không lỗi code cũ) ---
    public bool NopVatPham(int idVatPham)
    {
        // Logic cũ này có thể ít dùng nếu bạn chuyển sang dùng CheckVatPhamNopVao ở trên
        if (!dangChoVatPham || idVatPham != idVatPhamDangCan)
        {
            InventoryManager.Instance.ShowTooltip("Sai vật phẩm rồi!", Input.mousePosition);
            Invoke(nameof(AnTooltip), 2f);
            return false;
        }
        InventoryManager.Instance.RemoveItemByID(idVatPham);
        StartCoroutine(HienThongBao("Đã đưa vật phẩm thành công!"));
        dangChoVatPham = false;
        idVatPhamDangCan = 0;
        return true;
    }

    void AnTooltip() { InventoryManager.Instance.HideTooltip(); }

    IEnumerator HienThongBao(string noiDung)
    {
        if (thongBaoPanel)
        {
            noiDungThongBao.text = noiDung; thongBaoPanel.gameObject.SetActive(true); thongBaoPanel.alpha = 1;
            yield return new WaitForSeconds(2f);
            thongBaoPanel.gameObject.SetActive(false);
        }
    }

    void HienTatCaVatChung()
    {
        if (danhSachVatChung != null)
        {
            foreach (GameObject vatChung in danhSachVatChung)
                if (vatChung != null) vatChung.SetActive(true);
        }
    }

    void HienThiHoiThoai(DongHoiThoai dong)
    {
        PlaySFX(dong.amThanhKem);
        if (dong.laNhanVatChinh)
            InventoryManager.Instance.ShowDialogue(dong.noiDung, dong.anhCamXuc);
        else
            InventoryManager.Instance.ShowDialogueByID(dong.npcID, dong.noiDung, dong.anhCamXuc);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null) sfxAudioSource.PlayOneShot(clip);
    }

    void SetupLayoutButton(GameObject btn)
    {
        LayoutElement le = btn.GetComponent<LayoutElement>();
        if (le == null) le = btn.AddComponent<LayoutElement>();
        le.minHeight = 100;
        le.preferredHeight = 100;
    }

    IEnumerator HieuUngHienNut(List<LuaChon> danhSachLuaChon)
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        foreach (var luaChon in danhSachLuaChon)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = luaChon.noiDungNut;
            SetupLayoutButton(btn);
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = 0;
            StartCoroutine(FadeInButton(cg));
            btn.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(XuLyKhiChon(luaChon)));
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator FadeInButton(CanvasGroup cg)
    {
        float t = 0;
        while (t < 1) { t += Time.deltaTime * 5f; cg.alpha = t; yield return null; }
        cg.alpha = 1;
    }

    IEnumerator XuLyKhiChon(LuaChon luaChon)
    {
        // 1. Xóa các nút chọn
        if (choiceContainer != null)
        {
            foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        }

        // 2. MC thoại phản hồi
        if (!string.IsNullOrEmpty(luaChon.mcTraLoi))
        {
            PlaySFX(luaChon.sfxMC);
            InventoryManager.Instance.ShowDialogue(luaChon.mcTraLoi, luaChon.camXucMC);
            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // =========================================================
        // 🔥 TRƯỜNG HỢP 1: KẾT THÚC CHAPTER (BAD END / GƯƠNG VỠ)
        // =========================================================
        if (luaChon.ketThucChapterLuon)
        {
            Debug.Log("Đã chọn kết thúc Chapter.");

            // A. DI CHUYỂN NHÂN VẬT CHÍNH (Dùng biến nhanVatChinh mới)
            if (luaChon.viTriKetThuc != null)
            {
                // 🔥 SỬA ĐỔI: Dùng biến 'nhanVatChinh' thay vì 'quan'
                if (nhanVatChinh != null)
                {
                    yield return StartCoroutine(DiChuyenNhanVat(nhanVatChinh, luaChon.viTriKetThuc));
                }
                else
                {
                    Debug.LogError("Lỗi: Bạn chưa kéo GameObject vào biến 'Nhan Vat Chinh' trong Inspector!");
                }
            }

            // B. FADE MÀN HÌNH TỐI
            if (fadePanel != null)
            {
                fadePanel.gameObject.SetActive(true);
                fadePanel.blocksRaycasts = true;
                yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0, 1, 1.5f));
            }

            // C. ÂM THANH GƯƠNG VỠ
            if (luaChon.sfxGuongVo != null)
            {
                PlaySFX(luaChon.sfxGuongVo);
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }

            // D. HIỆN PANEL KẾT THÚC
            if (panelKetThucChapter != null)
            {
                panelKetThucChapter.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Chưa có Panel Kết Thúc, về MainMenu.");
                SceneManager.LoadScene("MainMenu");
            }

            yield break;
        }

        // =========================================================
        // 🔥 TRƯỜNG HỢP 2: CHUYỂN SCENE KHÁC (TIẾP TỤC)
        // =========================================================
        else if (luaChon.chuyenSceneKhac)
        {
            if (fadePanel != null)
            {
                fadePanel.gameObject.SetActive(true);
                fadePanel.blocksRaycasts = true;
                yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0, 1, 2f));
                yield return new WaitForSeconds(1f);
            }

            if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
            {
                SaveGameManager.Instance.CompleteEvent(cutsceneID);
                SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
            }

            if (!string.IsNullOrEmpty(luaChon.tenSceneTiepTheo))
            {
                SceneManager.LoadScene(luaChon.tenSceneTiepTheo);
            }
            yield break;
        }

        // =========================================================
        // 🔥 TRƯỜNG HỢP 3: THOẠI TIẾP
        // =========================================================
        else
        {
            string idDap = string.IsNullOrEmpty(luaChon.idNguoiDapLai) ? "quan" : luaChon.idNguoiDapLai;

            if (!string.IsNullOrEmpty(luaChon.npcDapLai))
            {
                PlaySFX(luaChon.sfxNPC);
                InventoryManager.Instance.ShowDialogueByID(idDap, luaChon.npcDapLai, luaChon.camXucNPC);
                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
            daChonXong = true;
        }
    }

    IEnumerator DiChuyenNhanVat(GameObject nhanVat, Transform diemDen)
    {
        if (nhanVat == null || diemDen == null) yield break;
        Animator anim = nhanVat.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isWalking", true);
        float currentScaleX = Mathf.Abs(nhanVat.transform.localScale.x);
        if (diemDen.position.x > nhanVat.transform.position.x)
            nhanVat.transform.localScale = new Vector3(currentScaleX, nhanVat.transform.localScale.y, nhanVat.transform.localScale.z);
        else
            nhanVat.transform.localScale = new Vector3(-currentScaleX, nhanVat.transform.localScale.y, nhanVat.transform.localScale.z);
        Vector3 targetPos = new Vector3(diemDen.position.x, diemDen.position.y, nhanVat.transform.position.z);
        while (Vector2.Distance(nhanVat.transform.position, targetPos) > 0.1f)
        {
            nhanVat.transform.position = Vector3.MoveTowards(nhanVat.transform.position, targetPos, tocDoDiChuyen * Time.deltaTime);
            yield return null;
        }
        nhanVat.transform.position = targetPos;
        if (anim != null) anim.SetBool("isWalking", false);
    }
}