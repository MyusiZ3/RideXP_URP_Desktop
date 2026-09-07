using System.Collections;
using UnityEngine;
using TMPro;
using SBPScripts;

public class SpeedBoostZone : MonoBehaviour
{
    [Header("Boost Settings")]
    public float speedMultiplier = 2f;
    public float boostDuration = 2f;

    [Header("SFX")]
    public AudioClip boostSFX;
    public AudioSource audioSource;

    [Header("UI Boost Text")]
    public TMP_Text boostText; // 🟡 Drag TMP_Text dari scene
    public float fadeDuration = 1f; // Durasi fade out text

    private bool isBoosting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BicycleController bike = other.GetComponent<BicycleController>();

            if (bike != null && !isBoosting)
            {
                isBoosting = true;
                bike.ApplySpeedBoost(speedMultiplier, boostDuration);
                Debug.Log("Speedup On!");

                if (boostSFX != null)
                {
                    (audioSource != null ? audioSource : GetComponent<AudioSource>())?.PlayOneShot(boostSFX);
                }

                // Tampilkan UI teks
                if (boostText != null)
                {
                    StartCoroutine(ShowBoostText());
                }

                StartCoroutine(ResetBoostCooldown());
            }
        }
    }

    IEnumerator ResetBoostCooldown()
    {
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
    }

    IEnumerator ShowBoostText()
    {
        boostText.text = "Speed Up!";
        boostText.alpha = 1f; // Full visible
        boostText.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            boostText.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        boostText.alpha = 0f;
        boostText.gameObject.SetActive(false);
    }
}
