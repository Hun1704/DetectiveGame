using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;
    Vector2 movement;

    void Update()
    {
        // --- ĐOẠN CODE QUAN TRỌNG ĐỂ DỪNG NHÂN VẬT ---
        // Kiểm tra xem InventoryManager có tồn tại và biến dangHoiThoai có đang bật không
        if (InventoryManager.Instance != null && InventoryManager.Instance.dangHoiThoai)
        {
            // 1. Reset vector di chuyển về 0 để FixedUpdate không đẩy nhân vật đi nữa
            movement = Vector2.zero;

            // 2. Dừng quán tính vật lý ngay lập tức
            // (Nếu bạn dùng Unity phiên bản cũ thì đổi 'linearVelocity' thành 'velocity')
            rb.linearVelocity = Vector2.zero;

            // 3. Ép Animation Speed về 0 để nhân vật đứng yên (Idle)
            animator.SetFloat("Speed", 0);

            // 4. Ngắt hàm Update, không cho nhận nút bấm bàn phím nữa
            return;
        }
        // ----------------------------------------------

        // Code di chuyển bình thường
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Cập nhật Animation
        animator.SetFloat("Speed", movement.sqrMagnitude);

        // Code xoay mặt nhân vật
        if (movement.x != 0)
        {
            float sizeX = Mathf.Abs(transform.localScale.x);
            float sizeY = transform.localScale.y;
            float sizeZ = transform.localScale.z;

            if (movement.x > 0)
                transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
            else
                transform.localScale = new Vector3(-sizeX, sizeY, sizeZ);
        }
    }

    void FixedUpdate()
    {
        // Khi đang hội thoại, movement đã bị set về (0,0) ở trên nên dòng này sẽ giữ nhân vật đứng yên
        rb.linearVelocity = movement.normalized * moveSpeed;
    }
}