using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ÂM THANH ---")]
    public AudioSource sfxAudioSource;
    // --- 1. CẬP NHẬT STRUCT LỰA CHỌN ---
    [System.Serializable]
    public class LuaChon
    {
        [Tooltip("Chữ hiện trên nút (VD: Cầu xin)")]
        public string noiDungNut;

        [TextArea]
        public string mcTraLoi;
        public Sprite camXucMC; // 🔥 MỚI: Cảm xúc MC khi trả lời
        public AudioClip sfxMC; // 🔥 MỚI: Âm thanh khi MC trả lời

        [Header("NPC ĐÁP TRẢ")]
        [Tooltip("Ai là người đáp lại câu này? Nhập ID (vd: quan, linh)")]
        public string idNguoiDapLai; 

        [TextArea]
        public string npcDapLai;
        public Sprite camXucNPC; // 🔥 MỚI: Cảm xúc NPC khi đáp trả
        public AudioClip sfxNPC; // 🔥 MỚI: Âm thanh khi NPC đáp trả
    }

    // --- 2. CẬP NHẬT STRUCT HỘI THOẠI ---
    [System.Serializable]
    public class DongHoiThoai
    {
        [Tooltip("Có phải nhân vật chính nói không?")]
        public bool laNhanVatChinh; // True = Player, False = NPC

        [Tooltip("Nếu KHÔNG phải Player, hãy nhập ID nhân vật vào đây (vd: quan, linh, ba_hang_xom)")]
        public string npcID; // 🔥 MỚI: Nhập ID nhân vật phụ

        [TextArea(2, 5)] public string noiDung;
        public Sprite anhCamXuc;     // 🔥 MỚI: Kéo ảnh mặt khóc/cười/sốc vào đây
        public AudioClip amThanhKem; // 🔥 MỚI: Kéo âm thanh (hốt hoảng, tiếng động) vào đây

        [Header("TÙY CHỌN RẼ NHÁNH")]
        public List<LuaChon> cacLuaChon;
    }

    [Header("UI Màn hình đen & Chữ")]
    public CanvasGroup fadePanel;
    public GameObject timeText;

    [Header("UI Lựa Chọn")]
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    [Header("Nhân vật xuất hiện (Vẫn giữ để điều khiển đi lại)")]
    public GameObject quan;
    public GameObject linh;
    public float tocDoDiChuyen = 3f;

    [Header("Điểm đến")]
    public Transform viTriDungQuan;
    public Transform viTriDungLinh;

    [Header("Vị trí Đi Ra")]
    public Transform loiRaQuan;
    public Transform loiRaLinh;

    [Header("KỊCH BẢN HỘI THOẠI")]
    public List<DongHoiThoai> kichBanHoiThoai;

    [Header("Tự Thoại Sau Cùng")]
    [TextArea(2, 5)]
    public List<string> loiTuThoaiKetThuc;

    [Header("Vật chứng sẽ hiện ra sau khi xong")]
    public List<GameObject> danhSachVatChung;

    private bool daChonXong = false;

    void Start()
    {
        if (SaveGameManager.Instance != null && SaveGameManager.Instance.daKichHoatHienVatChung)
        {
            HienTatCaVatChung();
        }

        // Tự động tìm AudioSource nếu quên kéo
        if (sfxAudioSource == null)
            sfxAudioSource = GetComponent<AudioSource>();
    }

    public void BatDauCutscene()
    {
        StartCoroutine(QuyTrinhChuyenCanh());
    }

    IEnumerator QuyTrinhChuyenCanh()
    {
        // 1. Chờ sạch sẽ các hội thoại cũ
        if (InventoryManager.Instance != null)
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 2. Màn hình đen & 30 phút sau
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.blocksRaycasts = true;
            float t = 0;
            while (t < 1) { t += Time.deltaTime; fadePanel.alpha = t; yield return null; }
            fadePanel.alpha = 1;
        }

        if (timeText != null) timeText.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        if (timeText != null) timeText.SetActive(false);

        // 3. Quan và Lính xuất hiện
        if (quan != null) quan.SetActive(true);
        if (linh != null) linh.SetActive(true);

        // Màn hình sáng lại
        if (fadePanel != null)
        {
            float t = 0;
            while (t < 1) { t += Time.deltaTime; fadePanel.alpha = 1 - t; yield return null; }
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }

        // 4. Di chuyển vào vị trí
        StartCoroutine(DiChuyenNhanVat(quan, viTriDungQuan));
        yield return StartCoroutine(DiChuyenNhanVat(linh, viTriDungLinh));

        // --- 5. BẮT ĐẦU HỘI THOẠI (CODE ĐÃ CẬP NHẬT) ---
        Debug.Log("Bắt đầu cuộc tranh luận...");

        foreach (DongHoiThoai dong in kichBanHoiThoai)
        {
            // GỌI HÀM HIỂN THỊ MỚI
            HienThiHoiThoai(dong);

            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

            // XỬ LÝ LỰA CHỌN
            if (dong.cacLuaChon != null && dong.cacLuaChon.Count > 0)
            {
                daChonXong = false;
                StartCoroutine(HieuUngHienNut(dong.cacLuaChon));
                yield return new WaitUntil(() => daChonXong);
            }
        }

        Debug.Log("Hội thoại xong. Quan và Lính đi về...");

        // Quan đi về
        yield return StartCoroutine(DiChuyenNhanVat(quan, loiRaQuan));
        if (quan != null) quan.SetActive(false);

        // Lính đi về
        yield return StartCoroutine(DiChuyenNhanVat(linh, loiRaLinh));
        if (linh != null) linh.SetActive(false);

        // Nhân vật chính tự thoại
        Debug.Log("Bắt đầu tự thoại...");
        if (loiTuThoaiKetThuc != null && loiTuThoaiKetThuc.Count > 0)
        {
            foreach (string cauNoi in loiTuThoaiKetThuc)
            {
                // Gọi hàm Player nói
                InventoryManager.Instance.ShowDialogue(cauNoi);

                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
        }

        Debug.Log("Hội thoại kết thúc! Hiện vật chứng...");
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.daKichHoatHienVatChung = true;
        }

        HienTatCaVatChung(); // Gọi hàm bật đồ
    }
    void HienTatCaVatChung()
    {
        if (danhSachVatChung != null)
        {
            foreach (GameObject vatChung in danhSachVatChung)
            {
                if (vatChung != null)
                {
                    vatChung.SetActive(true);
                }
            }
        }
    }

    // --- HÀM HỖ TRỢ HIỂN THỊ (ĐÃ SỬA ĐỔI) ---
    void HienThiHoiThoai(DongHoiThoai dong)
    {
        // 1. Phát âm thanh (nếu có)
        PlaySFX(dong.amThanhKem);

        // 2. Hiện hội thoại kèm cảm xúc
        if (dong.laNhanVatChinh)
        {
            // Player nói (Truyền ảnh cảm xúc vào hàm ShowDialogue)
            InventoryManager.Instance.ShowDialogue(dong.noiDung, dong.anhCamXuc);
        }
        else
        {
            // NPC nói (Truyền ảnh cảm xúc Override vào hàm ShowDialogueByID)
            InventoryManager.Instance.ShowDialogueByID(dong.npcID, dong.noiDung, dong.anhCamXuc);
        }
    }

    // Hàm phụ trợ phát nhạc cho gọn code
    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    // --- HỆ THỐNG XỬ LÝ LỰA CHỌN ---

    // Hàm tự động thêm LayoutElement
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
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            cg.alpha = t;
            yield return null;
        }
        cg.alpha = 1;
    }

    IEnumerator XuLyKhiChon(LuaChon luaChon)
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);

        // 1. MC Trả lời (Kèm Cảm xúc & SFX)
        PlaySFX(luaChon.sfxMC);
        InventoryManager.Instance.ShowDialogue(luaChon.mcTraLoi, luaChon.camXucMC);

        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 2. NPC Đáp trả (Kèm Cảm xúc & SFX)
        string idDap = string.IsNullOrEmpty(luaChon.idNguoiDapLai) ? "quan" : luaChon.idNguoiDapLai;
        PlaySFX(luaChon.sfxNPC);
        InventoryManager.Instance.ShowDialogueByID(idDap, luaChon.npcDapLai, luaChon.camXucNPC);

        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        daChonXong = true;
    }

    // --- HÀM DI CHUYỂN ---
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