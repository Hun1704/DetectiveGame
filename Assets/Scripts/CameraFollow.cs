using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;       // Kéo nhân vật vào đây
    public float smoothSpeed = 5f; // Độ mượt (càng thấp càng chậm, tầm 2-5 là đẹp)

    // Giới hạn bản đồ (Camera sẽ không đi quá các số này)
    public Vector2 minLimit; // Góc dưới-trái (X nhỏ nhất, Y nhỏ nhất)
    public Vector2 maxLimit; // Góc trên-phải (X lớn nhất, Y lớn nhất)

    void LateUpdate()
    {
        if (player == null) return;

        // Vị trí mục tiêu mà camera muốn tới (lấy X, Y của nhân vật)
        // Giữ nguyên Z của camera (thường là -10) để không bị mất hình
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);

        // Kẹp vị trí lại trong giới hạn bản đồ (Mathf.Clamp)
        targetPosition.x = Mathf.Clamp(targetPosition.x, minLimit.x, maxLimit.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minLimit.y, maxLimit.y);

        // Dùng Lerp để di chuyển mượt mà từ vị trí cũ sang vị trí mới
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}