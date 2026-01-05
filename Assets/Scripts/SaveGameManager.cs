using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public int chapter;
    public string saveTime;

    public float playerX;
    public float playerY;

    public List<int> danhSachVatChung;
}

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;

    private string saveFolderPath;

    public List<int> pendingVatChungRestore;

    public HashSet<int> vatChungDaNhat = new HashSet<int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFolderPath = Path.Combine(Application.persistentDataPath, "SaveData");
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== PATH DUY NHẤT =====
    private string GetSavePath(int slot)
    {
        return Path.Combine(saveFolderPath, $"save_slot_{slot}.json");
    }

    // ================= SAVE =================
    public void SaveGame(int slot)
    {
        SaveData data = new SaveData();

        data.sceneName = SceneManager.GetActiveScene().name;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerX = player.transform.position.x;
            data.playerY = player.transform.position.y;
        }

        PauseMenuController pause =
            FindFirstObjectByType<PauseMenuController>();
        data.chapter = pause != null ? pause.currentChapter : 1;

        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        data.danhSachVatChung = new List<int>(vatChungDaNhat);


        File.WriteAllText(GetSavePath(slot),
            JsonUtility.ToJson(data, true));

        Debug.Log("Đã lưu game slot " + slot);
    }

    // ================= LOAD =================
    public void LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(path));

        // 🔥 CHỈ LƯU TẠM
        pendingVatChungRestore = data.danhSachVatChung;
        vatChungDaNhat.Clear();

        foreach (int id in data.danhSachVatChung)
            vatChungDaNhat.Add(id);


        StartCoroutine(LoadSceneAndRestore(data));
    }

    private IEnumerator LoadSceneAndRestore(SaveData data)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(data.sceneName);
        while (!load.isDone)
            yield return null;

        yield return null; // chờ object spawn

        // Restore Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(
                data.playerX,
                data.playerY,
                player.transform.position.z
            );
        }

        // 🔥 RESTORE INVENTORY Ở ĐÂY (CHẮC CHẮN)
        if (InventoryManager.Instance != null && data.danhSachVatChung != null)
        {
            InventoryManager.Instance.RestoreInventoryFromSave(
                data.danhSachVatChung
            );
        }

        Debug.Log("Load xong – đã restore Inventory & chờ MindPalace");
    }


    // ================= DELETE =================
    public void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Đã xóa save slot " + slot);
        }
    }

    // ================= CHECK =================
    public bool HasSave(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public SaveData GetSaveData(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
