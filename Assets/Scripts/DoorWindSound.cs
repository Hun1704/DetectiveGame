using UnityEngine;

public class DoorWindSound : MonoBehaviour
{
    [Header("Cài đặt Âm thanh")]
    public AudioSource audioSource; // Kéo AudioSource tiếng gió vào đây
    public float khoangCachNgheThay = 5f; // Đi xa quá 5 mét sẽ không nghe thấy nữa
    public float maxVolume = 1f; // Âm lượng to nhất khi đứng sát cửa

    private Transform playerTransform;
    private bool daTatTiengGio = false;

    void Start()
    {
        // 1. Tìm nhân vật chính
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // 2. Tự tìm AudioSource nếu quên kéo
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 3. Đảm bảo tiếng gió lặp lại
        if (audioSource != null)
        {
            audioSource.loop = true;
            if (!audioSource.isPlaying) audioSource.Play();
        }
    }

    void Update()
    {
        // Nếu đã tắt tiếng gió hoặc không tìm thấy Player/Audio thì dừng
        if (daTatTiengGio || playerTransform == null || audioSource == null) return;

        // --- LOGIC TÍNH KHOẢNG CÁCH ---
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance < khoangCachNgheThay)
        {
            // Càng gần (distance nhỏ) -> Volume càng to (gần 1)
            // Càng xa (distance lớn) -> Volume càng nhỏ (gần 0)
            float newVolume = 1 - (distance / khoangCachNgheThay);

            // Giới hạn volume không vượt quá maxVolume
            audioSource.volume = Mathf.Clamp(newVolume * maxVolume, 0f, maxVolume);
        }
        else
        {
            // Đi xa quá thì volume = 0
            audioSource.volume = 0f;
        }
    }

    // Hàm này để LevelStartEvent gọi khi xong chuyện
    public void TatVinhVien()
    {
        daTatTiengGio = true;
        if (audioSource != null)
        {
            // Fade out nhanh cho mượt rồi tắt hẳn
            StartCoroutine(FadeOutVaTat());
        }
    }

    System.Collections.IEnumerator FadeOutVaTat()
    {
        float startVol = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime * 2f; // Giảm volume nhanh
            yield return null;
        }
        audioSource.Stop();
        this.enabled = false; // Tắt luôn script này cho nhẹ máy
    }
}