// using UnityEngine;
// using TMPro;

// public class LeaderboardDisplay : MonoBehaviour
// {
//     public GameObject leaderboardEntryPrefab; // Prefab untuk entry leaderboard
//     public Transform leaderboardParent; // Tempat di mana entri akan ditempatkan
//     private LeaderboardManager leaderboardManager;

//     void Start()
//     {
//         leaderboardManager = FindObjectOfType<LeaderboardManager>();

//         if (leaderboardEntryPrefab == null)
//         {
//             Debug.LogError("LeaderboardEntryPrefab belum diatur!");
//         }

//         if (leaderboardParent == null)
//         {
//             Debug.LogError("LeaderboardParent belum diatur!");
//         }

//         if (leaderboardManager == null)
//         {
//             Debug.LogError("LeaderboardManager tidak ditemukan!");
//         }

//         DisplayLeaderboard();
//     }

//     public void DisplayLeaderboard()
//     {
//         LeaderboardData data = leaderboardManager.LoadLeaderboard();

//         if (data == null || data.entries.Count == 0)
//         {
//             Debug.LogWarning("Tidak ada entri di leaderboard.");
//             return;
//         }

//         foreach (Transform child in leaderboardParent)
//         {
//             Destroy(child.gameObject);
//         }

//         for (int i = 0; i < data.entries.Count; i++)
//         {
//             LeaderboardEntry entry = data.entries[i];
//             GameObject leaderboardItem = Instantiate(leaderboardEntryPrefab, leaderboardParent);
//             LeaderboardEntryUI entryUI = leaderboardItem.GetComponent<LeaderboardEntryUI>();

//             entryUI.SetLeaderboardEntry(i + 1, entry.playerName, entry.time);
//         }
//     }
// }
using UnityEngine;
using TMPro;

public class LeaderboardDisplay : MonoBehaviour
{
    public GameObject leaderboardEntryPrefab;
    public Transform leaderboardParent;
    public TMP_Text yourPositionText; // <== ini untuk tampilkan YOU: #x
    private LeaderboardManager leaderboardManager;

    void Start()
    {
        leaderboardManager = FindObjectOfType<LeaderboardManager>();

        if (leaderboardEntryPrefab == null || leaderboardParent == null || yourPositionText == null)
        {
            Debug.LogError("Assign semua komponen di Inspector !");
            return;
        }

        DisplayLeaderboard();
    }

    public void DisplayLeaderboard()
    {
        LeaderboardData data = leaderboardManager.LoadLeaderboard();
        if (data == null || data.entries.Count == 0)
        {
            Debug.LogWarning("Leaderboard masih kosong.");
            return;
        }

        foreach (Transform child in leaderboardParent)
        {
            Destroy(child.gameObject);
        }

        string lastPlayerName = PlayerPrefs.GetString("LastPlayerName", "");

        int youRank = -1;

        for (int i = 0; i < data.entries.Count; i++)
        {
            LeaderboardEntry entry = data.entries[i];
            GameObject item = Instantiate(leaderboardEntryPrefab, leaderboardParent);
            LeaderboardEntryUI entryUI = item.GetComponent<LeaderboardEntryUI>();

            entryUI.SetLeaderboardEntry(i + 1, entry.playerName, entry.time);

            // Cek apakah ini adalah pemain terakhir
            if (entry.playerName == lastPlayerName)
                youRank = i + 1;
        }

        // Tampilkan rank "YOU"
        if (youRank > 0)
            yourPositionText.text = $"#{youRank}";
        else
            yourPositionText.text = $"#-";
    }
}
