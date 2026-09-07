using UnityEngine;
using UnityEngine.VFX;

public class VFXCheckpointController : MonoBehaviour
{
    public VisualEffect vfx;
    public float hitToCreateDelay = 2f; // ⏱️ Jeda dari hit ke create

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag("Player") || other.CompareTag("NPCRider")))
        {
            isTriggered = true;

            vfx.SendEvent("hit"); // 🔴 Trigger animasi HIT
            Debug.Log("🏁 VFX HIT triggered");

            StartCoroutine(ReturnToCreateAfterDelay());
        }
    }

    private System.Collections.IEnumerator ReturnToCreateAfterDelay()
    {
        yield return new WaitForSeconds(hitToCreateDelay); // ⏱️ Tunggu animasi hit selesai
        vfx.SendEvent("create"); // 🔁 Balik ke efek CREATE
        Debug.Log("🔁 VFX CREATE reset again");

        isTriggered = false; // Boleh trigger ulang kalau dibutuhkan
    }
}
