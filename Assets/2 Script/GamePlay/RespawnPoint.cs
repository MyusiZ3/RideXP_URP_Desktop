using UnityEngine;
using TMPro;
using System.Collections;

public class RespawnPoint : MonoBehaviour
{
    public RespawnManager respawnManager; // Referensi ke RespawnManager
    private Vector3 respawnPoint;
    private Quaternion respawnRotation;

    [Header("UI Feedback")]
    public TMP_Text respawnUpdateText; // Referensi ke TMP Text untuk feedback
    public float fadeDuration = 1.5f;  // Durasi fade out teks

    private void Start()
    {
        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager belum di-assign!");
        }

        if (respawnUpdateText != null)
        {
            respawnUpdateText.gameObject.SetActive(false); // Pastikan hidden di awal
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Simpan posisi respawn
            respawnPoint = transform.position;
            respawnRotation = transform.rotation;

            Debug.Log("Titik respawn disimpan: " + respawnPoint);

            // Informasikan ke manager
            respawnManager.SetRespawnPoint(respawnPoint, respawnRotation);
            respawnManager.PlayTriggerSound();

            // Tampilkan teks notifikasi
            if (respawnUpdateText != null)
            {
                StartCoroutine(ShowRespawnTextAndDisable());
            }
            else
            {
                // Kalau nggak ada teks, langsung nonaktifkan
                gameObject.SetActive(false);
            }
        }
    }
    IEnumerator ShowRespawnTextAndDisable()
    {
        respawnUpdateText.gameObject.SetActive(true);
        respawnUpdateText.text = "Respawn Point Updated";

        Color originalColor = respawnUpdateText.color;
        originalColor.a = 1f;
        respawnUpdateText.color = originalColor;

        yield return new WaitForSeconds(0.8f); // Tahan sebentar sebelum fade

        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            respawnUpdateText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        respawnUpdateText.gameObject.SetActive(false);

        // Sekarang baru nonaktifkan objek trigger checkpoint-nya
        gameObject.SetActive(false);
    }



    IEnumerator ShowRespawnText()
    {
        respawnUpdateText.gameObject.SetActive(true);
        respawnUpdateText.text = "Respawn Point Updated";

        Color originalColor = respawnUpdateText.color;
        originalColor.a = 1f;
        respawnUpdateText.color = originalColor;

        yield return new WaitForSeconds(0.8f); // Tahan sebentar sebelum fade

        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            respawnUpdateText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        respawnUpdateText.gameObject.SetActive(false); // Sembunyikan lagi
    }
}
