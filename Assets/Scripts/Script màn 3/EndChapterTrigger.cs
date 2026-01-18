using UnityEngine;
using System.Collections;

public class EndChapterTrigger : MonoBehaviour
{
    [Header("--- CẤU HÌNH UI ---")]
    [Tooltip("Kéo cái nút 'Rời đi' (World Space) hiện trên đầu nhân vật vào đây")]
    public GameObject nutTuongTac;

    [Tooltip("Kéo cái GameObject chứa script ChapterEndPanel của bạn vào đây")]
    public GameObject bangKetThucChapter;

    [Header("--- HỘI THOẠI TRƯỚC KHI ĐI ---")]
    [TextArea] public string loiThoaiCuoi = "Mọi việc đã xong. Mình cần rời khỏi đây ngay.";

    private bool playerOTrongVung = false;

    void Start()
    {
        // Ẩn nút và bảng lúc đầu
        if (nutTuongTac != null) nutTuongTac.SetActive(false);
        if (bangKetThucChapter != null) bangKetThucChapter.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Khi Player đi vào vùng Trigger
        if (other.CompareTag("Player"))
        {
            playerOTrongVung = true;
            if (nutTuongTac != null) nutTuongTac.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOTrongVung = false;
            if (nutTuongTac != null) nutTuongTac.SetActive(false);
        }
    }

    // 🔥 Gán hàm này vào sự kiện OnClick của nút "Rời đi"
    public void BamNutKetThuc()
    {
        StartCoroutine(QuyTrinhKetThuc());
    }

    IEnumerator QuyTrinhKetThuc()
    {
        // 1. Ẩn nút đi
        if (nutTuongTac != null) nutTuongTac.SetActive(false);

        // 2. Hiện câu thoại cuối cùng (nếu có)
        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(loiThoaiCuoi))
        {
            InventoryManager.Instance.ShowDialogue("Nhân vật chính", loiThoaiCuoi);
            yield return null;
            yield return new WaitUntil(() => !InventoryManager.Instance.dangHoiThoai);
        }

        // 3. 🔥 KÍCH HOẠT BẢNG CỦA BẠN
        // Khi dòng này chạy, script ChapterEndPanel của bạn sẽ tự động chạy hàm OnEnable() -> Fade In -> Chờ bấm tiếp tục
        if (bangKetThucChapter != null)
        {
            bangKetThucChapter.SetActive(true);
        }
    }
}