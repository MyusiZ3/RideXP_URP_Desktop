using UnityEngine;
using TMPro; // Untuk TextMeshPro

public class VehicleSpeedDisplay : MonoBehaviour
{
    public TMP_Text speedText; // Menggunakan TMP_Text dari TextMeshPro
    public Rigidbody vehicleRigidbody; // Assign Rigidbody kendaraan dari Inspector

    void Update()
    {
        if (vehicleRigidbody != null && speedText != null)
        {
            // 1. Dapatkan kecepatan aktual dari Rigidbody (gunakan .velocity)
            float currentPhysicalSpeed = vehicleRigidbody.linearVelocity.magnitude; // Kecepatan dalam m/s

            // 2. Konversi ke km/jam untuk ditampilkan
            float speedInKmh = currentPhysicalSpeed * 3.6f;

            // 3. Tampilkan kecepatan pada UI
            speedText.text = "Speed: " + Mathf.FloorToInt(speedInKmh).ToString() + " km/h";
        }
        else
        {
            if (speedText != null)
            {
                speedText.text = "Speed: N/A";
            }
            // Tambahkan Debug.LogWarning jika vehicleRigidbody null agar mudah ditrace
            if (vehicleRigidbody == null && speedText != null)
            {
                // Debug.LogWarning("Vehicle Rigidbody belum di-assign pada VehicleSpeedDisplay.");
            }
        }
    }
}
// using UnityEngine;
// using TMPro; // Untuk TextMeshPro

// public class VehicleSpeedDisplay : MonoBehaviour
// {
//     public TMP_Text speedText; // Menggunakan TMP_Text dari TextMeshPro
//     public Rigidbody vehicleRigidbody;
//     public float speed; // Kecepatan kendaraan dalam satuan meter per detik

//     // Menambahkan nilai untuk akselerasi
//     public float gravityInfluence = 9.81f; // Pengaruh gravitasi
//     public float dragMultiplier = 0.1f; // Faktor drag

//     void Update()
//     {
//         // Menghitung kecepatan kendaraan dengan Rigidbody
//         speed = vehicleRigidbody.linearVelocity.magnitude; // Menggunakan velocity untuk mendapatkan kecepatan

//         // Menghitung tambahan kecepatan karena gravitasi (penurunan)
//         if (vehicleRigidbody.linearVelocity.y < 0) // Cek apakah kendaraan sedang turun
//         {
//             speed += gravityInfluence * Time.deltaTime; // Tambahkan percepatan ke bawah karena gravitasi
//         }

//         // Terapkan drag (jika perlu) untuk memperlambat kendaraan
//         float drag = speed * dragMultiplier;
//         speed = Mathf.Max(0, speed - drag);

//         // Menampilkan kecepatan pada UI menggunakan TextMeshPro
//         speedText.text = "Speed: " + Mathf.Floor(speed * 3.6f).ToString() + " km/h"; // Mengonversi m/s ke km/h
//     }
// }
