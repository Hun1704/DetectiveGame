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
        btnComp ??= GetComponent<Button>();
        txtComp ??= GetComponentInChildren<TextMeshProUGUI>();
        imgComp ??= GetComponent<Image>();

        if (btnComp != null)
        {
            btnComp.onClick.RemoveAllListeners();
            btnComp.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.LogError($"MindPalaceItem '{gameObject.name}' thiếu Button!");
        }
    }

    public void SetupData(string noiDung, int id, bool benTrai)
    {
        idSuKien = id;
        isBenTrai = benTrai;

        if (txtComp != null) txtComp.text = noiDung;

        SetHighlight(false);
    }

    private void OnClick()
    {
        if (MindPalaceManager.Instance != null)
            MindPalaceManager.Instance.ChonManhMoi(this);
    }

    public void SetHighlight(bool isActive)
    {
        if (imgComp == null) return;
        imgComp.color = isActive ? Color.green : Color.white;
    }
}
