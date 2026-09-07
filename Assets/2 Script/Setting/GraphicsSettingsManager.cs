// GraphicsSettingsManager.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance { get; private set; }

    // --- Variabel untuk Menyimpan Pengaturan Grafis ---
    public bool isPostProcessingEnabled = true; // Global toggle untuk semua post processing
    public MotionBlurQuality motionBlurQuality = MotionBlurQuality.Medium; // Default Medium

    // --- Variabel untuk Menyimpan Pengaturan Kontrol Sepeda ---
    public float steerSensitivity = 15f;
    public bool instantSteering = false;
    public bool enableDoubleJump = false;

    // Enum untuk kualitas Motion Blur
    public enum MotionBlurQuality
    {
        Off,    // Ini otomatis bernilai 0
        Low,    // Ini otomatis bernilai 1
        Medium, // Ini otomatis bernilai 2
        High    // Ini otomatis bernilai 3
    }

    void Awake()
    {
        // Implementasi Singleton Pattern yang lebih robust untuk self-instantiation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Penting: Agar object ini tidak hancur saat ganti scene
            Debug.Log("[GraphicsSettingsManager] Instance created and persistent.");
        }
        else if (Instance != this) // Jika sudah ada instance lain, hancurkan yang ini
        {
            Debug.LogWarning("[GraphicsSettingsManager] Duplicate instance found, destroying this one.");
            Destroy(gameObject);
            return; // Penting untuk keluar agar tidak melanjutkan inisialisasi ganda
        }

        // Muat pengaturan saat Awake, hanya jika ini adalah instance yang aktif
        if (Instance == this)
        {
            LoadSettings();
        }
    }

    // Metode Statis untuk Memastikan Instance Tersedia (Jika diperlukan dari script lain yang mungkin di-Awake duluan)
    public static GraphicsSettingsManager GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[GraphicsSettingsManager] Instance not found, creating a new one dynamically.");
            GameObject managerGO = new GameObject("_GraphicsSettingsManager_RuntimeCreated");
            Instance = managerGO.AddComponent<GraphicsSettingsManager>();
            // DontDestroyOnLoad akan dihandle di Awake() yang baru dipanggil
            // load settings juga akan dihandle di Awake() yang baru dipanggil
        }
        return Instance;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("IsPostProcessingEnabled", isPostProcessingEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MotionBlurQuality", (int)motionBlurQuality);

        // Simpan Kontrol Sepeda
        PlayerPrefs.SetFloat("SteerSensitivity", steerSensitivity);
        PlayerPrefs.SetInt("InstantSteering", instantSteering ? 1 : 0);
        PlayerPrefs.SetInt("EnableDoubleJump", enableDoubleJump ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("[GraphicsSettingsManager] All Settings (Graphics & Bicycle Controls) saved to PlayerPrefs.");

        // Terapkan grafik secara real-time tanpa restart scene
        ApplyGraphicsRealtime();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("IsPostProcessingEnabled"))
        {
            isPostProcessingEnabled = PlayerPrefs.GetInt("IsPostProcessingEnabled") == 1;
            motionBlurQuality = (MotionBlurQuality)PlayerPrefs.GetInt("MotionBlurQuality");
        }
        else
        {
            isPostProcessingEnabled = true;
            motionBlurQuality = MotionBlurQuality.Medium;
        }

        // Load Kontrol Sepeda
        steerSensitivity = PlayerPrefs.GetFloat("SteerSensitivity", 15f);
        instantSteering = PlayerPrefs.GetInt("InstantSteering", 0) == 1;
        enableDoubleJump = PlayerPrefs.GetInt("EnableDoubleJump", 0) == 1;

        Debug.Log("[GraphicsSettingsManager] All Settings loaded from PlayerPrefs.");

        // Terapkan grafik secara real-time
        ApplyGraphicsRealtime();
    }

    public void ApplyGraphicsRealtime()
    {
        // 1. Terapkan toggle Post Processing ke semua kamera URP di scene aktif
        foreach (var cam in Camera.allCameras)
        {
            if (cam != null && cam.TryGetComponent<UniversalAdditionalCameraData>(out var data))
            {
                data.renderPostProcessing = isPostProcessingEnabled;
            }
        }

        // 2. Terapkan Motion Blur & Post Processing ke semua Volume Profile di scene
        Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var vol in volumes)
        {
            if (vol == null) continue;

            vol.enabled = isPostProcessingEnabled;

            if (vol.profile != null && vol.profile.TryGet<MotionBlur>(out var mb))
            {
                if (!isPostProcessingEnabled || motionBlurQuality == MotionBlurQuality.Off)
                {
                    mb.active = false;
                }
                else
                {
                    mb.active = true;
                    switch (motionBlurQuality)
                    {
                        case MotionBlurQuality.Low:
                            mb.intensity.value = 0.3f;
                            break;
                        case MotionBlurQuality.Medium:
                            mb.intensity.value = 0.6f;
                            break;
                        case MotionBlurQuality.High:
                            mb.intensity.value = 1.0f;
                            break;
                    }
                }
            }
        }
        Debug.Log($"[GraphicsSettingsManager] Realtime Graphics Applied: PostProcessing={isPostProcessingEnabled}, MotionBlur={motionBlurQuality}");
    }
}