// NPCBicycleAIController.cs (Final Fix Version + Personality)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SBPScripts;

public class NPCBicycleAIController : MonoBehaviour
{
    public WaypointManager waypointManager;
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;
    public float waypointThreshold = 2f;
    public float laneOffsetRange = 1.5f;
    public string obstacleTag = "Obstacle";
    public float obstacleCheckDistance = 5f;
    public float slowDownSpeed = 2f;
    private float lastJumpTime = -999f;
    public float jumpCooldown = 1f; // Bisa atur di Inspector, default 1 detik
    [Header("Jump Settings AI")] // Header baru untuk membedakan
    public float aiJumpChargeTime = 0.2f; // Durasi AI "menahan" lompatan untuk charge

    [Header("Stuck Detection Settings")]
    [Tooltip("Waktu (detik) NPC dianggap stuck sebelum respawn")]
    public float stuckThreshold = 5f;
    public bool isUnderSpeedBoost = false;
    [Header("Off Track Recovery")]
    public float maxOffTrackTime = 4f; // editable dari Inspector
    [Tooltip("Jarak maksimum dari jalur sebelum timer off-track dimulai")]
    public float offTrackDistanceThreshold = 10f; // Misal 10 meter dari jalur
    private float offTrackTimer = 0f;

    // --- Tambahan variabel untuk kontrol respawn agar tidak jitter ---
    [Header("Respawn Cooldown & Jitter Prevention")]
    [Tooltip("Cooldown setelah respawn sebelum NPC bisa respawn lagi")]
    public float respawnSafetyCooldown = 2f; // Misal 2 detik
    private float lastRespawnTime = -999f; // Waktu respawn terakhir

    [Header("Fall & Respawn Settings")]
    public float fallLimitY = -10f;
    public float flyLimitY = 100f;
    public float respawnHeightOffset = 1.5f;
    public float stuckAndBelowYRespawnLimit = 3f;
    public enum NPCPersonality { Normal, Aggressive, Lazy, Zigzag }

    [Header("NPC Personality")]
    public NPCPersonality personality = NPCPersonality.Normal;

    private int currentWaypointIndex = 0;
    private List<Vector3> waypoints;
    private Vector3 offset;
    private Animator animator;
    private Rigidbody rb;
    private bool isSlowingDown = false;
    private BicycleController bikeController; // Variabel baru untuk menyimpan referensi
    private float previousSteerInputForAnimation = 0f; // Untuk deteksi perubahan arah

    private Vector3 lastPosition;
    private float stuckTimer = 0f;

    void Start()
    {
        // Cegah tabrakan antar NPC dari animator (ragdoll / collider animator)
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"), true);

        
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager belum diassign pada NPC: " + gameObject.name); //
            enabled = false;
            return;
        }

        waypoints = waypointManager.waypoints;
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("WaypointManager tidak memiliki waypoints pada NPC: " + gameObject.name); //
            enabled = false;
            return;
        }

        offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f); //
        animator = GetComponent<Animator>(); //
        rb = GetComponent<Rigidbody>(); //
        lastPosition = transform.position; //

        // ... (kode Start Anda yang sudah ada untuk waypointManager, animator, rb) ...
        animator = GetComponent<Animator>(); //
        rb = GetComponent<Rigidbody>(); //
        
        
        // TAMBAHAN: Dapatkan dan simpan komponen BicycleController
        bikeController = GetComponent<BicycleController>(); //
        if (bikeController != null)
        {
            bikeController.isAIControlled = true; // Pastikan BicycleController tahu ini dikontrol AI
            Debug.Log($"{name}: BicycleController ditemukan, isAIControlled diatur ke true.");
        }
        else
        {
            Debug.LogWarning($"{name}: Komponen BicycleController tidak ditemukan pada NPC.");
        }

        lastPosition = transform.position; //
        // ... (sisa kode Start Anda untuk personality) ...

        // Apply personality modifiers
        switch (personality) //
        {
            case NPCPersonality.Aggressive: //
                moveSpeed += 2f;
                turnSpeed += 2f;
                break;
            case NPCPersonality.Lazy: //
                moveSpeed *= 0.6f;
                slowDownSpeed *= 0.6f;
                break;
        }
    }

    void Update()
    {
        // --- Periksa cooldown respawn sebelum melakukan deteksi lain ---
        if (Time.time < lastRespawnTime + respawnSafetyCooldown)
        {
            return; // Masih dalam cooldown respawn, abaikan deteksi untuk sementara
        }
        

        if (waypoints == null || waypoints.Count == 0) return; //

        if (transform.position.y < fallLimitY || transform.position.y > flyLimitY) //
        {
            HandleRespawn("Out of Bounds");
            return;
        }

        // ===========================================
        // MODIFIKASI FITUR RESPAWN OFF-TRACK DI SINI
        // ===========================================
        Vector3 currentTargetWaypoint = waypoints[currentWaypointIndex]; //
        Vector3 previousWaypoint = waypoints[Mathf.Max(0, currentWaypointIndex - 1)]; // Waypoint sebelumnya (min 0)
        
        // Menghitung jarak terpendek dari NPC ke segment garis antara previousWaypoint dan currentTargetWaypoint
        // Ini lebih akurat untuk mendeteksi off-track di jalur lurus panjang
        float distFromPath = HandleOffTrackDetection(previousWaypoint, currentTargetWaypoint, transform.position); //
        
        // KONDISI BARU: Cek jika NPC terlalu jauh DARI segment jalur yang aktif
        if (distFromPath > offTrackDistanceThreshold) //
        {
            offTrackTimer += Time.deltaTime; //
            // Debug.Log($"[{gameObject.name}] Off track! Distance: {distFromPath:F2}m, Timer: {offTrackTimer:F2}s");

            if (offTrackTimer >= maxOffTrackTime) //
            {
                HandleRespawn("Off track too long");
                return;
            }
        }
        else
        {
            offTrackTimer = 0f; // Reset timer jika kembali ke jalur atau jaraknya masih aman
        }
        // ===========================================
        // AKHIR MODIFIKASI RESPAWN OFF-TRACK
        // ===========================================

        Vector3 target = waypoints[currentWaypointIndex] + offset; // Target untuk navigasi
        Vector3 direction = target - transform.position; //
        direction.y = 0; //
        // === 🛞 Drift Detection (NPC keluar lintasan karena boost/overspeed) === 
        Vector3 flatDir = rb.linearVelocity;
        flatDir.y = 0;
        float angleDrift = Vector3.Angle(flatDir.normalized, transform.forward);

        // --- Sesuaikan nilai drift sensitivity, misal dari 45f ke 60f atau 70f ---
        if (angleDrift > 60f && rb.linearVelocity.magnitude > 3f) //
        {
            HandleRespawn("NPC tergelincir keluar lintasan");
            return;
        }

        // === 🚦 Slow Down Saat Belokan Tajam === 
        // float angleToWaypoint = Vector3.Angle(transform.forward, direction.normalized); 
        // if (angleToWaypoint > 35f && moveSpeed > 6f) 
        // { 
        //     isSlowingDown = true; 
        // } 


        RaycastHit hitInfo;
        isSlowingDown = false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hitInfo, obstacleCheckDistance)) //
        {
            if (hitInfo.collider.CompareTag(obstacleTag)) //
            {
                isSlowingDown = true;
                PerformObstacleAvoidance();
            }
        }

        if (direction.magnitude >= 0.1f) //
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
        

        float currentSpeed = isSlowingDown ? slowDownSpeed : moveSpeed; //
        // TAMBAHAN: Atur input pedal untuk BicycleController berdasarkan kecepatan NPC 
        if (bikeController != null) // Pastikan bikeController sudah di-assign
        {
            if (currentSpeed > 0.1f) // Jika NPC bergerak dengan kecepatan yang cukup
            {
                bikeController.pedalInput = 1f; // Beri input pedal maksimal (atau sesuaikan)
                // Anda mungkin juga ingin mengatur bikeController.steerInput di sini jika diperlukan untuk visual 
            }
            else if (currentSpeed > 0.01f) // Jika bergerak sangat pelan
            {
                bikeController.pedalInput = 0.3f; // Input pedal kecil
            }
            else // Jika hampir berhenti atau berhenti
            {
                bikeController.pedalInput = 0f; // Tidak ada input pedal
            }
            if (direction.magnitude > 0.1f) // direction adalah arah ke target waypoint
            {
                Vector3 localTargetDirection = transform.InverseTransformDirection(direction.normalized);
                // localTargetDirection.x akan negatif jika target di kiri, positif jika di kanan. 
                bikeController.steerInput = Mathf.Clamp(localTargetDirection.x * 2f, -1f, 1f); // Kalikan dengan faktor agar lebih responsif
            }
            else
            {
                bikeController.steerInput = 0f;
            }
            if (animator != null) //
            {
                animator.SetFloat("Speed", currentSpeed); //

                if (bikeController != null && bikeController.pedalInput > 0.1f) //
                {
                    animator.SetBool("IsPedaling", true); // kamu bisa atur blend tree / trigger
                }
                else
                {
                    animator.SetBool("IsPedaling", false);
                }
            }

            if (animator != null) //
            {
                animator.SetFloat("Speed", currentSpeed); // Animasi dasar berdasarkan kecepatan

                // Animasi Belok (Menggunakan steerInput dari BicycleController) 
                if (bikeController != null) //
                {
                    float currentSteer = bikeController.steerInput; // Ambil input stir saat ini
                    animator.SetFloat("SteerDirection", currentSteer); // Parameter -1 (kiri), 0 (lurus), 1 (kanan)

                    // Jika ingin trigger "IsTurning" boolean (lebih sederhana untuk beberapa setup animator) 
                    bool isCurrentlyTurning = Mathf.Abs(currentSteer) > 0.2f; // Anggap berbelok jika input stir cukup besar
                    animator.SetBool("IsTurning", isCurrentlyTurning); //


                }


                bool isSprinting = (currentSpeed > moveSpeed * 0.9f && currentSpeed > slowDownSpeed + 1f && !isSlowingDown); // Contoh kondisi sprint
                if (personality == NPCPersonality.Aggressive && currentSpeed > (moveSpeed - 1f)) //
                { // Aggressive NPC cenderung sprint 
                    isSprinting = true;
                }
                animator.SetBool("IsSprinting", isSprinting);

            }
        }

        if (personality == NPCPersonality.Zigzag) //
        {
            float zigzagOffset = Mathf.Sin(Time.time * 3f) * 0.5f;
            transform.position += (transform.forward + transform.right * zigzagOffset) * currentSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }

        if (animator != null)
            animator.SetFloat("Speed", currentSpeed);

        float distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, target.y, transform.position.z), target);
        if (distanceToTarget < waypointThreshold) //
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f);
            lastPosition = transform.position;
            stuckTimer = 0f;
        }
        // ... (logika pergerakan transform.position Anda) ... 

        if (animator != null)
            animator.SetFloat("Speed", currentSpeed); // Ini mengatur animasi umum berdasarkan kecepatan

        // --- Sesuaikan nilai stuckThreshold untuk lebih toleran ---
        // Jika terlalu sensitif, set stuckThreshold lebih tinggi (misal 8f-10f)
        if (Vector3.Distance(transform.position, lastPosition) < 0.1f && currentSpeed > 0.1f) //
        {
            stuckTimer += Time.deltaTime; //
            // Debug.Log($"{gameObject.name} stuckTimer: {stuckTimer:F2}, pos.y: {transform.position.y:F2}");

            if (stuckTimer >= stuckThreshold) //
            {
                // Tambahan: Reset stuckTimer di sini agar tidak langsung trigger lagi setelah respawn
                stuckTimer = 0f;
                if (transform.position.y < stuckAndBelowYRespawnLimit) //
                    HandleRespawn("Stuck + Low Y");
                else
                    HandleRespawn("Stuck (any Y, fallback)");
                return;
            }
        }
        else
        {
            stuckTimer = 0f; // Reset stuckTimer saat NPC bergerak normal lagi
            if (Vector3.Distance(transform.position, lastPosition) >= 1f) //
                lastPosition = transform.position;
        }
    }
    // Di dalam kelas NPCBicycleAIController.cs 
    public void ActivateAIForRace()
    {
        this.enabled = true; //

        if (rb != null) //
        {
            rb.isKinematic = false;
        }
        lastPosition = transform.position; //
        stuckTimer = 0f; //
        isSlowingDown = false; //

        // TAMBAHAN OPSIONAL: Beri input pedal awal jika NPC langsung bergerak 
        if (bikeController != null && moveSpeed > 0.1f) //
        {
            bikeController.pedalInput = 1f; // Langsung beri sinyal mengayuh
        }
        
        // Animator akan diupdate di frame Update() pertama setelah skrip ini aktif. 
        // Jika ingin animasi langsung aktif: 
        // if (animator != null && moveSpeed > 0.1f) 
        // { 
        //     animator.SetFloat("Speed", moveSpeed); 
        // } 


        Debug.Log(gameObject.name + " (NPC): AI ACTIVATED for race."); //
    }
    // --- Metode Baru untuk menginisiasi urutan lompat --- 
    void AttemptSingleJumpSequence()
    {
        if (Time.time < lastJumpTime + jumpCooldown) //
        {
            // Debug.Log($"{gameObject.name} AttemptSingleJumpSequence on cooldown."); 
            return; // Masih dalam cooldown 
        }
        // Debug.Log($"{gameObject.name} Mencoba Urutan Lompat Tunggal."); 
        if (TryGetComponent(out BicycleController bc)) //
        {
            StartCoroutine(ExecuteJumpSequence(bc)); //
            lastJumpTime = Time.time; // Perbarui waktu lompat terakhir
        }
        else Debug.LogWarning($"{gameObject.name} BicycleController tidak ditemukan untuk lompat tunggal.");
    }

    void AttemptDoubleJumpSequence()
    {
        if (Time.time < lastJumpTime + jumpCooldown) // Cooldown untuk lompatan pertama dari double jump
        {
            // Debug.Log($"{gameObject.name} AttemptDoubleJumpSequence on cooldown for first hop."); 
            return; // Masih dalam cooldown 
        }
        // Debug.Log($"{gameObject.name} Mencoba Urutan Lompat Ganda."); 
        if (TryGetComponent(out BicycleController bc)) //
        {
            StartCoroutine(ExecuteDoubleJumpInternal(bc)); // Coroutine internal untuk menangani dua lompatan
            lastJumpTime = Time.time; // Cooldown dimulai setelah menginisiasi urutan lompat ganda
        }
        else Debug.LogWarning($"{gameObject.name} BicycleController tidak ditemukan untuk lompat ganda.");
    }

    // Coroutine Inti untuk Urutan Input Lompat AI 
    IEnumerator ExecuteJumpSequence(BicycleController bc)
    {
        if (bc == null) {
            Debug.LogError($"{gameObject.name} ExecuteJumpSequence: BicycleController is null!");
            yield break;
        }

        Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] {gameObject.name} JUMP SEQ: State 1 (Charge)");
        bc.bunnyHopInputState = 1;
        yield return new WaitForSeconds(aiJumpChargeTime);

        Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] {gameObject.name} JUMP SEQ: State -1 (Release)");
        bc.bunnyHopInputState = -1;
        yield return null; 

        Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] {gameObject.name} JUMP SEQ: State 0 (Reset)");
        bc.bunnyHopInputState = 0;
    }

    // Coroutine untuk menangani dua bagian dari Lompat Ganda 
    IEnumerator ExecuteDoubleJumpInternal(BicycleController bc)
    {
        // Lompatan pertama 
        // Debug.Log($"{gameObject.name} Lompat Ganda - Lompatan Pertama"); 
        yield return StartCoroutine(ExecuteJumpSequence(bc)); //

        // Jeda asli dari skrip Anda (0.3 detik) 
        yield return new WaitForSeconds(0.3f); //

        // Lompatan kedua 
        // Debug.Log($"{gameObject.name} Lompat Ganda - Lompatan Kedua"); 
        yield return StartCoroutine(ExecuteJumpSequence(bc));
    }
    // --- Akhir dari Metode Sistem Lompat --- 

    void PerformObstacleAvoidance()
    {
        float choice = Random.value;
        Debug.Log($"{gameObject.name} avoid mode: {choice:F2}");

        if (choice < 0.33f) //
        {
            Debug.Log($"{gameObject.name} is turning");
            transform.Rotate(0, Random.Range(-45f, 45f), 0);
        }
        else if (choice < 0.66f) //
        {
            Debug.Log($"{gameObject.name} is trying to jump");
            AttemptSingleJumpSequence(); //
        }
        else
        {
            Debug.Log($"{gameObject.name} is attempting double jump");
            AttemptDoubleJumpSequence();
        }
    }


    // Kamu bisa hapus method ini, karena sudah diganti dengan AttemptSingleJumpSequence
    // void TryJump() 
    // { 
    //     if (TryGetComponent(out BicycleController bc)) 
    //         bc.bunnyHopInputState = 1; 
    // } 

    // Kamu bisa hapus method ini, karena sudah diganti dengan AttemptDoubleJumpSequence
    // IEnumerator DelayedDoubleJump() 
    // { 
    //     if (TryGetComponent(out BicycleController bc)) 
    //     { 
    //         bc.bunnyHopInputState = 1; 
    //         yield return new WaitForSeconds(0.3f); 
    //         bc.bunnyHopInputState = 1; 
    //     } 
    // } 

    void HandleRespawn(string reason)
    {
        // --- Periksa cooldown keamanan respawn ---
        if (Time.time < lastRespawnTime + respawnSafetyCooldown)
        {
            Debug.Log($"{gameObject.name} Respawn blocked by safety cooldown.");
            return;
        }

        // ===========================================
        // MODIFIKASI FITUR RESPAWN OFF-TRACK DI SINI
        // ===========================================
        // Reset offTrackTimer saat respawn
        offTrackTimer = 0f;
        // ===========================================
        // AKHIR MODIFIKASI RESPAWN OFF-TRACK
        // ===========================================

        Debug.Log($"{gameObject.name} respawning due to: {reason}"); //
        if (waypoints == null || waypoints.Count == 0) //
        {
            transform.position = Vector3.up * respawnHeightOffset;
            return;
        }

        Vector3 basePoint = waypoints[Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1)]; //
        Vector3 respawnPosition = basePoint + Vector3.up * respawnHeightOffset; //

        if (Physics.Raycast(basePoint + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f)) //
        {
            Vector3 safePos = hit.point + Vector3.up * respawnHeightOffset; //

            // Tambahan: pastikan tidak respawn ke dalam collider
            Collider[] overlaps = Physics.OverlapSphere(safePos, 0.5f); //
            if (overlaps.Length == 0) //
            {
                respawnPosition = safePos;
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} gagal respawn aman, fallback ke posisi waypoint asli."); //
                respawnPosition = basePoint + Vector3.up * 2f; // fallback kasar
            }
        }


        Vector3 directionToNext = (waypoints[(currentWaypointIndex + 1) % waypoints.Count] - basePoint).normalized;
        directionToNext.y = 0;

        transform.position = respawnPosition;
        if (directionToNext != Vector3.zero) //
            transform.rotation = Quaternion.LookRotation(directionToNext);

        if (rb != null) //
        {
            rb.linearVelocity = Vector3.zero; // DIPERBAIKI
            rb.angularVelocity = Vector3.zero;
        }

        stuckTimer = 0f;
        isSlowingDown = false;
        lastPosition = transform.position;
        lastRespawnTime = Time.time; // <--- Set waktu respawn terakhir di sini
    }

    // ===========================================
    // METHOD BARU UNTUK MENGHITUNG JARAK KE SEGMENT GARIS
    // Letakkan method ini di bagian paling bawah class NPCBicycleAIController,
    // di bawah semua method lainnya atau di atas method OnTriggerEnter.
    // ===========================================
    float HandleOffTrackDetection(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineDirection = lineEnd - lineStart;
        float lineLengthSquared = lineDirection.sqrMagnitude; // Kuadrat panjang segmen garis

        if (lineLengthSquared == 0) // Jika lineStart dan lineEnd sama (hanya satu titik)
        {
            return Vector3.Distance(point, lineStart);
        }

        // Hitung proyeksi titik 'point' ke garis yang dibentuk oleh lineStart dan lineEnd
        // clamp01 memastikan proyeksi berada di dalam segmen garis (antara 0 dan 1)
        float t = Vector3.Dot(point - lineStart, lineDirection) / lineLengthSquared;
        t = Mathf.Clamp01(t); // Pastikan proyeksi ada di dalam segmen garis

        // Hitung titik terdekat di segmen garis dari 'point'
        Vector3 projection = lineStart + t * lineDirection;

        // Hitung jarak dari 'point' ke titik proyeksi
        return Vector3.Distance(point, projection);
    }
    // ===========================================
    // AKHIR METHOD BARU
    // ===========================================


    void OnTriggerEnter(Collider other)
    {
        // Cek tag NPC Anda. Jika collider yang masuk adalah NPC itu sendiri, abaikan. 
        if (other.CompareTag("NPCRider") && other.gameObject == this.gameObject) //
        {
            return;
        }

        if (other.CompareTag("ObstacleTrigger")) // Misal ini adalah "Zona Lompat"
        {
            Debug.Log($"{gameObject.name} memasuki zona lompat: {other.name}");
            AttemptSingleJumpSequence(); // Gunakan metode baru
        }
        else if (other.CompareTag("DoubleJumpZone")) //
        {
            Debug.Log($"{gameObject.name} memasuki zona lompat ganda: {other.name}");
            AttemptDoubleJumpSequence(); // Gunakan metode baru
        }
        else if (other.CompareTag("TurnTrigger")) //
        {
            Debug.Log($"{gameObject.name} memasuki zona belok: {other.name}, belok instan.");
            // Untuk belok, cooldown mungkin juga berguna jika tidak ingin terlalu sering 
            if (Time.time > lastJumpTime + jumpCooldown) // Menggunakan jumpCooldown untuk trigger belok juga
            {
                transform.Rotate(0, Random.Range(-45f, 45f), 0); // Belok instan
                lastJumpTime = Time.time; // Perbarui waktu untuk cooldown belok
            }
        }
        else if (other.CompareTag("SpeedBoost")) //
        {
            StartCoroutine(SpeedBoostRoutine());
        }
        else if (other.CompareTag("BranchPoint")) //
        {
            var branch = other.GetComponent<BranchSelector>();
            if (branch != null) //
                Debug.Log(gameObject.name + " bertemu BranchPoint: " + other.name);
        }
    }
    void OnTriggerStay(Collider other)
    {
        // Cek tag NPC Anda. Jika collider yang masuk adalah NPC itu sendiri, abaikan. 
        if (other.CompareTag("NPCRider") && other.gameObject == this.gameObject) //
        {
            return;
        }

        // Aksi berulang di trigger zone jika cooldown memungkinkan 
        if (other.CompareTag("ObstacleTrigger")) //
        {
            // Debug.Log($"{gameObject.name} berada di zona lompat: {other.name}, mencoba lompat jika cooldown selesai."); 
            AttemptSingleJumpSequence(); // Cooldown sudah dicek di dalam metode ini
        }
        else if (other.CompareTag("DoubleJumpZone")) //
        {
            // Debug.Log($"{gameObject.name} berada di zona lompat ganda: {other.name}, mencoba lompat ganda jika cooldown selesai."); 
            AttemptDoubleJumpSequence(); // Cooldown sudah dicek di dalam metode ini
        }
        else if (other.CompareTag("TurnTrigger")) //
        {
            // Untuk TurnTrigger, cooldown penting agar tidak berputar terus menerus 
            if (Time.time > lastJumpTime + jumpCooldown) // Menggunakan jumpCooldown untuk trigger belok juga
            {
                Debug.Log($"{gameObject.name} belok ulang di zona belok: {other.name}");
                transform.Rotate(0, Random.Range(-45f, 45f), 0); // Belok instan
                lastJumpTime = Time.time; // Update waktu untuk menghormati cooldown
            }
        }
    }


    IEnumerator SpeedBoostRoutine()
    {
        Debug.Log($"{gameObject.name} SpeedBoost diaktifkan!");
        float originalSpeed = moveSpeed;
        float boostMultiplier = 1.5f;
        float maxBoostSpeed = 12f; // batas aman saat speed boost

        moveSpeed *= boostMultiplier;
        isUnderSpeedBoost = true;

        if (TryGetComponent(out BicycleController bc)) //
        {
            bc.ApplySpeedBoost(boostMultiplier, 3f);
        }
        else if (animator != null)
            animator.SetFloat("SpeedMultiplier", boostMultiplier);

        float duration = 3f;
        while (duration > 0f) //
        {
            // Clamping speed biar nggak ngebut ngawur 
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxBoostSpeed);
            duration -= Time.deltaTime;
            yield return null;
        }

        moveSpeed = originalSpeed;
        isUnderSpeedBoost = false;
        if (animator != null) animator.SetFloat("SpeedMultiplier", 1f);
        Debug.Log($"{gameObject.name} SpeedBoost berakhir.");
    }

    // Letakkan ini di dalam kelas NPCBicycleAIController.cs 
    public void SetNewPathFromSpline(RamSpline newSpline)
    {
        // 1. Validasi Input Awal 
        if (newSpline == null || newSpline.points == null || newSpline.points.Count == 0) //
        {
            Debug.LogWarning("SetNewPathFromSpline dipanggil dengan spline tidak valid untuk NPC: " + gameObject.name);
            return;
        }

        List<Vector3> newPathPoints = new List<Vector3>(newSpline.points); // Salin poin ke list baru

        // 2. Inisialisasi untuk Mencari Titik Awal Terbaik 
        int bestStartingIndex = 0; // Indeks default adalah 0 (waypoint pertama)
        if (newPathPoints.Count > 0) //
        {
            float minDistanceToPoint = float.MaxValue;
            Vector3 npcPosition = transform.position;   // Posisi NPC saat ini
            Vector3 npcsForward = transform.forward;    // Arah hadap NPC saat ini

            // 3. MODIFIKASI UTAMA: Mencari Waypoint Terdekat di Jalur Baru 
            // Loop ini akan mencari titik di 'newPathPoints' yang secara geometris paling dekat dengan posisi NPC. 
            for (int i = 0; i < newPathPoints.Count; i++) //
            {
                float distance = Vector3.Distance(npcPosition, newPathPoints[i]);
                if (distance < minDistanceToPoint) //
                {
                    minDistanceToPoint = distance;
                    bestStartingIndex = i; // 'bestStartingIndex' kini menunjuk ke waypoint terdekat
                }
            }

            // 4. MODIFIKASI UTAMA: Logika "Pintar" untuk Mencegah Putar Balik 
            // Setelah menemukan waypoint terdekat, kita cek apakah waypoint tersebut ada di depan NPC 
            // atau justru di belakangnya. Jika di belakang, kita coba lihat waypoint setelahnya. 
            if (newPathPoints.Count > 1) // Perlu minimal 2 poin untuk bisa menentukan arah segmen spline
            {
                // Hitung arah dari NPC ke waypoint terdekat yang ditemukan 
                Vector3 directionToClosestPoint = (newPathPoints[bestStartingIndex] - npcPosition).normalized;
                // Hitung dot product antara arah hadap NPC dan arah ke waypoint terdekat. 
                // Jika dotProduct < 0, berarti waypoint terdekat ada di belakang NPC. 
                // Jika dotProduct > 0, berarti di depan. 
                // Jika dotProduct ~ 0, berarti di samping. 
                float dotProduct = Vector3.Dot(npcsForward, directionToClosestPoint);

                // Kondisi A: Jika waypoint terdekat ada di BELAKANG NPC (dotProduct < -0.3f, angka -0.3f memberi sedikit toleransi) 
                // DAN masih ada waypoint setelah waypoint terdekat tersebut. 
                if (dotProduct < -0.3f && bestStartingIndex + 1 < newPathPoints.Count) //
                {
                    // Kita cek arah segmen DARI waypoint terdekat KE waypoint SETELAHNYA, 
                    // relatif terhadap arah hadap NPC. 
                    Vector3 directionOfSegmentAfterClosest = (newPathPoints[bestStartingIndex + 1] - newPathPoints[bestStartingIndex]).normalized;
                    if (Vector3.Dot(npcsForward, directionOfSegmentAfterClosest) > 0f) // Jika segmen berikutnya ini mengarah ke DEPAN NPC
                    {
                        // Maka, lebih baik NPC memulai dari waypoint SETELAH waypoint terdekat. 
                        bestStartingIndex = (bestStartingIndex + 1);
                        Debug.Log($"{gameObject.name} ({newSpline.name}): Titik terdekat di belakang, memulai dari indeks berikutnya {bestStartingIndex}");
                    }
                    else
                    {
                        // Jika titik terdekat di belakang, dan segmen setelahnya juga tidak mengarah ke depan, 
                        // maka tetap gunakan titik terdekat (meskipun mungkin perlu berputar sedikit). 
                        Debug.Log($"{gameObject.name} ({newSpline.name}): Titik terdekat di belakang ({dotProduct:F2}), tapi segmen berikutnya juga tidak ideal. Tetap di titik terdekat {bestStartingIndex}.");
                    }
                }
                else // Kondisi B: Waypoint terdekat ada di depan atau di samping NPC
                {
                    Debug.Log($"{gameObject.name} ({newSpline.name}): Memulai dari titik terdekat di indeks {bestStartingIndex} (dotProduct: {dotProduct:F2}).");
                }
            }
        } // Akhir dari blok if (newPathPoints.Count > 0) 

        // 5. Atur Path Baru dan Reset State NPC 
        waypoints = newPathPoints;
        currentWaypointIndex = bestStartingIndex; // NPC akan mulai dari 'bestStartingIndex' yang sudah ditentukan

        offset = new Vector3(Random.Range(-laneOffsetRange, laneOffsetRange), 0f, Random.Range(-laneOffsetRange, laneOffsetRange) * 0.2f);
        if (transform != null) //
            lastPosition = transform.position;
        else if (waypoints.Count > 0) // Fallback jika transform null (sangat tidak mungkin terjadi di MonoBehaviour)
            lastPosition = waypoints[0];

        stuckTimer = 0f;
        isSlowingDown = false;
        // isAvoiding = false; // Reset state ini jika Anda menggunakannya 

        Debug.Log(gameObject.name + " menerima jalur baru: " + (newSpline.name ?? "UnnamedSpline") + " dengan " + waypoints.Count + " waypoints. Mulai dari indeks " + currentWaypointIndex);

        // 6. Opsi Tambahan (dikomentari): Langsung Hadapkan NPC ke Target Baru 
        // Bagian ini bisa diaktifkan jika Anda ingin NPC langsung berputar menghadap waypoint pertama 
        // di jalur baru secara instan. Bisa membuat transisi terlihat lebih 'patah' tapi mungkin 
        // lebih cepat mengarahkan NPC. Tanpa ini, NPC akan berputar secara bertahap di frame Update berikutnya. 
        // if (waypoints.Count > currentWaypointIndex) { 
        //     Vector3 immediateTarget = waypoints[currentWaypointIndex] + offset; 
        //     Vector3 immediateDirection = (immediateTarget - transform.position); 
        //     immediateDirection.y = 0; 
        //     if (immediateDirection.sqrMagnitude > 0.01f) { // hindari look rotation ke zero vector 
        //         transform.rotation = Quaternion.LookRotation(immediateDirection.normalized); 
        //     } 
        // } 
    }
}