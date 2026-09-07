using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFXTriggerHandler : MonoBehaviour
{
    [Header("VFX References")]
    public VisualEffect vfxSpeedBoost;   // Efek burst speed
    public VisualEffect vfxSpeedTrail;   // Efek trail
    public VisualEffect vfxRespawn;      // Efek respawn looping

    [Header("VFX Duration")]
    public float speedBoostDuration = 1.5f;
    public float trailHitDelay = 1.5f;
    public float respawnHitDelay = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpeedBoost"))
        {
            // ✨ VFX burst
            if (vfxSpeedBoost != null)
            {
                vfxSpeedBoost.SendEvent("in");
                StartCoroutine(DisableAfterDelay(vfxSpeedBoost, speedBoostDuration));
            }

            // ✨ Trail (create → hit)
            if (vfxSpeedTrail != null)
            {
                vfxSpeedTrail.SendEvent("create");
                StartCoroutine(TriggerDelayedEvent(vfxSpeedTrail, "hit", trailHitDelay));
            }

            Debug.Log("⚡ SpeedBoost triggered");
        }
        else if (other.CompareTag("RespawnPoint"))
        {
            // ✨ Respawn (create → hit)
            if (vfxRespawn != null)
            {
                vfxRespawn.SendEvent("loop");
                StartCoroutine(TriggerDelayedEvent(vfxRespawn, "hit", respawnHitDelay));
            }

            Debug.Log("🌀 RespawnPoint triggered");
        }
    }

    IEnumerator DisableAfterDelay(VisualEffect vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        vfx.SendEvent("out");
    }

    IEnumerator TriggerDelayedEvent(VisualEffect vfx, string eventName, float delay)
    {
        yield return new WaitForSeconds(delay);
        vfx.SendEvent(eventName);
    }
}
