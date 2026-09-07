using UnityEngine;
using System.Collections.Generic;

public class BranchSelector : MonoBehaviour
{
    // Enum untuk menentukan mode perilaku BranchSelector ini
    public enum BehaviorMode
    {
        OfferBranches,          // Mode standar: menawarkan pilihan cabang atau lurus
        ReturnToSpecificSpline  // Mode baru: seperti EndOfBranchTrigger, mengarahkan ke spline tertentu
    }

    [Header("Mode Perilaku Trigger")]
    [Tooltip("Tentukan bagaimana trigger ini akan berperilaku saat NPC masuk.")]
    public BehaviorMode currentBehavior = BehaviorMode.OfferBranches;

    [Header("Pengaturan untuk Mode 'OfferBranches'")]
    [Tooltip("Jalur utama jika NPC memilih untuk tidak mengambil cabang (opsional)")]
    public RamSpline mainPathContinuationSpline;

    [Tooltip("Cabang jalur alternatif yang bisa dipilih NPC")]
    public List<RamSpline> branchOptions;

    [Range(0f, 1f)]
    [Tooltip("Probabilitas NPC mengambil cabang (0 = selalu lurus, 1 = selalu ambil cabang jika ada)")]
    public float branchTakeProbability = 0.5f;

    [Header("Pengaturan untuk Mode 'ReturnToSpecificSpline'")]
    [Tooltip("Spline jalan utama yang akan dituju NPC jika mode adalah ReturnToSpecificSpline.")]
    public RamSpline destinationSplineOnEnter; // Menggantikan mainRoadSpline dari EndOfBranchTrigger

    [Header("Pengaturan untuk OnTriggerExit (Opsional)")]
    [Tooltip("Spline yang akan dituju NPC saat KELUAR dari trigger ini. Hati-hati jika digunakan bersamaan dengan mode ReturnToSpecificSpline.")]
    public RamSpline splineToFollowOnExit; // Sebelumnya 'returnToSpline'

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPCRider")) // Pastikan tag NPC Anda adalah "NPCRider"
        {
            var ai = other.GetComponent<NPCBicycleAIController>();
            if (ai == null)
            {
                Debug.LogWarning($"Objek {other.name} dengan tag NPCRider tidak memiliki komponen NPCBicycleAIController.");
                return;
            }

            if (currentBehavior == BehaviorMode.OfferBranches)
            {
                HandleOfferBranches(ai);
            }
            else if (currentBehavior == BehaviorMode.ReturnToSpecificSpline)
            {
                HandleReturnToSpecificSpline(ai);
            }
        }
    }

    private void HandleOfferBranches(NPCBicycleAIController ai)
    {
        RamSpline chosenPath = null;
        bool wantsToBranch = Random.value < branchTakeProbability;

        if (wantsToBranch && branchOptions != null && branchOptions.Count > 0)
        {
            chosenPath = branchOptions[Random.Range(0, branchOptions.Count)];
            Debug.Log($"NPC {ai.name} ({gameObject.name}) [Mode: OfferBranches] memilih jalur CABANG: {(chosenPath != null ? chosenPath.name : "TIDAK ADA")}");
        }
        else if (mainPathContinuationSpline != null)
        {
            chosenPath = mainPathContinuationSpline;
            Debug.Log($"NPC {ai.name} ({gameObject.name}) [Mode: OfferBranches] memilih jalur UTAMA/LURUS: {(chosenPath != null ? chosenPath.name : "TIDAK ADA")}");
        }
        else if (branchOptions != null && branchOptions.Count > 0)
        {
            // Fallback jika tidak ada mainPathContinuationSpline tapi ada branchOptions (dan randomnya memilih lurus)
            chosenPath = branchOptions[Random.Range(0, branchOptions.Count)];
            Debug.Log($"NPC {ai.name} ({gameObject.name}) [Mode: OfferBranches] fallback memilih jalur CABANG: {(chosenPath != null ? chosenPath.name : "TIDAK ADA")}");
        }

        if (chosenPath != null)
        {
            ai.SetNewPathFromSpline(chosenPath);
        }
        else
        {
            Debug.LogWarning($"NPC {ai.name} ({gameObject.name}) [Mode: OfferBranches] tidak menemukan jalur untuk dipilih.");
        }
    }

    private void HandleReturnToSpecificSpline(NPCBicycleAIController ai)
    {
        if (destinationSplineOnEnter != null)
        {
            Debug.Log($"NPC {ai.name} ({gameObject.name}) [Mode: ReturnToSpecificSpline] kembali ke jalur: {destinationSplineOnEnter.name}");
            ai.SetNewPathFromSpline(destinationSplineOnEnter);
        }
        else
        {
            Debug.LogWarning($"NPC {ai.name} ({gameObject.name}) [Mode: ReturnToSpecificSpline] destinationSplineOnEnter belum di-assign.");
        }
    }

    // OnTriggerExit tetap ada, tapi penggunaannya perlu hati-hati.
    // Jika Anda menggunakan trigger khusus di ujung cabang dengan mode ReturnToSpecificSpline (yang bekerja pada OnTriggerEnter),
    // maka logika OnTriggerExit pada BranchSelector *awal* mungkin tidak lagi diperlukan atau bisa menyebabkan konflik.
    private void OnTriggerExit(Collider other)
    {
        if (splineToFollowOnExit != null && other.CompareTag("NPCRider"))
        {
            var ai = other.GetComponent<NPCBicycleAIController>();
            if (ai != null)
            {
                // Pesan log ini membantu memahami kapan ini terpicu.
                Debug.Log($"NPC {ai.name} KELUAR dari trigger {gameObject.name}. Jika splineToFollowOnExit di-assign ({splineToFollowOnExit.name}), akan pindah jalur. Ini mungkin konflik dengan trigger EndOfBranch.");
                // Pertimbangkan baik-baik apakah baris di bawah ini masih diperlukan jika Anda menggunakan trigger EndOfBranch dengan mode ReturnToSpecificSpline.
                // ai.SetNewPathFromSpline(splineToFollowOnExit);
            }
        }
    }
}