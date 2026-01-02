using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SequentialTitleController : MonoBehaviour
{
    [System.Serializable]
    public class TitlePart
    {
        public GameObject obj;         // Kéo object chữ vào đây
        [HideInInspector] public CanvasGroup canvasGroup;
        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public Vector2 finalPos;
        [HideInInspector] public Vector2 startPos;
        [HideInInspector] public bool isFinished = false; // Đã hiện xong chưa?
    }

    [Header("Danh sách Tên Game (Kéo theo thứ tự xuất hiện)")]
    public List<TitlePart> titleParts;

    [Header("Cấu hình Xuất hiện")]
    public float delayBeforeStart = 1.0f; // Chờ bao lâu mới bắt đầu chữ đầu tiên
    public float timeBetweenParts = 0.5f; // Chữ trước hiện xong, chờ bao lâu đến chữ sau?
    public float appearDuration = 2.5f;   // Thời gian để 1 chữ hiện ra (càng lâu càng ảo)
    public float startOffset = -60f;      // Vị trí bắt đầu lệch xuống dưới bao nhiêu

    [Header("Cấu hình Nhấp nhô (Floating)")]
    public bool enableFloating = true;
    public float floatSpeed = 1.0f;
    public float floatDistance = 8f;

    void Start()
    {
        // 1. Thiết lập ban đầu: Ẩn tất cả và dời vị trí xuống dưới
        foreach (var part in titleParts)
        {
            if (part.obj != null)
            {
                part.canvasGroup = part.obj.GetComponent<CanvasGroup>();
                part.rectTransform = part.obj.GetComponent<RectTransform>();

                // Nếu quên chưa thêm CanvasGroup thì tự thêm vào
                if (part.canvasGroup == null) part.canvasGroup = part.obj.AddComponent<CanvasGroup>();

                // Lưu vị trí gốc (Vị trí bạn đặt trong Editor là đích đến)
                part.finalPos = part.rectTransform.anchoredPosition;
                // Tính vị trí xuất phát (Lệch xuống dưới)
                part.startPos = new Vector2(part.finalPos.x, part.finalPos.y + startOffset);

                // Áp dụng trạng thái ẩn
                part.canvasGroup.alpha = 0f;
                part.rectTransform.anchoredPosition = part.startPos;
                part.isFinished = false;
            }
        }

        // Bắt đầu chuỗi hiệu ứng
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // Chờ background ổn định
        yield return new WaitForSeconds(delayBeforeStart);

        // Duyệt qua từng phần của tên game
        foreach (var part in titleParts)
        {
            if (part.obj == null) continue;

            // Bắt đầu chạy hiệu ứng cho phần này
            StartCoroutine(AnimateSinglePart(part));

            // Logic chờ đợi: 
            // Bạn muốn chữ sau hiện khi chữ trước ĐÃ XONG? Hay hiện đè lên nhau?
            // Ở đây mình để: Chờ chữ trước hiện được 70% quãng đường thì chữ sau bắt đầu (cho mượt)
            yield return new WaitForSeconds(appearDuration * 0.7f + timeBetweenParts);
        }
    }

    IEnumerator AnimateSinglePart(TitlePart part)
    {
        float timer = 0f;

        while (timer < appearDuration)
        {
            timer += Time.deltaTime;
            float percent = timer / appearDuration;

            // Hiệu ứng chuyển động mềm mại
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            // 1. Fade In
            part.canvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothPercent);

            // 2. Trôi từ dưới lên
            part.rectTransform.anchoredPosition = Vector2.Lerp(part.startPos, part.finalPos, smoothPercent);

            yield return null;
        }

        // Đảm bảo kết thúc chính xác
        part.canvasGroup.alpha = 1f;
        part.rectTransform.anchoredPosition = part.finalPos;
        part.isFinished = true; // Đánh dấu để bắt đầu nhấp nhô
    }

    void Update()
    {
        // Xử lý hiệu ứng nhấp nhô cho những chữ đã hiện xong
        if (enableFloating)
        {
            foreach (var part in titleParts)
            {
                if (part.isFinished && part.obj != null)
                {
                    // Mỗi chữ nhấp nhô lệch pha nhau một chút dựa trên vị trí X của nó 
                    // (để 2 dòng không nhấp nhô cùng lúc như robot)
                    float noise = part.finalPos.x * 0.01f;

                    float newY = part.finalPos.y + Mathf.Sin((Time.time + noise) * floatSpeed) * floatDistance;
                    part.rectTransform.anchoredPosition = new Vector2(part.finalPos.x, newY);
                }
            }
        }
    }
}