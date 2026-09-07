using UnityEngine;

public class WarningLight : MonoBehaviour // Pastikan class ini mewarisi MonoBehaviour
{
    // Variabel untuk mengatur delay hidup dan mati lampu
    public float onDelay = 1.0f; // Delay untuk lampu hidup
    public float offDelay = 1.0f; // Delay untuk lampu mati

    private Light warningLight; // Komponen Light
    private bool isOn = false; // Status lampu

    void Start()
    {
        // Mendapatkan komponen Light dari objek
        warningLight = GetComponent<Light>();

        // Memulai loop untuk menyalakan dan mematikan lampu
        StartCoroutine(LightCycle());
    }

    private System.Collections.IEnumerator LightCycle()
    {
        // Loop untuk terus-menerus menghidupkan dan mematikan lampu
        while (true)
        {
            // Hidupkan lampu
            isOn = true;
            warningLight.enabled = isOn;
            yield return new WaitForSeconds(onDelay); // Tunggu selama delay hidup

            // Matikan lampu
            isOn = false;
            warningLight.enabled = isOn;
            yield return new WaitForSeconds(offDelay); // Tunggu selama delay mati
        }
    }
}
