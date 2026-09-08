using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Persistent singleton manager handling graphics preferences and bicycle control configurations.
/// </summary>
public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance { get; private set; }

    [Header("Graphics Preferences")]
    public bool isPostProcessingEnabled = true;
    public MotionBlurQuality motionBlurQuality = MotionBlurQuality.Medium;

    [Header("Bicycle Control Preferences")]
    public float steerSensitivity = 15f;
    public bool instantSteering = false;
    public bool enableDoubleJump = false;

    public enum MotionBlurQuality
    {
        Off,
        Low,
        Medium,
        High
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == this)
        {
            LoadSettings();
        }
    }

    /// <summary>
    /// Ensures a valid singleton instance is available, instantiating one if missing.
    /// </summary>
    public static GraphicsSettingsManager GetInstance()
    {
        if (Instance == null)
        {
            GameObject managerGO = new GameObject("_GraphicsSettingsManager_RuntimeCreated");
            Instance = managerGO.AddComponent<GraphicsSettingsManager>();
        }
        return Instance;
    }

    /// <summary>
    /// Saves current graphics and control configurations to PlayerPrefs and applies changes in real-time.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("IsPostProcessingEnabled", isPostProcessingEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MotionBlurQuality", (int)motionBlurQuality);
        PlayerPrefs.SetFloat("SteerSensitivity", steerSensitivity);
        PlayerPrefs.SetInt("InstantSteering", instantSteering ? 1 : 0);
        PlayerPrefs.SetInt("EnableDoubleJump", enableDoubleJump ? 1 : 0);
        PlayerPrefs.Save();

        ApplyGraphicsRealtime();
    }

    /// <summary>
    /// Loads graphics and control settings from PlayerPrefs.
    /// </summary>
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

        steerSensitivity = PlayerPrefs.GetFloat("SteerSensitivity", 15f);
        instantSteering = PlayerPrefs.GetInt("InstantSteering", 0) == 1;
        enableDoubleJump = PlayerPrefs.GetInt("EnableDoubleJump", 0) == 1;

        ApplyGraphicsRealtime();
    }

    /// <summary>
    /// Applies active post-processing and motion blur settings to scene cameras and volume profiles in real-time.
    /// </summary>
    public void ApplyGraphicsRealtime()
    {
        // Apply post-processing state to main cameras while bypassing minimap cameras
        foreach (var cam in Camera.allCameras)
        {
            if (cam != null && cam.TryGetComponent<UniversalAdditionalCameraData>(out var data))
            {
                bool isMinimapCam = cam.targetTexture != null || cam.name.ToLower().Contains("minimap") || cam.GetComponent<MinimapCameraFollow>() != null;
                data.renderPostProcessing = isMinimapCam ? false : isPostProcessingEnabled;
            }
        }

        // Apply motion blur quality settings to URP Volume profiles
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
    }
}