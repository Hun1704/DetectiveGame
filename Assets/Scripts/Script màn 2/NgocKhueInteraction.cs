using UnityEngine;

public class NgocKhueInteraction : MonoBehaviour
{
    [Header("Cấu hình")]
    public GameObject nutNoiChuyen;
    public CutsceneManager cutsceneGiaoThuoc;

    void Start()
    {
        if (nutNoiChuyen != null) nutNoiChuyen.SetActive(false);
    }

    // 🔥 ĐỔI TỪ Enter SANG Stay ĐỂ CẬP NHẬT LIÊN TỤC
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra điều kiện: Đã nấu xong VÀ Chưa giao thuốc
            if (SaveGameManager.Instance != null &&
                SaveGameManager.Instance.CheckEvent("da_nau_thuoc_xong") &&
                !SaveGameManager.Instance.CheckEvent("da_giao_thuoc_cho_tuba"))
            {
                if (nutNoiChuyen != null && !nutNoiChuyen.activeSelf)
                    nutNoiChuyen.SetActive(true);
            }
            else
            {
                // Nếu không thỏa mãn (hoặc đã giao rồi) thì tắt nút
                if (nutNoiChuyen != null && nutNoiChuyen.activeSelf)
                    nutNoiChuyen.SetActive(false);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (nutNoiChuyen != null) nutNoiChuyen.SetActive(false);
        }
    }

    public void BamNutNoiChuyen()
    {
        if (cutsceneGiaoThuoc != null)
        {
            if (nutNoiChuyen != null) nutNoiChuyen.SetActive(false);
            cutsceneGiaoThuoc.BatDauCutscene();
        }
    }
}