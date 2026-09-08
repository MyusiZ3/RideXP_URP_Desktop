using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Controls UI panel bindings, input event listeners, and visual states for graphics and bicycle settings.
/// </summary>
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
    public Button[] motionBlurButtons;

    [Header("Button Style Colors")]
    public Color activeBgColor = new Color32(0xDF, 0x31, 0x4F, 0xFF);
    public Color inactiveBgColor = Color.white;
    public Color activeTextColor = Color.white;
    public Color inactiveTextColor = Color.black;

    private void Start()
    {
        if (GraphicsSettingsManager.GetInstance() == null)
        {
            Debug.LogError("[MainMenuGraphicsController] GraphicsSettingsManager instance not found.");
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

    private void OnDestroy()
    {
        if (postProcessingToggle != null)
        {
            postProcessingToggle.onValueChanged.RemoveListener(OnPostProcessingToggleChanged);
        }
    }

    /// <summary>
    /// Binds click listeners to the motion blur quality buttons.
    /// </summary>
    private void SetupMotionBlurButtonListeners()
    {
        if (motionBlurButtons == null || motionBlurButtons.Length == 0) return;

        for (int i = 0; i < motionBlurButtons.Length; i++)
        {
            GraphicsSettingsManager.MotionBlurQuality quality = (GraphicsSettingsManager.MotionBlurQuality)i;

            if (motionBlurButtons[i] != null)
            {
                motionBlurButtons[i].onClick.RemoveAllListeners();
                motionBlurButtons[i].onClick.AddListener(() => OnMotionBlurButtonClicked(quality));
            }
        }
    }

    /// <summary>
    /// Loads current preferences from GraphicsSettingsManager into UI components.
    /// </summary>
    private void LoadCurrentSettingsToUI()
    {
        if (GraphicsSettingsManager.Instance == null) return;

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

        UpdateSteerSensitivitySliderInteractability(GraphicsSettingsManager.Instance.instantSteering);
        UpdateMotionBlurButtonVisuals(GraphicsSettingsManager.Instance.motionBlurQuality);
        UpdateMotionBlurButtonInteractability(GraphicsSettingsManager.Instance.isPostProcessingEnabled);
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

    /// <summary>
    /// Updates steer sensitivity slider interactability and opacity based on instant steering state.
    /// </summary>
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

    public void OnPostProcessingToggleChanged(bool isOn)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.isPostProcessingEnabled = isOn;
            GraphicsSettingsManager.Instance.SaveSettings();
            UpdateMotionBlurButtonInteractability(isOn);
        }
    }

    public void OnMotionBlurButtonClicked(GraphicsSettingsManager.MotionBlurQuality selectedQuality)
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.motionBlurQuality = selectedQuality;
            GraphicsSettingsManager.Instance.SaveSettings();
            UpdateMotionBlurButtonVisuals(selectedQuality);
        }
    }

    /// <summary>
    /// Updates button color themes for active and inactive motion blur quality selections.
    /// </summary>
    private void UpdateMotionBlurButtonVisuals(GraphicsSettingsManager.MotionBlurQuality currentQuality)
    {
        if (motionBlurButtons == null || motionBlurButtons.Length == 0) return;

        for (int i = 0; i < motionBlurButtons.Length; i++)
        {
            if (motionBlurButtons[i] != null)
            {
                Image buttonBg = motionBlurButtons[i].GetComponent<Image>();
                TextMeshProUGUI buttonText = motionBlurButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                bool isActive = (GraphicsSettingsManager.MotionBlurQuality)i == currentQuality;

                if (buttonBg != null)
                {
                    buttonBg.color = isActive ? activeBgColor : inactiveBgColor;
                }
                if (buttonText != null)
                {
                    buttonText.color = isActive ? activeTextColor : inactiveTextColor;
                }
            }
        }
    }

    /// <summary>
    /// Enables or disables motion blur quality buttons based on post-processing toggle state.
    /// </summary>
    public void UpdateMotionBlurButtonInteractability(bool isPostProcessingOn)
    {
        if (motionBlurButtons == null) return;

        foreach (Button btn in motionBlurButtons)
        {
            if (btn != null)
            {
                btn.interactable = isPostProcessingOn;

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

    public void ApplyAndStartGame()
    {
        if (GraphicsSettingsManager.Instance != null)
        {
            GraphicsSettingsManager.Instance.SaveSettings();
        }
    }
}