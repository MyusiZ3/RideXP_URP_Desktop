using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SimpleCinematicShot
{
    public GameObject cameraObject;
    public float duration = 3f;
}

public class SimpleCinematicSequence : MonoBehaviour
{
    public List<SimpleCinematicShot> shots = new List<SimpleCinematicShot>();
    public Camera mainCamera;
    public Image fadePanel;
    public float fadeDuration = 0.5f;
    public AudioSource winMusic;

    public void PlaySequence()
    {
        StartCoroutine(PlaySequenceRoutine());
    }

    private IEnumerator PlaySequenceRoutine()
    {
        if (mainCamera != null) mainCamera.enabled = false;
        if (winMusic != null) winMusic.Play();

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 1); // Start black
        }

        for (int i = 0; i < shots.Count; i++)
        {
            // Fade from black
            if (fadePanel != null) yield return StartCoroutine(FadeTo(0));

            if (i > 0 && shots[i - 1].cameraObject != null)
                shots[i - 1].cameraObject.SetActive(false);

            if (shots[i].cameraObject != null)
                shots[i].cameraObject.SetActive(true);

            yield return new WaitForSeconds(shots[i].duration);

            // Fade to black before next cam
            if (fadePanel != null) yield return StartCoroutine(FadeTo(1));
        }

        // Turn off last cam
        if (shots.Count > 0 && shots[shots.Count - 1].cameraObject != null)
            shots[shots.Count - 1].cameraObject.SetActive(false);

        // Back to main camera
        if (mainCamera != null) mainCamera.enabled = true;

        // Fade in back to main
        if (fadePanel != null) yield return StartCoroutine(FadeTo(0));
        if (fadePanel != null) fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadePanel.color.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}
