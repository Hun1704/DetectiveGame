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

    // --- STRUCT LỰA CHỌN (Giữ nguyên) ---
    [System.Serializable]
    public class LuaChon
    {
        public string noiDungNut;
        [TextArea] public string mcTraLoi;
        public Sprite camXucMC;
        public AudioClip sfxMC;
        public string idNguoiDapLai;
        [TextArea] public string npcDapLai;
        public Sprite camXucNPC;
        public AudioClip sfxNPC;
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

        [Header("🔥 YÊU CẦU VẬT PHẨM")]
        public int idVatPhamYeuCau = 0;
        public string cauThoaiSauKhiNhan;

        [Header("🔥 CHUYỂN CẢNH (FLASHBACK)")]
        [Tooltip("Tích vào đây nếu muốn sau khi nói xong câu này thì chuyển Scene ngay.")]
        public bool chuyenCanhSauCauNay = false;
        [Tooltip("Tên Scene muốn chuyển tới (VD: Man3_QuaKhu).")]
        public string tenSceneChuyenToi;
    }

    [Header("UI Màn hình đen & Chữ")]
    public CanvasGroup fadePanel;
    public GameObject timeText;

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

                // 🔥 [MỚI] Ẩn luôn Tú Bà nếu đã xong
                if (tuBaObject != null) tuBaObject.SetActive(false);

                HienTatCaVatChung();

                if (tiengGioCua != null) tiengGioCua.TatVinhVien();

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

        // --- PHẦN 1: MÀN HÌNH ĐEN (Giữ nguyên code cũ của bạn) ---
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

        // --- 4. HỘI THOẠI ---
        Debug.Log("Bắt đầu hội thoại...");
        for (int i = 0; i < kichBanHoiThoai.Count; i++)
        {
            DongHoiThoai dong = kichBanHoiThoai[i];
            if (dong.kichHoatTuBaXuatHien)
            {
                // 1. Fade Tối (Dùng màn che riêng)
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 0, 1, 1f));
                else yield return new WaitForSeconds(1f);

                // 2. Tráo đổi diễn viên trong bóng tối
                if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(false);
                if (ngocKhueDung != null) ngocKhueDung.SetActive(false);

                // Hiện Tú Bà
                if (tuBaObject != null) tuBaObject.SetActive(true);

                // Chờ 1 giây cho kịch tính
                yield return new WaitForSeconds(1f);

                // 3. Fade Sáng lại
                if (manCheTuBa != null) yield return StartCoroutine(FadeCanvasGroup(manCheTuBa, 1, 0, 1f));
            }
            if (dong.kichHoatTuBaNgatXiu)
            {   
                // Ẩn bà đứng, hiện bà nằm
                if (tuBaObject != null) tuBaObject.SetActive(false);
                if (tuBaNgatXiu != null) tuBaNgatXiu.SetActive(true);

                // Rung màn hình nhẹ hoặc âm thanh "Bịch" (Tùy chọn)
                Debug.Log("Tú Bà đã ngất!");
            }
            // ==========================================================

            HienThiHoiThoai(dong);

            // Xử lý yêu cầu vật phẩm
            if (dong.idVatPhamYeuCau > 0)
            {
                dangChoVatPham = true;
                idVatPhamDangCan = dong.idVatPhamYeuCau;

                // 🔥 [MỚI] Đồng bộ ID để hàm CheckVatPhamNopVao sử dụng
                idDoDangCan = dong.idVatPhamYeuCau;
                Debug.Log("Đang dừng chờ người chơi kéo vật phẩm ID: " + idDoDangCan);

                // 🔥 VÒNG LẶP CHỜ: Tại đây game sẽ dừng lại chờ bạn kéo đồ vào NPC
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
                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }

            if (dong.cacLuaChon != null && dong.cacLuaChon.Count > 0)
            {
                daChonXong = false;
                StartCoroutine(HieuUngHienNut(dong.cacLuaChon));
                yield return new WaitUntil(() => daChonXong);
            }
            if (dong.chuyenCanhSauCauNay)
            {
                Debug.Log($"[CutsceneManager] Chuẩn bị chuyển sang scene: {dong.tenSceneChuyenToi}");

                // 1. Lưu game (Đánh dấu đã xong cutscene này để lần sau quay lại không bị xem lại)
                if (SaveGameManager.Instance != null && !string.IsNullOrEmpty(cutsceneID))
                {
                    SaveGameManager.Instance.CompleteEvent(cutsceneID);
                    SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
                }

                // 2. Fade Màn hình
                if (fadePanel != null)
                {
                    // Đảm bảo bật lên và chặn raycast
                    fadePanel.gameObject.SetActive(true);
                    fadePanel.blocksRaycasts = true;
                    yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0, 1, 2f)); // Fade trong 2 giây
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }

                // 3. Load Scene Mới
                if (!string.IsNullOrEmpty(dong.tenSceneChuyenToi))
                {
                    SceneManager.LoadScene(dong.tenSceneChuyenToi);
                }
                else
                {
                    Debug.LogError("[CutsceneManager] Bạn quên điền tên Scene chuyển tới!");
                }

                yield break; // 🔥 DỪNG LUÔN SCRIPT, KHÔNG CHẠY CÁC DÒNG BÊN DƯỚI NỮA
            }
        }

        // --- TRÁO ẢNH NGỒI -> ĐỨNG ---
        if (ngocKhueNgoi != null) ngocKhueNgoi.SetActive(false);
        if (ngocKhueDung != null)
        {
            ngocKhueDung.SetActive(true);
            if (ngocKhueNgoi != null) ngocKhueDung.transform.position = ngocKhueNgoi.transform.position;
        }

        // --- PHẦN 5: DI CHUYỂN RA ---
        GameObject objToMove = (ngocKhueDung != null && ngocKhueDung.activeSelf) ? ngocKhueDung : quan;

        if (objToMove != null && loiRaQuan != null)
        {
            yield return StartCoroutine(DiChuyenNhanVat(objToMove, loiRaQuan));
            objToMove.SetActive(false);
        }

        if (linh != null && loiRaLinh != null)
        {
            yield return StartCoroutine(DiChuyenNhanVat(linh, loiRaLinh));
            linh.SetActive(false);
        }

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
            Debug.Log($"[CHECK] Đã đánh dấu hoàn thành sự kiện: {cutsceneID}");

            // 2. 🔥 BẮT BUỘC: GHI NGAY XUỐNG Ổ CỨNG (AUTO SAVE)
            SaveGameManager.Instance.SaveGame(SaveGameManager.Instance.currentSlot);
            Debug.Log($"[CHECK] Đã Auto-Save xuống file Slot {SaveGameManager.Instance.currentSlot}");
        }
        else
        {
            Debug.LogError($"[LỖI NGHIÊM TRỌNG] CutsceneID bị TRỐNG! Game không thể lưu trạng thái này.");
        }

        // --- KẾT THÚC ---
        if (tuBaObject != null) tuBaObject.SetActive(false); // 🔥 [MỚI] Ẩn Tú Bà khi hết cutscene
        if (tiengGioCua != null) tiengGioCua.TatVinhVien();
        if (SaveGameManager.Instance != null) SaveGameManager.Instance.daKichHoatHienVatChung = true;
        HienTatCaVatChung();
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
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        PlaySFX(luaChon.sfxMC);
        InventoryManager.Instance.ShowDialogue(luaChon.mcTraLoi, luaChon.camXucMC);
        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        string idDap = string.IsNullOrEmpty(luaChon.idNguoiDapLai) ? "quan" : luaChon.idNguoiDapLai;
        PlaySFX(luaChon.sfxNPC);
        InventoryManager.Instance.ShowDialogueByID(idDap, luaChon.npcDapLai, luaChon.camXucNPC);
        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        daChonXong = true;
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