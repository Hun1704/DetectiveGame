using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("--- CÀI ĐẶT ÂM THANH ---")]
    public AudioSource sfxAudioSource;

    [Header("--- LIÊN KẾT TIẾNG GIÓ ---")]
    public DoorWindSound tiengGioCua;

    // --- STRUCT LỰA CHỌN & HỘI THOẠI (GIỮ NGUYÊN) ---
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
    }

    [Header("UI Màn hình đen & Chữ (ĐỂ TRỐNG NẾU KHÔNG CẦN)")]
    public CanvasGroup fadePanel;
    public GameObject timeText;

    [Header("UI Lựa Chọn")]
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    [Header("Nhân vật (ĐỂ TRỐNG NẾU KHÔNG CẦN)")]
    public GameObject quan;
    public GameObject linh;
    public float tocDoDiChuyen = 3f;

    [Header("Điểm đến (ĐỂ TRỐNG NẾU KHÔNG CẦN)")]
    public Transform viTriDungQuan;
    public Transform viTriDungLinh;

    [Header("Vị trí Đi Ra (ĐỂ TRỐNG NẾU KHÔNG CẦN)")]
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
        // 1. Chờ sạch sẽ các hội thoại cũ
        if (InventoryManager.Instance != null)
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // --- PHẦN 1: MÀN HÌNH ĐEN (Chỉ chạy nếu có FadePanel) ---
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
        }

        // --- PHẦN 2: NHÂN VẬT XUẤT HIỆN (Chỉ chạy nếu có gán nhân vật) ---
        if (quan != null) quan.SetActive(true);
        if (linh != null) linh.SetActive(true);

        // Fade sáng lại (Chỉ chạy nếu có FadePanel)
        if (fadePanel != null)
        {
            float t = 0;
            while (t < 1) { t += Time.deltaTime; fadePanel.alpha = 1 - t; yield return null; }
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }

        // --- PHẦN 3: DI CHUYỂN VÀO (Chỉ chạy nếu có điểm đến) ---
        // Nếu không gán vị trí -> Code sẽ bỏ qua bước này
        if (quan != null && viTriDungQuan != null)
            StartCoroutine(DiChuyenNhanVat(quan, viTriDungQuan));

        if (linh != null && viTriDungLinh != null)
            yield return StartCoroutine(DiChuyenNhanVat(linh, viTriDungLinh));

        // --- 4. BẮT ĐẦU HỘI THOẠI (LUÔN CHẠY) ---
        Debug.Log("Bắt đầu hội thoại...");

        foreach (DongHoiThoai dong in kichBanHoiThoai)
        {
            HienThiHoiThoai(dong);

            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

            if (dong.cacLuaChon != null && dong.cacLuaChon.Count > 0)
            {
                daChonXong = false;
                StartCoroutine(HieuUngHienNut(dong.cacLuaChon));
                yield return new WaitUntil(() => daChonXong);
            }
        }

        Debug.Log("Hội thoại xong.");

        // --- PHẦN 5: DI CHUYỂN RA (Chỉ chạy nếu có lối ra) ---
        if (quan != null && loiRaQuan != null)
        {
            yield return StartCoroutine(DiChuyenNhanVat(quan, loiRaQuan));
            quan.SetActive(false);
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

        // --- KẾT THÚC ---
        if (tiengGioCua != null) tiengGioCua.TatVinhVien();

        if (SaveGameManager.Instance != null)
            SaveGameManager.Instance.daKichHoatHienVatChung = true;

        HienTatCaVatChung();
    }

    // --- CÁC HÀM HỖ TRỢ GIỮ NGUYÊN ---
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