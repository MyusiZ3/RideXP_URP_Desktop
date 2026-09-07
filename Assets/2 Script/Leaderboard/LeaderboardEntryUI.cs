// using TMPro;
// using UnityEngine;

// public class LeaderboardEntryUI : MonoBehaviour
// {
//     public TMP_Text rankText;
//     public TMP_Text nameText;
//     public TMP_Text timeText;

//     public void SetLeaderboardEntry(int rank, string playerName, float time)
//     {
//         rankText.text = $"#{rank}";
//         nameText.text = playerName;
//         timeText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60), Mathf.FloorToInt(time % 60));
//     }
// }

using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text timeText;

    public void SetLeaderboardEntry(int rank, string playerName, float time)
    {
        rankText.text = $"#{rank}";
        nameText.text = playerName;
        timeText.text = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60), Mathf.FloorToInt(time % 60));

        // Optional: Highlight Top 3
        // if (rank == 1)
        //     rankText.color = new Color(1f, 0.84f, 0f); // Emas
        // else if (rank == 2)
        //     rankText.color = Color.gray; // Silver
        // else if (rank == 3)
        //     rankText.color = new Color(0.8f, 0.5f, 0.2f); // Bronze
    }
}
