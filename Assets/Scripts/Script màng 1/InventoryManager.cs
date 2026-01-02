using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI; // Dùng cho Image
using TMPro;

// Tạo một lớp nhỏ để lưu trữ cặp thông tin (Tên + Icon)
[System.Serializable]
public class CollectedItemData
{
    public string name;
    public Sprite icon;
    public string description;


    public CollectedItemData(string n, Sprite i, string d) { name = n; icon = i; description = d; }
}

public class InventoryManager : MonoBehaviour
{
    public Vector3 offset = new Vector3(15, 15, 0); // Khoảng cách lệch so với chuột

    void Update()
    {
        transform.position = Input.mousePosition + offset;
    }

    public static InventoryManager Instance; // Singleton

    [Header("Thông tin nhân vật")]
    public GameObject nhanVatPlayer;
    public Sprite anhThamTuMacDinh;

    [Header("Cài đặt UI")]
    public GameObject inventoryPanel; // Kéo cái Panel to vào đây
    public Transform contentArea;     // Kéo cái ContentArea (chứa Layout Group) vào đây
    public GameObject itemIconPrefab;

    [Header("Cài đặt Tooltip")]
    public GameObject tooltipPanel;   // Kéo cái Panel Tooltip vào
    public TMP_Text tooltipText;      // Kéo cái Text trong Tooltip vào
    public Vector2 tooltipOffset = new Vector2(10, 10);

    [Header("UI Hội thoại - NHÂN VẬT CHÍNH")]
    public GameObject playerDialogueGroup; // Kéo DialogueGroup_Player vào đây
    public TMP_Text playerNameText;        // Kéo NameText của Player
    public TMP_Text playerDialogueText;    // Kéo DialogueText của Player
    public Image playerPortraitImage;      // Kéo Portrait của Player

    [Header("UI Hội thoại - NPC (MỚI)")]
    public GameObject npcDialogueGroup;    // Kéo DialogueGroup_NPC vào đây
    public TMP_Text npcNameText;           // Kéo NameText của NPC
    public TMP_Text npcDialogueText;       // Kéo DialogueText của NPC
    public Image npcPortraitImage;         // Kéo Portrait của NPC

    [Header("Hiệu ứng chữ chạy")]
    [Range(0.01f, 0.1f)]
    public float tocDoGo = 0.05f; // Thời gian đợi giữa các chữ (càng nhỏ càng nhanh)
    private Coroutine tienTrinhGoChu; // Biến để lưu tiến trình đang chạy
    private bool dangGoChu = false;   // Kiểm tra xem có đang chạy chữ không
    private string noiDungDayDu;      // Lưu lại nội dung gốc để hiển thị khi skip
    private bool dangLaNPCNoi = false;

    public bool dangHoiThoai = false;

    // Danh sách lưu trữ kiểu dữ liệu mới (CollectedItemData)
    private List<CollectedItemData> collectedItems = new List<CollectedItemData>();

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
    }

    // Hàm gọi khi nhặt được đồ mới
    public void AddItem(string itemName, Sprite itemIcon, string itemDesc)
    {
        // Tạo data mới và thêm vào list
        collectedItems.Add(new CollectedItemData(itemName, itemIcon, itemDesc));
        Debug.Log("Đã thêm: " + itemName);
    }

    // Hàm gọi khi bấm vào icon Cuốn sách
    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            RefreshUI();
            dangHoiThoai = true; // Mở túi -> Dừng di chuyển
        }
        else
        {
            HideTooltip();
            // Chỉ cho phép đi lại nếu KHÔNG có bảng hội thoại nào đang mở
            if (!playerDialogueGroup.activeSelf && !npcDialogueGroup.activeSelf)
            {
                dangHoiThoai = false;
            }
        }
    }

    // Vẽ lại danh sách vật chứng lên màn hình
    void RefreshUI()
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // Duyệt qua danh sách dữ liệu mới
        foreach (CollectedItemData data in collectedItems)
        {
            // Tạo icon
            GameObject newSlot = Instantiate(itemIconPrefab, contentArea);

            // 1. Gán hình ảnh cho icon
            newSlot.GetComponent<Image>().sprite = data.icon;

            Button btn = newSlot.GetComponent<Button>();
            if (btn == null) btn = newSlot.AddComponent<Button>();
            btn.onClick.AddListener(() => ShowDialogue(data.description));

            // 2. Gán tên vào script hover để nó biết nó tên gì
            InventorySlotHover hoverScript = newSlot.GetComponent<InventorySlotHover>();
            if (hoverScript != null)
            {
                hoverScript.itemNameData = data.name;
            }
        }
    }

    public void ShowDialogue(string content, Sprite emotion = null, float speedOverride = -1f)
    {
        // 1. Chỉ tắt UI cũ, KHÔNG ĐƯỢC set dangHoiThoai = false ở đây
        AnHetBangHoiThoai();

        // 2. Bật cờ dừng di chuyển
        dangHoiThoai = true;

        // 3. Hiện UI mới
        dangLaNPCNoi = false;
        playerDialogueGroup.SetActive(true);

        if (nhanVatPlayer != null) playerNameText.text = nhanVatPlayer.name;
        if (playerPortraitImage != null)
        {
            if (emotion != null) playerPortraitImage.sprite = emotion;
            else if (anhThamTuMacDinh != null) playerPortraitImage.sprite = anhThamTuMacDinh;
        }

        BatDauChayChu(playerDialogueText, content, speedOverride);
    }

    // --- XỬ LÝ HỘI THOẠI NPC ---
    public void ShowDialogueNPC(GameObject npcObj, string content, Sprite portraitNPC = null)
    {
        // 1. Chỉ tắt UI cũ
        AnHetBangHoiThoai();

        // 2. Bật cờ dừng di chuyển
        dangHoiThoai = true;

        // 3. Hiện UI mới
        dangLaNPCNoi = true;
        npcDialogueGroup.SetActive(true);

        if (npcObj != null) npcNameText.text = npcObj.name;
        else npcNameText.text = "Người lạ";

        if (npcPortraitImage != null && portraitNPC != null)
            npcPortraitImage.sprite = portraitNPC;

        BatDauChayChu(npcDialogueText, content, -1f);
    }

    // --- HÀM PHỤ TRỢ MỚI: CHỈ TẮT UI ---
    // Hàm này giúp chuyển từ Player nói sang NPC nói mà không bị nhân vật "giật" di chuyển
    private void AnHetBangHoiThoai()
    {
        if (playerDialogueGroup != null) playerDialogueGroup.SetActive(false);
        if (npcDialogueGroup != null) npcDialogueGroup.SetActive(false);

        if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
        dangGoChu = false;

        // QUAN TRỌNG: Hàm này KHÔNG set dangHoiThoai = false
    }

    // Hàm xử lý chạy chữ chung cho cả 2
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
        // Xác định xem đang dùng bảng nào
        TMP_Text currentText = dangLaNPCNoi ? npcDialogueText : playerDialogueText;

        if (dangGoChu)
        {
            // SKIP: Hiện hết chữ
            if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
            currentText.text = noiDungDayDu;
            dangGoChu = false;
        }
        else
        {
            // CLOSE: Đóng bảng
            CloseDialogue();
        }
    }

    public void CloseDialogue()
    {
        AnHetBangHoiThoai(); // Tắt UI

        // Khi này mới thực sự cho nhân vật đi lại
        // (Kiểm tra thêm: Nếu túi đồ đang mở thì vẫn không được đi)
        if (!inventoryPanel.activeSelf)
        {
            dangHoiThoai = false;
        }
    }

    // Hàm phụ trợ: Chỉ tắt UI (để chuyển giữa Player và NPC mà không bị giật trạng thái di chuyển)
    private void CloseDialogueUIOnly()
    {
        playerDialogueGroup.SetActive(false);
        npcDialogueGroup.SetActive(false);
        if (tienTrinhGoChu != null) StopCoroutine(tienTrinhGoChu);
        dangGoChu = false;
    }

    public void ShowTooltip(string text, Vector3 unused) // Không cần dùng biến vị trí nữa
    {
        tooltipText.text = text;

        // Chỉ cần bật nó lên. Script TooltipFollow sẽ tự lo việc di chuyển.
        tooltipPanel.SetActive(true);

        // Đẩy lên cùng để không bị che
        tooltipPanel.transform.SetAsLastSibling();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}