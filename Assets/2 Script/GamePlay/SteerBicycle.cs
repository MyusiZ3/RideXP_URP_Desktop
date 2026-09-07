using UnityEngine;

public class SteerBicycle : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Steering Settings")]
    [Tooltip("Defines the maximum steering angle for the bicycle")]
    [SerializeField] float maxSteeringAngle = 45f; // Sudut maksimal belokan roda depan
    [Tooltip("Sets how current_MaxSteering is reduced based on the speed of the Rigidbody")]
    [Range(0f, 1f)] [SerializeField] float steerReductorAmount = 0.5f; // Pengurangan steering pada kecepatan tinggi
    [Tooltip("Sets the Steering sensitivity [Steering Stiffness] 0 - No turn, 1 - FastTurn)")]
    [Range(0.001f, 1f)] [SerializeField] float steerSensitivity = 1f; // Sensitivitas belokan
    [Tooltip("Steering smoothing factor for gradual turning")]
    [Range(0.1f, 1f)] [SerializeField] float turnSmoothing = 0.5f; // Penghalusan belokan untuk transisi lebih halus

    // Untuk kontrol steering
    private float customSteerAxis;

    [Header("Object References")]
    [SerializeField] Transform frontWheelTransform; // Roda depan sepeda

    private float currentSteeringAngle; // Sudut belokan saat ini
    private float current_maxSteeringAngle; // Sudut belokan maksimal yang sedang diterapkan

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (frontWheelTransform == null)
        {
            Debug.LogError("Roda depan tidak di-assign! Pastikan transform roda depan sudah diatur.");
        }
    }

    void Update()
    {
        // Sampel input steering horizontal pada Update() agar responsif
        customSteerAxis = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        // Menghitung pengurangan steering berdasarkan kecepatan di FixedUpdate
        MaxSteeringReductor();

        // Menghitung sudut belokan berdasarkan input dan sensitivitas
        float turnAngle = customSteerAxis * current_maxSteeringAngle * steerSensitivity;

        // Terapkan rotasi pada roda depan sepeda di FixedUpdate
        ApplySteering(turnAngle);
    }

    // Fungsi untuk menghitung pengurangan steering berdasarkan kecepatan
    void MaxSteeringReductor()
    {
        // Mengurangi steering pada kecepatan tinggi
        float speedFactor = rb.linearVelocity.magnitude / 30f; // Asumsi kecepatan 30 adalah kecepatan tertinggi
        float reductionFactor = Mathf.Clamp(speedFactor * steerReductorAmount, 0f, 1f); // Batas pengurangan steering
        current_maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 5f, reductionFactor); // Mengurangi steering pada kecepatan tinggi
    }

    // Fungsi untuk menerapkan steering pada roda depan
    void ApplySteering(float turnAngle)
    {
        // Pastikan kita hanya memutar roda depan
        if (frontWheelTransform != null)
        {
            // Rotasi roda depan sesuai dengan sudut yang dihitung
            frontWheelTransform.localRotation = Quaternion.Euler(0, turnAngle, 0);
        }

        // Menetapkan sudut belokan sepeda sesuai dengan input steering (menggunakan Time.fixedDeltaTime)
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, current_maxSteeringAngle * customSteerAxis, turnSmoothing * 10f * Time.fixedDeltaTime);
    }

    // Update visual handle (jika diperlukan untuk visualisasi steering)
    public void UpdateHandle()
    {
        if (frontWheelTransform != null)
        {
            // Update posisi handle untuk visualisasi
            frontWheelTransform.localRotation = Quaternion.Euler(0, currentSteeringAngle, 0);
        }
    }
}
