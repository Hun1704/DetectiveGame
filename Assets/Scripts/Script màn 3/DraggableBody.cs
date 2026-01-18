using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DraggableBody : MonoBehaviour
{
    [Header("--- CẤU HÌNH CƠ BẢN ---")]
    public string characterID = "NguoiCha";
    public bool coRotMau = false; // Người cha chắc không rớt máu (như bạn nói)

    [Header("--- [MỚI] YÊU CẦU VẬT PHẨM ---")]
    public bool canVatPhamDeKeo = false; // Người Mẹ = false, Người Cha = true
    public int idDayThung = 0; // Nhập ID của dây thừng trong Database
    public string cauThoaiThieuDay = "Cái xác này nặng quá, tay không thì không kéo nổi. Cần tìm dây thừng.";
    public string cauThoaiCotDayXong = "Đã cột chắc chắn. Giờ có thể kéo đi.";

    [Header("--- [MỚI] XEM CHI TIẾT ---")]
    public GameObject panelChiTiet; // Kéo cái Panel hiển thị ảnh to vào đây
    public GameObject nutXemChiTiet; // Nút bấm để mở panel

    
    [System.Serializable]
    public class LoiThoaiKeo
    {
        public string idNguoiNoi;
        [TextArea] public string noiDung;
        public Sprite bieuCam;
    }
    public List<LoiThoaiKeo> danhSachHoiThoai;

    [Header("UI Tương tác")]
    public GameObject nutKeo; // Nút Kéo (Chỉ hiện khi đã đủ điều kiện)
    public GameObject nutCotDay; // Nút Cột Dây (Hiện khi chưa cột)

    // Biến nội bộ
    private bool daBamNutKeo = false;
    private bool daCotDay = false; // Trạng thái đã cột dây chưa

    void Start()
    {
        // Ẩn tất cả nút ban đầu
        if (nutKeo != null) nutKeo.SetActive(false);
        if (nutCotDay != null) nutCotDay.SetActive(false);
        if (nutXemChiTiet != null) nutXemChiTiet.SetActive(false);
        if (panelChiTiet != null) panelChiTiet.SetActive(false);

        // Nếu không cần vật phẩm thì coi như đã cột dây rồi
        if (!canVatPhamDeKeo) daCotDay = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !daBamNutKeo)
        {
            if (PlayerDragController.Instance != null && !PlayerDragController.Instance.dangKeoXac)
            {
                HienThiNutTuongTac();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AnTatCaNut();
        }
    }

    void HienThiNutTuongTac()
    {
        // Luôn hiện nút xem chi tiết (nếu có gán)
        if (nutXemChiTiet != null) nutXemChiTiet.SetActive(true);

        if (daCotDay)
        {
            // Nếu đã cột dây (hoặc không cần dây) -> Hiện nút Kéo
            if (nutKeo != null) nutKeo.SetActive(true);
            if (nutCotDay != null) nutCotDay.SetActive(false);
        }
        else
        {
            // Nếu chưa cột dây -> Hiện nút Cột Dây
            if (nutKeo != null) nutKeo.SetActive(false);
            if (nutCotDay != null) nutCotDay.SetActive(true);
        }
    }

    void AnTatCaNut()
    {
        if (nutKeo != null) nutKeo.SetActive(false);
        if (nutCotDay != null) nutCotDay.SetActive(false);
        if (nutXemChiTiet != null) nutXemChiTiet.SetActive(false);
    }

    // --- CÁC HÀM SỰ KIỆN NÚT BẤM ---

    // 1. Gán vào nút "Xem Chi Tiết"
    public void BamNutXemChiTiet()
    {
        if (panelChiTiet != null)
        {
            panelChiTiet.SetActive(true);
            // Tạm thời tắt các nút tương tác để đỡ vướng
            AnTatCaNut();
        }
    }

    // 2. Gán vào nút "Đóng" của Panel Chi Tiết
    public void DongPanelChiTiet()
    {
        if (panelChiTiet != null) panelChiTiet.SetActive(false);
        // Hiện lại nút tương tác vì Player vẫn đang đứng đó
        HienThiNutTuongTac();
    }

    // 3. Gán vào nút "Cột Dây" (Icon dây thừng hoặc bàn tay)
    public void BamNutCotDay()
    {
        if (InventoryManager.Instance != null)
        {
            // Kiểm tra trong túi có dây thừng chưa
            if (InventoryManager.Instance.CheckItemExist(idDayThung))
            {
                // CÓ DÂY -> THÀNH CÔNG
                daCotDay = true;
                InventoryManager.Instance.ShowDialogue("Nhân vật chính", cauThoaiCotDayXong);

                // Refresh lại nút (Ẩn nút Cột, hiện nút Kéo)
                HienThiNutTuongTac();
            }
            else
            {
                // CHƯA CÓ DÂY
                InventoryManager.Instance.ShowDialogue("Nhân vật chính", cauThoaiThieuDay);
            }
        }
    }

    // 4. Gán vào nút "Kéo" (Giống code cũ)
    public void BamNutKeo()
    {
        AnTatCaNut();
        daBamNutKeo = true;

        if (InventoryManager.Instance != null) InventoryManager.Instance.cheDoVuaDiVuaThoai = true;
        if (PlayerDragController.Instance != null) PlayerDragController.Instance.BatDauKeo(this.gameObject, characterID, coRotMau);

        StartCoroutine(ChayHoiThoaiSongSong());
    }

    IEnumerator ChayHoiThoaiSongSong()
    {
        if (danhSachHoiThoai != null && danhSachHoiThoai.Count > 0)
        {
            foreach (var dong in danhSachHoiThoai)
            {
                if (!this.gameObject.activeSelf) yield break;
                if (InventoryManager.Instance != null)
                {
                    if (string.IsNullOrEmpty(dong.idNguoiNoi)) InventoryManager.Instance.ShowDialogue("Nhân vật chính", dong.noiDung);
                    else InventoryManager.Instance.ShowDialogueByID(dong.idNguoiNoi, dong.noiDung, dong.bieuCam);
                }
                yield return null;
                if (InventoryManager.Instance != null) yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
            }
        }
        if (InventoryManager.Instance != null) InventoryManager.Instance.cheDoVuaDiVuaThoai = false;
    }
}