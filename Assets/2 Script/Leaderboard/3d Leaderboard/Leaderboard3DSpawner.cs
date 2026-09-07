// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Leaderboard3DSpawner : MonoBehaviour
// {
//     public GameObject entryPrefab;    // Prefab untuk entri leaderboard
//     public Transform spawnRoot;       // Tempat spawn di dunia 3D
//     public float verticalSpacing = 1.5f; // Jarak vertikal antar entri
//     public float spawnDelay = 0.3f;   // Delay antar spawn entri

//     void Start()
//     {
//         Debug.Log("💡 Memulai spawn leaderboard 3D...");
//         StartCoroutine(SpawnEntriesWithDelay());
//     }

//     IEnumerator SpawnEntriesWithDelay()
//     {
//         // Mengambil top 5 leaderboard dari LocalLeaderboardManager
//         List<LocalLeaderboardManager.LeaderboardEntry> top5 = LocalLeaderboardManager.Instance.GetTop5();

//         Debug.Log($"📄 Total data top 5: {top5.Count}");

//         if (top5.Count == 0)
//         {
//             Debug.LogWarning("⚠ Tidak ada data leaderboard yang ditemukan.");
//             yield break;
//         }

//         for (int i = 0; i < top5.Count; i++)
//         {
//             Debug.Log($"✅ Memproses entry ke-{i + 1}: {top5[i].playerName} - Waktu: {top5[i].time}");

//             GameObject entryObj = Instantiate(entryPrefab, spawnRoot);
//             entryObj.transform.localPosition = new Vector3(0, -i * verticalSpacing, 0);  // Menentukan posisi spawn entri

//             var entryComponent = entryObj.GetComponent<LeaderboardEntryComponent>();
//             if (entryComponent != null)
//             {
//                 entryComponent.SetData(
//                     top5[i].playerName,
//                     top5[i].time,
//                     i + 1
//                 );
//                 Debug.Log($"📝 Entri ke-{i + 1} telah diset: Nama: {top5[i].playerName}, Waktu: {top5[i].time}, Posisi: {i + 1}");
//             }
//             else
//             {
//                 Debug.LogError("❌ Component LeaderboardEntryComponent tidak ditemukan di prefab!");
//             }

//             // Animasi pembesaran entri
//             StartCoroutine(AnimateScale(entryObj.transform));

//             yield return new WaitForSeconds(spawnDelay);
//         }
//     }

//     IEnumerator AnimateScale(Transform target)
//     {
//         float duration = 0.3f;
//         float timer = 0f;
//         Vector3 startScale = Vector3.zero;
//         Vector3 endScale = Vector3.one;

//         target.localScale = startScale;

//         while (timer < duration)
//         {
//             timer += Time.deltaTime;
//             float t = timer / duration;
//             target.localScale = Vector3.Lerp(startScale, endScale, t);
//             yield return null;
//         }

//         target.localScale = endScale;
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard3DSpawner : MonoBehaviour
{
    public GameObject entryPrefab;    // Prefab untuk entri leaderboard
    public Transform spawnRoot;       // Tempat spawn di dunia 3D
    public float verticalSpacing = 1.5f; // Jarak vertikal antar entri
    public float spawnDelay = 0.3f;   // Delay antar spawn entri
    public int maxEntries = 10;       // Maksimal jumlah entri yang dapat tampil

    private List<GameObject> entryObjects = new List<GameObject>();  // List untuk menyimpan entri yang sudah di-spawn
    private bool hasNewData = false;   // Untuk memeriksa apakah ada data baru

    void Start()
    {
        Debug.Log("💡 Memulai spawn leaderboard 3D...");
        StartCoroutine(SpawnEntriesWithDelay());
    }

    IEnumerator SpawnEntriesWithDelay()
    {
        while (true)
        {
            // Mengambil top 5 leaderboard dari LocalLeaderboardManager
            LocalLeaderboardManager.Instance.ReloadEntries(); // ⬅️ Refresh data
            List<LocalLeaderboardManager.LeaderboardEntry> top5 = LocalLeaderboardManager.Instance.GetTop5();

            Debug.Log($"📄 Total data top 5: {top5.Count}");

            if (top5.Count == 0)
            {
                Debug.LogWarning("⚠ Tidak ada data leaderboard yang ditemukan.");
                yield break;
            }

            // Periksa apakah ada perubahan data dibandingkan dengan sebelumnya
            if (IsDataChanged(top5))
            {
                // Hapus entri leaderboard lama jika sudah melebihi maxEntries
                while (entryObjects.Count > maxEntries)
                {
                    Destroy(entryObjects[0]);  // Hapus entri pertama (terlama)
                    entryObjects.RemoveAt(0);   // Hapus dari list
                }

                // Spawn entri baru untuk 5 entri leaderboard teratas
                for (int i = 0; i < top5.Count; i++)
                {
                    GameObject entryObj;

                    // Jika entri sudah ada, gunakan entri yang ada
                    if (entryObjects.Count > i)
                    {
                        entryObj = entryObjects[i];
                        entryObj.transform.localPosition = new Vector3(0, -i * verticalSpacing, 0);  // Update posisi
                    }
                    else
                    {
                        // Jika entri belum ada, buat entri baru
                        entryObj = Instantiate(entryPrefab, spawnRoot);
                        entryObj.transform.localPosition = new Vector3(0, -i * verticalSpacing, 0);  // Set posisi spawn

                        entryObjects.Add(entryObj);  // Tambahkan ke list entri
                    }

                    // Update data entri leaderboard
                    var entryComponent = entryObj.GetComponent<LeaderboardEntryComponent>();
                    if (entryComponent != null)
                    {
                        entryComponent.SetData(
                            top5[i].playerName,
                            top5[i].time,
                            i + 1
                        );
                        Debug.Log($"📝 Entri ke-{i + 1} telah diset: Nama: {top5[i].playerName}, Waktu: {top5[i].time}, Posisi: {i + 1}");
                    }
                    else
                    {
                        Debug.LogError("❌ Component LeaderboardEntryComponent tidak ditemukan di prefab!");
                    }

                    // Animasi pembesaran entri hanya jika data baru
                    if (hasNewData)
                    {
                        StartCoroutine(AnimateScale(entryObj.transform));
                    }

                    yield return new WaitForSeconds(spawnDelay);
                }

                hasNewData = false;  // Reset indikator animasi
            }
            else
            {
                Debug.Log("📄 Tidak ada perubahan data leaderboard.");
            }

            yield return new WaitForSeconds(60f); // Memeriksa pembaruan leaderboard setiap 5 detik
        }
    }

    // Memeriksa apakah data baru berbeda dari yang lama
    private bool IsDataChanged(List<LocalLeaderboardManager.LeaderboardEntry> newData)
    {
        // Periksa apakah jumlah entri berbeda
        if (entryObjects.Count != newData.Count)
        {
            hasNewData = true;
            return true;  // Jika jumlah entri berbeda, berarti ada perubahan
        }

        for (int i = 0; i < newData.Count; i++)
        {
            // Mengonversi timeText.text (format MM:SS) menjadi detik
            float existingTime = 0f;
            bool isValidTime = TryParseTime(entryObjects[i].GetComponent<LeaderboardEntryComponent>().timeText.text, out existingTime);

            if (!isValidTime)
            {
                Debug.LogWarning($"❌ Format waktu tidak valid: {entryObjects[i].GetComponent<LeaderboardEntryComponent>().timeText.text}");
                existingTime = 0f;  // Jika format tidak valid, anggap waktu sebagai 0
            }

            // Membandingkan nama dan waktu dalam detik
            if (newData[i].playerName != entryObjects[i].GetComponent<LeaderboardEntryComponent>().namaText.text || 
                existingTime != newData[i].time)
            {
                hasNewData = true;
                return true;  // Jika ada perbedaan data, berarti ada perubahan
            }
        }

        hasNewData = false;
        return false;  // Tidak ada perubahan data
    }

    // Fungsi untuk mengonversi waktu dalam format MM:SS menjadi detik
    private bool TryParseTime(string timeString, out float timeInSeconds)
    {
        timeInSeconds = 0f;

        // Coba untuk memparsing waktu dalam format MM:SS
        string[] timeParts = timeString.Split(':');
        if (timeParts.Length == 2) // Format valid MM:SS
        {
            float minutes;
            float seconds;

            if (float.TryParse(timeParts[0], out minutes) && float.TryParse(timeParts[1], out seconds))
            {
                timeInSeconds = minutes * 60 + seconds;  // Konversi ke detik
                return true;
            }
        }

        return false;
    }


    IEnumerator AnimateScale(Transform target)
    {
        float duration = 0.3f;
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        target.localScale = startScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale;
    }



}
