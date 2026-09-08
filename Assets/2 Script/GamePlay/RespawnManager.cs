using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles player checkpoint updates, respawn positioning, countdown timers, and hardware/keyboard inputs.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    [Header("Audio & UI")]
    public TextMeshProUGUI timerText;
    public AudioClip countdownSound;
    public AudioClip triggerSound;
    private AudioSource audioSource;

    [Header("Hardware Settings")]
    public bool enableArduinoHardware = false;
    public SerialController serialController;

    [Header("Respawn Settings")]
    public float respawnDelay = 3f;
    private bool isRespawning = false;
    private Vector3 lastRespawnPoint;
    private Quaternion lastRespawnRotation;

    private bool arduinoRespawnButtonState = false;
    private bool prevArduinoRespawnButtonState = false;

    [Header("UI Debugging")]
    public TextMeshProUGUI debugSerialDataText;
    public TextMeshProUGUI debugButtonStateText;
    public TextMeshProUGUI debugRespawnPointText;

    [Header("Checkpoint UI Feedback")]
    public TextMeshProUGUI checkpointUpdateTextUI;
    public float checkpointFadeDuration = 1.5f;
    public float checkpointTextDisplayTime = 0.8f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[RespawnManager] timerText reference missing.");
        }

        if (enableArduinoHardware && serialController == null)
        {
            Debug.LogWarning("[RespawnManager] Arduino hardware is enabled but SerialController is missing.");
        }

        if (debugSerialDataText != null) debugSerialDataText.text = enableArduinoHardware ? "Serial Data: Waiting..." : "Serial Data: Disabled (Desktop Mode)";
        if (debugButtonStateText != null) debugButtonStateText.text = "Button State: 0 (Released)";

        lastRespawnPoint = Vector3.zero;
        if (debugRespawnPointText != null) debugRespawnPointText.text = "Respawn Point: Not Set";

        if (checkpointUpdateTextUI != null)
        {
            checkpointUpdateTextUI.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isRespawning && Input.GetKeyDown(KeyCode.R) && lastRespawnPoint != Vector3.zero)
        {
            StartRespawn(lastRespawnPoint, lastRespawnRotation);
            PlayTriggerSound();
        }

        if (debugRespawnPointText != null)
        {
            debugRespawnPointText.text = "Respawn Point: " + (lastRespawnPoint != Vector3.zero ? "SET (" + lastRespawnPoint.ToString("F1") + ")" : "Not Set");
        }

        if (enableArduinoHardware && serialController != null)
        {
            string message = serialController.ReadSerialMessage();
            if (message != null)
            {
                if (debugSerialDataText != null) debugSerialDataText.text = "Serial Data: " + message;
                ProcessSerialData(message);
            }
        }

        if (debugButtonStateText != null)
        {
            debugButtonStateText.text = "Button State: " + (arduinoRespawnButtonState ? "1 (Pressed)" : "0 (Released)");
        }

        if (!isRespawning && arduinoRespawnButtonState && !prevArduinoRespawnButtonState && lastRespawnPoint != Vector3.zero)
        {
            StartRespawn(lastRespawnPoint, lastRespawnRotation);
            PlayTriggerSound();
        }

        prevArduinoRespawnButtonState = arduinoRespawnButtonState;

        if (isRespawning)
        {
            respawnDelay -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.Ceil(respawnDelay).ToString();
            }

            if (Mathf.FloorToInt(respawnDelay) != Mathf.FloorToInt(respawnDelay + Time.deltaTime) && countdownSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(countdownSound);
                }
            }

            if (respawnDelay <= 0)
            {
                RespawnPlayer();
            }
        }
    }

    private void ProcessSerialData(string data)
    {
        string[] parts = data.Split(',');
        foreach (string part in parts)
        {
            if (part.StartsWith("BTN2:"))
            {
                string buttonStateString = part.Substring("BTN2:".Length);
                arduinoRespawnButtonState = (buttonStateString == "1");
                break;
            }
        }
    }

    /// <summary>
    /// Sets the active respawn position and rotation from a checkpoint trigger.
    /// </summary>
    public void SetRespawnPoint(Vector3 respawnPoint, Quaternion respawnRotation)
    {
        lastRespawnPoint = respawnPoint;
        lastRespawnRotation = respawnRotation;
    }

    /// <summary>
    /// Initiates the respawn countdown sequence.
    /// </summary>
    public void StartRespawn(Vector3 respawnPoint, Quaternion respawnRotation)
    {
        if (isRespawning)
            return;

        lastRespawnPoint = respawnPoint;
        lastRespawnRotation = respawnRotation;
        isRespawning = true;
        respawnDelay = 3f;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }
    }

    private void RespawnPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = lastRespawnPoint;
            player.transform.rotation = lastRespawnRotation;

            Rigidbody[] allRbs = player.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody r in allRbs)
            {
                r.linearVelocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }

            SBPScripts.BicycleController controller = player.GetComponent<SBPScripts.BicycleController>();
            if (controller != null)
            {
                controller.ResetPhysicsState();
            }

            Physics.SyncTransforms();
        }
        else
        {
            Debug.LogError("[RespawnManager] Player object with 'Player' tag not found.");
        }

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }

        isRespawning = false;
    }

    /// <summary>
    /// Plays the trigger audio effect.
    /// </summary>
    public void PlayTriggerSound()
    {
        if (triggerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(triggerSound);
        }
    }

    /// <summary>
    /// Displays checkpoint notification UI and disables the trigger object.
    /// </summary>
    public void ShowCheckpointTextAndDisableTrigger(GameObject triggerObject)
    {
        if (checkpointUpdateTextUI != null)
        {
            StartCoroutine(FadeOutCheckpointTextAndDisable(triggerObject));
        }
        else if (triggerObject != null)
        {
            triggerObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutCheckpointTextAndDisable(GameObject objectToDisable)
    {
        if (checkpointUpdateTextUI == null) yield break;

        checkpointUpdateTextUI.gameObject.SetActive(true);
        checkpointUpdateTextUI.text = "Checkpoint Updated!";

        Color originalColor = checkpointUpdateTextUI.color;
        originalColor.a = 1f;
        checkpointUpdateTextUI.color = originalColor;

        yield return new WaitForSeconds(checkpointTextDisplayTime);

        float t = 0f;
        while (t < checkpointFadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / checkpointFadeDuration);
            checkpointUpdateTextUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        checkpointUpdateTextUI.gameObject.SetActive(false);
        checkpointUpdateTextUI.color = originalColor;

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }
    }
}