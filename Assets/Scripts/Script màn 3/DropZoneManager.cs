    using UnityEngine;
    using System.Collections;

    public class DropZoneManager : MonoBehaviour
    {
        [Header("--- HIỆU ỨNG ---")]
        public CanvasGroup fadePanel; // Kéo Panel đen vào
        public SpriteRenderer backgroundRenderer; // Kéo cái ảnh nền Background vào
        public Sprite backgroundMoi; // Kéo ảnh Background MỚI vào đây

        [Header("--- [MỚI] VỊ TRÍ XÁC SAU KHI GIẤU ---")]
        [Tooltip("Kéo GameObject Nhân vật A (đã nằm sẵn ở vị trí đẹp) vào đây. Ban đầu tắt Active đi.")]
        public GameObject xacNguoiA_DaGiau;

        [Tooltip("Kéo GameObject Nhân vật B (đã nằm sẵn ở vị trí đẹp) vào đây. Ban đầu tắt Active đi.")]
        public GameObject xacNguoiB_DaGiau;

        void Start()
        {
            // Đảm bảo lúc đầu game 2 cái xác kết quả này phải ẩn đi
            if (xacNguoiA_DaGiau != null) xacNguoiA_DaGiau.SetActive(false);
            if (xacNguoiB_DaGiau != null) xacNguoiB_DaGiau.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Khi Player (đang kéo xác) đi vào vùng này
            if (other.CompareTag("Player"))
            {
                PlayerDragController player = other.GetComponent<PlayerDragController>();

                if (player != null && player.dangKeoXac)
                {
                    StartCoroutine(XuLyGiauXac(player));
                }
            }
        }

        IEnumerator XuLyGiauXac(PlayerDragController player)
        {
            string id = player.idXacDangKeo;
            Debug.Log("Đã kéo " + id + " đến nơi.");

            // 1. Fade Tối màn hình
            if (fadePanel != null) yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0, 1, 1f));
            else yield return new WaitForSeconds(0.5f);
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.CloseDialogue(); // Tắt bảng thoại ngay
            InventoryManager.Instance.cheDoVuaDiVuaThoai = false; // Trả về chế độ thường
        }

        // 2. Player thả xác đang kéo ra (Hàm ThaXac bên Player sẽ ẩn cái xác đó đi)
        player.ThaXac();

            // 3. XỬ LÝ THEO TỪNG NGƯỜI
            if (id == "NguoiA")
            {
                // Đổi Background
                if (backgroundRenderer != null && backgroundMoi != null)
                {
                    backgroundRenderer.sprite = backgroundMoi;
                }

                // 🔥 HIỆN CÁI XÁC TĨNH CỦA NGƯỜI A
                if (xacNguoiA_DaGiau != null) xacNguoiA_DaGiau.SetActive(true);
            }
            else if (id == "NguoiB")
            {
                // Không đổi background (giữ nguyên cái đã đổi)

                // 🔥 HIỆN CÁI XÁC TĨNH CỦA NGƯỜI B
                if (xacNguoiB_DaGiau != null) xacNguoiB_DaGiau.SetActive(true);
            }

            yield return new WaitForSeconds(0.5f);

            // 4. Fade Sáng lại
            if (fadePanel != null) yield return StartCoroutine(FadeCanvasGroup(fadePanel, 1, 0, 1f));

            // 5. Thoại kết thúc
            if (InventoryManager.Instance != null)
            {
                if (id == "NguoiA")
                    InventoryManager.Instance.ShowDialogue("Nhân vật chính", "Đã xong một người.");
                else
                    InventoryManager.Instance.ShowDialogue("Nhân vật chính", "Mọi việc đã hoàn tất, tiếp theo đi thay đồ rồi lên Phủ Tâm Hoa thôi.");
            }
        }

        IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
        {
            cg.gameObject.SetActive(true);
            float t = 0;
            while (t < 1) { t += Time.deltaTime / duration; cg.alpha = Mathf.Lerp(start, end, t); yield return null; }
            cg.alpha = end;
            if (end == 0) cg.blocksRaycasts = false; else cg.blocksRaycasts = true;
        }
    }