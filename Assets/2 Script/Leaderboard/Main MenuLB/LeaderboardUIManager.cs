using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUIManager : MonoBehaviour
{
    public GameObject leaderboardEntryPrefab;
    public Transform leaderboardParent;

    private void Start()
    {
        // Ganti dari LeaderboardManager ke LocalLeaderboardManager
        var leaderboard = LocalLeaderboardManager.Instance;

        if (leaderboard == null)
        {
            Debug.LogError("❌ LocalLeaderboardManager tidak ditemukan!");
            return;
        }

        // Ambil top 50 kalau mau (atau 5 pakai GetTop5)
        List<LocalLeaderboardManager.LeaderboardEntry> topEntries = leaderboard
            .GetTop50(); // ← method ini belum ada, kita bikin di bawah

        foreach (Transform child in leaderboardParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < topEntries.Count; i++)
        {
            var entry = topEntries[i];
            GameObject item = Instantiate(leaderboardEntryPrefab, leaderboardParent);
            var entryUI = item.GetComponent<LeaderboardEntryUI>();

            entryUI.SetLeaderboardEntry(i + 1, entry.playerName, entry.time);
        }
    }
}
