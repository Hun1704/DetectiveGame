using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MindPalaceItem : MonoBehaviour
{
    public int idSuKien;
    public bool isBenTrai;

    [Header("Components (Tự động tìm)")]
    [SerializeField] private Button btnComp;
    [SerializeField] private TextMeshProUGUI txtComp;
    [SerializeField] private Image imgComp;

    private void Awake()
    {
        // Tự động tìm component để tránh việc quên kéo thả
        btnComp = GetComponent<Button>();
        txtComp = GetComponentInChildren<TextMeshProUGUI>();
        imgComp = GetComponent<Image>();

        if (btnComp != null)
        {
            btnComp.onClick.RemoveAllListeners();
            btnComp.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogError($"LỖI PREFAB: '{gameObject.name}' thiếu component BUTTON!");
        }
    }

    public void SetupData(string noiDung, int id, bool benTrai)
    {
        idSuKien = id;
        isBenTrai = benTrai;

        if (txtComp != null) txtComp.text = noiDung;

        // Reset trạng thái
        SetHighlight(false);
    }

    private void OnClick()
    {
        Debug.Log($"Đã click vào nút: {gameObject.name} | ID: {idSuKien}"); // Debug kiểm tra click

        if (MindPalaceManager.Instance != null)
        {
            MindPalaceManager.Instance.ChonManhMoi(this);
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (imgComp == null) return;
        // Đổi màu xanh khi chọn, màu trắng khi bỏ chọn
        imgComp.color = isActive ? Color.green : Color.white;
    }
}