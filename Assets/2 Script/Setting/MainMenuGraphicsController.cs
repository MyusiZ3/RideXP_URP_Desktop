// MainMenuGraphicsController.cs
using UnityEngine;
using UnityEngine.UI; // Tetap perlu untuk Toggle
using System.Collections.Generic;
using TMPro; // Untuk TextMeshProUGUI

public class MainMenuGraphicsController : MonoBehaviour
{
    [Header("UI References")]
    public Toggle postProcessingToggle;

    [Header("Bicycle Controls UI References")]
    public Slider steerSensitivitySlider;
    public TextMeshProUGUI steerSensitivityValueText;
    public Toggle instantSteeringToggle;
    public Toggle doubleJumpToggle;

    [Header("Motion Blur Quality Buttons")]
    public Button[] motionBlurButtons; // Array Button untuk Off, Low, Medium, High

    // Warna untuk visual feedback pada button
    public Color activeBgColor = new Color32(0xDF, 0x31, 0x4F, 0xFF); // Warna merah terang #DF314F
    public Color inactiveBgColor = Color.white; // Warna putih padat (non-transparan)
    public Color activeTextColor = Color.white; // Warna putih
    public Color inactiveTextColor = Color.black; // Warna hitam

    void Start()
    {
        // Pastikan GraphicsSettingsManager sudah ada menggunakan GetInstance()
        if (GraphicsSettingsManager.GetInstance() == null) // Panggil GetInstance() di sini
        {
            Debug.LogError("[MainMenuGraphicsController] GraphicsSettingsManager instance still null after GetInstance()! This should not happen.");
            return;
        }

        LoadCurrentSettingsToUI();
        SetupMotionBlurButtonListeners();

        if (postProcessingToggle != null)
        {
            postProcessingToggle.onValueChanged.AddListener(OnPostProcessingToggleChanged);
        }

        if (steerSensitivitySlider != null)
        {
            steerSensitivitySlider.onValueChanged.AddListener(OnSteerSensitivitySliderChanged);
        }
        if (instantSteeringToggle != null)
        {
            instantSteeringToggle.onValueChanged.AddListener(OnInstantSteeringToggleChanged);
        }
        if (doubleJumpToggle != null)
        {
            doubleJumpToggle.onValueChanged.AddListener(OnDoubleJumpToggleChanged);
        }
    }

    void OnDestroy()
    {
        // Hapus Listener saat object dihancurkan
        if (postProcessingToggle != null)
        {
            postProcessingToggle.onValueChanged.RemoveListener(OnPostProcessingToggleChanged);
        }

        // Hapus Listener untuk Button Motion Blur
        if (motionBlurButtons != null)
        {
            for (int i = 0; i < motionBlurButtons.Length; i++)
            {
                if (motionBlurButtons[i] != null)
                {
                    // Pastikan indeks sesuai dengan enum MotionBlurQuality saat remove listener
                    GraphicsSettingsManager.MotionBlurQuality quality = (GraphicsSettingsManager.MotionBlurQuality)i;
                    // Note: Untuk lambda di AddListener, RemoveListener harus sedikit berbeda
                    // tapi untuk kasus ini, Unity biasanya cukup pintar melepasnya saat OnDestroy.
                    // Jika ada masalah, perlu Store action delegate in a field for proper removal.
                    // Untuk kesederhanaan, kita abaikan detail ini dulu.
                    // motionBlurButtons[i].onClick.RemoveListener(() => OnMotionBlurButtonClicked(quality)); // Ini tidak akan bekerja untuk lambda
                }
            }
        }
    }

    // Method untuk setup listener pada setiap button Motion Blur
    void SetupMotionBlurButtonListeners()
    {
        if (motionBlurButtons == null || motionBlurButtons.Length == 0)
        {
            Debug.LogWarning("[MainMenuGraphicsController] Motion Blur Buttons are not assigned!");
            return;
        }

        for (int i = 0; i < motionBlurButtons.Length; i++)
        {
            // Pastikan indeks button sesuai dengan nilai enum MotionBlurQuality
            // Off=0, Low=1, Medium=2, High=3
            GraphicsSettingsManager.MotionBlurQuality quality = (GraphicsSettingsManager.MotionBlurQuality)i;

            if (motionBlurButtons[i] != null)
            {
                // Hapus listener sebelumnya untuk mencegah double-subscription jika Start dipanggil lagi
                motionBlurButtons[i].onClick.RemoveAllListeners();
                
                // Tambahkan listener untuk event onClick
                // Kita gunakan lambda expression untuk mengirim parameter 'quality' saat button diklik
                motionBlurButtons[i].onClick.AddListener(() => OnMotionBlurButtonClicked(quality));
                Debug.Log($"[MainMenuGraphicsController] Added listener to Motion Blur Button: {quality}");
            }
        }
    }

    void LoadCurrentSettingsToUI()
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            if (postProcessingToggle != null)
            {
                postProcessingToggle.isOn = GraphicsSettingsManager.Instance.isPostProcessingEnabled;
            }

            if (steerSensitivitySlider != null)
            {
                steerSensitivitySlider.value = GraphicsSettingsManager.Instance.steerSensitivity;
                if (steerSensitivityValueText != null)
                    steerSensitivityValueText.text = GraphicsSettingsManager.Instance.steerSensitivity.ToString("F0");
            }

            if (instantSteeringToggle != null)
            {
                instantSteeringToggle.isOn = GraphicsSettingsManager.Instance.instantSteering;
            }

            if (doubleJumpToggle != null)
            {
                doubleJumpToggle.isOn = GraphicsSettingsManager.Instance.enableDoubleJump;
            }
            
            // Update interaktivitas Slider Steer Sensitivity berdasarkan Instant Steering Toggle
            UpdateSteerSensitivitySliderInteractability(GraphicsSettingsManager.Instance.instantSteering);

            // Update visual button Motion Blur sesuai setting yang dimuat
            UpdateMotionBlurButtonVisuals(GraphicsSettingsManager.Instance.motionBlurQuality);
            
            // Update interaktivitas button Motion Blur berdasarkan Post Processing Toggle
            UpdateMotionBlurButtonInteractability(GraphicsSettingsManager.Instance.isPostProcessingEnabled);
        }
    }

    public void OnSteerSensitivitySliderChanged(float value)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.steerSensitivity = value;
            if (steerSensitivityValueText != null)
                steerSensitivityValueText.text = value.ToString("F0");
            GraphicsSettingsManager.Instance.SaveSettings();
        }
    }

    public void OnInstantSteeringToggleChanged(bool isOn)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.instantSteering = isOn;
            GraphicsSettingsManager.Instance.SaveSettings();

            UpdateSteerSensitivitySliderInteractability(isOn);
        }
    }

    public void UpdateSteerSensitivitySliderInteractability(bool isInstantSteerActive)
    {
        if (steerSensitivitySlider != null)
        {
            steerSensitivitySlider.interactable = !isInstantSteerActive;

            CanvasGroup group = steerSensitivitySlider.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = steerSensitivitySlider.gameObject.AddComponent<CanvasGroup>();
            }
            group.alpha = isInstantSteerActive ? 0.35f : 1.0f;
        }

        if (steerSensitivityValueText != null)
        {
            steerSensitivityValueText.alpha = isInstantSteerActive ? 0.35f : 1.0f;
        }
    }

    public void OnDoubleJumpToggleChanged(bool isOn)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.enableDoubleJump = isOn;
            GraphicsSettingsManager.Instance.SaveSettings();
        }
    }

    // Dipanggil saat Toggle Post Processing berubah
    public void OnPostProcessingToggleChanged(bool isOn)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.isPostProcessingEnabled = isOn;
            Debug.Log($"[MainMenuGraphicsController] Post Processing set to: {isOn}");
            GraphicsSettingsManager.Instance.SaveSettings(); // Simpan ke PlayerPrefs

            UpdateMotionBlurButtonInteractability(isOn); // Panggil method untuk mengatur interaktivitas button
        }
    }

    // Method: Dipanggil saat salah satu button Motion Blur diklik
    public void OnMotionBlurButtonClicked(GraphicsSettingsManager.MotionBlurQuality selectedQuality)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.motionBlurQuality = selectedQuality;
            Debug.Log($"[MainMenuGraphicsController] Motion Blur Quality selected: {selectedQuality}");
            GraphicsSettingsManager.Instance.SaveSettings(); // Simpan ke PlayerPrefs

            // Update visual button setelah ada yang diklik
            UpdateMotionBlurButtonVisuals(selectedQuality);
        }
    }

    // Method: Mengatur visual (warna) dari button-button Motion Blur
    void UpdateMotionBlurButtonVisuals(GraphicsSettingsManager.MotionBlurQuality currentQuality)
    {
        if (motionBlurButtons == null || motionBlurButtons.Length == 0) return;

        for (int i = 0; i < motionBlurButtons.Length; i++)
        {
            if (motionBlurButtons[i] != null)
            {
                Image buttonBg = motionBlurButtons[i].GetComponent<Image>();
                TextMeshProUGUI buttonText = motionBlurButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                // Cek apakah button ini adalah kualitas yang sedang aktif
                bool isActive = (GraphicsSettingsManager.MotionBlurQuality)i == currentQuality;

                if (buttonBg != null)
                {
                    buttonBg.color = isActive ? activeBgColor : inactiveBgColor;
                    Debug.Log($"[MainMenuGraphicsController] Button {i} background color set to: {(isActive ? "ACTIVE" : "INACTIVE")}");
                }
                if (buttonText != null)
                {
                    buttonText.color = isActive ? activeTextColor : inactiveTextColor;
                    Debug.Log($"[MainMenuGraphicsController] Button {i} text color set to: {(isActive ? "ACTIVE" : "INACTIVE")}");
                }
            }
        }
    }

    // Method: Mengatur interaktivitas button Motion Blur
    public void UpdateMotionBlurButtonInteractability(bool isPostProcessingOn)
    {
        if (motionBlurButtons == null) return;

        foreach (Button btn in motionBlurButtons)
        {
            if (btn != null)
            {
                // Button Motion Blur hanya bisa diinteraksi kalau Post Processing ON
                btn.interactable = isPostProcessingOn;

                // Tambahan: Kalau Post Processing OFF, semua button jadi warna non-aktif
                // Ini optional, tapi visualnya lebih konsisten
                if (!isPostProcessingOn)
                {
                    Image buttonBg = btn.GetComponent<Image>();
                    TextMeshProUGUI buttonText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonBg != null) buttonBg.color = inactiveBgColor;
                    if (buttonText != null) buttonText.color = inactiveTextColor;
                }
            }
        }
    }


    // Contoh: Untuk tombol "Apply" atau saat mau pindah scene
    public void ApplyAndStartGame()
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.SaveSettings(); // Pastikan pengaturan tersimpan
            // Load Game Scene Kamu di sini
            // SceneManager.LoadScene("NamaGameSceneKamu"); // Ganti dengan nama scene game Kamu
        }
    }
}