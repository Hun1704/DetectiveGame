using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Thư viện UI
using System.Collections;
using System.Collections.Generic; // Thư viện List

public class CreditManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH CHỮ (BÊN TRÁI) ---")]
    public float tocDoChay = 100f;
    public float tocDoTuaNhanh = 500f;
    [Tooltip("Kéo 'CreditContent' (cái chứa chữ) vào đây")]
    public RectTransform noiDungCredit;

    [Header("--- CẤU HÌNH ẢNH (BÊN PHẢI) ---")]
    [Tooltip("Kéo cái Image bên phải (RightSide_Visuals) vào đây")]
    public Image hinhAnhHienThi;

    [Tooltip("Kéo danh sách các ảnh muốn hiển thị vào đây")]
    public List<Sprite> danhSachAnh;

    [Tooltip("Thời gian hiển thị mỗi bức ảnh (giây)")]
    public float thoiGianDoiAnh = 3f;

    [Header("--- CHUNG ---")]
    public string tenSceneMenu = "MainMenu";
    public AudioSource nhacNen;

    private float chieuCaoNoiDung;
    private bool daKetThuc = false;

    void Start()
    {
        // --- 1. SETUP CHỮ (SỬA LỖI VỊ TRÍ) ---
        if (noiDungCredit != null)
        {
            // Bắt buộc cập nhật lại kích thước Content để lấy chiều cao chính xác
            LayoutRebuilder.ForceRebuildLayoutImmediate(noiDungCredit);

            // Lấy chiều cao của nội dung chữ
            chieuCaoNoiDung = noiDungCredit.rect.height;

            // 🔥 [SỬA QUAN TRỌNG]: Lấy chiều cao của khung chứa cha (Cái Mask)
            // Thay vì dùng Screen.height (dễ bị sai do Canvas Scaler)
            RectTransform parentRect = noiDungCredit.parent.GetComponent<RectTransform>();
            float heightKhungChua = parentRect.rect.height;

            // Đặt vị trí bắt đầu:
            // Pivot Y = 1 (Top) -> Y = 0 là ở đỉnh trên cùng.
            // Muốn tụt xuống đáy thì Y = -ChiềuCaoKhungChua.
            // Trừ thêm 50 đơn vị nữa để chắc chắn nó nằm khuất hẳn bên dưới.
            noiDungCredit.anchoredPosition = new Vector2(0, -heightKhungChua - 50);
        }

        // --- 2. SETUP SLIDESHOW ẢNH (GIỮ NGUYÊN) ---
        if (hinhAnhHienThi != null && danhSachAnh != null && danhSachAnh.Count > 0)
        {
            StartCoroutine(ChaySlideshow());
        }
    }

    void Update()
    {
        if (daKetThuc || noiDungCredit == null) return;

        // SKIP (Thoát)
        if (Input.GetKeyDown(KeyCode.Escape)) { KetThucCredit(); return; }

        // DI CHUYỂN CHỮ
        float speed = (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) ? tocDoTuaNhanh : tocDoChay;

        // Cộng Y lên để chạy lên trên
        noiDungCredit.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        // KIỂM TRA KẾT THÚC
        // Khi đáy của nội dung (vị trí Y) chạy quá chiều cao của nó -> Hết phim
        if (noiDungCredit.anchoredPosition.y > chieuCaoNoiDung)
        {
            KetThucCredit();
        }
    }

    // Coroutine chạy Slide ảnh
    IEnumerator ChaySlideshow()
    {
        int index = 0;
        while (!daKetThuc)
        {
            // Gán ảnh hiện tại
            hinhAnhHienThi.sprite = danhSachAnh[index];

            // Chờ X giây
            yield return new WaitForSeconds(thoiGianDoiAnh);

            // Chuyển sang ảnh tiếp theo (nếu hết thì quay về 0)
            index++;
            if (index >= danhSachAnh.Count) index = 0;
        }
    }

    void KetThucCredit()
    {
        if (daKetThuc) return;
        daKetThuc = true;
        StartCoroutine(ChuyenVeMenu());
    }

    IEnumerator ChuyenVeMenu()
    {
        if (nhacNen != null)
        {
            while (nhacNen.volume > 0) { nhacNen.volume -= Time.deltaTime; yield return null; }
        }
        SceneManager.LoadScene(tenSceneMenu);
    }
}