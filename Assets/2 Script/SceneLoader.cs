using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider loadingBar;
    public TMP_Text loadingText;
    public float minLoadingTime = 3f;

    public CanvasGroup canvasGroup; // <- assign CanvasGroup dari loadingScreen

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(ShowLoadingThenLoad(sceneName));
    }

    IEnumerator ShowLoadingThenLoad(string sceneName)
    {
        // Reset
        canvasGroup.alpha = 0f;
        loadingScreen.SetActive(true);

        // Fade in UI loading
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }

        // Setelah fade selesai → baru lanjut ke load scene
        yield return StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float fakeProgress = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float targetProgress = (timer < minLoadingTime)
                ? Mathf.Lerp(0f, 0.9f, timer / minLoadingTime)
                : realProgress;

            fakeProgress = Mathf.MoveTowards(fakeProgress, targetProgress, Time.deltaTime);

            loadingBar.value = fakeProgress;
            int percent = Mathf.RoundToInt(fakeProgress * 100f);
            loadingText.text = $"Loading... {percent}%";

            if (fakeProgress >= 1f && operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
