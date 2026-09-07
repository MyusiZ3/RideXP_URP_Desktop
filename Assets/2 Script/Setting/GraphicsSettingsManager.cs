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
        PlayerPrefs.Save();
        Debug.Log("[GraphicsSettingsManager] Settings saved to PlayerPrefs.");
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("IsPostProcessingEnabled"))
        {
            isPostProcessingEnabled = PlayerPrefs.GetInt("IsPostProcessingEnabled") == 1;
            motionBlurQuality = (MotionBlurQuality)PlayerPrefs.GetInt("MotionBlurQuality");
            Debug.Log("[GraphicsSettingsManager] Settings loaded from PlayerPrefs.");
        }
        else
        {
            Debug.Log("[GraphicsSettingsManager] No saved settings found, using defaults.");
            isPostProcessingEnabled = true;
            motionBlurQuality = MotionBlurQuality.Medium;
        }
    }
}