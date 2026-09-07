// using UnityEngine;
// using System.IO.Ports;

// public class SerialOverlayTrigger : MonoBehaviour
// {
//     SerialPort serial = new SerialPort("COM4", 115200); // Ganti COMX dengan port ESP kamu
//     public GameObject overlayPanel; // Assign dari Inspector

//     void Start()
//     {
//         serial.Open();
//         serial.ReadTimeout = 50;
//     }

//     void Update()
//     {
//         if (serial.IsOpen)
//         {
//             try
//             {
//                 string data = serial.ReadLine().Trim();
//                 Debug.Log("Data dari ESP: " + data);

//                 if (data == "mulai")
//                 {
//                     OpenOverlay(); // Ini trigger overlay
//                 }
//             }
//             catch (System.Exception) { }
//         }
//     }

//     void OpenOverlay()
//     {
//         overlayPanel.SetActive(true);
//     }

//     void OnApplicationQuit()
//     {
//         if (serial.IsOpen) serial.Close();
//     }
// }
using UnityEngine;
using System.IO.Ports;
using TMPro;
using System.Collections;

public class SerialOverlayTrigger : MonoBehaviour
{
    public bool enableSerialOverlay = false; // Set to false for Desktop Mode
    public string portName = "COM4";
    public int baudRate = 115200;

    private SerialPort serial;

    public GameObject overlayPanel; // Panel overlay
    public TextMeshProUGUI countdownText; // Teks countdown "Closed in ..."
    public AudioSource audioSource; // Komponen AudioSource
    public AudioClip openSound; // Suara ketika overlay muncul
    public float overlayDuration = 30f; // Waktu overlay tampil

    private Coroutine countdownCoroutine;

    void Start()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false); // Biar pas start langsung hidden
        if (countdownText != null) countdownText.text = ""; // Kosongkan dulu

        if (enableSerialOverlay)
        {
            try
            {
                serial = new SerialPort(portName, baudRate);
                serial.Open();
                serial.ReadTimeout = 50;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("SerialOverlayTrigger Gagal Membuka Port Serial: " + ex.Message);
            }
        }
    }

    void Update()
    {
        if (enableSerialOverlay && serial != null && serial.IsOpen)
        {
            try
            {
                string data = serial.ReadLine().Trim();
                Debug.Log("Data dari ESP: " + data);

                if (data == "mulai")
                {
                    OpenOverlay();
                }
            }
            catch (System.Exception) { }
        }
    }

    void OpenOverlay()
    {
        overlayPanel.SetActive(true);

        // Mainkan suara
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // Mulai countdown baru
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownAndClose());
    }

    IEnumerator CountdownAndClose()
    {
        float timeLeft = overlayDuration;

        while (timeLeft > 0)
        {
            countdownText.text = "Closed in " + Mathf.CeilToInt(timeLeft).ToString() + "s";
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // Sembunyikan overlay
        overlayPanel.SetActive(false);
        countdownText.text = "";
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen) serial.Close();
    }
}
