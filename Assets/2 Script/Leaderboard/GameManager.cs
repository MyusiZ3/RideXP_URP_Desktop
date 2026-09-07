using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Tambahan untuk akses tombol

using UnityEngine.Events;
using TMPro; // Untuk TextMeshPro

public class GameManager : MonoBehaviour
{
    [Header("Tag Settings")]
    public string playerTag = "Player";  // Tag yang akan dicocokkan dengan objek yang memasuki trigger

    [Header("UI Settings")]
    public Button saveButton; // Tombol untuk menyimpan skor

    [Header("Status Message Settings")]
    public TMP_Text statusMessageText; // Untuk pesan error/sukses
    public float messageDisplayDuration = 2f; // Lama tampilan pesan dalam detik
    public GameObject leaderboardOverlay; // Overlay leaderboard
    public float fadeDuration = 0.5f;
    public float displayTime = 2f;
    [Header("Cinematic Sequence")]
    [SerializeField] private SimpleCinematicSequence cinematicSequence;

    public TMP_Text timeText;
    public GameObject overlay;
    public TMP_Text overlayTimeText; 
    public TMP_InputField playerNameInput;
    public UnityEvent OnFinishEvent;

    [Header("Map Settings")]
    public string mapName = "DefaultMap";  // Nama Map, diisi manual di Inspector

    private float elapsedTime; // Waktu yang telah berlalu
    private float finalTime; // waktu saat finish

    private bool isTimerRunning; // Apakah timer sedang berjalan
    public Rigidbody playerRigidbody; // Rigidbody untuk Player

    private bool isGameOver = false; // Flag untuk memeriksa apakah game sudah selesai
    private LeaderboardManager leaderboardManager; // Referensi ke LeaderboardManager

// Untuk menampilkan pesan status
    private Coroutine messageCoroutine;


    private void ShowStatusMessage(string message)
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(FadeMessageRoutine(message));
    }

    private IEnumerator FadeMessageRoutine(string message)
    {
        statusMessageText.text = message;
        statusMessageText.alpha = 0f;

        // Fade In
        float t = 0;
        while (t < fadeDuration)
        {
            statusMessageText.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        statusMessageText.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0;
        while (t < fadeDuration)
        {
            statusMessageText.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        statusMessageText.alpha = 0f;
        statusMessageText.text = "";
    }



    void Start()
    {
        leaderboardManager = FindObjectOfType<LeaderboardManager>();
        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardManager tidak ditemukan di scene!");
        }

        // Tambahkan listener untuk validasi input nama
        playerNameInput.onValueChanged.AddListener(delegate { ValidateNameInput(); });

        // Nonaktifkan tombol save di awal
        saveButton.interactable = false;

        elapsedTime = 0f;
        // isTimerRunning = true;
        isTimerRunning = false; // Timer belum mulai sampai countdown selesai
        overlay.SetActive(false);
    }
    public void StartRaceTimer()
    {
        isTimerRunning = true;
    }



    void Update()
    {
        if (isGameOver)
            return; // Jika game sudah selesai, tidak perlu update lagi

        if (isTimerRunning)
        {
            // Tambahkan waktu yang berlalu setiap frame
            elapsedTime += Time.deltaTime;

            // Tampilkan waktu pada UI utama (misalnya, dalam format detik)
            float minutes = Mathf.FloorToInt(elapsedTime / 60);
            float seconds = Mathf.FloorToInt(elapsedTime % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Pastikan player tidak menembus collider finish area
        if (playerRigidbody.linearVelocity.magnitude > 10f) // Kecepatan lebih dari 10 m/s
        {
            playerRigidbody.linearVelocity = playerRigidbody.linearVelocity.normalized * 10f; // Batasi kecepatan
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag(playerTag))
    //     {
    //         isTimerRunning = false;

    //         // Simpan waktu final
    //         finalTime = elapsedTime;

    //         float minutes = Mathf.FloorToInt(finalTime / 60);
    //         float seconds = Mathf.FloorToInt(finalTime % 60);
    //         overlayTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    //         overlay.SetActive(true);
    //         OnFinishEvent.Invoke();
    //     }
    // }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isTimerRunning = false;
            finalTime = elapsedTime;

            float minutes = Mathf.FloorToInt(finalTime / 60);
            float seconds = Mathf.FloorToInt(finalTime % 60);
            overlayTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            overlay.SetActive(true);
            OnFinishEvent.Invoke();

            // 🎥 Mainkan cinematic sequence versi simple
            if (cinematicSequence != null)
            {
                cinematicSequence.PlaySequence();
            }
        }
    }


    // Fungsi untuk memvalidasi input nama pemain
    private void ValidateNameInput()
    {
        string playerName = playerNameInput.text.Trim();

        if (!string.IsNullOrEmpty(playerName) && playerName.Length <= 12)
        {
            saveButton.interactable = true;
        }
        else
        {
            saveButton.interactable = false;
        }
    }


    private void SendToLeaderboard(float minutes, float seconds)
    {
        if (playerNameInput == null)
        {
            Debug.LogError("playerNameInput tidak terhubung!");
            statusMessageText.text = "Name input is not assigned!";
            return;
        }

        string playerName = playerNameInput.text.Trim();

        // Validasi nama
        if (string.IsNullOrEmpty(playerName))
        {
            ShowStatusMessage("Please enter your name!");
            return;
        }

        if (playerName.Length > 12)
        {
            ShowStatusMessage("Name cannot be longer than 12 characters!");
            return;
        }


        if (leaderboardManager == null)
        {
            Debug.LogError("LeaderboardManager tidak ditemukan!");
            statusMessageText.text = "Leaderboard system not found!";
            return;
        }

        float timeInSeconds = minutes * 60 + seconds;
        
        PlayerPrefs.SetString("LastPlayerName", playerName); // simpan nama pemain terakhir


        leaderboardManager.SaveScore(playerName, timeInSeconds, mapName);

        ShowStatusMessage("Score saved successfully!");

        // Aktifkan overlay leaderboard (jika ada)
        if (leaderboardOverlay != null)
        {
            leaderboardOverlay.SetActive(true);
        }

        Debug.Log("Data berhasil disimpan: " + playerName + " - " + timeInSeconds.ToString("F2"));
    }




    // Fungsi untuk menangani aksi tombol Save
    public void OnSaveScoreButtonClick()
    {
        float minutes = Mathf.FloorToInt(finalTime / 60);
        float seconds = Mathf.FloorToInt(finalTime % 60);
        SendToLeaderboard(minutes, seconds);
    }

}
