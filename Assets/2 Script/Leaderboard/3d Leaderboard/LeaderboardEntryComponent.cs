using TMPro;
using UnityEngine;

public class LeaderboardEntryComponent : MonoBehaviour
{
    public TMP_Text namaText;   // Text component untuk menampilkan nama
    public TMP_Text timeText;   // Text component untuk menampilkan waktu
    public TMP_Text posisiText; // Text component untuk menampilkan posisi

    // Method untuk mengatur data pada entri leaderboard
    public void SetData(string nama, float waktu, int posisi)
    {
        // Debug log untuk melihat data yang diterima
        Debug.Log($"📝 Set data pada entri leaderboard: Nama = {nama}, Waktu = {waktu}, Posisi = {posisi}");

        // Set nama pemain
        namaText.text = nama;

        // Format waktu ke menit:detik (contoh: 11:59)
        int menit = Mathf.FloorToInt(waktu / 60);
        int detik = Mathf.FloorToInt(waktu % 60);
        timeText.text = $"{menit:00}:{detik:00}";  // Format waktu dalam MM:SS

        // Set posisi
        posisiText.text = $"#{posisi}";  // Menampilkan posisi (misalnya: #1, #2, #3, dll.)
    }
}
