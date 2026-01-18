using UnityEngine;

public class PlayerDragController : MonoBehaviour
{
    public static PlayerDragController Instance;

    [Header("Cấu hình Máu")]
    public GameObject vetMauPrefab;
    public float khoangCachRotMau = 0.5f;
    [Tooltip("Vết máu sẽ tự biến mất sau bao nhiêu giây?")]
    public float thoiGianVetMauBienMat = 5f;

    [Header("--- [MỚI] CẤU HÌNH TỐC ĐỘ ---")]
    [Tooltip("Tốc độ khi đang kéo xác (nên để thấp, VD: 2 hoặc 3).")]
    public float tocDoKhiKeo = 2f;

    // Biến lưu tốc độ gốc để trả lại sau khi thả
    private float tocDoGoc;

    // 🔥 [QUAN TRỌNG] Tham chiếu đến Script di chuyển
    // Nếu Script đi lại của bạn tên khác (VD: PlayerController), hãy sửa chữ 'PlayerMovement' bên dưới thành tên đó.
    private PlayerMovement movementScript;

    // Biến nội bộ
    [HideInInspector] public bool dangKeoXac = false;
    [HideInInspector] public GameObject xacDangKeo;
    [HideInInspector] public string idXacDangKeo;

    private bool coRotMau = false;
    private Vector3 viTriCu;
    private float demKhoangCach;

    void Awake()
    {
        Instance = this;

        // 🔥 Tự tìm script di chuyển trên người Player
        movementScript = GetComponent<PlayerMovement>();

        // Nếu không tìm thấy thì báo lỗi để bạn biết
        if (movementScript == null)
            Debug.LogError("LỖI: Không tìm thấy script 'PlayerMovement'! Hãy sửa tên trong code PlayerDragController cho đúng với script di chuyển của bạn.");
    }

    void Update()
    {
        if (dangKeoXac && coRotMau)
        {
            float dist = Vector3.Distance(transform.position, viTriCu);
            if (dist > 0.05f)
            {
                demKhoangCach += dist;
                if (demKhoangCach >= khoangCachRotMau)
                {
                    TaoVetMau();
                    demKhoangCach = 0;
                }
                viTriCu = transform.position;
            }
        }
        else
        {
            viTriCu = transform.position;
        }
    }

    public void BatDauKeo(GameObject xac, string id, bool rotMau)
    {
        dangKeoXac = true;
        xacDangKeo = xac;
        idXacDangKeo = id;
        coRotMau = rotMau;

        viTriCu = transform.position;
        demKhoangCach = 0;

        // 🔥 [MỚI] GIẢM TỐC ĐỘ
        if (movementScript != null)
        {
            // 1. Lưu lại tốc độ hiện tại (VD: 5)
            tocDoGoc = movementScript.moveSpeed;

            // 2. Gán tốc độ chậm (VD: 2)
            movementScript.moveSpeed = tocDoKhiKeo;
        }

        // GẮN XÁC VÀO PLAYER
        xac.transform.SetParent(this.transform);
        xac.transform.localPosition = new Vector3(-0.5f, -0.2f, 0);
    }

    public void ThaXac()
    {
        if (xacDangKeo != null)
        {
            xacDangKeo.transform.SetParent(null);
            xacDangKeo.SetActive(false);
        }

        // 🔥 [MỚI] TRẢ LẠI TỐC ĐỘ CŨ
        if (movementScript != null)
        {
            movementScript.moveSpeed = tocDoGoc;
        }

        dangKeoXac = false;
        xacDangKeo = null;
        idXacDangKeo = "";
        coRotMau = false;
    }

    void TaoVetMau()
    {
        if (vetMauPrefab != null)
        {
            GameObject mau = Instantiate(vetMauPrefab, transform.position, Quaternion.identity);
            Destroy(mau, thoiGianVetMauBienMat);
        }
    }
}