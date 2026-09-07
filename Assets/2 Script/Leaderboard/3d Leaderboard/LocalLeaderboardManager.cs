
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class LocalLeaderboardManager : MonoBehaviour
{
    public static LocalLeaderboardManager Instance;
    private string filePath;

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;  // Nama pemain
        public float time;         // Waktu
        public string mapName;     // Nama peta
        public string date;        // Tanggal
    }

    // Wrapper untuk menangani array 'entries' di dalam JSON
    [System.Serializable]
    private class LeaderboardWrapper
    {
        public List<LeaderboardEntry> entries;
    }

    private List<LeaderboardEntry> allEntries = new();
    private List<LeaderboardEntry> previousEntries = new(); // Menyimpan entri sebelumnya untuk perbandingan

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Application.persistentDataPath + "/leaderboard.json";  // Path file JSON
            LoadEntries();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void LoadEntries()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);  // Membaca file JSON
            Debug.Log($"📄 File JSON ditemukan di {filePath}. Membaca data...");
            LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);  // Parse JSON ke dalam Wrapper
            List<LeaderboardEntry> currentEntries = wrapper.entries;  // Ambil list entri

            // Cek apakah data baru berbeda dengan data sebelumnya
            if (!AreEntriesEqual(previousEntries, currentEntries))
            {
                allEntries = currentEntries;  // Update entri leaderboard
                previousEntries = new List<LeaderboardEntry>(currentEntries);  // Simpan entri sebelumnya untuk perbandingan
                Debug.Log($"📄 JSON berhasil diparsing, jumlah entri: {allEntries.Count}");
            }
            else
            {
                Debug.Log("📄 Tidak ada perubahan dalam data leaderboard.");
            }
        }
        else
        {
            Debug.LogError("❌ Leaderboard file tidak ditemukan: " + filePath);
        }
    }

    // Membandingkan data lama dengan data baru
    private bool AreEntriesEqual(List<LeaderboardEntry> oldEntries, List<LeaderboardEntry> newEntries)
    {
        if (oldEntries.Count != newEntries.Count)
        {
            return false;  // Jumlah entri berbeda, berarti ada perubahan
        }

        for (int i = 0; i < oldEntries.Count; i++)
        {
            if (oldEntries[i].playerName != newEntries[i].playerName ||
                oldEntries[i].time != newEntries[i].time ||
                oldEntries[i].mapName != newEntries[i].mapName ||
                oldEntries[i].date != newEntries[i].date)
            {
                return false;  // Ada perbedaan data, berarti ada perubahan
            }
        }

        return true;  // Tidak ada perubahan data
    }
    public void ReloadEntries()
    {
        LoadEntries(); // Refresh data dari file
    }

    // Mendapatkan 5 entri teratas berdasarkan waktu
    public List<LeaderboardEntry> GetTop5()
    {
        Debug.Log("📄 Mengambil 5 teratas dari leaderboard...");
        return allEntries
            .OrderBy(entry => entry.time)  // Mengurutkan berdasarkan waktu (ascending)
            .Take(5)                      // Ambil 5 entri teratas
            .ToList();
    }
    public List<LeaderboardEntry> GetTop50()
    {
        return allEntries
            .OrderBy(entry => entry.time)
            .Take(50)
            .ToList();
    }

}
