using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using SBPScripts;

public class RaceCountdownManager : MonoBehaviour
{
    public GameManager gameManager; // Drag dari Inspector atau pakai FindObjectOfType

    public float prepareDuration = 15f;
    public float countdownDuration = 3f;
    public GameObject blockerCube;
    public AudioSource audioSource;
    public AudioClip combinedBeepClip;

    public TMP_Text prepareText;     // Teks buat 'Waiting: X'
    public TMP_Text countdownText;   // Teks buat '3 2 1 GO'

    public Camera[] regularCams; // Cinematic cams
    public Camera playerCam;     // Kamera FPP player
    public float camSwitchInterval = 5f;
    public GameObject cinematicBarOverlay; // UI Panel berisi 2 black bars (atas bawah)
    public GameObject[] uiToHideDuringCinematic; // Semua UI yang perlu disembunyi pas cinematic
    // Referensi ke Player dan NPC
    [Tooltip("Skrip yang mengontrol gerakan Player (misalnya PlayerController atau BicycleController milik Player)")]
    public MonoBehaviour playerMovementScript; // Anda sudah punya ini
    [Tooltip("Array berisi semua komponen NPCBicycleAIController di scene")]
    public NPCBicycleAIController[] npcControllers; // Anda sudah punya ini

    
    public UnityEvent StartEvent;

    private float camTimer;
    private int currentCamIndex;
    void Awake() // Jika belum ada, tambahkan metode ini. Jika sudah, tambahkan panggilannya.
    {
        // Nonaktifkan gerakan player dan NPC di awal
        SetPlayerAndNPCMovementActive(false); // Panggil metode baru ini
    }

    void Start()
    {
        StartCoroutine(PrepareAndCountdownRoutine());
    }
    IEnumerator FadeText(TMP_Text text, float duration, float holdTime)
    {
        float t = 0;
        text.alpha = 0f;

        // Fade In
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(holdTime);

        // Fade Out
        t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(1, 0, t / duration);
            yield return null;
        }

        text.gameObject.SetActive(false); // Optional
    }
    IEnumerator PunchScale(TMP_Text text, float duration, float intensity)
    {
        Vector3 originalScale = text.transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float scale = 1 + Mathf.Sin(timer / duration * Mathf.PI) * intensity;
            text.transform.localScale = originalScale * scale;
            yield return null;
        }

        text.transform.localScale = originalScale;
    }



    IEnumerator PrepareAndCountdownRoutine()
    {
        blockerCube.SetActive(true);
        countdownText.gameObject.SetActive(true);

        // Aktifkan cinematic bar
        if (cinematicBarOverlay != null)
            cinematicBarOverlay.SetActive(true);

        // Sembunyikan UI lainnya
        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null) ui.SetActive(false);
        }

        currentCamIndex = 0;
        ActivateCam(currentCamIndex);

        // 🔵 Tampilkan teks prepare (waiting)
        prepareText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false); // Sembunyikan dulu

        float elapsed = 0f;
        playerCam.enabled = false;

        while (elapsed < prepareDuration)
        {
            camTimer += Time.deltaTime;
            if (camTimer >= camSwitchInterval)
            {
                camTimer = 0f;
                currentCamIndex = (currentCamIndex + 1) % regularCams.Length;
                ActivateCam(currentCamIndex);
            }

            prepareText.text = $"Get Ready For: {(int)(prepareDuration - elapsed)}";
            elapsed += Time.deltaTime;
            yield return null;
        }


        // Play countdown sound (3, 2, 1, beep)
        audioSource.PlayOneShot(combinedBeepClip);

        // 🔁 Swap UI teks
        // 🔁 Swap UI teks
        prepareText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText.alpha = 1f; // biar langsung muncul

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.gameObject.SetActive(true);

            StartCoroutine(PunchScale(countdownText, 0.5f, 0.25f)); // Zoom efek
            StartCoroutine(FadeText(countdownText, 0.3f, 0.7f));     // Fade in + out

            yield return new WaitForSeconds(1f);
        }

        StartEvent?.Invoke();
        // GO!
        // GO!
        countdownText.text = "GO!";
        countdownText.gameObject.SetActive(true); // Pastikan teks GO terlihat
        StartCoroutine(PunchScale(countdownText, 0.6f, 0.3f));
        StartCoroutine(FadeText(countdownText, 0.4f, 0.8f)); // Durasinya disesuaikan agar tidak hilang terlalu cepat
        
        blockerCube.SetActive(false);

        // === TAMBAHKAN BARIS INI untuk mengaktifkan gerakan ===
        SetPlayerAndNPCMovementActive(true);
        // ======================================================

        StartEvent?.Invoke(); // Panggil event setelah semua siap bergerak

        // ... (sisa kode coroutine Anda untuk mematikan cinematic bar, menampilkan UI, mengaktifkan playerCam, dll.) ...

        if (cinematicBarOverlay != null)
            cinematicBarOverlay.SetActive(false);

        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null) ui.SetActive(true);
        }

        DeactivateAllCams();
        if(playerCam != null) playerCam.enabled = true;


        if (gameManager != null)
        {
            gameManager.StartRaceTimer();
        }
        else
        {
            Debug.LogWarning("GameManager belum terpasang di RaceCountdownManager!");
        }

        yield return new WaitForSeconds(1f); // Waktu agar teks "GO!" bisa dibaca
        countdownText.gameObject.SetActive(false);
    }
    // --- METODE YANG DIMODIFIKASI UNTUK MENGONTROL GERAKAN, KINEMATIC, DAN ANIMASI ---
    void SetPlayerAndNPCMovementActive(bool canMove)
    {
        // Atur Player
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = canMove; // Nonaktifkan/Aktifkan skrip kontrol player

            Rigidbody playerRb = playerMovementScript.GetComponent<Rigidbody>();
            // Coba dapatkan BicycleController dari GameObject yang sama dengan playerMovementScript
            BicycleController playerBikeController = playerMovementScript.GetComponent<BicycleController>();
            // Coba dapatkan Animator dari GameObject yang sama
            Animator playerAnimator = playerMovementScript.GetComponent<Animator>();


            if (playerRb != null)
            {
                if (!canMove) {
                    if (!playerRb.isKinematic)
                    {
                        playerRb.linearVelocity = Vector3.zero;
                        playerRb.angularVelocity = Vector3.zero;
                    }
                }
                playerRb.isKinematic = !canMove;
            }

            if (!canMove) // Aksi tambahan saat MENGHENTIKAN player
            {
                if (playerBikeController != null)
                {
                    playerBikeController.bunnyHopInputState = 0; // Reset state lompat
                    // Jika BicycleController menyimpan input axis secara internal dan tidak otomatis reset saat disabled:
                    playerBikeController.customAccelerationAxis = 0f;
                    playerBikeController.rawCustomAccelerationAxis = 0f;
                    playerBikeController.customSteerAxis = 0f;
                    playerBikeController.customLeanAxis = 0f;
                    // playerBikeController.sprint = false; // Jika 'sprint' adalah variabel publik di BicycleController
                }
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Speed", 0f); // Asumsi parameter "Speed" ada
                    playerAnimator.Rebind(); // Reset state animator
                    playerAnimator.Update(0f); // Paksa update dengan state baru
                }
            }
            Debug.Log("Player control " + (canMove ? "ENABLED" : "DISABLED") + ", Kinematic: " + !canMove);
        }
        else
        {
            Debug.LogWarning("PlayerMovementScript belum di-assign di RaceCountdownManager.");
        }

        // Atur semua NPC
        if (npcControllers != null)
        {
            foreach (NPCBicycleAIController npcAI in npcControllers)
            {
                if (npcAI != null)
                {
                    npcAI.enabled = canMove; // Nonaktifkan/Aktifkan skrip AI NPC

                    Rigidbody npcRb = npcAI.GetComponent<Rigidbody>();
                    BicycleController npcBikeController = npcAI.GetComponent<BicycleController>();
                    Animator npcAnimator = npcAI.GetComponent<Animator>(); // Atau npcAI.GetComponentInChildren<Animator>();

                    if (npcRb != null)
                    {
                        if (!canMove) {
                            if (!npcRb.isKinematic)
                            {
                                npcRb.linearVelocity = Vector3.zero;
                                npcRb.angularVelocity = Vector3.zero;
                            }
                        }
                        npcRb.isKinematic = !canMove;
                    }

                    if (!canMove) // Aksi tambahan saat MENGHENTIKAN NPC
                    {
                        if (npcBikeController != null && npcBikeController.isAIControlled)
                        {
                            npcBikeController.pedalInput = 0f;  // Input AI untuk pedal
                            npcBikeController.steerInput = 0f;  // Input AI untuk stir
                            npcBikeController.bunnyHopInputState = 0; // Reset state lompat NPC juga
                            // Reset juga axis internal di BicycleController yang mungkin masih menyimpan nilai dari AI
                            npcBikeController.customAccelerationAxis = 0f;
                            npcBikeController.rawCustomAccelerationAxis = 0f;
                            npcBikeController.customSteerAxis = 0f;
                        }
                        if (npcAnimator != null)
                        {
                            npcAnimator.SetFloat("Speed", 0f); // Asumsi parameter "Speed" ada
                            npcAnimator.Rebind(); // Reset state animator
                            npcAnimator.Update(0f); // Paksa update dengan state baru
                        }
                    }
                }
            }
            Debug.Log("All NPC AI " + (canMove ? "ENABLED" : "DISABLED") + ", Kinematic: " + !canMove);
        }
        else
        {
            Debug.LogWarning("NpcControllers array belum di-assign atau kosong di RaceCountdownManager.");
        }
    }
    // ------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------


    void ActivateCam(int index)
    {
        for (int i = 0; i < regularCams.Length; i++)
        {
            regularCams[i].enabled = (i == index);
        }
    }

    void DeactivateAllCams()
    {
        foreach (Camera cam in regularCams)
        {
            cam.enabled = false;
        }
    }
}

// using System.Collections;
// using UnityEngine;
// using TMPro;
// using UnityEngine.Events;

// public class RaceCountdownManager : MonoBehaviour
// {
//     public GameManager gameManager; // Drag dari Inspector atau pakai FindObjectOfType

//     public float prepareDuration = 15f;
//     public float countdownDuration = 3f;
//     public GameObject blockerCube;
//     public AudioSource audioSource;
//     public AudioClip combinedBeepClip;

//     public TMP_Text prepareText;     // Teks buat 'Waiting: X'
//     public TMP_Text countdownText;   // Teks buat '3 2 1 GO'

//     public Camera[] regularCams; // Cinematic cams
//     public Camera playerCam;     // Kamera FPP player
//     public float camSwitchInterval = 5f;
//     public GameObject cinematicBarOverlay; // UI Panel berisi 2 black bars (atas bawah)
//     public GameObject[] uiToHideDuringCinematic; // Semua UI yang perlu disembunyi pas cinematic
//     public UnityEvent StartEvent;

//     private float camTimer;
//     private int currentCamIndex;

//     void Start()
//     {
//         StartCoroutine(PrepareAndCountdownRoutine());
//     }
//     IEnumerator FadeText(TMP_Text text, float duration, float holdTime)
//     {
//         float t = 0;
//         text.alpha = 0f;

//         // Fade In
//         while (t < duration)
//         {
//             t += Time.deltaTime;
//             text.alpha = Mathf.Lerp(0, 1, t / duration);
//             yield return null;
//         }

//         // Hold
//         yield return new WaitForSeconds(holdTime);

//         // Fade Out
//         t = 0;
//         while (t < duration)
//         {
//             t += Time.deltaTime;
//             text.alpha = Mathf.Lerp(1, 0, t / duration);
//             yield return null;
//         }

//         text.gameObject.SetActive(false); // Optional
//     }
//     IEnumerator PunchScale(TMP_Text text, float duration, float intensity)
//     {
//         Vector3 originalScale = text.transform.localScale;
//         float timer = 0f;

//         while (timer < duration)
//         {
//             timer += Time.deltaTime;
//             float scale = 1 + Mathf.Sin(timer / duration * Mathf.PI) * intensity;
//             text.transform.localScale = originalScale * scale;
//             yield return null;
//         }

//         text.transform.localScale = originalScale;
//     }



//     IEnumerator PrepareAndCountdownRoutine()
//     {
//         blockerCube.SetActive(true);
//         countdownText.gameObject.SetActive(true);

//         // Aktifkan cinematic bar
//         if (cinematicBarOverlay != null)
//             cinematicBarOverlay.SetActive(true);

//         // Sembunyikan UI lainnya
//         foreach (var ui in uiToHideDuringCinematic)
//         {
//             if (ui != null) ui.SetActive(false);
//         }

//         currentCamIndex = 0;
//         ActivateCam(currentCamIndex);

//         // 🔵 Tampilkan teks prepare (waiting)
//         prepareText.gameObject.SetActive(true);
//         countdownText.gameObject.SetActive(false); // Sembunyikan dulu

//         float elapsed = 0f;
//         playerCam.enabled = false;

//         while (elapsed < prepareDuration)
//         {
//             camTimer += Time.deltaTime;
//             if (camTimer >= camSwitchInterval)
//             {
//                 camTimer = 0f;
//                 currentCamIndex = (currentCamIndex + 1) % regularCams.Length;
//                 ActivateCam(currentCamIndex);
//             }

//             prepareText.text = $"Get Ready For: {(int)(prepareDuration - elapsed)}";
//             elapsed += Time.deltaTime;
//             yield return null;
//         }


//         // Play countdown sound (3, 2, 1, beep)
//         audioSource.PlayOneShot(combinedBeepClip);

//         // 🔁 Swap UI teks
//         // 🔁 Swap UI teks
//         prepareText.gameObject.SetActive(false);
//         countdownText.gameObject.SetActive(true);
//         countdownText.alpha = 1f; // biar langsung muncul

//         for (int i = (int)countdownDuration; i > 0; i--)
//         {
//             countdownText.text = i.ToString();
//             countdownText.gameObject.SetActive(true);

//             StartCoroutine(PunchScale(countdownText, 0.5f, 0.25f)); // Zoom efek
//             StartCoroutine(FadeText(countdownText, 0.3f, 0.7f));     // Fade in + out

//             yield return new WaitForSeconds(1f);
//         }

//         StartEvent?.Invoke();
//         // GO!
//         countdownText.text = "GO!";
//         countdownText.gameObject.SetActive(true);
//         StartCoroutine(PunchScale(countdownText, 0.6f, 0.3f));
//         StartCoroutine(FadeText(countdownText, 0.4f, 0.8f));
//         blockerCube.SetActive(false);


//         // Matikan cinematic bar
//         if (cinematicBarOverlay != null)
//             cinematicBarOverlay.SetActive(false);

//         // Tampilkan kembali UI yang disembunyikan
//         foreach (var ui in uiToHideDuringCinematic)
//         {
//             if (ui != null) ui.SetActive(true);
//         }

//         // Deactivate all cinematic cams
//         DeactivateAllCams();

//         // Activate player FPP camera
//         playerCam.enabled = true;
//         // Start race timer dari GameManager
//         if (gameManager != null)
//         {
//             gameManager.StartRaceTimer();
//         }
//         else
//         {
//             Debug.LogWarning("GameManager belum terpasang di RaceCountdownManager!");
//         }

//         yield return new WaitForSeconds(1f);
//         countdownText.gameObject.SetActive(false);
//     }


//     void ActivateCam(int index)
//     {
//         for (int i = 0; i < regularCams.Length; i++)
//         {
//             regularCams[i].enabled = (i == index);
//         }
//     }

//     void DeactivateAllCams()
//     {
//         foreach (Camera cam in regularCams)
//         {
//             cam.enabled = false;
//         }
//     }
// }
