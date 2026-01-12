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
    public List<string> suKienDaHoanThanh;
    public bool daHienVatChung = false;
    public bool isNewChapterStart;
}

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;
    public int currentSlot = 1;

    private string saveFolderPath;

    public HashSet<int> vatChungDaNhat = new HashSet<int>();
    public HashSet<string> suKienDaXong = new HashSet<string>();
    public bool daKichHoatHienVatChung = false;

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

        PauseMenuController pause = FindFirstObjectByType<PauseMenuController>();
        data.chapter = pause != null ? pause.currentChapter : 1;

        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        data.danhSachVatChung = new List<int>(vatChungDaNhat);
        data.suKienDaHoanThanh = new List<string>(suKienDaXong);
        data.daHienVatChung = daKichHoatHienVatChung;
        data.isNewChapterStart = false;

        File.WriteAllText(GetSavePath(slot),
            JsonUtility.ToJson(data, true));

        Debug.Log("Đã lưu game slot " + slot);
    }


    // ================= LOAD =================
    public void LoadGame(int slot)
    {
        currentSlot = slot;
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(path));

        // 🔥 CHỈ LƯU TẠM
        vatChungDaNhat.Clear();
        foreach (int id in data.danhSachVatChung)
            vatChungDaNhat.Add(id);

        suKienDaXong.Clear();
        if (data.suKienDaHoanThanh != null)
        {
            foreach (string id in data.suKienDaHoanThanh)
                suKienDaXong.Add(id);
        }

        daKichHoatHienVatChung = data.daHienVatChung;

        StartCoroutine(LoadSceneAndRestore(data));
    }

    public void SaveAsNewChapter(int nextChapter, string nextSceneName)
    {
        SaveData data = new SaveData();

        // 1. Cập nhật thông tin Chapter mới
        data.sceneName = nextSceneName;
        data.chapter = nextChapter;
        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // 2. Đánh dấu đây là khởi đầu màn mới -> Để khi Load KHÔNG set vị trí cũ
        data.isNewChapterStart = true;

        // Reset trạng thái hiện vật chứng (Sang màn mới phải giấu đi chứ không hiện ngay)
        data.daHienVatChung = false;

        // 3. Giữ nguyên Inventory & Sự kiện đã làm
        data.danhSachVatChung = new List<int>(vatChungDaNhat);
        data.suKienDaHoanThanh = new List<string>(suKienDaXong);

        // 4. Ghi đè vào Slot đang chơi hiện tại
        File.WriteAllText(GetSavePath(currentSlot), JsonUtility.ToJson(data, true));
        Debug.Log($"Auto-Save chuyển Chapter: Slot {currentSlot} -> {nextSceneName}");
    }

    private IEnumerator LoadSceneAndRestore(SaveData data)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(data.sceneName);
        while (!load.isDone)
            yield return new WaitForEndOfFrame();

        // Restore Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (data.isNewChapterStart)
            {
                Debug.Log("Load màn mới -> Giữ nguyên vị trí Spawn mặc định");
                SaveGame(currentSlot);
            }
            else
            {
                player.transform.position = new Vector3(
                    data.playerX,
                    data.playerY,
                    player.transform.position.z
                );
            }
        }
        PauseMenuController pause = FindFirstObjectByType<PauseMenuController>();
        if (pause != null)
        {
            pause.currentChapter = data.chapter;
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
    public bool CheckEvent(string id)
    {
        return suKienDaXong.Contains(id);
    }

    public void CompleteEvent(string id)
    {
        if (!suKienDaXong.Contains(id))
            suKienDaXong.Add(id);
    }
}
