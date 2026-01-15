using UnityEngine;

public class IngredientClick : MonoBehaviour
{
    public string idNguyenLieu;    // Tên (VD: Duong)
    public GameObject assetInBowl; // Prefab đống đường

    // Dùng cái này nếu Hũ là UI Button
    public void OnClickButton()
    {
        XuLyClick();
    }

    // Dùng cái này nếu Hũ là vật thể trong game (có Collider)
    void OnMouseDown()
    {
        XuLyClick();
    }

    void XuLyClick()
    {
        if (MixingManager.Instance != null)
            MixingManager.Instance.ThemVaoTo(idNguyenLieu, assetInBowl);
        else
            Debug.LogError("Lỗi: Không tìm thấy MixingManager!");
    }
}