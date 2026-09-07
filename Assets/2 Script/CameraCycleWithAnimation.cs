using UnityEngine;
using System.Collections;

public class CameraCycleWithAnimation : MonoBehaviour
{
    [System.Serializable]
    public class CameraInfo
    {
        public Camera camera;
        public float duration = 60f; // Durasi tampil per kamera
    }

    public CameraInfo[] animatedCameras; // Kamera + durasi masing-masing
    private int currentCamIndex = 0;

    void Start()
    {
        // Aktifkan hanya kamera pertama
        for (int i = 0; i < animatedCameras.Length; i++)
        {
            animatedCameras[i].camera.gameObject.SetActive(i == 0);
        }

        StartCoroutine(SwitchCameraRoutine());
    }

    IEnumerator SwitchCameraRoutine()
    {
        while (true)
        {
            float currentDuration = animatedCameras[currentCamIndex].duration;
            yield return new WaitForSeconds(currentDuration);

            // Matikan kamera saat ini
            animatedCameras[currentCamIndex].camera.gameObject.SetActive(false);

            // Ganti ke kamera berikutnya
            currentCamIndex = (currentCamIndex + 1) % animatedCameras.Length;

            // Aktifkan kamera baru
            animatedCameras[currentCamIndex].camera.gameObject.SetActive(true);
        }
    }
}
