using UnityEngine;
using System.Collections;

public class MusicZoneTrigger : MonoBehaviour
{
    [Header("Cấu hình Nhạc Nền")]
    public AudioSource musicAudioSource; // Kéo cái Loa phát nhạc nền vào đây

    [Tooltip("Bài nhạc mở đầu (Chạy 1 lần). Để trống nếu không cần.")]
    public AudioClip nhacIntro;

    [Tooltip("Bài nhạc lặp lại (Chạy mãi mãi).")]
    public AudioClip nhacLoop;

    private bool daKichHoat = false;

    void Start()
    {
        // Tự động tìm AudioSource nếu quên kéo
        if (musicAudioSource == null)
            musicAudioSource = GetComponent<AudioSource>();

        // 🔥 CHECK LOGIC LOAD GAME:
        // Nếu người chơi Load Game ngay tại vị trí này, ta cần đảm bảo nhạc vẫn chạy
        // (Kiểm tra xem nhạc có đang tắt không, nếu tắt thì bật lên)
        if (musicAudioSource != null && !musicAudioSource.isPlaying && daKichHoat)
        {
            PlayMusic();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ kích hoạt khi Player bước vào VÀ nhạc chưa từng bật
        if (other.CompareTag("Player") && !daKichHoat)
        {
            // Kiểm tra: Nếu nhạc đã đang chạy rồi thì thôi (tránh bị reset nhạc khi đi ra đi vô)
            if (musicAudioSource.isPlaying && musicAudioSource.clip == nhacLoop)
                return;

            PlayMusic();
        }
    }

    void PlayMusic()
    {
        daKichHoat = true;
        StartCoroutine(QuyTrinhPhatNhac());
    }

    IEnumerator QuyTrinhPhatNhac()
    {
        // 1. Phát nhạc Intro (nếu có)
        if (nhacIntro != null)
        {
            musicAudioSource.clip = nhacIntro;
            musicAudioSource.loop = false;
            musicAudioSource.Play();

            // Chờ hết bài Intro
            yield return new WaitForSeconds(nhacIntro.length);
        }

        // 2. Chuyển sang nhạc Loop (Chính)
        if (nhacLoop != null)
        {
            musicAudioSource.clip = nhacLoop;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
    }
}