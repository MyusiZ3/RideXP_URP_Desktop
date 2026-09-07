using UnityEngine;
using UnityEngine.VFX;

public class VFXTriggerCooldown : MonoBehaviour
{
    public VisualEffect vfx;
    public float hitToCreateDelay = 2f;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Boleh trigger kalau tag-nya Player atau NPCRider
        if (!isTriggered && (other.CompareTag("Player") || other.CompareTag("NPCRider")))
        {
            isTriggered = true;

            vfx.SendEvent("hit"); // 🔥 Mainkan animasi hit
            Debug.Log($"🎯 Triggered by: {other.tag}");

            StartCoroutine(ReturnToCreateAfterDelay());
        }
    }

    private System.Collections.IEnumerator ReturnToCreateAfterDelay()
    {
        yield return new WaitForSeconds(hitToCreateDelay);
        vfx.SendEvent("create"); // 🔁 Reset ke animasi awal
        Debug.Log("🔁 VFX RESET to create");

        isTriggered = false; // Boleh dipakai lagi
    }
}
