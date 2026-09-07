using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public float time;
    public string mapName;
    public string date;
}

[System.Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager : MonoBehaviour
{
    private string leaderboardFilePath;
    private LeaderboardData leaderboardData;

    void Start()
    {
        leaderboardFilePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");

        if (File.Exists(leaderboardFilePath))
        {
            string json = File.ReadAllText(leaderboardFilePath);
            leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
            Debug.Log("Leaderboard data berhasil dimuat.");
        }
        else
        {
            leaderboardData = new LeaderboardData(); // Inisialisasi data jika file tidak ada
            SaveLeaderboardToFile(); // Membuat file baru jika belum ada
            Debug.LogWarning("File leaderboard.json tidak ditemukan, membuat file baru.");
        }
    }

    public void SaveScore(string playerName, float time, string mapName)
    {
        if (string.IsNullOrEmpty(playerName) || time <= 0 || string.IsNullOrEmpty(mapName))
        {
            Debug.LogError("Data tidak valid. Skor tidak dapat disimpan.");
            return;
        }

        LeaderboardEntry newEntry = new LeaderboardEntry
        {
            playerName = playerName,
            time = time,
            mapName = mapName,
            date = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

        leaderboardData.entries.Add(newEntry);
        leaderboardData.entries.Sort((entry1, entry2) => entry1.time.CompareTo(entry2.time));

        while (leaderboardData.entries.Count > 50)
        {
            leaderboardData.entries.RemoveAt(leaderboardData.entries.Count - 1);
        }


        SaveLeaderboardToFile();
    }
    

    public LeaderboardData LoadLeaderboard()
    {
        return leaderboardData;
    }

    private void SaveLeaderboardToFile()
    {
        string json = JsonUtility.ToJson(leaderboardData, true);
        File.WriteAllText(leaderboardFilePath, json);
        Debug.Log("Leaderboard disimpan di: " + leaderboardFilePath);
    }
}
