using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // Thư viện xử lý Video
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class IntroVideoController : MonoBehaviour
{
    [Header("Cài đặt Video")]
    public VideoPlayer videoPlayer; // Kéo Video Player vào đây
    public RawImage manHinhHienThi; // Kéo RawImage (UI) vào đây

    [Header("Cài đặt Hội thoại")]
    public TextMeshProUGUI textHienThi; // Kéo TextMeshPro vào đây
    public float tocDoGoChu = 0.05f;
    public GameObject iconTiepTuc; // Mũi tên nhấp nháy báo hiệu bấm tiếp (nếu có)

    [Header("Nội dung Cốt truyện")]
    [TextArea(3, 5)]
    public List<string> danhSachLoiThoai;

    [Header("Chuyển cảnh sau khi xong")]
    public string tenSceneTiepTheo = "GameScene"; // Tên màn 1

    // Biến nội bộ
    private int indexHienTai = 0;
    private bool dangGoChu = false;
    private string noiDungDayDu;

    void Start()
    {
        // 1. Cấu hình Video lặp lại vô tận
        if (videoPlayer != null)
        {
            videoPlayer.isLooping = true; // 🔥 QUAN TRỌNG: Cho video lặp
            videoPlayer.Play();
        }

        // 2. Bắt đầu dòng thoại đầu tiên
        if (danhSachLoiThoai.Count > 0)
        {
            ShowLine();
        }
        else
        {
            // Nếu không có thoại thì vào game luôn
            ChuyenVaoGame();
        }
    }

    void Update()
    {
        // Nhận nút bấm chuột trái hoặc Space để qua thoại
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (dangGoChu)
            {
                // Nếu đang gõ -> Hiện full luôn
                StopAllCoroutines();
                textHienThi.text = noiDungDayDu;
                dangGoChu = false;
                if (iconTiepTuc) iconTiepTuc.SetActive(true);
            }
            else
            {
                // Nếu đã hiện hết -> Qua câu tiếp theo
                NextLine();
            }
        }
    }

    void ShowLine()
    {
        noiDungDayDu = danhSachLoiThoai[indexHienTai];
        StartCoroutine(GoChuHieuUng(noiDungDayDu));
    }

    void NextLine()
    {
        indexHienTai++;
        if (indexHienTai < danhSachLoiThoai.Count)
        {
            ShowLine();
        }
        else
        {
            // Hết thoại -> Chuyển Scene
            ChuyenVaoGame();
        }
    }

    IEnumerator GoChuHieuUng(string content)
    {
        dangGoChu = true;
        textHienThi.text = "";
        if (iconTiepTuc) iconTiepTuc.SetActive(false);

        foreach (char c in content.ToCharArray())
        {
            textHienThi.text += c;
            yield return new WaitForSeconds(tocDoGoChu);
        }

        dangGoChu = false;
        if (iconTiepTuc) iconTiepTuc.SetActive(true);
    }

    void ChuyenVaoGame()
    {
        Debug.Log("Hết Intro -> Vào Game");
        SceneManager.LoadScene(tenSceneTiepTheo);
    }
}