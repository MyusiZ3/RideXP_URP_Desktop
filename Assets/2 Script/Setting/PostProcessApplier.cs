// PostProcessApplier.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessApplier : MonoBehaviour
{
    private Volume volume;

    void Awake()
    {
        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("[PostProcessApplier] Volume component not found on this GameObject! Make sure it's a URP Volume. Disabling script.");
            enabled = false;
            return;
        }
        
        GraphicsSettingsManager manager = GraphicsSettingsManager.GetInstance();

        ApplySettings();
    }

    void ApplySettings()
    {
        // Add null check for volume here, even though it should be set in Awake.
        // This is primarily to handle potential editor timing issues.
        if (volume == null)
        {
            Debug.LogWarning("[PostProcessApplier] Attempted to apply settings, but Volume is null. Skipping.");
            return;
        }

        if (GraphicsSettingsManager.Instance == null)
        {
            Debug.LogWarning("[PostProcessApplier] GraphicsSettingsManager instance is null. Cannot apply settings fully.");
            // We might still enable/disable the volume based on some default if manager is truly missing
            volume.enabled = true; // Fallback to ON if manager is null
            return;
        }

        Debug.Log($"[PostProcessApplier] Applying settings. isPostProcessingEnabled: {GraphicsSettingsManager.Instance.isPostProcessingEnabled}, MotionBlurQuality: {GraphicsSettingsManager.Instance.motionBlurQuality}");

        // 1. Atur Global Post Processing (ON/OFF)
        volume.enabled = GraphicsSettingsManager.Instance.isPostProcessingEnabled;
        Debug.Log($"[PostProcessApplier] URP Volume enabled: {volume.enabled}");

        if (!volume.enabled)
        {
            Debug.Log("[PostProcessApplier] URP Volume is OFF, Motion Blur will be off too.");
            MotionBlur specificMotionBlur;
            // Add null check for volume.profile
            if (volume.profile != null && volume.profile.TryGet(out specificMotionBlur)) // Add check for volume.profile
            {
                 specificMotionBlur.active = false;
            }
            return;
        }

        // 2. Atur Motion Blur Quality
        // 2. Atur Motion Blur Quality
        MotionBlur motionBlur;
        if (volume.profile != null && volume.profile.TryGet(out motionBlur)) // Pastikan volume.profile tidak null
        {
            GraphicsSettingsManager.MotionBlurQuality selectedQuality = GraphicsSettingsManager.Instance.motionBlurQuality;

            if (selectedQuality == GraphicsSettingsManager.MotionBlurQuality.Off)
            {
                motionBlur.active = false;
                Debug.Log("[PostProcessApplier] Motion Blur: OFF");
            }
            else
            {
                motionBlur.active = true;
                switch (selectedQuality)
                {
                    case GraphicsSettingsManager.MotionBlurQuality.Low:
                        motionBlur.intensity.value = 0.3f; // <-- Sesuaikan nilai Low (antara 0-1)
                        Debug.Log("[PostProcessApplier] Motion Blur: LOW - Intensity: 0.3");
                        break;
                    case GraphicsSettingsManager.MotionBlurQuality.Medium:
                        motionBlur.intensity.value = 0.6f; // <-- Sesuaikan nilai Medium (antara 0-1)
                        Debug.Log("[PostProcessApplier] Motion Blur: MEDIUM - Intensity: 0.6");
                        break;
                    case GraphicsSettingsManager.MotionBlurQuality.High:
                        motionBlur.intensity.value = 1.0f; // <-- Sesuaikan nilai High (maksimal 1.0)
                        Debug.Log("[PostProcessApplier] Motion Blur: HIGH - Intensity: 1.0");
                        break;
                }
                // Catatan: Jika URP Motion Blur kamu punya parameter 'Sample Count' atau sejenisnya
                // kamu juga bisa adjust itu untuk menambah kualitas visual
                // motionBlur.sampleCount.value = 10; // Contoh jika ada parameter sampleCount
            }
        }
        else
        {
            Debug.LogWarning("[PostProcessApplier] Motion Blur setting not found in URP Volume Profile! Make sure it's added and active, or volume.profile is null.");
        }

        // ... (sisa kode efek lainnya, tambahkan juga null check untuk volume.profile) ...
        ColorAdjustments colorAdjustments;
        if (volume.profile != null && volume.profile.TryGet(out colorAdjustments)) { /* ... */ }

        Vignette vignette;
        if (volume.profile != null && volume.profile.TryGet(out vignette)) { /* ... */ }

        Tonemapping tonemapping;
        if (volume.profile != null && volume.profile.TryGet(out tonemapping)) { /* ... */ }
    }
}