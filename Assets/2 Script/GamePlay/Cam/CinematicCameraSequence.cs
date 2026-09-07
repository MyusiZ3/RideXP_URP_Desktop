// CinematicCameraSequence.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Untuk Image fade (Panel UI)

// Kelas CinematicShot bisa tetap di sini atau dipisah jika diinginkan
[System.Serializable]
public class CinematicShot
{
    public GameObject cameraObject; // GameObject Kamera yang akan diaktifkan
    public float duration = 3f;     // Durasi shot ini (waktu kamera aktif sebelum pindah)

    [Header("Pengaturan Orbit untuk Shot Ini")]
    public float orbitSpeed = 20f;
    public float orbitDistance = 7f;
    public float orbitHeight = 3f;
    // Anda bisa menambahkan public Vector3 orbitAxisPerShot = Vector3.up; jika tiap shot punya sumbu orbit berbeda
}

public class CinematicCameraSequence : MonoBehaviour
{
    [Tooltip("Daftar kamera dan pengaturannya untuk sequence")]
    public List<CinematicShot> shots = new List<CinematicShot>();
    
    [Tooltip("Kamera utama player yang akan dinonaktifkan selama sequence")]
    public Camera mainCamera;
    
    [Tooltip("AudioSource untuk musik kemenangan (opsional)")]
    public AudioSource winMusicSource;

    [Header("Pengaturan Fade Transisi")]
    [Tooltip("Komponen Image UI (warna hitam) untuk efek fade. Harus menutupi seluruh layar.")]
    public Image fadePanel;
    [Tooltip("Durasi untuk setiap proses fade in atau fade out (detik)")]
    public float fadeTransitionDuration = 0.5f;

    private GameObject currentActiveCinematicCamGO = null; // Menyimpan GameObject kamera sinematik yang sedang aktif

    // Metode publik untuk memulai sequence, dipanggil oleh GameManager
    public void PlaySequence(Transform targetToFollow)
    {
        if (shots == null || shots.Count == 0)
        {
            Debug.LogWarning("[CinematicCameraSequence] Tidak ada shots yang di-assign untuk dimainkan.");
            // Aktifkan kembali kamera utama jika ada, untuk keamanan
            if (mainCamera != null) mainCamera.enabled = true;
            return;
        }
        StartCoroutine(PlayCinematicCoroutine(targetToFollow));
    }

    // Coroutine utama yang mengatur seluruh sequence
    private IEnumerator PlayCinematicCoroutine(Transform target)
    {
        Log("Memulai Cinematic Sequence...");

        // 1. Persiapan Awal
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
            Log("Kamera utama dinonaktifkan.");
        }

        if (winMusicSource != null && !winMusicSource.isPlaying)
        {
            winMusicSource.Play();
            Log("Musik kemenangan diputar.");
        }

        currentActiveCinematicCamGO = null;

        // Jika menggunakan fade panel, pastikan ia siap (hitam penuh) sebelum kamera pertama
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 1f); // Mulai dari hitam
            Log("Fade panel disiapkan (hitam).");
        }

        // 2. Loop Melalui Setiap Shot Kamera Sinematik
        for (int i = 0; i < shots.Count; i++)
        {
            CinematicShot currentShot = shots[i];
            if (currentShot.cameraObject == null)
            {
                Log($"CameraObject pada shot index {i} belum di-assign. Melewati...", true);
                continue;
            }

            Log($"Memulai Shot {i + 1}/{shots.Count}: {currentShot.cameraObject.name}");

            // A. Nonaktifkan kamera sinematik sebelumnya (jika ada)
            //    Fade out untuk kamera sebelumnya sudah terjadi di akhir iterasi loop sebelumnya (atau tidak ada jika ini kamera pertama)
            if (currentActiveCinematicCamGO != null && currentActiveCinematicCamGO != currentShot.cameraObject)
            {
                currentActiveCinematicCamGO.SetActive(false);
                Log($"Kamera sebelumnya ({currentActiveCinematicCamGO.name}) dinonaktifkan.");
            }

            // B. Aktifkan dan setup kamera sinematik saat ini
            currentShot.cameraObject.SetActive(true);
            currentActiveCinematicCamGO = currentShot.cameraObject;
            Log($"Kamera saat ini ({currentActiveCinematicCamGO.name}) diaktifkan.");

            CinematicFinishCamera followOrbitScript = currentActiveCinematicCamGO.GetComponent<CinematicFinishCamera>();
            if (followOrbitScript == null)
            {
                followOrbitScript = currentActiveCinematicCamGO.AddComponent<CinematicFinishCamera>();
                Log($"Komponen CinematicFinishCamera ditambahkan ke {currentActiveCinematicCamGO.name}.");
            }
            // Atur parameter orbit untuk kamera ini
            followOrbitScript.Activate(target, currentShot.orbitDistance, currentShot.orbitHeight, currentShot.orbitSpeed);
            Log($"CinematicFinishCamera di {currentActiveCinematicCamGO.name} diaktifkan dengan target: {target.name}, Speed: {currentShot.orbitSpeed}");


            // C. Fade From Black (Memunculkan view kamera baru)
            if (fadePanel != null)
            {
                Log("Memulai Fade From Black...");
                yield return StartCoroutine(FadeEffect(0f, fadeTransitionDuration)); // Fade dari hitam ke jernih
            }

            // D. Tunggu sesuai durasi shot (ini adalah waktu kamera aktif terlihat)
            Log($"Shot {currentActiveCinematicCamGO.name} akan tayang selama {currentShot.duration} detik.");
            yield return new WaitForSeconds(currentShot.duration);

            // E. Fade To Black sebelum pindah ke kamera berikutnya (jika ini bukan shot terakhir)
            if (fadePanel != null && i < shots.Count - 1) // Hanya fade ke hitam jika ada kamera berikutnya
            {
                Log("Memulai Fade To Black sebelum kamera berikutnya...");
                yield return StartCoroutine(FadeEffect(1f, fadeTransitionDuration)); // Fade ke hitam
            }
        }

        // 3. Setelah semua shot sinematik selesai
        // Fade out shot sinematik terakhir jika belum (jika hanya ada 1 shot, atau ini adalah akhir loop)
        if (fadePanel != null && currentActiveCinematicCamGO != null && shots.Count > 0) // Pastikan ada shot yang dimainkan
        {
             // Jika tidak ada fade out di akhir loop (karena itu shot terakhir), lakukan sekarang
             // Tapi jika sudah fade out di iterasi terakhir (i < shots.Count - 1) tidak terpenuhi, berarti sudah di fade out.
             // Kondisi ini lebih untuk memastikan layar hitam sebelum kembali ke main cam jika belum.
             if (fadePanel.color.a < 0.9f) { // Jika belum sepenuhnya hitam
                Log("Memulai Fade To Black untuk shot sinematik terakhir...");
                yield return StartCoroutine(FadeEffect(1f, fadeTransitionDuration));
             }
        }


        if (currentActiveCinematicCamGO != null)
        {
            currentActiveCinematicCamGO.SetActive(false);
            Log($"Kamera sinematik terakhir ({currentActiveCinematicCamGO.name}) dinonaktifkan.");
        }

        // Aktifkan kembali kamera utama player
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            Log("Kamera utama diaktifkan kembali.");
        }

        // Fade in untuk menampilkan view kamera utama player
        if (fadePanel != null)
        {
            Log("Memulai Fade From Black untuk kamera utama...");
            yield return StartCoroutine(FadeEffect(0f, fadeTransitionDuration));
        }
        
        Log("Cinematic sequence selesai.");
        // Opsional: Nonaktifkan panel fade setelah semuanya selesai
        // if (fadePanel != null) fadePanel.gameObject.SetActive(false);
    }

    // Coroutine untuk efek fade pada fadePanel
    private IEnumerator FadeEffect(float targetAlpha, float duration)
    {
        if (fadePanel == null)
        {
            Log("Fade Panel belum di-assign, efek fade dilewati.", true);
            yield break;
        }

        // Pastikan panel aktif jika kita ingin memunculkannya (fade in dari transparan atau fade out ke visible black)
        if (targetAlpha > fadePanel.color.a || !fadePanel.gameObject.activeSelf) {
             fadePanel.gameObject.SetActive(true);
        }


        float startAlpha = fadePanel.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, targetAlpha);

        // Jika target alpha adalah 0 (sepenuhnya transparan), nonaktifkan GameObject panelnya
        if (targetAlpha == 0f)
        {
            fadePanel.gameObject.SetActive(false);
            Log("Fade panel dinonaktifkan (alpha 0).");
        }
    }
    
    // Helper untuk Debug Log yang bisa di-toggle jika Anda punya variabel global debug
    private void Log(string message, bool isWarning = false)
    {
        // if (!enableGlobalDebug && !isWarning) return; // Contoh jika ada toggle debug
        if (isWarning) Debug.LogWarning($"[CinematicCameraSequence] {message}");
        else Debug.Log($"[CinematicCameraSequence] {message}");
    }
}