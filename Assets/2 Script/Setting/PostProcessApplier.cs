using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Applies URP Volume profile overrides based on global graphics settings.
/// </summary>
public class PostProcessApplier : MonoBehaviour
{
    private Volume volume;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        if (volume == null)
        {
            Debug.LogError("[PostProcessApplier] Volume component missing. Disabling script.");
            enabled = false;
            return;
        }

        GraphicsSettingsManager.GetInstance();
        ApplySettings();
    }

    /// <summary>
    /// Applies active post-processing and motion blur settings to the local URP Volume component.
    /// </summary>
    private void ApplySettings()
    {
        if (volume == null || GraphicsSettingsManager.Instance == null) return;

        volume.enabled = GraphicsSettingsManager.Instance.isPostProcessingEnabled;

        if (!volume.enabled)
        {
            if (volume.profile != null && volume.profile.TryGet(out MotionBlur specificMotionBlur))
            {
                specificMotionBlur.active = false;
            }
            return;
        }

        if (volume.profile != null && volume.profile.TryGet(out MotionBlur motionBlur))
        {
            GraphicsSettingsManager.MotionBlurQuality selectedQuality = GraphicsSettingsManager.Instance.motionBlurQuality;

            if (selectedQuality == GraphicsSettingsManager.MotionBlurQuality.Off)
            {
                motionBlur.active = false;
            }
            else
            {
                motionBlur.active = true;
                switch (selectedQuality)
                {
                    case GraphicsSettingsManager.MotionBlurQuality.Low:
                        motionBlur.intensity.value = 0.3f;
                        break;
                    case GraphicsSettingsManager.MotionBlurQuality.Medium:
                        motionBlur.intensity.value = 0.6f;
                        break;
                    case GraphicsSettingsManager.MotionBlurQuality.High:
                        motionBlur.intensity.value = 1.0f;
                        break;
                }
            }
        }
    }
}