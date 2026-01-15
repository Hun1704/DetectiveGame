using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI; 
using TMPro;

// --- 1. ĐỊNH NGHĨA DỮ LIỆU ---

// Class lưu thông tin vật phẩm (Giữ nguyên)
[System.Serializable]
public class CollectedItemData
{
    public string name;
    public Sprite icon;
    public string description;
    public CollectedItemData(string n, Sprite i, string d) { name = n; icon = i; description = d; }
}

// [MỚI] Class lưu thông tin Nhân Vật (để khai báo trong Inspector)
[System.Serializable]
public class CharacterData
{
    public string id;
    public string tenHienThi;
    public Sprite avatar;

    [Tooltip("Nhân vật này có hiển thị ảnh đại diện không?")]
    public bool coAnhDaiDien = true;
}

[System.Serializable]
public class VatChungData
{
    public int id;
    public string ten;
    public Sprite icon;
    [TextArea] public string moTa;

    [Header("Nội dung cho Mind Palace")]
    [TextArea] public string noiDungSuyLuan;
}



public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; 

    public Vector3 offset = new Vector3(15, 15, 0); 

    // --- 2. KHAI BÁO BIẾN ---

    [Header("=== DỮ LIỆU NHÂN VẬT (QUAN TRỌNG) ===")]
    // Bạn sẽ bấm dấu + ở đây để thêm Quan, Bà Hàng Xóm, v.v...
    public List<CharacterData> danhSachNhanVat;

    // Thêm hàm này vào InventoryManager
    public VatChungData GetVatChungDataByID(int id)
    {
        return databaseVatChung.Find(v => v.id == id);
    }


    [Header("Thông tin nhân vật chính (Player)")]
    public GameObject nhanVatPlayer;
    public Sprite anhThamTuMacDinh;

    [Header("Cài đặt UI Inventory")]
    public GameObject inventoryPanel; 
    public Transform contentArea;      
    public GameObject itemIconPrefab;

    [Header("Cài đặt Tooltip")]
    public GameObject tooltipPanel;   
    public TMP_Text tooltipText;      

    [Header("UI Hội thoại - PLAYER (Riêng biệt)")]
    public GameObject playerDialogueGroup; 
    public TMP_Text playerNameText;        
    public TMP_Text playerDialogueText;    
    public Image playerPortraitImage;      

    [Header("UI Hội thoại - CÁC NHÂN VẬT KHÁC (Dùng chung)")]
    // Cái này ngày xưa là của Quan, giờ ta dùng chung cho tất cả NPC
    public GameObject npcDialogueGroup;    
    public TMP_Text npcNameText;           
    public TMP_Text npcDialogueText;       
    public Image npcPortraitImage;         

    [Header("Cài đặt chữ chạy")]
    [Range(0.01f, 0.1f)] public float tocDoGo = 0.05f; 
    
    // Biến nội bộ
    private Coroutine tienTrinhGoChu; 
    private bool dangGoChu = false;   
    private string noiDungDayDu;      
    private bool dangLaNPCNoi = false; 
    public bool dangHoiThoai = false;
    public GameObject inventory;

    private List<CollectedItemData> collectedItems = new List<CollectedItemData>();

    [Header("=== DATABASE VẬT CHỨNG (SAVE / LOAD) ===")]
    public List<VatChungData> databaseVatChung;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        HideTooltip();
        AnHetBangHoiThoai();
        dangHoiThoai = false;

        // 🔥 RESTORE VẬT CHỨNG TỪ SAVE
        if (SaveGameManager.Instance != null && SaveGameManager.Instance.vatChungDaNhat.Count > 0)
        {
            foreach (int id in SaveGameManager.Instance.vatChungDaNhat)
            {
                AddVatChungByID(id);
            }
            Debug.Log("Inventory: Đã restore từ SaveGameManager (An toàn)");
        }
    }


    void Update()
    {
        if(inventoryPanel.activeSelf) transform.position = Input.mousePosition + offset;
        
    }
    public void SetPanelVisibility(bool isVisible)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isVisible);
        }
    }
    public void OnItemClicked(string itemName)
    {
        // --- PHẦN QUAN TRỌNG BẠN CẦN: TẮT TÚI ĐỒ ---
        Debug.Log("Đã chọn vật phẩm: " + itemName);

        // Kiểm tra xem biến inventoryPanel đã được gán chưa
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // Lệnh ẩn túi đồ
        }
        else
        {
            Debug.LogError("Chưa kéo thả Inventory Panel vào InventoryManager!");
        }
    }


    // --- 3. CÁC HÀM INVENTORY (Giữ nguyên) ---
    public void AddItem(string itemName, Sprite itemIcon, string itemDesc)
    {
        collectedItems.Add(new CollectedItemData(itemName, itemIcon, itemDesc));
    }
    public void AddItem(string itemName, Sprite itemIcon)
    {
        // Tự động điền mô tả mặc định
        AddItem(itemName, itemIcon, "Vật phẩm này không có mô tả.");
    }

    public void AddVatChungByID(int id)
    {
        VatChungData data = databaseVatChung.Find(x => x.id == id);
        if (data == null)
        {
            Debug.LogWarning("Không tìm thấy vật chứng ID = " + id);
            return;
        }

        // Tránh add trùng
        if (collectedItems.Exists(x => x.name == data.ten))
            return;

        collectedItems.Add(new CollectedItemData(
            data.ten,
            data.icon,
            data.moTa
        ));

        Debug.Log("Inventory: Restore vật chứng ID " + id);
    }

    public void RestoreInventoryFromSave(List<int> ids)
    {
        collectedItems.Clear();

        foreach (int id in ids)
        {
            AddVatChungByID(id);
        }

        Debug.Log("Inventory: Đã restore vật chứng từ save");
    }





    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive) { RefreshUI(); dangHoiThoai = true; }
        else 
        { 
            HideTooltip();
            if (!playerDialogueGroup.activeSelf && !npcDialogueGroup.activeSelf) dangHoiThoai = false;
        }
    }

    // Tìm đến hàm RefreshUI và sửa lại như sau:
    // --- SỬA HÀM NÀY TRONG InventoryManager.cs ---
    void RefreshUI()
    {
        // Xóa các slot cũ
        foreach (Transform child in contentArea) Destroy(child.gameObject);

        foreach (CollectedItemData data in collectedItems)
        {
            GameObject newSlot = Instantiate(itemIconPrefab, contentArea);
            newSlot.GetComponent<Image>().sprite = data.icon;

            // --- CẬP NHẬT GÁN ID CHO KÉO THẢ ---
            InventoryDragItem dragScript = newSlot.GetComponent<InventoryDragItem>();
            if (dragScript == null) dragScript = newSlot.AddComponent<InventoryDragItem>();

            // 1. Tìm ID trong Database dựa theo tên vật phẩm
            VatChungData dbData = databaseVatChung.Find(x => x.ten == data.name);

            if (dbData != null)
            {
                dragScript.itemID = dbData.id; // Gán ID chuẩn từ database
                dragScript.itemName = data.name;
            }
            else
            {
                // [ĐÃ SỬA] CHẾ ĐỘ AN TOÀN: Nếu quên thêm vào Database, vẫn cho hiện nhưng ID = -1
                // Để tránh spam lỗi đỏ làm lag game
                Debug.LogWarning($"CẢNH BÁO: Món đồ '{data.name}' chưa được thêm vào DatabaseVatChung trong Inspector! Hãy thêm nó vào để có ID.");
                dragScript.itemID = -1; // -1 nghĩa là vật phẩm không có dữ liệu gốc
                dragScript.itemName = data.name;
            }
            // ------------------------------------

            Button btn = newSlot.GetComponent<Button>();
            if (btn == null) btn = newSlot.AddComponent<Button>();

            // Sửa lỗi closure biến trong vòng lặp foreach (Cần tạo biến cục bộ)
            string desc = data.description;
            btn.onClick.AddListener(() => ShowDialogue(desc));

            InventorySlotHover hoverScript = newSlot.GetComponent<InventorySlotHover>();
            if (hoverScript != null) hoverScript.itemNameData = data.name;
        }
    }

    // --- 4. HỆ THỐNG HỘI THOẠI ĐA NHÂN VẬT (NÂNG CẤP) ---

    // Hàm 1: Dành cho Player (Giữ nguyên tên ShowDialogue để code cũ không bị lỗi)
    public void ShowDialogue(string content, Sprite emotion = null, float speedOverride = -1f)
    {
        AnHetBangHoiThoai(); // Tắt bảng NPC đi
        dangHoiThoai = true;
        dangLaNPCNoi = false;

        playerDialogueGroup.SetActive(true); // Bật bảng Player

        if (nhanVatPlayer != null) playerNameText.text = nhanVatPlayer.name;
        if (playerPortraitImage != null)
        {
            if (emotion != null) playerPortraitImage.sprite = emotion;
            else if (anhThamTuMacDinh != null) playerPortraitImage.sprite = anhThamTuMacDinh;
        }

        BatDauChayChu(playerDialogueText, content, speedOverride);
    }

    // [MỚI] Hàm 2: Dành cho TẤT CẢ nhân vật khác (Quan, Bà Hàng Xóm, v.v...)
    // Cách dùng: InventoryManager.Instance.ShowDialogueByID("quan", "Ta là quan huyện!");
    // Trong InventoryManager.cs

    // Sửa hàm này để nhận thêm tham số emotionOverride (Sprite)
    public void ShowDialogueByID(string characterID, string content, Sprite emotionOverride = null)
    {
        AnHetBangHoiThoai();
        dangHoiThoai = true;
        dangLaNPCNoi = true;

        npcDialogueGroup.SetActive(true);

        CharacterData data = danhSachNhanVat.Find(x => x.id == characterID);

        if (data != null)
        {
            npcNameText.text = data.tenHienThi;

            if (npcPortraitImage != null)
            {
                if (data.coAnhDaiDien)
                {
                    npcPortraitImage.gameObject.SetActive(true);

                    // 🔥 LOGIC MỚI: Ưu tiên dùng ảnh cảm xúc (nếu có), nếu không thì dùng avatar mặc định
                    if (emotionOverride != null)
                        npcPortraitImage.sprite = emotionOverride;
                    else
                        npcPortraitImage.sprite = data.avatar;
                }
                else
                {
                    npcPortraitImage.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            npcNameText.text = "???";
            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
        }

        BatDauChayChu(npcDialogueText, content, -1f);
    }


    // Hàm 3: Hỗ trợ code cũ (ShowDialogueNPC dùng GameObject)
    // Tôi giữ lại hàm này để các script cũ của bạn không bị lỗi đỏ
    public void ShowDialogueNPC(GameObject npcObj, string content, Sprite portraitNPC = null)
    {
        AnHetBangHoiThoai();
        dangHoiThoai = true;
        dangLaNPCNoi = true;

        npcDialogueGroup.SetActive(true);

        npcNameText.text = (npcObj != null) ? npcObj.name : "Người lạ";
        if (npcPortraitImage != null && portraitNPC != null) npcPortraitImage.sprite = portraitNPC;

        BatDauChayChu(npcDialogueText, content, -1f);
    }

    // --- 5. CÁC HÀM XỬ LÝ CHUNG (Logic tắt bật bảng) ---

    private void AnHetBangHoiThoai()
    {
        if (playerDialogueGroup != null) playerDialogueGroup.SetActive(false);
        if (npcDialogueGroup != null) npcDialogueGroup.SetActive(false);
        if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
        dangGoChu = false;
    }

    void BatDauChayChu(TMP_Text targetText, string content, float speedOverride)
    {
        noiDungDayDu = content;
        float currentSpeed = (speedOverride > 0) ? speedOverride : tocDoGo;
        if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
        tienTrinhGoChu = StartCoroutine(ChayChuTungKyTu(targetText, content, currentSpeed));
    }

    IEnumerator ChayChuTungKyTu(TMP_Text targetText, string doanVan, float speed)
    {
        dangGoChu = true;
        targetText.text = "";
        foreach (char kyTu in doanVan.ToCharArray())
        {
            targetText.text += kyTu;
            yield return new WaitForSeconds(speed);
        }
        dangGoChu = false;
    }

    public void OnDialoguePanelClick()
    {
        TMP_Text currentText = dangLaNPCNoi ? npcDialogueText : playerDialogueText;
        if (dangGoChu)
        {
            if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
            currentText.text = noiDungDayDu;
            dangGoChu = false;
        }
        else
        {
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        AnHetBangHoiThoai();
        if (!inventoryPanel.activeSelf) dangHoiThoai = false;
    }

    public void ShowTooltip(string text, Vector3 unused)
    {
        tooltipText.text = text;
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
    public void RemoveItemByID(int id)
    {
        // 1. Tìm tên vật phẩm dựa trên ID (để xóa trong list hiển thị)
        VatChungData data = databaseVatChung.Find(x => x.id == id);
        if (data != null)
        {
            // Xóa khỏi danh sách hiển thị trong túi
            collectedItems.RemoveAll(x => x.name == data.ten);

            // Cập nhật lại giao diện túi đồ (nếu đang mở)
            RefreshUI();
            Debug.Log($"Đã xóa vật phẩm: {data.ten} khỏi túi.");
        }

        // 2. Xóa khỏi SaveGameManager (Để lần sau load game nó không hiện lại nữa)
        if (SaveGameManager.Instance != null)
        {
            if (SaveGameManager.Instance.vatChungDaNhat.Contains(id))
            {
                SaveGameManager.Instance.vatChungDaNhat.Remove(id);
            }
        }
    }
}