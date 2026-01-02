using UnityEngine;
using System.Collections.Generic;

public class CharacterAudio : MonoBehaviour
{

    [Header("Cài đặt Âm thanh")]
    public AudioSource audioSource;

    // Một danh sách các file âm thanh để chọn ngẫu nhiên (cho đỡ nhàm chán)
    public List<AudioClip> tiengBuocChan;

    [Range(0.8f, 1.2f)]
    public float doTramBong = 1f; // Pitch (độ cao thấp của âm thanh)

    void Start()
    {
        // Tự tìm AudioSource nếu chưa kéo thả
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // --- HÀM NÀY SẼ ĐƯỢC GỌI TỪ ANIMATION ---
    public void PlayFootstep()
    {
        Debug.Log("Đã nhận lệnh phát tiếng chân!");
        if (tiengBuocChan.Count > 0 && audioSource != null)
        {
            // 1. Chọn ngẫu nhiên 1 âm thanh trong list
            int index = Random.Range(0, tiengBuocChan.Count);

            // 2. Chỉnh độ cao thấp ngẫu nhiên một chút cho tự nhiên
            audioSource.pitch = Random.Range(0.8f, 1.2f);

            // 3. Chỉnh âm lượng ngẫu nhiên một chút
            audioSource.volume = Random.Range(0.8f, 1f);

            // 4. Phát âm thanh (PlayOneShot cho phép các âm thanh đè lên nhau mà không bị ngắt)
            audioSource.PlayOneShot(tiengBuocChan[index]);
        }
    }
}