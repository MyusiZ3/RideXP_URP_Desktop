using UnityEngine;

public class PointLightMovement : MonoBehaviour
{
    public Transform pointA;    // Titik A
    public Transform pointB;    // Titik B
    public float speed = 2f;    // Kecepatan pergerakan
    private float journeyLength; // Jarak total perjalanan
    private float startTime;    // Waktu mulai pergerakan
    private bool isMovingTowardsB = true; // Mengontrol arah pergerakan (A ke B atau B ke A)

    void Start()
    {
        if (pointA != null && pointB != null)
        {
            journeyLength = Vector3.Distance(pointA.position, pointB.position);  // Menghitung jarak dari A ke B
            startTime = Time.time; // Mencatat waktu mulai pergerakan
        }
    }

    void Update()
    {
        if (pointA != null && pointB != null)
        {
            // Menghitung jarak yang sudah ditempuh berdasarkan waktu
            float distanceCovered = (Time.time - startTime) * speed;
            
            // Menentukan berapa persen dari perjalanan yang sudah ditempuh
            float fractionOfJourney = distanceCovered / journeyLength;

            // Update posisi point light di antara titik A dan B
            if (isMovingTowardsB)
            {
                transform.position = Vector3.Lerp(pointA.position, pointB.position, fractionOfJourney);
            }
            else
            {
                transform.position = Vector3.Lerp(pointA.position, pointB.position, fractionOfJourney);
            }

            // Jika sudah sampai di titik B, langsung balik lagi ke titik A
            if (fractionOfJourney >= 1)
            {
                startTime = Time.time; // Reset waktu mulai pergerakan
                isMovingTowardsB = !isMovingTowardsB; // Tukar arah gerakan
            }
        }
    }
}
