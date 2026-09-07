using UnityEngine;
using TMPro;  // Pastikan untuk menggunakan namespace TMP

public class ButtonColorChanger : MonoBehaviour
{
    // Referensi ke TextMeshPro untuk button Maps dan Leaderboard
    public TextMeshProUGUI mapsText;
    public TextMeshProUGUI leaderboardText;

    // Variabel warna untuk custom color yang bisa diubah di Inspector
    public Color defaultColor = Color.white;  // Warna default (Putih)
    public Color selectedColor = Color.red;   // Warna yang dipilih (Merah)

    void Start()
    {
        // Set warna teks awal (Maps berwarna merah pertama kali)
        mapsText.color = selectedColor;  // Maps pertama kali tampil dengan warna merah
        leaderboardText.color = defaultColor;  // Leaderboard pertama kali tampil dengan warna putih
    }

    // Fungsi untuk mengubah warna teks ketika button Maps diklik
    public void OnMapsButtonClicked()
    {
        mapsText.color = selectedColor;  // Ubah teks Maps menjadi merah
        leaderboardText.color = defaultColor;  // Ubah teks Leaderboard menjadi putih
    }

    // Fungsi untuk mengubah warna teks ketika button Leaderboard diklik
    public void OnLeaderboardButtonClicked()
    {
        leaderboardText.color = selectedColor;  // Ubah teks Leaderboard menjadi merah
        mapsText.color = defaultColor;  // Ubah teks Maps menjadi putih
    }
}
