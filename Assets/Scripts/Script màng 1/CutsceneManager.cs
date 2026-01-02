using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; 

public class CutsceneManager : MonoBehaviour
{
    
    [System.Serializable]
    public class LuaChon
    {
        [Tooltip("Chữ hiện trên nút (VD: Cầu xin)")]
        public string noiDungNut;

        [TextArea]
        [Tooltip("Câu MC nói lại sau khi chọn")]
        public string mcTraLoi;

        [TextArea]
        [Tooltip("Câu Quan/Lính đáp trả sau khi MC nói")]
        public string npcDapLai;
    }
    
    [System.Serializable]
    public class DongHoiThoai
    {
        public enum NguoiNoi { NhanVatChinh, Quan, Linh }
        [Tooltip("Ai là người nói câu này?")]
        public NguoiNoi nguoiNoi;

        [Tooltip("Kéo ảnh biểu cảm vào đây. Nếu để trống sẽ dùng ảnh mặc định.")]
        public Sprite anhBieuCam;

        [TextArea(2, 5)]
        [Tooltip("Nội dung câu nói (Hoặc câu hỏi dẫn dắt trươc khi hiện lựa chọn).")]
        public string noiDung;

        [Header("TÙY CHỌN RẼ NHÁNH (MỚI)")]
        [Tooltip("Nếu danh sách này > 0, game sẽ dừng lại hiện nút chọn.")]
        public List<LuaChon> cacLuaChon;
    }

    [Header("UI Màn hình đen & Chữ")]
    public CanvasGroup fadePanel;
    public GameObject timeText;

    [Header("UI Lựa Chọn (MỚI - Cần kéo vào)")]
    public Transform choiceContainer;      // Panel chứa các nút (Grid/Vertical Layout)
    public GameObject choiceButtonPrefab;  // Prefab nút bấm


    [Header("Nhân vật xuất hiện")]
    public GameObject quan;
    public GameObject linh;
    public float tocDoDiChuyen = 3f;

    [Header("Điểm đến")]
    public Transform viTriDungQuan;
    public Transform viTriDungLinh;

    [Header("Vị trí Đi Ra (MỚI - Lúc về)")]
    public Transform loiRaQuan; // Kéo vị trí Quan đi ra vào đây
    public Transform loiRaLinh; // Kéo vị trí Lính đi ra vào đây

    [Header("KỊCH BẢN HỘI THOẠI")]
    public List<DongHoiThoai> kichBanHoiThoai;

    [Header("Tự Thoại Sau Cùng (MỚI)")]
    [Tooltip("Những câu nhân vật chính tự nói một mình sau khi Quan về")]
    [TextArea(2, 5)]
    public List<string> loiTuThoaiKetThuc;

    [Header("Vật chứng sẽ hiện ra sau khi xong")]
    public List<GameObject> danhSachVatChung;

    // Biến kiểm soát việc chọn xong chưa
    private bool daChonXong = false;

    public void BatDauCutscene()
    {
        StartCoroutine(QuyTrinhChuyenCanh());
    }

    IEnumerator QuyTrinhChuyenCanh()
    {
        // 1. Chờ sạch sẽ các hội thoại cũ
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 2. Màn hình đen & 30 phút sau (Fade In)
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

        // Màn hình sáng lại (Fade Out)
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

        // --- 5. BẮT ĐẦU HỘI THOẠI ---
        Debug.Log("Bắt đầu cuộc tranh luận...");

        // Duyệt qua từng dòng hội thoại (Element 0, 1, ..., 12, 13...)
        foreach (DongHoiThoai dong in kichBanHoiThoai)
        {
            // BƯỚC A: Hiển thị câu nói chính (câu hỏi hoặc câu thoại thường)
            HienThiHoiThoai(dong);

            // Chờ người chơi đọc xong câu này (bấm click để đóng hộp thoại InventoryManager)
            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

            // BƯỚC B: Kiểm tra xem dòng này có LỰA CHỌN không? (Ví dụ Element 12)
            if (dong.cacLuaChon != null && dong.cacLuaChon.Count > 0)
            {
                // Reset cờ hiệu
                daChonXong = false;

                // Hiện các nút bấm (Dùng hàm có hiệu ứng animation cho đẹp)
                StartCoroutine(HieuUngHienNut(dong.cacLuaChon));

                // Đứng yên tại đây chờ người chơi bấm nút
                yield return new WaitUntil(() => daChonXong);

                // Sau khi daChonXong = true, vòng lặp foreach sẽ chạy tiếp sang Element tiếp theo
            }
        } // <--- ĐÓNG VÒNG LẶP FOREACH TẠI ĐÂY (SỬA LỖI 1)

        // --- ĐOẠN CODE NÀY ĐƯỢC DỜI RA NGOÀI VÒNG LẶP ---
        Debug.Log("Hội thoại xong. Quan và Lính đi về...");

        // 1. Quan đi về -> Tắt Quan
        yield return StartCoroutine(DiChuyenNhanVat(quan, loiRaQuan));
        if (quan != null) quan.SetActive(false);

        // 2. Lính đi về -> Tắt Lính
        yield return StartCoroutine(DiChuyenNhanVat(linh, loiRaLinh));
        if (linh != null) linh.SetActive(false);

        // 3. Nhân vật chính tự thoại (Monologue)
        Debug.Log("Bắt đầu tự thoại...");
        if (loiTuThoaiKetThuc != null && loiTuThoaiKetThuc.Count > 0)
        {
            foreach (string cauNoi in loiTuThoaiKetThuc)
            {
                // Gọi hàm hiển thị hội thoại (Dùng tên Nhân vật chính)
                InventoryManager.Instance.ShowDialogue(cauNoi);

                // Chờ đọc xong
                yield return null;
                yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
        }

        // ----------------------------

        Debug.Log("Hội thoại kết thúc! Hiện vật chứng...");

        // 6. Hiện vật chứng
        if (danhSachVatChung != null)
        {
            foreach (GameObject vatChung in danhSachVatChung)
            {
                if (vatChung != null) vatChung.SetActive(true);
            }
        }
    }

    // --- HÀM HỖ TRỢ HIỂN THỊ (Gọn code) ---
    void HienThiHoiThoai(DongHoiThoai dong)
    {
        switch (dong.nguoiNoi)
        {
            case DongHoiThoai.NguoiNoi.NhanVatChinh:
                InventoryManager.Instance.ShowDialogue(dong.noiDung);
                break;
            case DongHoiThoai.NguoiNoi.Quan:
                InventoryManager.Instance.ShowDialogueNPC(quan, dong.noiDung);
                break;
            case DongHoiThoai.NguoiNoi.Linh:
                InventoryManager.Instance.ShowDialogueNPC(linh, dong.noiDung);
                break;
        }
    }

    // --- HỆ THỐNG XỬ LÝ LỰA CHỌN ---

    void SpawnChoices(List<LuaChon> danhSachLuaChon)
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        foreach (var luaChon in danhSachLuaChon)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = luaChon.noiDungNut;
            // Thêm Layout Element tự động để tránh lỗi giao diện
            SetupLayoutButton(btn); 
            btn.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(XuLyKhiChon(luaChon)));
        }
    }

    // Hàm tự động thêm LayoutElement cho nút (để sửa lỗi nút bị bẹp)
    void SetupLayoutButton(GameObject btn)
    {
        LayoutElement le = btn.GetComponent<LayoutElement>();
        if (le == null) le = btn.AddComponent<LayoutElement>();
        le.minHeight = 100; // Chiều cao tối thiểu cho nút
        le.preferredHeight = 100;
    }

    IEnumerator HieuUngHienNut(List<LuaChon> danhSachLuaChon)
    {
        // Xóa nút cũ trước khi tạo
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);

        foreach (var luaChon in danhSachLuaChon)
        {
            GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = luaChon.noiDungNut;

            // Setup layout để nút không bị bẹp
            SetupLayoutButton(btn);

            // --- THÊM HIỆU ỨNG Ở ĐÂY ---
            // 1. Ban đầu cho nút tàng hình (Alpha 0)
            CanvasGroup cg = btn.GetComponent<CanvasGroup>();
            if (cg == null) cg = btn.AddComponent<CanvasGroup>();
            cg.alpha = 0;

            // 2. Cho nút bay từ dưới lên nhẹ nhàng
            StartCoroutine(FadeInButton(cg));

            // Gán sự kiện click
            btn.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(XuLyKhiChon(luaChon)));

            // Chờ 0.1s rồi mới hiện nút tiếp theo (tạo hiệu ứng bậc thang)
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator FadeInButton(CanvasGroup cg)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f; // Tốc độ hiện
            cg.alpha = t;
            yield return null;
        }
        cg.alpha = 1;
    }

    IEnumerator XuLyKhiChon(LuaChon luaChon)
    {
        // 1. Xóa nút ngay lập tức để không bấm lại
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);

        // 2. Hiện MC trả lời (Tái sử dụng hàm ShowDialogue của bạn)
        InventoryManager.Instance.ShowDialogue(luaChon.mcTraLoi);

        // Chờ người chơi đọc xong và đóng hộp thoại
        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 3. Hiện Quan/Lính đáp trả
        // Mặc định lấy Quan để đáp trả (hoặc bạn có thể thêm biến để biết ai đáp trả)
        InventoryManager.Instance.ShowDialogueNPC(quan, luaChon.npcDapLai);

        // Chờ đọc xong
        yield return null;
        yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);

        // 4. Đánh dấu đã xong để vòng lặp chính tiếp tục sang Element 13
        daChonXong = true;
    }

    // --- HÀM DI CHUYỂN (ĐÃ SỬA LỖI ĐI LÙI) ---
    IEnumerator DiChuyenNhanVat(GameObject nhanVat, Transform diemDen)
    {
        if (nhanVat == null || diemDen == null) yield break;

        Animator anim = nhanVat.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isWalking", true);

        // --- SỬA LỖI ĐI LÙI (BẮT ĐẦU) ---
        // Lấy hướng hiện tại (giá trị dương)
        float currentScaleX = Mathf.Abs(nhanVat.transform.localScale.x);

        // Kiểm tra xem đích đến ở bên Trái hay bên Phải so với nhân vật
        if (diemDen.position.x > nhanVat.transform.position.x)
        {
            // Đích đến bên PHẢI -> Scale X dương (Mặt quay phải)
            nhanVat.transform.localScale = new Vector3(currentScaleX, nhanVat.transform.localScale.y, nhanVat.transform.localScale.z);
        }
        else
        {
            // Đích đến bên TRÁI -> Scale X âm (Mặt quay trái)
            nhanVat.transform.localScale = new Vector3(-currentScaleX, nhanVat.transform.localScale.y, nhanVat.transform.localScale.z);
        }
        // --- SỬA LỖI ĐI LÙI (KẾT THÚC) ---

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