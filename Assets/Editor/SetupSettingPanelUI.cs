using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.UI;
using TMPro;

public class SetupSettingPanelUI : MonoBehaviour
{
    [MenuItem("Tools/RideXP/Add Bicycle Controls to Setting Panel")]
    public static void InjectBicycleControlsToSettingPanel()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Perhatian!", "Harap jalankan tool ini saat EDIT MODE (matikan Play Mode) agar perubahan UI tersimpan secara permanen di Scene!", "OK");
            return;
        }

        MainMenuGraphicsController graphicsController = FindGraphicsController();
        if (graphicsController == null)
        {
            GameObject targetGO = GameObject.Find("PanelSetting") ?? GameObject.Find("UI Main") ?? GameObject.Find("Setting");
            if (targetGO != null)
            {
                graphicsController = Undo.AddComponent<MainMenuGraphicsController>(targetGO);
            }
        }

        if (graphicsController == null)
        {
            EditorUtility.DisplayDialog("Error", "MainMenuGraphicsController tidak ditemukan di scene aktif!", "OK");
            return;
        }

        Transform settingPanelTransform = graphicsController.transform;

        // Cek apakah sudah dibuat sebelumnya
        Transform existingControls = settingPanelTransform.Find("BicycleControlsGroup");
        if (existingControls != null)
        {
            Undo.DestroyObjectImmediate(existingControls.gameObject);
        }

        // Buat Parent Group untuk Bicycle Controls UI
        GameObject controlsGroup = new GameObject("BicycleControlsGroup", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(controlsGroup, "Create Bicycle Controls UI");
        controlsGroup.transform.SetParent(settingPanelTransform, false);

        RectTransform groupRect = controlsGroup.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.5f);
        groupRect.anchorMax = new Vector2(0.5f, 0.5f);
        groupRect.pivot = new Vector2(0.5f, 0.5f);
        groupRect.anchoredPosition = new Vector2(-150f, -120f); // Posisikan di bawah Post Processing agar terlihat jelas
        groupRect.sizeDelta = new Vector2(300f, 200f);

        // 1. Header Text
        GameObject headerGO = new GameObject("Header_BicycleControls", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerGO.transform.SetParent(controlsGroup.transform, false);
        TextMeshProUGUI headerText = headerGO.GetComponent<TextMeshProUGUI>();
        headerText.text = "BICYCLE CONTROLS";
        headerText.fontSize = 20;
        headerText.fontStyle = FontStyles.Bold;
        headerText.color = new Color32(0xDF, 0x31, 0x4F, 0xFF); // Red #DF314F
        RectTransform headerRect = headerGO.GetComponent<RectTransform>();
        headerRect.anchoredPosition = new Vector2(0f, 85f);
        headerRect.sizeDelta = new Vector2(300f, 30f);

        // 2. Steer Sensitivity Slider & Label
        GameObject sliderGO = new GameObject("Slider_SteerSensitivity", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(controlsGroup.transform, false);
        Slider slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 1f;
        slider.maxValue = 50f;
        slider.value = 15f;

        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(0f, 35f);
        sliderRect.sizeDelta = new Vector2(200f, 20f);

        // Slider Background
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        Image bgImg = bgGO.GetComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        bgImg.raycastTarget = true;
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Fill Area & Fill
        GameObject fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        Image fillImg = fillGO.GetComponent<Image>();
        fillImg.color = new Color32(0xDF, 0x31, 0x4F, 0xFF);
        fillImg.raycastTarget = false;
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        slider.fillRect = fillRect;

        // Handle Slide Area & Handle
        GameObject handleAreaGO = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleAreaGO.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleAreaGO.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = Vector2.zero;

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(handleAreaGO.transform, false);
        Image handleImg = handleGO.GetComponent<Image>();
        handleImg.color = Color.white;
        handleImg.raycastTarget = true;
        RectTransform handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(16f, 24f);
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        // Slider Label & Value Text
        GameObject labelSliderGO = new GameObject("Label_Sensitivity", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelSliderGO.transform.SetParent(controlsGroup.transform, false);
        TextMeshProUGUI labelSlider = labelSliderGO.GetComponent<TextMeshProUGUI>();
        labelSlider.text = "Steer Sensitivity";
        labelSlider.fontSize = 14;
        labelSlider.color = Color.white;
        RectTransform labelSliderRect = labelSliderGO.GetComponent<RectTransform>();
        labelSliderRect.anchoredPosition = new Vector2(-60f, 58f);
        labelSliderRect.sizeDelta = new Vector2(150f, 20f);

        GameObject valueTextGO = new GameObject("Text_SensitivityValue", typeof(RectTransform), typeof(TextMeshProUGUI));
        valueTextGO.transform.SetParent(controlsGroup.transform, false);
        TextMeshProUGUI valueText = valueTextGO.GetComponent<TextMeshProUGUI>();
        valueText.text = "15";
        valueText.fontSize = 14;
        valueText.fontStyle = FontStyles.Bold;
        valueText.color = new Color32(0xDF, 0x31, 0x4F, 0xFF);
        RectTransform valueTextRect = valueTextGO.GetComponent<RectTransform>();
        valueTextRect.anchoredPosition = new Vector2(110f, 35f);
        valueTextRect.sizeDelta = new Vector2(40f, 20f);

        // 3. Instant Steering Toggle
        Toggle instantToggle = CreateToggle(controlsGroup.transform, "Toggle_InstantSteer", "Instant Steering", new Vector2(0f, -15f));

        // 4. Double Jump Toggle
        Toggle doubleJumpToggle = CreateToggle(controlsGroup.transform, "Toggle_DoubleJump", "Enable Double Jump", new Vector2(0f, -65f));

        // Auto-assign ke MainMenuGraphicsController
        Undo.RecordObject(graphicsController, "Assign Bicycle Controls UI");
        graphicsController.steerSensitivitySlider = slider;
        graphicsController.steerSensitivityValueText = valueText;
        graphicsController.instantSteeringToggle = instantToggle;
        graphicsController.doubleJumpToggle = doubleJumpToggle;

        // Wire UnityEvents secara persistent agar terlihat jelas di Inspector
        EventToolsClearAndAddListener(slider.onValueChanged, graphicsController, "OnSteerSensitivitySliderChanged");
        EventToolsClearAndAddListener(instantToggle.onValueChanged, graphicsController, "OnInstantSteeringToggleChanged");
        EventToolsClearAndAddListener(doubleJumpToggle.onValueChanged, graphicsController, "OnDoubleJumpToggleChanged");

        EditorUtility.SetDirty(graphicsController);
        EditorSceneManager.MarkSceneDirty(graphicsController.gameObject.scene);

        EditorUtility.DisplayDialog("Sukses!", "UI Kontrol Sepeda (Steer Sensitivity Slider dengan Handle, Instant Steering, Double Jump) berhasil dibuat dan di-wire ke MainMenuGraphicsController!", "OK");
    }

    [MenuItem("Tools/RideXP/Wire Existing Bicycle Controls (Preserve Style)")]
    public static void WireExistingBicycleControlsToSettingPanel()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Perhatian!", "Harap jalankan tool ini saat EDIT MODE (matikan Play Mode)!", "OK");
            return;
        }

        MainMenuGraphicsController graphicsController = FindGraphicsController();
        if (graphicsController == null)
        {
            GameObject targetGO = GameObject.Find("PanelSetting") ?? GameObject.Find("UI Main") ?? GameObject.Find("Setting");
            if (targetGO == null)
            {
                GameObject groupGO = GameObject.Find("BicycleControlsGroup");
                if (groupGO != null && groupGO.transform.parent != null)
                {
                    targetGO = groupGO.transform.parent.gameObject;
                }
            }

            if (targetGO != null)
            {
                graphicsController = Undo.AddComponent<MainMenuGraphicsController>(targetGO);
            }
        }

        if (graphicsController == null)
        {
            EditorUtility.DisplayDialog("Error", "MainMenuGraphicsController tidak ditemukan dan tidak dapat memasangnya ke PanelSetting!", "OK");
            return;
        }

        Transform settingPanelTransform = graphicsController.transform;
        Transform controlsGroup = settingPanelTransform.Find("BicycleControlsGroup");
        if (controlsGroup == null)
        {
            // Jika tidak ada di bawah settingPanelTransform, cari di seluruh scene
            GameObject groupGO = GameObject.Find("BicycleControlsGroup");
            if (groupGO != null) controlsGroup = groupGO.transform;
        }

        if (controlsGroup == null)
        {
            EditorUtility.DisplayDialog("Error", "BicycleControlsGroup tidak ditemukan di Scene!", "OK");
            return;
        }

        Undo.RecordObject(graphicsController, "Wire Bicycle Controls UI");

        // Cari komponen UI yang ada
        Slider slider = controlsGroup.GetComponentInChildren<Slider>(true);
        TextMeshProUGUI valueText = controlsGroup.Find("Text_SensitivityValue")?.GetComponent<TextMeshProUGUI>();
        if (valueText == null)
        {
            // Fallback cari TextMeshProUGUI apapun yang namanya ada Sensitivity / Value
            foreach (var tmpro in controlsGroup.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmpro.gameObject.name.Contains("Value") || tmpro.gameObject.name.Contains("SensitivityValue"))
                {
                    valueText = tmpro;
                    break;
                }
            }
        }

        Toggle instantToggle = controlsGroup.Find("Toggle_InstantSteer")?.GetComponent<Toggle>();
        Toggle doubleJumpToggle = controlsGroup.Find("Toggle_DoubleJump")?.GetComponent<Toggle>();

        // Fallback untuk Toggle jika nama beda
        if (instantToggle == null || doubleJumpToggle == null)
        {
            Toggle[] toggles = controlsGroup.GetComponentsInChildren<Toggle>(true);
            if (toggles.Length >= 1 && instantToggle == null) instantToggle = toggles[0];
            if (toggles.Length >= 2 && doubleJumpToggle == null) doubleJumpToggle = toggles[1];
        }

        // Perbaiki RaycastTarget & TargetGraphic Slider jika belum lengkap agar bisa di-drag
        if (slider != null)
        {
            Image handleImg = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;
            if (handleImg == null)
            {
                Image[] childImgs = slider.GetComponentsInChildren<Image>(true);
                foreach (var img in childImgs)
                {
                    if (img.gameObject.name.Contains("Handle") || img.gameObject.name.Contains("Knob"))
                    {
                        handleImg = img;
                        slider.handleRect = img.rectTransform;
                        break;
                    }
                }
            }
            if (slider.targetGraphic == null && handleImg != null)
            {
                slider.targetGraphic = handleImg;
            }
            if (slider.targetGraphic == null)
            {
                Image bgImg = slider.GetComponent<Image>();
                if (bgImg != null) slider.targetGraphic = bgImg;
            }

            graphicsController.steerSensitivitySlider = slider;
            EventToolsClearAndAddListener(slider.onValueChanged, graphicsController, "OnSteerSensitivitySliderChanged");
        }

        if (valueText != null)
        {
            graphicsController.steerSensitivityValueText = valueText;
        }

        if (instantToggle != null)
        {
            graphicsController.instantSteeringToggle = instantToggle;
            EventToolsClearAndAddListener(instantToggle.onValueChanged, graphicsController, "OnInstantSteeringToggleChanged");
        }

        if (doubleJumpToggle != null)
        {
            graphicsController.doubleJumpToggle = doubleJumpToggle;
            EventToolsClearAndAddListener(doubleJumpToggle.onValueChanged, graphicsController, "OnDoubleJumpToggleChanged");
        }

        // Auto-wire Post Processing Toggle jika belum terhubung
        if (graphicsController.postProcessingToggle == null)
        {
            Toggle[] allToggles = settingPanelTransform.GetComponentsInChildren<Toggle>(true);
            foreach (var t in allToggles)
            {
                if (t.transform.IsChildOf(controlsGroup)) continue;
                
                if (t.gameObject.name.ToLower().Contains("post") || (t.transform.parent != null && t.transform.parent.name.ToLower().Contains("post")))
                {
                    graphicsController.postProcessingToggle = t;
                    break;
                }
                // Fallback: Toggle pertama di luar BicycleControlsGroup
                if (graphicsController.postProcessingToggle == null)
                {
                    graphicsController.postProcessingToggle = t;
                }
            }
        }

        if (graphicsController.postProcessingToggle != null)
        {
            EventToolsClearAndAddListener(graphicsController.postProcessingToggle.onValueChanged, graphicsController, "OnPostProcessingToggleChanged");
        }

        // Auto-wire Motion Blur Buttons (Index 0: Off, Index 1: Low, Index 2: Medium, Index 3: High)
        Button btnOff = null, btnLow = null, btnMed = null, btnHigh = null;
        Button[] allBtns = settingPanelTransform.GetComponentsInChildren<Button>(true);
        foreach (var b in allBtns)
        {
            string name = b.gameObject.name.ToLower();
            TextMeshProUGUI tmpText = b.GetComponentInChildren<TextMeshProUGUI>(true);
            string textContent = tmpText != null ? tmpText.text.ToLower() : "";

            if (name.Contains("off") || textContent == "off") btnOff = b;
            else if (name.Contains("low") || textContent == "low") btnLow = b;
            else if (name.Contains("med") || textContent == "medium" || textContent == "med") btnMed = b;
            else if (name.Contains("high") || textContent == "high") btnHigh = b;
        }

        if (btnOff != null && btnLow != null && btnMed != null && btnHigh != null)
        {
            graphicsController.motionBlurButtons = new Button[] { btnOff, btnLow, btnMed, btnHigh };
        }
        else
        {
            // Fallback: Jika nama beda, ambil 4 button berturut-turut di luar BicycleControlsGroup
            System.Collections.Generic.List<Button> foundList = new System.Collections.Generic.List<Button>();
            foreach (var b in allBtns)
            {
                if (b.transform.IsChildOf(controlsGroup)) continue;
                foundList.Add(b);
            }
            if (foundList.Count >= 4)
            {
                graphicsController.motionBlurButtons = foundList.GetRange(0, 4).ToArray();
            }
        }

        graphicsController.activeBgColor = new Color32(0xDF, 0x31, 0x4F, 0xFF);
        graphicsController.inactiveBgColor = Color.white;
        graphicsController.activeTextColor = Color.white;
        graphicsController.inactiveTextColor = Color.black;

        EditorUtility.SetDirty(graphicsController);
        EditorSceneManager.MarkSceneDirty(graphicsController.gameObject.scene);

        EditorUtility.DisplayDialog("Sukses Wire UI!", "Desain & style UI Anda TETAP UTUH (tidak di-reset). Post Processing Toggle, Motion Blur Buttons (Off, Low, Medium, High dengan warna Putih saat non-aktif), dan Bicycle Controls berhasil di-wire ke MainMenuGraphicsController!", "OK");
    }

    private static void EventToolsClearAndAddListener<T>(UnityEngine.Events.UnityEvent<T> unityEvent, MonoBehaviour target, string methodName)
    {
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, i);
        }
        var method = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<T>), target, methodName) as UnityEngine.Events.UnityAction<T>;
        if (method != null)
        {
            UnityEventTools.AddPersistentListener(unityEvent, method);
        }
    }

    private static Toggle CreateToggle(Transform parent, string goName, string labelStr, Vector2 position)
    {
        GameObject toggleGO = new GameObject(goName, typeof(RectTransform), typeof(Toggle));
        toggleGO.transform.SetParent(parent, false);
        Toggle toggle = toggleGO.GetComponent<Toggle>();

        RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
        toggleRect.anchoredPosition = position;
        toggleRect.sizeDelta = new Vector2(250f, 30f);

        // Checkbox Background
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(toggleGO.transform, false);
        Image bgImg = bgGO.GetComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        bgImg.raycastTarget = true;
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.5f);
        bgRect.anchorMax = new Vector2(0f, 0.5f);
        bgRect.anchoredPosition = new Vector2(15f, 0f);
        bgRect.sizeDelta = new Vector2(24f, 24f);

        // Checkmark
        GameObject checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(bgGO.transform, false);
        Image checkImg = checkGO.GetComponent<Image>();
        checkImg.color = new Color32(0xDF, 0x31, 0x4F, 0xFF); // Red #DF314F
        checkImg.raycastTarget = false;
        RectTransform checkRect = checkGO.GetComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = new Vector2(-4f, -4f);
        toggle.graphic = checkImg;

        // Label
        GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(toggleGO.transform, false);
        TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
        label.text = labelStr;
        label.fontSize = 15;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(40f, 0f);
        labelRect.sizeDelta = new Vector2(200f, 24f);

        toggle.targetGraphic = bgImg;
        return toggle;
    }

    private static MainMenuGraphicsController FindGraphicsController()
    {
        MainMenuGraphicsController controller = Object.FindFirstObjectByType<MainMenuGraphicsController>(FindObjectsInactive.Include);
        if (controller != null) return controller;

        MainMenuGraphicsController[] all = Resources.FindObjectsOfTypeAll<MainMenuGraphicsController>();
        foreach (var c in all)
        {
            if (c.gameObject.scene.isLoaded)
            {
                return c;
            }
        }
        return null;
    }
}
