using UnityEngine;
using TMPro; // Pastikan ini ada untuk TextMeshPro
using System.Collections; // Pastikan ini ada untuk Coroutine

public class RespawnManager : MonoBehaviour
{
    // UI Timer dan suara countdown
    public TextMeshProUGUI timerText; // Untuk menampilkan countdown respawn
    public AudioClip countdownSound; // Suara saat countdown
    public AudioClip triggerSound;   // Suara saat respawn point diinjak atau tombol respawn ditekan
    private AudioSource audioSource; // Komponen AudioSource di GameObject ini

    [Header("Hardware Settings")]
    public bool enableArduinoHardware = false; // Set to false for Desktop Mode

    // Referensi ke SerialController untuk komunikasi dengan Arduino
    public SerialController serialController;

    // Waktu countdown yang dapat disesuaikan dari Inspector
    [Header("Respawn Settings")]
    public float respawnDelay = 3f; // Delay respawn awal (dapat diubah dari Inspector)
    private bool isRespawning = false; // Flag apakah sedang dalam proses respawn
    private Vector3 lastRespawnPoint; // Posisi respawn terakhir
    private Quaternion lastRespawnRotation;  // Rotasi respawn terakhir

    // Variabel untuk melacak status tombol dari Arduino
    private bool arduinoRespawnButtonState = false;      // Status tombol Arduino saat ini (true = ditekan, false = dilepas)
    private bool prevArduinoRespawnButtonState = false; // Status tombol Arduino di frame sebelumnya

    [Header("UI Debugging")]
    public TextMeshProUGUI debugSerialDataText; // Untuk menampilkan data serial mentah dari Arduino
    public TextMeshProUGUI debugButtonStateText; // Untuk menampilkan status deteksi tombol Arduino (Pressed/Released)
    public TextMeshProUGUI debugRespawnPointText; // Untuk menampilkan status apakah respawn point sudah diset

    // Variabel untuk Text UI Feedback dari Checkpoint
    [Header("Checkpoint UI Feedback")]
    public TextMeshProUGUI checkpointUpdateTextUI; // Referensi ke TMP Text untuk feedback update checkpoint
    public float checkpointFadeDuration = 1.5f;     // Durasi fade out teks checkpoint
    public float checkpointTextDisplayTime = 0.8f;  // Waktu teks tampil sebelum fade


    // Start is called before the first frame update
    void Start()
    {
        // Mendapatkan atau menambahkan komponen AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            // Opsional: atur setting default AudioSource
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        // Sembunyikan timer UI di awal permainan
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI timerText belum di-assign di RespawnManager!");
        }

        // Pastikan SerialController telah diatur di Inspector (hanya jika enableArduinoHardware aktif)
        if (enableArduinoHardware && serialController == null)
        {
            Debug.LogWarning("enableArduinoHardware diaktifkan tetapi SerialController belum di-assign di RespawnManager.");
        }

        // Inisialisasi UI Debugging dengan pesan awal
        if (debugSerialDataText != null) debugSerialDataText.text = enableArduinoHardware ? "Serial Data: Waiting..." : "Serial Data: Disabled (Desktop Mode)";
        if (debugButtonStateText != null) debugButtonStateText.text = "Button State: 0 (Release)";
        // Inisialisasi lastRespawnPoint dengan Vector3.zero agar bisa dicek apakah sudah diset
        lastRespawnPoint = Vector3.zero;
        if (debugRespawnPointText != null) debugRespawnPointText.text = "Respawn Point: Not Set";

        // Sembunyikan checkpoint update text di awal
        if (checkpointUpdateTextUI != null)
        {
            checkpointUpdateTextUI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI checkpointUpdateTextUI belum di-assign di RespawnManager!");
        }

        Debug.Log("RespawnManager siap!");
    }

    // Update is called once per frame
    void Update()
    {
        // --- Cek Input Keyboard 'R' (Respawn Desktop) ---
        // Jika tidak sedang respawning DAN tombol 'R' ditekan DAN respawn point sudah diset
        if (!isRespawning && Input.GetKeyDown(KeyCode.R) && lastRespawnPoint != Vector3.zero)
        {
            Debug.Log("<color=cyan>Respawn triggered by R key.</color>");
            StartRespawn(lastRespawnPoint, lastRespawnRotation); // Mulai countdown respawn
            PlayTriggerSound();
        }

        // --- Update UI Debugging untuk status Respawn Point ---
        if (debugRespawnPointText != null)
        {
            debugRespawnPointText.text = "Respawn Point: " + (lastRespawnPoint != Vector3.zero ? "<color=green>SET</color> (" + lastRespawnPoint.ToString("F1") + ")" : "<color=red>Not Set</color>");
        }

        // --- Baca Data Serial dari Arduino (hanya jika enableArduinoHardware diaktifkan) ---
        if (enableArduinoHardware && serialController != null)
        {
            string message = serialController.ReadSerialMessage();
            if (message != null)
            {
                if (debugSerialDataText != null) debugSerialDataText.text = "Serial Data: " + message;
                ProcessSerialData(message); // Proses data yang diterima
            }
        }

        // --- Update UI Debugging untuk status Tombol Arduino ---
        if (debugButtonStateText != null)
        {
            debugButtonStateText.text = "Button State: " + (arduinoRespawnButtonState ? "<color=lime>1 (Pressed)</color>" : "<color=grey>0 (Released)</color>");
        }

        // --- Cek Input Tombol Arduino untuk Respawn (GetKeyDown ala Arduino) ---
        // Jika tidak sedang respawning
        // DAN tombol Arduino saat ini ditekan (`arduinoRespawnButtonState` true)
        // DAN tombol Arduino di frame sebelumnya TIDAK ditekan (`prevArduinoRespawnButtonState` false) -> Ini mendeteksi hanya saat TEKAN AWAL
        // DAN respawn point sudah diset
        if (!isRespawning && arduinoRespawnButtonState && !prevArduinoRespawnButtonState && lastRespawnPoint != Vector3.zero)
        {
            Debug.Log("<color=lime>Respawn triggered by Arduino button!</color>");
            StartRespawn(lastRespawnPoint, lastRespawnRotation); // Mulai countdown respawn
            PlayTriggerSound(); // Mainkan suara trigger saat tombol Arduino ditekan
        }

        // Simpan state tombol Arduino saat ini untuk perbandingan di frame berikutnya
        prevArduinoRespawnButtonState = arduinoRespawnButtonState;


        // --- Logika Countdown Respawn ---
        if (isRespawning)
        {
            respawnDelay -= Time.deltaTime;  // Kurangi waktu delay dengan waktu frame

            // Update timer di UI, pakai Mathf.Ceil biar angka gak langsung 0 di awal
            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(respawnDelay).ToString();
            }

            // Putar suara countdown tiap detik (saat nilai detik berubah)
            if (Mathf.FloorToInt(respawnDelay) != Mathf.FloorToInt(respawnDelay + Time.deltaTime) && countdownSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(countdownSound);
                }
            }

            // Jika waktu habis, lakukan respawn
            if (respawnDelay <= 0)
            {
                RespawnPlayer();
            }
        }
    }

    // Fungsi untuk memproses data serial yang masuk dari Arduino
    private void ProcessSerialData(string data)
    {
        // Contoh format data yang diharapkan: "POT:XXX, SPD:YYY, BTN1:Z, BTN2:W"
        // Kita hanya tertarik pada bagian "BTN2:"
        string[] parts = data.Split(','); // Pisahkan string berdasarkan koma

        foreach (string part in parts)
        {
            // Cek apakah bagian string dimulai dengan "BTN2:"
            if (part.StartsWith("BTN2:"))
            {
                // Ambil nilai setelah "BTN2:" (misal: "1" atau "0")
                string buttonStateString = part.Substring("BTN2:".Length);
                
                // Konversi string ke boolean: true jika "1", false jika "0"
                arduinoRespawnButtonState = (buttonStateString == "1");
                
                // Debug untuk memastikan parsingnya benar
                // Debug.Log($"Parsed BTN2: {buttonStateString}, arduinoRespawnButtonState: {arduinoRespawnButtonState}");

                break; // Hentikan iterasi setelah menemukan dan memproses BTN2
            }
        }
    }

    // Fungsi public untuk menyimpan titik respawn dan rotasi (dipanggil oleh script RespawnPoint)
    public void SetRespawnPoint(Vector3 respawnPoint, Quaternion respawnRotation)
    {
        lastRespawnPoint = respawnPoint;   // Set posisi respawn terakhir
        lastRespawnRotation = respawnRotation;  // Set rotasi respawn terakhir
        Debug.Log("<color=yellow>Respawn point diset ke: " + lastRespawnPoint.ToString("F2") + "</color>");
    }

    // Fungsi public untuk memulai countdown dan timer respawn
    public void StartRespawn(Vector3 respawnPoint, Quaternion respawnRotation)
    {
        // Pencegahan agar tidak mulai respawn jika sudah dalam proses respawn
        if (isRespawning)
        {
            Debug.LogWarning("RespawnManager: Sudah dalam proses respawn. Permintaan start respawn diabaikan.");
            return;
        }

        lastRespawnPoint = respawnPoint; // Set posisi respawn (walaupun biasanya sudah diset via checkpoint)
        lastRespawnRotation = respawnRotation; // Set rotasi respawn
        isRespawning = true;                // Set flag sedang respawning
        respawnDelay = 3f;                  // Reset countdown ke nilai default
        
        if (timerText != null)
        {
            timerText.gameObject.SetActive(true); // Menampilkan timer UI
        }
        
        Debug.Log("Memulai countdown respawn...");
    }

    // Fungsi private untuk melakukan respawn pemain
    private void RespawnPlayer()
    {
        Debug.Log("<color=orange>Melakukan Respawn ke: " + lastRespawnPoint.ToString("F2") + "</color>");

        // Mencari objek pemain dengan Tag "Player"
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastRespawnPoint;  // Pindahkan posisi pemain
            player.transform.rotation = lastRespawnRotation;  // Set rotasi pemain

            // Reset seluruh Rigidbody komponen & anak objek (roda depan, roda belakang, bodi)
            Rigidbody[] allRbs = player.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody r in allRbs)
            {
                r.linearVelocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }

            // Panggil ResetPhysicsState pada BicycleController jika ada
            SBPScripts.BicycleController controller = player.GetComponent<SBPScripts.BicycleController>();
            if (controller != null)
            {
                controller.ResetPhysicsState();
            }

            Debug.Log("Seluruh Rigidbody velocity dan state pemain berhasil direset ke nol saat respawn.");
        }
        else
        {
            Debug.LogError("Objek 'Player' tidak ditemukan! Pastikan pemain memiliki tag 'Player' di Inspector.");
        }

        // Sembunyikan timer UI setelah respawn selesai
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        
        isRespawning = false; // Reset flag respawn
        Debug.Log("Respawn selesai.");
    }

    // Fungsi public untuk memutar suara trigger (dipanggil oleh script RespawnPoint atau saat tombol respawn ditekan)
    public void PlayTriggerSound()
    {
        if (triggerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(triggerSound);  // Memainkan suara satu kali
            Debug.Log("Memutar suara trigger.");
        }
        else
        {
            Debug.LogWarning("Trigger Sound atau AudioSource belum di-assign pada RespawnManager untuk PlayTriggerSound.");
        }
    }

    // Fungsi untuk menampilkan teks feedback checkpoint dan menonaktifkan objek trigger
    // Fungsi ini dipanggil dari script RespawnPoint
    public void ShowCheckpointTextAndDisableTrigger(GameObject triggerObject)
    {
        if (checkpointUpdateTextUI != null)
        {
            StartCoroutine(FadeOutCheckpointTextAndDisable(triggerObject));
        }
        else
        {
            Debug.LogWarning("checkpointUpdateTextUI belum di-assign di RespawnManager!");
            // Jika teks UI tidak ada, langsung nonaktifkan objek trigger
            if (triggerObject != null)
            {
                triggerObject.SetActive(false);
                Debug.Log($"GameObject checkpoint '{triggerObject.name}' langsung dinonaktifkan.");
            }
        }
    }

    private IEnumerator FadeOutCheckpointTextAndDisable(GameObject objectToDisable)
    {
        if (checkpointUpdateTextUI == null) yield break; // Pastikan teks UI tidak null

        checkpointUpdateTextUI.gameObject.SetActive(true);
        checkpointUpdateTextUI.text = "Checkpoint Updated!"; // Atau teks lain yang kamu mau

        Color originalColor = checkpointUpdateTextUI.color;
        originalColor.a = 1f; // Pastikan Alpha mulai dari 1 (full opacity)
        checkpointUpdateTextUI.color = originalColor;

        yield return new WaitForSeconds(checkpointTextDisplayTime); // Tahan sebentar sebelum fade

        float t = 0f;
        while (t < checkpointFadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / checkpointFadeDuration);
            checkpointUpdateTextUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        checkpointUpdateTextUI.gameObject.SetActive(false); // Sembunyikan teks
        checkpointUpdateTextUI.color = originalColor; // Reset warna untuk pemanggilan berikutnya

        // Setelah semua selesai, baru nonaktifkan GameObject trigger
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
            Debug.Log($"GameObject checkpoint '{objectToDisable.name}' dinonaktifkan setelah fade teks.");
        }
    }
}