using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using SBPScripts;

/// <summary>
/// Manages cinematic sequence, countdown UI, camera switches, and participant mobility prior to race start.
/// </summary>
public class RaceCountdownManager : MonoBehaviour
{
    [Header("Dependencies")]
    public GameManager gameManager;
    public GameObject blockerCube;
    public AudioSource audioSource;
    public AudioClip combinedBeepClip;

    [Header("Countdown Timings")]
    public float prepareDuration = 15f;
    public float countdownDuration = 3f;

    [Header("UI Elements")]
    public TMP_Text prepareText;
    public TMP_Text countdownText;
    public GameObject cinematicBarOverlay;
    public GameObject[] uiToHideDuringCinematic;

    [Header("Camera Control")]
    public Camera[] regularCams;
    public Camera playerCam;
    public float camSwitchInterval = 5f;

    [Header("Race Participants")]
    [Tooltip("Movement script for the player vehicle.")]
    public MonoBehaviour playerMovementScript;
    [Tooltip("Array of NPC AI controllers participating in the race.")]
    public NPCBicycleAIController[] npcControllers;

    [Header("Events")]
    public UnityEvent StartEvent;

    private float camTimer;
    private int currentCamIndex;

    private void Awake()
    {
        SetPlayerAndNPCMovementActive(false);
    }

    private void Start()
    {
        StartCoroutine(PrepareAndCountdownRoutine());
    }

    private IEnumerator FadeText(TMP_Text text, float duration, float holdTime)
    {
        float t = 0f;
        text.alpha = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        yield return new WaitForSeconds(holdTime);

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }

        text.gameObject.SetActive(false);
    }

    private IEnumerator PunchScale(TMP_Text text, float duration, float intensity)
    {
        Vector3 originalScale = text.transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float scale = 1f + Mathf.Sin(timer / duration * Mathf.PI) * intensity;
            text.transform.localScale = originalScale * scale;
            yield return null;
        }

        text.transform.localScale = originalScale;
    }

    private IEnumerator PrepareAndCountdownRoutine()
    {
        blockerCube.SetActive(true);
        countdownText.gameObject.SetActive(true);

        if (cinematicBarOverlay != null)
            cinematicBarOverlay.SetActive(true);

        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null) ui.SetActive(false);
        }

        currentCamIndex = 0;
        ActivateCam(currentCamIndex);

        prepareText.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(false);

        float elapsed = 0f;
        if (playerCam != null) playerCam.enabled = false;

        while (elapsed < prepareDuration)
        {
            camTimer += Time.deltaTime;
            if (camTimer >= camSwitchInterval && regularCams.Length > 0)
            {
                camTimer = 0f;
                currentCamIndex = (currentCamIndex + 1) % regularCams.Length;
                ActivateCam(currentCamIndex);
            }

            prepareText.text = $"Get Ready For: {(int)(prepareDuration - elapsed)}";
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (audioSource != null && combinedBeepClip != null)
            audioSource.PlayOneShot(combinedBeepClip);

        prepareText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText.alpha = 1f;

        for (int i = (int)countdownDuration; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.gameObject.SetActive(true);

            StartCoroutine(PunchScale(countdownText, 0.5f, 0.25f));
            StartCoroutine(FadeText(countdownText, 0.3f, 0.7f));

            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "GO!";
        countdownText.gameObject.SetActive(true);
        StartCoroutine(PunchScale(countdownText, 0.6f, 0.3f));
        StartCoroutine(FadeText(countdownText, 0.4f, 0.8f));

        blockerCube.SetActive(false);
        SetPlayerAndNPCMovementActive(true);

        StartEvent?.Invoke();

        if (cinematicBarOverlay != null)
            cinematicBarOverlay.SetActive(false);

        foreach (var ui in uiToHideDuringCinematic)
        {
            if (ui != null) ui.SetActive(true);
        }

        DeactivateAllCams();
        if (playerCam != null) playerCam.enabled = true;

        if (gameManager != null)
        {
            gameManager.StartRaceTimer();
        }
        else
        {
            Debug.LogWarning("[RaceCountdownManager] GameManager reference is missing.");
        }

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void SetPlayerAndNPCMovementActive(bool canMove)
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = canMove;

            Rigidbody playerRb = playerMovementScript.GetComponent<Rigidbody>();
            BicycleController playerBikeController = playerMovementScript.GetComponent<BicycleController>();
            Animator playerAnimator = playerMovementScript.GetComponent<Animator>();

            if (playerRb != null)
            {
                if (!canMove && !playerRb.isKinematic)
                {
                    playerRb.linearVelocity = Vector3.zero;
                    playerRb.angularVelocity = Vector3.zero;
                }
                playerRb.isKinematic = !canMove;
            }

            if (!canMove)
            {
                if (playerBikeController != null)
                {
                    playerBikeController.bunnyHopInputState = 0;
                    playerBikeController.customAccelerationAxis = 0f;
                    playerBikeController.rawCustomAccelerationAxis = 0f;
                    playerBikeController.customSteerAxis = 0f;
                    playerBikeController.customLeanAxis = 0f;
                }
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Speed", 0f);
                    playerAnimator.Rebind();
                    playerAnimator.Update(0f);
                }
            }
        }

        if (npcControllers != null)
        {
            foreach (NPCBicycleAIController npcAI in npcControllers)
            {
                if (npcAI != null)
                {
                    npcAI.enabled = canMove;

                    Rigidbody npcRb = npcAI.GetComponent<Rigidbody>();
                    BicycleController npcBikeController = npcAI.GetComponent<BicycleController>();
                    Animator npcAnimator = npcAI.GetComponent<Animator>();

                    if (npcRb != null)
                    {
                        if (!canMove && !npcRb.isKinematic)
                        {
                            npcRb.linearVelocity = Vector3.zero;
                            npcRb.angularVelocity = Vector3.zero;
                        }
                        npcRb.isKinematic = !canMove;
                    }

                    if (!canMove)
                    {
                        if (npcBikeController != null && npcBikeController.isAIControlled)
                        {
                            npcBikeController.pedalInput = 0f;
                            npcBikeController.steerInput = 0f;
                            npcBikeController.bunnyHopInputState = 0;
                            npcBikeController.customAccelerationAxis = 0f;
                            npcBikeController.rawCustomAccelerationAxis = 0f;
                            npcBikeController.customSteerAxis = 0f;
                        }
                        if (npcAnimator != null)
                        {
                            npcAnimator.SetFloat("Speed", 0f);
                            npcAnimator.Rebind();
                            npcAnimator.Update(0f);
                        }
                    }
                }
            }
        }
    }

    private void ActivateCam(int index)
    {
        for (int i = 0; i < regularCams.Length; i++)
        {
            if (regularCams[i] != null)
                regularCams[i].enabled = (i == index);
        }
    }

    private void DeactivateAllCams()
    {
        foreach (Camera cam in regularCams)
        {
            if (cam != null)
                cam.enabled = false;
        }
    }
}
