using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Tạm Dừng")]
    public GameObject pausePanel; // Kéo cái Panel Pause vào đây
    public string mainMenuSceneName = "MainMenu"; // Tên scene màn hình chính

    // Biến kiểm tra xem game có đang pause không
    public static bool IsPaused = false;

    void Update()
    {
        // Lắng nghe phím ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // 1. Hiện bảng menu
        pausePanel.SetActive(true);

        // 2. Đóng băng thời gian (Mọi chuyển động, vật lý sẽ dừng lại)
        Time.timeScale = 0f;

        // 3. Cập nhật trạng thái
        IsPaused = true;
    }

    public void ResumeGame()
    {
        // 1. Ẩn bảng menu
        pausePanel.SetActive(false);

        // 2. Trả lại thời gian bình thường
        Time.timeScale = 1f;

        // 3. Cập nhật trạng thái
        IsPaused = false;
    }

    public void OnMenuClick()
    {
        // QUAN TRỌNG: Trước khi chuyển cảnh phải trả thời gian về 1
        // Nếu không, qua màn hình chính mọi thứ sẽ bị đông cứng
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitClick()
    {
        Debug.Log("Thoát Game...");
        Application.Quit();
    }
}