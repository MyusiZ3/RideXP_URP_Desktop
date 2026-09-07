// DelayedAnimatorEnabler.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedAnimatorEnabler : MonoBehaviour
{
    [Tooltip("Daftar semua komponen Animator NPC yang ingin dikontrol.")]
    public List<Animator> targetNpcAnimators;

    [Tooltip("Durasi tunda (detik) setelah sinyal 'mulai' sebelum Animator NPC diaktifkan.")]
    public float activationDelay = 2.0f;

    private Coroutine enableCoroutine;

    /// <summary>
    /// Metode publik untuk menonaktifkan semua Animator yang ada di daftar targetNpcAnimators.
    /// Ini akan dipanggil oleh RaceCountdownManager saat OnRacePrepare.
    /// </summary>
    public void DeactivateTargetAnimators()
    {
        if (targetNpcAnimators == null || targetNpcAnimators.Count == 0)
        {
            Debug.LogWarning("[DelayedAnimatorEnabler] Tidak ada Animator NPC yang di-assign untuk dinonaktifkan.");
            return;
        }

        Debug.Log("[DelayedAnimatorEnabler] Menonaktifkan Animator untuk semua NPC target...");
        foreach (Animator anim in targetNpcAnimators)
        {
            if (anim != null)
            {
                // Set parameter ke idle dulu sebelum disable, untuk jaga-jaga
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("SteerDirection", 0f);
                anim.SetBool("IsTurning", false);
                anim.SetBool("IsSprinting", false);
                // Anda bisa tambahkan reset trigger lompat jika ada
                // anim.ResetTrigger("StartJumpTrigger"); 

                if (anim.gameObject.activeInHierarchy) // Hanya jika GameObject-nya aktif
                {
                    anim.enabled = false;
                }
            }
        }
        Debug.Log("[DelayedAnimatorEnabler] Semua Animator NPC target telah dinonaktifkan.");
    }

    /// <summary>
    /// Metode publik untuk memulai coroutine yang akan mengaktifkan Animator setelah delay.
    /// Ini akan dipanggil oleh RaceCountdownManager saat OnRaceStart.
    /// </summary>
    public void ActivateTargetAnimatorsAfterDelay()
    {
        if (targetNpcAnimators == null || targetNpcAnimators.Count == 0)
        {
            Debug.LogWarning("[DelayedAnimatorEnabler] Tidak ada Animator NPC yang di-assign untuk diaktifkan.");
            return;
        }

        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine); // Hentikan coroutine lama jika ada
        }
        enableCoroutine = StartCoroutine(EnableAnimatorsCoroutine());
    }

    private IEnumerator EnableAnimatorsCoroutine()
    {
        Debug.Log($"[DelayedAnimatorEnabler] Menunggu {activationDelay} detik sebelum mengaktifkan Animator NPC...");
        yield return new WaitForSeconds(activationDelay);

        Debug.Log("[DelayedAnimatorEnabler] Waktu penundaan selesai. Mengaktifkan Animator NPC target...");
        foreach (Animator anim in targetNpcAnimators)
        {
            if (anim != null)
            {
                // Hanya aktifkan animator jika GameObject induknya (NPC) juga aktif dan skrip AI-nya aktif
                NPCBicycleAIController npcAI = anim.GetComponentInParent<NPCBicycleAIController>(); // Dapatkan skrip AI
                if (npcAI != null && npcAI.enabled && anim.gameObject.activeInHierarchy)
                {
                    anim.enabled = true;
                    anim.Rebind(); // Rebind untuk memastikan state awal yang bersih
                    anim.Update(0f); // Paksa update animator
                    Debug.Log($"[DelayedAnimatorEnabler] Animator untuk {anim.gameObject.name} diaktifkan.");
                }
                else if (npcAI != null && !npcAI.enabled)
                {
                    Debug.Log($"[DelayedAnimatorEnabler] Skrip AI untuk {anim.gameObject.name} tidak aktif, Animator tidak diaktifkan.");
                }
            }
        }
        enableCoroutine = null; // Reset referensi coroutine
        Debug.Log("[DelayedAnimatorEnabler] Proses aktivasi Animator NPC selesai.");
    }

    // Opsional: Jika Anda ingin bisa men-trigger ini dari event lain atau tombol
    public void StopActivationProcess()
    {
        if (enableCoroutine != null)
        {
            StopCoroutine(enableCoroutine);
            enableCoroutine = null;
            Debug.Log("[DelayedAnimatorEnabler] Proses aktivasi Animator dihentikan.");
        }
    }
}
