// CinematicFinishCamera.cs
using UnityEngine;

public class CinematicFinishCamera : MonoBehaviour
{
    public Transform targetToFollow;
    public float orbitDistance = 7.0f; // Default jika tidak di-override atau dihitung
    public float orbitHeight = 3.0f;   // Default jika tidak di-override atau dihitung
    public float orbitSpeed = 20.0f;
    public Vector3 orbitAxis = Vector3.up;
    public bool lookAtTarget = true;

    private float currentOrbitAngle = 0.0f;

    // Panggil metode ini dari CinematicCameraSequence untuk mengaktifkan dan mengatur kamera
    public void Activate(Transform newTarget, float distanceSetting, float heightSetting, float speedSetting)
    {
        targetToFollow = newTarget;
        orbitSpeed = speedSetting;

        if (targetToFollow != null)
        {
            // 1. Logika untuk Orbit Distance
            if (distanceSetting <= 0.01f) // Jika nilai dari Inspector adalah 0 atau sangat kecil
            {
                // Hitung jarak horizontal saat ini dari kamera ke target
                Vector3 planarCamPos = new Vector3(transform.position.x, targetToFollow.position.y, transform.position.z);
                this.orbitDistance = Vector3.Distance(planarCamPos, targetToFollow.position);
                if (this.orbitDistance < 0.1f) this.orbitDistance = 5f; // Fallback jika terlalu dekat
                Debug.Log($"[{gameObject.name}] OrbitDistance dihitung dari posisi editor: {this.orbitDistance}");
            }
            else
            {
                this.orbitDistance = distanceSetting;
            }

            // 2. Logika untuk Orbit Height (serupa dengan distance)
            // Anda bisa menggunakan nilai khusus (misalnya -999) untuk menandakan "gunakan Y editor"
            // atau jika heightSetting <= 0 (namun 0 bisa jadi height yang valid di atas target).
            // Untuk kesederhanaan, kita anggap heightSetting dari Inspector selalu eksplisit,
            // atau Anda bisa tambahkan logika seperti distance:
            // if (heightSetting adalah nilai khusus atau kondisi tertentu)
            // {
            //     this.orbitHeight = transform.position.y - targetToFollow.position.y;
            // } else {
            this.orbitHeight = heightSetting;
            // }


            // 3. Inisialisasi Sudut Orbit agar Lebih Mulus dari Posisi Awal Kamera
            Vector3 initialDirectionToTarget = targetToFollow.position - transform.position;
            initialDirectionToTarget.y = 0; // Fokus pada arah horizontal

            if (initialDirectionToTarget.sqrMagnitude > 0.001f)
            {
                // Hitung sudut awal berdasarkan posisi kamera relatif terhadap target dan sumbu orbit
                // Kita ingin mencari sudut di sekitar orbitAxis.
                // Jika orbitAxis adalah Y (horizontal orbit):
                // Kita bisa menggunakan Vector3.SignedAngle dari arah referensi (misal, Vector3.forward global)
                // ke proyeksi vektor dari target ke kamera pada bidang XZ.
                Vector3 vectorFromTargetToCamXZ = new Vector3(transform.position.x - targetToFollow.position.x, 0, transform.position.z - targetToFollow.position.z);
                if (vectorFromTargetToCamXZ.sqrMagnitude > 0.001f)
                {
                    // Menghitung sudut awal agar orbit dimulai dari posisi kamera saat ini
                    // Arah referensi untuk orbit horizontal biasanya -target.forward atau Vector3.forward
                    // Kita ingin sudut dari arah mana offset akan dihitung.
                    // Jika orbit menggunakan (Quaternion.AngleAxis * Vector3.forward), maka Vector3.forward adalah 0 derajat.
                    currentOrbitAngle = Vector3.SignedAngle(Vector3.forward, vectorFromTargetToCamXZ.normalized, orbitAxis);
                }
                else currentOrbitAngle = 0f; // Default jika kamera tepat di atas/bawah target
            }
            else
            {
                currentOrbitAngle = 0f; // Default jika tidak ada arah jelas
            }
            Debug.Log($"[{gameObject.name}] Sudut orbit awal dihitung: {currentOrbitAngle}");

        }
        else
        {
            // Fallback jika target null
            this.orbitDistance = distanceSetting > 0.01f ? distanceSetting : 7f;
            this.orbitHeight = heightSetting; // Gunakan saja settingan atau defaultnya jika target null
            currentOrbitAngle = 0f;
            Debug.LogWarning($"[{gameObject.name}] Target null di Activate. Menggunakan parameter orbit default/dari Inspector.");
        }
    }

    // Overload Activate jika hanya ingin mengubah target dan parameter lain sudah di-set di Inspector kamera itu sendiri
    public void Activate(Transform newTarget)
    {
        // Panggil Activate utama dengan nilai -1 (atau 0) untuk distance dan height
        // agar logika "ambil dari editor" terpicu jika nilai Inspector kamera ini juga 0.
        // Atau, asumsikan nilai yang sudah ada di instance ini adalah yang diinginkan.
        float existingDistance = (this.orbitDistance <= 0.01f && newTarget != null) ? Vector3.Distance(new Vector3(transform.position.x, newTarget.position.y, transform.position.z), newTarget.position) : this.orbitDistance;
        float existingHeight = this.orbitHeight; // Asumsikan height sudah benar atau diatur dari CinematicShot

        Activate(newTarget, existingDistance, existingHeight, this.orbitSpeed);
    }

    void LateUpdate()
    {
        if (targetToFollow == null)
        {
            return;
        }

        currentOrbitAngle += orbitSpeed * Time.deltaTime;
        currentOrbitAngle %= 360f;

        Quaternion rotation = Quaternion.AngleAxis(currentOrbitAngle, orbitAxis.normalized);
        // Menggunakan -transform.forward dari target sebagai arah "belakang" target untuk memulai orbit
        // atau Vector3.forward global jika ingin konsisten.
        // Untuk orbit yang dimulai relatif terhadap posisi kamera awal, currentOrbitAngle sudah dihitung di Activate.
        // Jadi, Vector3.forward sebagai arah referensi 0 derajat untuk AngleAxis sudah tepat.
        Vector3 offsetDirection = rotation * Vector3.forward; 
        Vector3 desiredPosition = targetToFollow.position + (offsetDirection * orbitDistance);
        desiredPosition.y = targetToFollow.position.y + orbitHeight;

        transform.position = desiredPosition;

        if (lookAtTarget)
        {
            // Arahkan kamera ke titik sedikit di atas pivot target (misalnya, ke dada/kepala player)
            transform.LookAt(targetToFollow.position + Vector3.up * 1.0f); 
        }
    }
}