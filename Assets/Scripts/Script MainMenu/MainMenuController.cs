using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("--- SCENE GAME ---")]
    public string gameSceneName = "IntroScene";

    [Header("--- ÂM THANH ---")]
    public AudioSource backgroundMusic; 
    public float musicFadeDuration = 1.5f; 

    [Header("--- BACKGROUND HIỆU ỨNG ---")]
    public Image displayImage;
    public List<Sprite> backgroundImages;
    public float showTime = 6f;
    public float fadeSpeed = 1.5f;
    public float zoomScale = 1.05f;

    [Header("--- CÁC CỬA SỔ UI ---")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadGamePanel;

    [Header("--- CÀI ĐẶT ---")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    // Biến nội bộ
    private CanvasGroup bgCanvasGroup;
    private RectTransform bgRect;
    private Vector3 initialScale;
    private int currentIndex = 0;

    [Header("Load Slot Buttons")]
    public Button slot1Button;
    public Button slot2Button;
    public Button slot3Button;


    void Start()
    {
        // 1. Setup Background
        SetupBackground();

        // 2. Setup Âm thanh & Cài đặt
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // Đảm bảo nhạc đang chạy
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }

        ShowPanel(mainMenuPanel);
    }

    // --- CÁC HÀM NÚT BẤM ---

    public void OnStartClick()
    {
        SaveGameManager.Instance.currentSlot = 1;
        StartCoroutine(FadeOutMusicAndLoadScene());
    }

    // Coroutine xử lý chuyển cảnh mượt mà
    IEnumerator FadeOutMusicAndLoadScene()
    {
        // 1. Fade out nhạc
        if (backgroundMusic != null)
        {
            float startVolume = backgroundMusic.volume;
            float timer = 0;

            while (timer < musicFadeDuration)
            {
                timer += Time.deltaTime;
                // Giảm volume từ mức hiện tại về 0
                backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
                yield return null;
            }
            backgroundMusic.volume = 0;
        }

        // 3. Chuyển cảnh
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnLoadClick()
    {
        ShowPanel(loadGamePanel);

        foreach (var slot in loadGamePanel.GetComponentsInChildren<SaveSlotUI>())
        {
            slot.Refresh();
        }
    }


    public void OnSettingsClick()
    {
        ShowPanel(settingsPanel);
    }

    public void OnQuitClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void LoadSlot1()
    {
        if (SaveGameManager.Instance.HasSave(1))
        {
            SaveGameManager.Instance.currentSlot = 1; // 🔥 MỚI: Nhớ Slot 1
            SaveGameManager.Instance.LoadGame(1);
        }
    }

    public void LoadSlot2()
    {
        if (SaveGameManager.Instance.HasSave(2))
        {
            SaveGameManager.Instance.currentSlot = 2; // 🔥 MỚI: Nhớ Slot 2
            SaveGameManager.Instance.LoadGame(2);
        }
    }

    public void LoadSlot3()
    {
        if (SaveGameManager.Instance.HasSave(3))
        {
            SaveGameManager.Instance.currentSlot = 3; // 🔥 MỚI: Nhớ Slot 3
            SaveGameManager.Instance.LoadGame(3);
        }
    }

    public void DeleteSlot1()
    {
        SaveGameManager.Instance.DeleteSave(1);
        RefreshLoadSlots();
    }

    public void DeleteSlot2()
    {
        SaveGameManager.Instance.DeleteSave(2);
        RefreshLoadSlots();
    }

    public void DeleteSlot3()
    {
        SaveGameManager.Instance.DeleteSave(3);
        RefreshLoadSlots();
    }


    public void RefreshLoadSlots()
    {
        Refresh(slot1Button, 1);
        Refresh(slot2Button, 2);
        Refresh(slot3Button, 3);
    }

    void Refresh(Button btn, int slot)
    {
        if (btn == null) return;

        bool hasSave = SaveGameManager.Instance.HasSave(slot);
        btn.interactable = hasSave;
    }

    // --- CÁC HÀM HỆ THỐNG ---

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume; // Chỉnh âm lượng tổng
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void OnBackToMenuClick()
    {
        ShowPanel(mainMenuPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (loadGamePanel) loadGamePanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }

    // --- LOGIC BACKGROUND (GIỮ NGUYÊN) ---
    void SetupBackground()
    {
        if (displayImage != null)
        {
            bgCanvasGroup = displayImage.GetComponent<CanvasGroup>();
            bgRect = displayImage.GetComponent<RectTransform>();
            if (bgCanvasGroup == null) bgCanvasGroup = displayImage.gameObject.AddComponent<CanvasGroup>();
            initialScale = bgRect.localScale;

            if (backgroundImages.Count > 0)
            {
                displayImage.sprite = backgroundImages[0];
                bgCanvasGroup.alpha = 1;
                StartCoroutine(CycleBackgrounds());
            }
        }
    }

    IEnumerator CycleBackgrounds()
    {
        while (true)
        {
            float timer = 0;
            Vector3 startScale = initialScale;
            Vector3 endScale = initialScale * zoomScale;

            while (timer < showTime)
            {
                timer += Time.deltaTime;
                bgRect.localScale = Vector3.Lerp(startScale, endScale, timer / showTime);
                yield return null;
            }

            float fadeTimer = 0;
            while (fadeTimer < 1f)
            {
                fadeTimer += Time.deltaTime * fadeSpeed;
                bgCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer);
                yield return null;
            }

            currentIndex = (currentIndex + 1) % backgroundImages.Count;
            displayImage.sprite = backgroundImages[currentIndex];
            bgRect.localScale = initialScale;

            fadeTimer = 0;
            while (fadeTimer < 1f)
            {
                fadeTimer += Time.deltaTime * fadeSpeed;
                bgCanvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer);
                bgRect.localScale = Vector3.Lerp(initialScale, initialScale * (1 + (zoomScale - 1) * 0.1f), fadeTimer);
                yield return null;
            }
        }
    }
}